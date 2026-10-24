using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Ölüm ganimeti yığını (SRS 3.10, FR-81). LootSystem Create ile kurar.
    /// İçinde gerçek adetler durur; ekranda toplama göre 1, 2 veya 3 üst üste yaprak görünür.
    /// Canlı bir karakter pickupRadiusPx içine girince her tür için Inventory.TryAdd çağrılır;
    /// sığmayan adet yığında kalır. lifetimeSec sonra kaybolur. 80 yaprak sınırına sayılmaz.
    /// </summary>
    public class LootPile : MonoBehaviour, IPickup
    {
        // Yer tutucu görsel (Furkan'ın sprite'ları gelince değişir).
        const float VisualSizePx = 16f;
        const float StackStepPx = 10f;
        const int MaxVisible = 3;
        static readonly Color PileColor = new Color32(190, 110, 50, 255);

        readonly int[] _counts = new int[4];   // LeafType sırasıyla: Red, Yellow, Green, Orange
        Transform[] _visuals;
        float _age;

        public Vector2 Position => transform.position;
        public bool IsLoot => true;

        /// <param name="counts">LeafType sırasıyla gerçek adetler (K, S, Y, T).</param>
        public static LootPile Create(Transform parent, Vector2 position, int[] counts)
        {
            var go = new GameObject("Ganimet") { layer = LayerMask.NameToLayer("Pickup") };
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = Units.ToUnits(VisualSizePx) * 0.5f;
            collider.isTrigger = true;

            var pile = go.AddComponent<LootPile>();
            counts.CopyTo(pile._counts, 0);

            pile._visuals = new Transform[MaxVisible];
            for (int i = 0; i < MaxVisible; i++)
            {
                var visual = new GameObject("Gorsel" + i);
                visual.transform.SetParent(go.transform, false);
                visual.transform.localPosition = new Vector3(0f, Units.ToUnits(StackStepPx) * i, 0f);
                visual.transform.localScale = new Vector3(Units.ToUnits(VisualSizePx), Units.ToUnits(VisualSizePx), 1f);

                var sr = visual.AddComponent<SpriteRenderer>();
                sr.sprite = MapGenerator.Circle;
                sr.color = Color.Lerp(PileColor, Color.white, i * 0.25f);   // her kat biraz daha açık: sayısı görünsün
                sr.sortingLayerName = "Pile";
                sr.sortingOrder = i;
                pile._visuals[i] = visual.transform;
            }

            pile.Refresh();
            return pile;
        }

        int Total => _counts[0] + _counts[1] + _counts[2] + _counts[3];

        void Update()
        {
            _age += Time.deltaTime;
            if (_age >= Services.Config.loot.lifetimeSec)
            {
                Destroy(gameObject);
                return;
            }

            TryCollect();
        }

        // Yakındaki her canlı karakter için: sığdığı kadar al, kalan yığında kalsın.
        void TryCollect()
        {
            float r = Units.ToUnits(Services.Config.inventory.pickupRadiusPx);
            foreach (Character c in Services.Match.Characters)
            {
                if (c == null || !c.IsAlive) continue;
                if ((c.Position - Position).sqrMagnitude > r * r) continue;

                for (int i = 0; i < _counts.Length; i++)
                    if (_counts[i] > 0)
                        _counts[i] -= c.Inventory.TryAdd((LeafType)i, _counts[i]);

                if (Total == 0)
                {
                    Destroy(gameObject);
                    return;
                }
                Refresh();
            }
        }

        // 1–oneLeafMaxTotal: 1 yaprak, –twoLeavesMaxTotal: 2, üstü: 3 (SRS 3.10).
        void Refresh()
        {
            var loot = Services.Config.loot;
            int total = Total;
            int shown = total <= loot.oneLeafMaxTotal ? 1 : total <= loot.twoLeavesMaxTotal ? 2 : 3;
            for (int i = 0; i < _visuals.Length; i++)
                _visuals[i].gameObject.SetActive(i < shown);
        }
    }
}
