using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Yerdeki yaprak (SRS 3.5, 3.6; FR-31–FR-33). LeafSpawner Create ile kurar.
    /// Düşerken (en fazla fallMaxSec) toplanamaz; yere inince lifetimeSec sonra kaybolur,
    /// son blinkBeforeSec saniyede yanıp söner. Canlı bir karakter pickupRadiusPx içine girince
    /// Inventory.TryAdd(tür, 1) çağrılır: 1 dönerse yaprak yok olur, 0 dönerse (envanter dolu) yerde kalır.
    /// </summary>
    public class Leaf : MonoBehaviour, IPickup
    {
        // Yer tutucu görsel (Furkan'ın sprite'ları gelince değişir).
        const float VisualSizePx = 16f;
        const float FallHeightUnits = 0.75f;   // süzülme başlangıç yüksekliği (yalnızca görsel)
        const float BlinkPeriodSec = 0.2f;

        static readonly Color RedColor = new Color32(200, 50, 40, 255);
        static readonly Color YellowColor = new Color32(230, 200, 50, 255);
        static readonly Color GreenColor = new Color32(80, 170, 60, 255);
        static readonly Color OrangeColor = new Color32(240, 140, 30, 255);

        Transform _visual;
        SpriteRenderer _renderer;
        Collider2D _collider;
        float _fallLeft;
        float _age;

        public LeafType Type { get; private set; }
        public Vector2 Position => transform.position;
        public bool IsLoot => false;

        /// <param name="falling">true: yere süzülerek iner (bu sırada toplanamaz). false: zaten yerde.</param>
        public static Leaf Create(Transform parent, Vector2 position, LeafType type, bool falling)
        {
            var go = new GameObject("Yaprak_" + type) { layer = LayerMask.NameToLayer("Pickup") };
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = Units.ToUnits(VisualSizePx) * 0.5f;
            collider.isTrigger = true;

            var visual = new GameObject("Gorsel");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = new Vector3(Units.ToUnits(VisualSizePx), Units.ToUnits(VisualSizePx), 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = MapGenerator.Circle;
            sr.color = ColorOf(type);
            sr.sortingLayerName = "Pickups";

            var leaf = go.AddComponent<Leaf>();
            leaf.Type = type;
            leaf._visual = visual.transform;
            leaf._renderer = sr;
            leaf._collider = collider;
            leaf._fallLeft = falling ? Services.Config.leaves.fallMaxSec : 0f;
            leaf.ApplyFall();
            return leaf;
        }

        void Update()
        {
            var leaves = Services.Config.leaves;

            if (_fallLeft > 0f)
            {
                _fallLeft -= Time.deltaTime;
                ApplyFall();
                return;
            }

            _age += Time.deltaTime;
            if (_age >= leaves.lifetimeSec)
            {
                Destroy(gameObject);
                return;
            }

            // FR-33: kaybolmasına blinkBeforeSec kala yanıp söner.
            bool blinking = _age >= leaves.lifetimeSec - leaves.blinkBeforeSec;
            _renderer.enabled = !blinking || (int)(_age / BlinkPeriodSec) % 2 == 0;

            TryCollect();
        }

        // Süzülürken görsel yukarıdan iner ve collider kapalıdır; yere inince collider açılır.
        void ApplyFall()
        {
            float fallMax = Services.Config.leaves.fallMaxSec;
            float t = fallMax > 0f ? Mathf.Clamp01(_fallLeft / fallMax) : 0f;
            _visual.localPosition = new Vector3(0f, FallHeightUnits * t, 0f);
            _collider.enabled = _fallLeft <= 0f;
        }

        void TryCollect()
        {
            float r = Units.ToUnits(Services.Config.inventory.pickupRadiusPx);
            foreach (Character c in Services.Match.Characters)
            {
                if (c == null || !c.IsAlive) continue;
                if ((c.Position - Position).sqrMagnitude > r * r) continue;
                if (c.Inventory.TryAdd(Type, 1) > 0)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        static Color ColorOf(LeafType type)
        {
            switch (type)
            {
                case LeafType.Red: return RedColor;
                case LeafType.Yellow: return YellowColor;
                case LeafType.Green: return GreenColor;
                default: return OrangeColor;
            }
        }
    }
}
