using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Yaprak üretimi (SRS 3.5; FR-30, FR-34, FR-73). Map.prefab üzerinde, MapGenerator haritayı kurduktan sonra çalışır.
    /// - Her ağacın kendi sayacı var (ulu ağaç, küme ve koru aralıkları ayrı); sayaç ancak maç Playing iken akar.
    /// - Haritada en fazla leaves.maxOnMap yaprak; sınır doluyken üretim olmaz.
    /// - Alan dışında kalan ağaç üretmez (FR-73); kesilen ağaç üretmez.
    /// - Ağaç devrilince (TreeFelled) 6-10 yaprak saçılır; bunlar sınıra sayılmaz.
    /// - Düşme noktası IsBlocked ise yeniden seçilir (en fazla maxDropTries).
    /// Bütün rastgelelik Services.Rng'den.
    /// </summary>
    public class LeafSpawner : MonoBehaviour
    {
        class Source
        {
            public ITree Tree;
            public float Interval;
            public float OrangeChance;
            public float Countdown;
        }

        readonly List<Source> _sources = new List<Source>();
        readonly List<Leaf> _capped = new List<Leaf>();   // 80 sınırına sayılan yapraklar
        Transform _leaves;

        void OnEnable() => GameEvents.TreeFelled += OnTreeFelled;
        void OnDisable() => GameEvents.TreeFelled -= OnTreeFelled;

        void Start()
        {
            _leaves = new GameObject("Yapraklar").transform;
            _leaves.SetParent(transform, false);

            BuildSources();
            DropStartLeaves();
        }

        void Update()
        {
            if (Services.Match.State != MatchState.Playing) return;

            foreach (Source s in _sources)
            {
                if (s.Tree.Felled) continue;

                s.Countdown -= Time.deltaTime;
                if (s.Countdown > 0f) continue;
                s.Countdown += s.Interval;

                if (!Services.Zone.IsInside(s.Tree.Position)) continue;              // FR-73
                if (CappedCount() >= Services.Config.leaves.maxOnMap) continue;      // sınır doluyken üretim yok

                Drop(s.Tree.Position, Roll(s.OrangeChance), true, true);
            }
        }

        // ---------------------------------------------------------------- kurulum

        void BuildSources()
        {
            var trees = Services.Config.trees;

            GreatTree great = GetComponentInChildren<GreatTree>();
            if (great != null) AddSource(great, trees.greatSpawnIntervalSec, trees.greatOrangeChance);

            foreach (ChoppableTree tree in GetComponentsInChildren<ChoppableTree>())
            {
                bool cluster = tree.Kind == MapItemKind.ClusterTree;
                AddSource(tree,
                    cluster ? trees.clusterSpawnIntervalSec : trees.groveSpawnIntervalSec,
                    cluster ? trees.clusterOrangeChance : trees.groveOrangeChance);
            }
        }

        // Her ağaç aynı anda üretmesin diye ilk sayaç rastgele; sonrasında aralık kadar sürer.
        void AddSource(ITree tree, float interval, float orangeChance)
        {
            _sources.Add(new Source
            {
                Tree = tree,
                Interval = interval,
                OrangeChance = orangeChance,
                Countdown = Services.Rng.Range(0f, interval),
            });
        }

        // Maç başında: ulu ağacın çevresinde startAtGreatTree yaprak (startAtGreatTreeOrange tanesi turuncu),
        // her kümenin çevresinde startPerCluster yaprak.
        void DropStartLeaves()
        {
            var leaves = Services.Config.leaves;
            var trees = Services.Config.trees;

            GreatTree great = GetComponentInChildren<GreatTree>();
            if (great != null)
            {
                for (int i = 0; i < leaves.startAtGreatTree; i++)
                {
                    LeafType type = i < leaves.startAtGreatTreeOrange ? LeafType.Orange : RollRyg();
                    Drop(great.Position, type, true, false);
                }
            }

            var clusters = new SortedDictionary<int, List<ChoppableTree>>();
            foreach (ChoppableTree tree in GetComponentsInChildren<ChoppableTree>())
            {
                if (tree.Kind != MapItemKind.ClusterTree) continue;
                if (!clusters.TryGetValue(tree.Group, out List<ChoppableTree> list))
                    clusters[tree.Group] = list = new List<ChoppableTree>();
                list.Add(tree);
            }

            foreach (List<ChoppableTree> cluster in clusters.Values)
            {
                for (int i = 0; i < leaves.startPerCluster; i++)
                {
                    ChoppableTree tree = Services.Rng.Pick(cluster);
                    Drop(tree.Position, Roll(trees.clusterOrangeChance), true, false);
                }
            }
        }

        // ---------------------------------------------------------------- yaprak bırakma

        // FR-36: devrilen ağaç felledLeavesMin–felledLeavesMax yaprak saçar; 80 sınırına sayılmaz.
        void OnTreeFelled(ITree tree, Character by)
        {
            var trees = Services.Config.trees;
            int count = Services.Rng.Range(trees.felledLeavesMin, trees.felledLeavesMax + 1);
            for (int i = 0; i < count; i++)
                Drop(tree.Position, Roll(trees.felledOrangeChance), false, true);
        }

        // center'ın dropRadiusPx çevresinde geçerli bir nokta bulup yaprak koyar (FR-34).
        // Boşluk yarıçapı karakter yarıçapı kadardır: karakterin durabileceği bir nokta olsun, yaprak gövdeye gömülmesin.
        void Drop(Vector2 center, LeafType type, bool countsToCap, bool falling)
        {
            float radius = Units.ToUnits(Services.Config.trees.dropRadiusPx);
            float clearPx = Services.Config.character.radiusPx;

            for (int i = 0; i < Services.Config.leaves.maxDropTries; i++)
            {
                Vector2 pos = center + Services.Rng.InsideCircle(radius);
                if (Services.World.IsBlocked(pos, clearPx)) continue;

                Leaf leaf = Leaf.Create(_leaves, pos, type, falling);
                if (countsToCap) _capped.Add(leaf);
                GameEvents.RaiseLeafSpawned(pos, type);
                return;
            }
        }

        int CappedCount()
        {
            _capped.RemoveAll(leaf => leaf == null);   // toplanan veya süresi dolan yapraklar
            return _capped.Count;
        }

        static LeafType Roll(float orangeChance) => Services.Rng.Chance(orangeChance) ? LeafType.Orange : RollRyg();

        // K, S, Y eşit olasılıkla (enum sırası: Red, Yellow, Green, Orange).
        static LeafType RollRyg() => (LeafType)Services.Rng.Range(0, 3);
    }
}
