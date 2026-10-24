using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Ulu ağacın tacı (SRS 3.11, FR-54): yalnızca görsel, saklanma sağlamaz. Taç yarıçapı içinde canlı bir karakter
    /// varsa taç yarı saydam çizilir. Taç yarıçapı balance.json map.greatTreeCanopyRadiusPx'ten gelir.
    /// MapGenerator ulu ağaca takar. Furkan'ın taç sprite'ı gelince yalnızca görsel değişir.
    /// </summary>
    public class CanopyFade : MonoBehaviour
    {
        // Yer tutucu görsel (Furkan'ın sprite'ı gelince değişir). Oyun değeri değil.
        static readonly Color CanopyColor = new Color32(60, 120, 45, 255);
        const float NormalAlpha = 0.55f;
        const float FadedAlpha = 0.25f;
        const float FadeSpeed = 6f;   // saniyede alfa değişimi

        SpriteRenderer _renderer;
        float _radiusUnits;
        float _alpha = NormalAlpha;

        public void Init(float radiusPx)
        {
            _radiusUnits = Units.ToUnits(radiusPx);

            var go = new GameObject("Tac");
            go.hideFlags = Application.isPlaying ? HideFlags.None : HideFlags.DontSave;
            go.transform.SetParent(transform, false);
            go.transform.localScale = new Vector3(_radiusUnits * 2f, _radiusUnits * 2f, 1f);

            _renderer = go.AddComponent<SpriteRenderer>();
            _renderer.sprite = MapGenerator.Circle;
            _renderer.sortingLayerName = "Canopy";
            Apply();
        }

        void Update()
        {
            if (_renderer == null) return;

            float target = AnyoneUnder() ? FadedAlpha : NormalAlpha;
            _alpha = Mathf.MoveTowards(_alpha, target, FadeSpeed * Time.deltaTime);
            Apply();
        }

        bool AnyoneUnder()
        {
            float sqr = _radiusUnits * _radiusUnits;
            Vector2 center = transform.position;
            foreach (Character c in Services.Match.Characters)
            {
                if (c == null || !c.IsAlive) continue;
                if ((c.Position - center).sqrMagnitude <= sqr) return true;
            }
            return false;
        }

        void Apply()
        {
            Color color = CanopyColor;
            color.a = _alpha;
            _renderer.color = color;
        }
    }
}
