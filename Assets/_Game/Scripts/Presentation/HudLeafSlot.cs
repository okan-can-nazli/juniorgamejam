using LeafBrawl.Characters;
using LeafBrawl.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// UI-04: bir yaprak türünün ikonu ve adedi. Sınırdaki türün adedi farklı renkte (capColor).
    /// FR-46: eksik yaprakta Flash ile kısa süre kırmızı çerçeve.
    /// </summary>
    public class HudLeafSlot : MonoBehaviour
    {
        [SerializeField] LeafType type;
        [SerializeField] Image icon;
        [SerializeField] Image flashFrame;
        [SerializeField] TMP_Text count;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color capColor = new Color(1f, 0.8f, 0.2f);

        float _flashUntil = float.NegativeInfinity;

        public LeafType Type => type;

        public void Refresh(Inventory inventory)
        {
            int n = inventory.Count(type);
            if (count != null)
            {
                count.text = n.ToString();
                count.color = n >= inventory.Cap(type) ? capColor : normalColor;
            }
            if (flashFrame != null) flashFrame.enabled = Time.unscaledTime < _flashUntil;
        }

        public void Flash(float seconds) => _flashUntil = Time.unscaledTime + seconds;
    }
}
