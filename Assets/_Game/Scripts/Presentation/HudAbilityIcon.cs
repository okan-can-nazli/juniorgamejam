using LeafBrawl.Characters;
using LeafBrawl.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// UI-05: balta veya 5. seviye yeteneğinin ikonu.
    /// Kilitli: gri + kilit ("Seviye 5"). Bekleme: dairesel dolum + kalan saniye. Kullanılmış: üstü çizili.
    /// Balta yetenek değildir: beklemesi LastSwingTime ve axe.cooldownSec'ten okunur.
    /// </summary>
    public class HudAbilityIcon : MonoBehaviour
    {
        [SerializeField] bool isAxe;
        [SerializeField] AbilityId ability;
        [SerializeField] Image icon;
        [SerializeField] Image cooldownFill;      // Image.Type = Filled, Radial 360
        [SerializeField] TMP_Text cooldownText;
        [SerializeField] GameObject lockedOverlay; // kilit + "Seviye 5"
        [SerializeField] GameObject usedStrike;    // üstü çizili
        [SerializeField] Color readyColor = Color.white;
        [SerializeField] Color lockedColor = new Color(0.45f, 0.45f, 0.45f);

        public void Refresh(Character player)
        {
            AbilityState state;
            float left, total;
            if (isAxe)
            {
                total = Services.Config.axe.cooldownSec;
                left = Mathf.Max(0f, total - (Time.time - player.LastSwingTime));
                state = left > 0f ? AbilityState.Cooldown : AbilityState.Ready;
            }
            else
            {
                var abilities = player.Abilities;
                state = abilities != null ? abilities.State(ability) : AbilityState.Locked;
                left = abilities != null ? abilities.CooldownLeft(ability) : 0f;
                total = Services.Config.abilities.dashCooldownSec;   // yalnızca atılmanın beklemesi var
            }

            if (icon != null) icon.color = state == AbilityState.Locked ? lockedColor : readyColor;
            if (lockedOverlay != null) lockedOverlay.SetActive(state == AbilityState.Locked);
            if (usedStrike != null) usedStrike.SetActive(state == AbilityState.Used);

            bool cooling = state == AbilityState.Cooldown && left > 0f;
            if (cooldownFill != null)
            {
                cooldownFill.enabled = cooling;
                cooldownFill.fillAmount = cooling && total > 0f ? left / total : 0f;
            }
            if (cooldownText != null)
            {
                cooldownText.enabled = cooling;
                if (cooling) cooldownText.text = left >= 1f ? Mathf.CeilToInt(left).ToString() : left.ToString("0.0");
            }
        }
    }
}
