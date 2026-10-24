using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Ulu ağacın tacı (SRS 3.11, FR-54): yalnızca görsel, saklanma sağlamaz. Taç yarıçapı içinde canlı bir karakter
    /// varsa taç yarı saydam çizilir. Taç yarıçapı balance.json map.greatTreeCanopyRadiusPx'ten gelir.
    /// MapGenerator ağaçlara takar. Sprite verilirse Furkan'ın tacı ölçeklenmeden çizilir; verilmezse yer tutucu daire.
    /// </summary>
    public class CanopyFade : MonoBehaviour
    {
        // Saydamlık değerleri yalnızca görsel. Oyun değeri değil.
        static readonly Color PlaceholderColor = new Color32(60, 120, 45, 255);
        const float PlaceholderAlpha = 0.55f;   // yer tutucu daire altındakini hep göstersin
        const float PlaceholderFadedAlpha = 0.25f;
        const float SpriteFadedAlpha = 0.5f;    // Furkan'ın tacı: normalde opak, altında karakter varken yarı saydam
        const float FadeSpeed = 6f;   // saniyede alfa değişimi

        SpriteRenderer _renderer;
        Color _color;
        float _radiusUnits;
        float _normalAlpha, _fadedAlpha, _alpha;

        /// <param name="radiusPx">Altında karakter aranacak yarıçap.</param>
        /// <param name="sprite">Furkan'ın taç sprite'ı; null ise yer tutucu daire.</param>
        public void Init(float radiusPx, Sprite sprite = null)
        {
            _radiusUnits = Units.ToUnits(radiusPx);

            var go = new GameObject("Tac");
            go.hideFlags = Application.isPlaying ? HideFlags.None : HideFlags.DontSave;
            go.transform.SetParent(transform, false);

            _renderer = go.AddComponent<SpriteRenderer>();
            _renderer.sortingLayerName = "Canopy";
            if (sprite != null)
            {
                _renderer.sprite = sprite;
                _color = Color.white;
                _normalAlpha = 1f;
                _fadedAlpha = SpriteFadedAlpha;
            }
            else
            {
                go.transform.localScale = new Vector3(_radiusUnits * 2f, _radiusUnits * 2f, 1f);
                _renderer.sprite = MapGenerator.Circle;
                _color = PlaceholderColor;
                _normalAlpha = PlaceholderAlpha;
                _fadedAlpha = PlaceholderFadedAlpha;
            }
            _alpha = _normalAlpha;
            Apply();
        }

        void Update()
        {
            if (_renderer == null) return;

            float target = AnyoneUnder() ? _fadedAlpha : _normalAlpha;
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
            Color color = _color;
            color.a = _alpha;
            _renderer.color = color;
        }
    }
}
