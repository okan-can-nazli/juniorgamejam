using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>UI-07: tarif üretilirken oyuncunun üstünde dolan çubuk (Crafting.Progress01). Ekran uzayında karakteri izler.</summary>
    public class HudCraftBar : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image fill;   // Image.Type = Filled, Horizontal

        RectTransform _rect;
        RectTransform _canvasRect;

        void Awake()
        {
            _rect = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null) _canvasRect = (RectTransform)canvas.rootCanvas.transform;
        }

        public void Refresh(Character player)
        {
            bool show = player.Crafting.IsCrafting;
            if (root != null) root.SetActive(show);
            if (!show) return;
            if (fill != null) fill.fillAmount = player.Crafting.Progress01;

            var camera = Camera.main;
            if (camera == null || _canvasRect == null) return;
            // Karakterin bir çapı kadar yukarısı.
            Vector3 world = player.Position + Vector2.up * Units.ToUnits(Services.Config.character.radiusPx * 2f);
            Vector2 screen = camera.WorldToScreenPoint(world);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_rect.parent, screen, null, out Vector2 local))
                _rect.anchoredPosition = local;
        }
    }
}
