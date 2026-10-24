using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Ekran prefab'larının kökü: yerleşim 640×360 sanat birimindedir. Kök, üst Canvas'ın Canvas Scaler referansı ne olursa olsun
    /// kendini üst alanın yüksekliğine göre (camera.referenceHeight) ölçekler. HUD'daki HudView aynı kuralı uygular.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ScreenFit : MonoBehaviour
    {
        RectTransform _rect;
        Vector2 _parentSize;

        void Awake() => _rect = (RectTransform)transform;
        void OnEnable() => _parentSize = Vector2.zero;

        void LateUpdate()
        {
            if (!(transform.parent is RectTransform parent)) return;
            Vector2 size = parent.rect.size;
            if (size == _parentSize || size.y <= 0f) return;
            _parentSize = size;
            float scale = size.y / Services.Config.camera.referenceHeight;
            _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
            _rect.pivot = new Vector2(0.5f, 0.5f);
            _rect.anchoredPosition = Vector2.zero;
            _rect.localScale = Vector3.one * scale;
            _rect.sizeDelta = size / scale;
        }
    }
}
