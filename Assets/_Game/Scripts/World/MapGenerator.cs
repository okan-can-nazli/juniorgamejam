using System.Collections.Generic;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Map.prefab üzerinde (SRS 3.12). Maç başında yerleşimi MapLayoutGenerator'dan alıp sahneye kurar.
    /// Görseller gelene kadar yer tutucu şekiller kullanılır; çarpışma ve katmanlar gerçektir.
    /// WorldService ve LeafSpawner'dan önce hazır olsun diye Awake'te ve erken sırada çalışır.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class MapGenerator : MonoBehaviour
    {
        // Yer tutucu renkler (Furkan'ın sprite'ları gelince görseller değişir).
        static readonly Color GrassColor = new Color32(86, 140, 70, 255);
        static readonly Color ClearingColor = new Color32(150, 120, 80, 255);
        static readonly Color TrunkColor = new Color32(90, 60, 35, 255);
        static readonly Color GreatTrunkColor = new Color32(60, 40, 25, 255);
        static readonly Color RockColor = new Color32(130, 130, 130, 255);
        static readonly Color BushColor = new Color32(40, 100, 50, 200);

        static Sprite _circle, _square;

        readonly List<Vector2> _spawnPoints = new List<Vector2>();
        int _layerObstacle, _layerTree, _layerBush;

        /// <summary>Kurulan yerleşim (px, merkeze göre).</summary>
        public MapLayout Layout { get; private set; }

        /// <summary>Doğma noktaları, Unity birimi. WorldService.SpawnPoints bunu kullanacak.</summary>
        public IReadOnlyList<Vector2> SpawnPoints => _spawnPoints;

        void Awake()
        {
            MapGenSettings s = MapGenSettings.FromConfig(Services.Config);
            Build(MapLayoutGenerator.Generate(s, Services.Rng), s);
        }

        /// <summary>Eski çocukları siler ve verilen yerleşimi baştan kurar. Editördeki önizleme de bunu kullanır.</summary>
        public void Build(MapLayout layout, MapGenSettings s)
        {
            _layerObstacle = LayerMask.NameToLayer("Obstacle");
            _layerTree = LayerMask.NameToLayer("Tree");
            _layerBush = LayerMask.NameToLayer("Bush");

            Clear();
            Layout = layout;
            _spawnPoints.Clear();

            BuildGround(s);
            BuildBounds(s);

            foreach (MapItem it in layout.Items)
            {
                switch (it.Kind)
                {
                    case MapItemKind.GreatTree: BuildTree(it, "UluAgac", GreatTrunkColor); break;
                    case MapItemKind.ClusterTree: BuildTree(it, "Kume" + it.Group + "_Agac", TrunkColor); break;
                    case MapItemKind.GroveTree: BuildTree(it, "Koru" + it.Group + "_Agac", TrunkColor); break;
                    case MapItemKind.Rock: BuildRock(it, s); break;
                    case MapItemKind.OuterBush:
                    case MapItemKind.InnerBush: BuildBush(it); break;
                    case MapItemKind.Spawn: BuildSpawn(it); break;
                }
            }
        }

        // ---------------------------------------------------------------- parçalar

        void BuildGround(MapGenSettings s)
        {
            GameObject ground = NewObject("Zemin", transform, 0);
            AddSprite(ground.transform, Square, GrassColor, "Ground", 0, s.MapSizePx, s.MapSizePx);
            AddSprite(ground.transform, Circle, ClearingColor, "Ground", 1, s.ClearingRadiusPx * 2f, s.ClearingRadiusPx * 2f);
        }

        // Harita sınırı: 4 kutu collider, Obstacle katmanında, haritanın hemen dışında.
        void BuildBounds(MapGenSettings s)
        {
            GameObject bounds = NewObject("Sinir", transform, _layerObstacle);
            float half = Units.ToUnits(s.MapSizePx) * 0.5f;
            const float t = 2f;   // duvar kalınlığı (birim)

            AddWall(bounds, new Vector2(0f, half + t * 0.5f), new Vector2(half * 2f + t * 2f, t));
            AddWall(bounds, new Vector2(0f, -half - t * 0.5f), new Vector2(half * 2f + t * 2f, t));
            AddWall(bounds, new Vector2(half + t * 0.5f, 0f), new Vector2(t, half * 2f));
            AddWall(bounds, new Vector2(-half - t * 0.5f, 0f), new Vector2(t, half * 2f));
        }

        static void AddWall(GameObject go, Vector2 offset, Vector2 size)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.offset = offset;
            box.size = size;
        }

        // Gövde: Obstacle katmanında çarpışan collider. Balta için ayrıca Tree katmanında trigger "Vurus" çocuğu.
        void BuildTree(MapItem it, string name, Color color)
        {
            GameObject tree = NewObject(name, transform, _layerObstacle);
            tree.transform.localPosition = ToUnits(it.PosPx);

            var body = tree.AddComponent<CircleCollider2D>();
            body.radius = Units.ToUnits(it.RadiusPx);

            GameObject hit = NewObject("Vurus", tree.transform, _layerTree);
            var hitbox = hit.AddComponent<CircleCollider2D>();
            hitbox.radius = body.radius;
            hitbox.isTrigger = true;

            AddSprite(tree.transform, Circle, color, "Characters", 0, it.RadiusPx * 2f, it.RadiusPx * 2f);
        }

        void BuildRock(MapItem it, MapGenSettings s)
        {
            GameObject rock = NewObject("Kaya" + it.Group, transform, _layerObstacle);
            rock.transform.localPosition = ToUnits(it.PosPx);

            var box = rock.AddComponent<BoxCollider2D>();
            box.size = new Vector2(Units.ToUnits(s.RockSizePx), Units.ToUnits(s.RockSizePx));

            AddSprite(rock.transform, Square, RockColor, "Characters", 0, s.RockSizePx, s.RockSizePx);
        }

        // Çalılık engel değil: Bush katmanında trigger.
        void BuildBush(MapItem it)
        {
            string name = (it.Kind == MapItemKind.OuterBush ? "DisCalilik" : "IcCalilik") + it.Group;
            GameObject bush = NewObject(name, transform, _layerBush);
            bush.transform.localPosition = ToUnits(it.PosPx);

            var trigger = bush.AddComponent<CircleCollider2D>();
            trigger.radius = Units.ToUnits(it.RadiusPx);
            trigger.isTrigger = true;

            AddSprite(bush.transform, Circle, BushColor, "Bushes", 0, it.RadiusPx * 2f, it.RadiusPx * 2f);
        }

        void BuildSpawn(MapItem it)
        {
            GameObject spawn = NewObject("Dogma" + it.Group, transform, 0);
            spawn.transform.localPosition = ToUnits(it.PosPx);
            _spawnPoints.Add(new Vector2(Units.ToUnits(it.PosPx.x), Units.ToUnits(it.PosPx.y)));
        }

        // ---------------------------------------------------------------- yardımcılar

        void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        // Editörde (Play dışı) kurulan önizleme sahneye kaydedilmesin diye DontSave.
        static GameObject NewObject(string name, Transform parent, int layer)
        {
            var go = new GameObject(name) { layer = layer };
            go.hideFlags = Application.isPlaying ? HideFlags.None : HideFlags.DontSave;
            go.transform.SetParent(parent, false);
            return go;
        }

        static Vector3 ToUnits(Vector2 px) => new Vector3(Units.ToUnits(px.x), Units.ToUnits(px.y), 0f);

        // Görsel ayrı bir çocukta: ölçek yalnızca resmi büyütür, collider'lar etkilenmez.
        static void AddSprite(Transform parent, Sprite sprite, Color color, string sortingLayer, int order, float widthPx, float heightPx)
        {
            GameObject visual = NewObject("Gorsel", parent, 0);
            visual.transform.localScale = new Vector3(Units.ToUnits(widthPx), Units.ToUnits(heightPx), 1f);

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = order;
        }

        // Yer tutucu şekiller koddan üretilir (dosya eklemeyiz: Art/ Furkan'ın). 64x64 beyaz resim, 1 birim büyüklüğünde.
        static Sprite Circle => _circle != null ? _circle : (_circle = MakeSprite(true));
        static Sprite Square => _square != null ? _square : (_square = MakeSprite(false));

        static Sprite MakeSprite(bool circle)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            var pixels = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - r, dy = y + 0.5f - r;
                    bool inside = !circle || dx * dx + dy * dy <= r * r;
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);   // PPU = size → 1 birim
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
