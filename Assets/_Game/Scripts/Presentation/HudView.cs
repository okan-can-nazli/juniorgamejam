using System.Collections.Generic;
using System.Text;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Maç HUD'u (SRS 6.2 UI-01–UI-07, FR-05, FR-45, FR-46, FR-71, 3.4). HUD.prefab kökünde.
    /// Yalnızca GameEvents dinler ve sözleşmedeki değerleri okur; oyun durumuna yazmaz. Yalnızca oyuncunun olaylarını gösterir.
    /// Yerleşim 640×360 sanat birimindedir: kök, üst Canvas ne olursa olsun kendini 360 yüksekliğe göre ölçekler.
    /// Görseller yer tutucudur (düz renkli Image); font ve ikonlar Furkan'ın.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        [Header("UI-01 sol üst")]
        [SerializeField] Image hpFill;            // Filled, Horizontal
        [SerializeField] TMP_Text hpText;         // "85 / 115"
        [SerializeField] TMP_Text levelsText;     // "Güç 2 · Hız 1 · Sağlık 2"

        [Header("UI-02 üst orta")]
        [SerializeField] TMP_Text timeText;       // "2:35"
        [SerializeField] TMP_Text zoneText;       // "Alan daralıyor: 0:12"

        [Header("UI-03 sağ üst")]
        [SerializeField] TMP_Text aliveText;      // "Kalan: 3"
        [SerializeField] TMP_Text killsText;      // "Eleme: 1"

        [Header("UI-04 alt orta")]
        [SerializeField] HudLeafSlot[] leafSlots; // K, S, Y, T

        [Header("UI-05 sağ alt")]
        [SerializeField] HudAbilityIcon[] abilityIcons;   // balta, atılma, kalkan, ulti

        [Header("UI-06 yaprak çubuğunun üstü")]
        [SerializeField] TMP_Text readyText;      // "Güç hazır [1]   Hız hazır [2]"
        [SerializeField] RectTransform readyBox;  // spr_hud_tarif_hazir_kutu (9-slice); yazı varken görünür, yazıya göre genişler
        [SerializeField] TMP_Text notSafeText;    // "Güvende değilsin" (FR-46)
        [SerializeField] Color readyColor = Color.white;
        [SerializeField] Color readyUnsafeColor = new Color(1f, 1f, 1f, 0.35f);

        [Header("UI-07 karakterin üstü")]
        [SerializeField] HudCraftBar craftBar;

        [Header("Geri sayım, bildirim, alan dışı")]
        [SerializeField] TMP_Text countdownText;  // 3, 2, 1
        [SerializeField] TMP_Text notifyText;     // "Güç 2 → 3", "Can +40", "Ulti açıldı [E]"
        [SerializeField] GameObject outsideZoneEdges;   // FR-71 kırmızı ekran kenarları

        RectTransform _rect;
        Vector2 _parentSize;
        float _notifyUntil = float.NegativeInfinity, _notSafeUntil = float.NegativeInfinity;
        readonly List<string> _notifications = new List<string>();
        readonly StringBuilder _sb = new StringBuilder();
        bool _frozen;

        static readonly RecipeId[] Recipes = { RecipeId.Power, RecipeId.Speed, RecipeId.Health, RecipeId.Potion };

        void Awake() => _rect = (RectTransform)transform;

        void OnEnable()
        {
            GameEvents.CountdownTick += OnCountdown;
            GameEvents.MatchStateChanged += OnMatchState;
            GameEvents.CraftCompleted += OnCraftCompleted;
            GameEvents.CraftRejected += OnCraftRejected;
            GameEvents.AbilityUnlocked += OnAbilityUnlocked;
        }

        void OnDisable()
        {
            GameEvents.CountdownTick -= OnCountdown;
            GameEvents.MatchStateChanged -= OnMatchState;
            GameEvents.CraftCompleted -= OnCraftCompleted;
            GameEvents.CraftRejected -= OnCraftRejected;
            GameEvents.AbilityUnlocked -= OnAbilityUnlocked;
        }

        void Start()
        {
            if (countdownText != null) countdownText.enabled = Services.Match.State == MatchState.Countdown;
            if (notifyText != null) notifyText.enabled = false;
            if (notSafeText != null) notSafeText.enabled = false;
        }

        void LateUpdate()
        {
            FitToReference();
            if (_frozen) return;   // oyuncu ölünce veya maç bitince HUD donar

            var player = Services.Match.Player;
            if (player == null) return;

            RefreshVitals(player);
            RefreshClock();
            RefreshAlive(player);
            foreach (var slot in leafSlots) if (slot != null) slot.Refresh(player.Inventory);
            foreach (var icon in abilityIcons) if (icon != null) icon.Refresh(player);
            RefreshReady(player);
            if (craftBar != null) craftBar.Refresh(player);
            if (outsideZoneEdges != null) outsideZoneEdges.SetActive(player.IsAlive && !Services.Zone.IsInside(player.Position));
            if (notifyText != null) notifyText.enabled = Time.unscaledTime < _notifyUntil;
            if (notSafeText != null) notSafeText.enabled = Time.unscaledTime < _notSafeUntil;
            if (!player.IsAlive) _frozen = true;
        }

        // ---------------------------------------------------------------- UI-01 – UI-03, UI-06

        void RefreshVitals(Character player)
        {
            var health = player.Health;
            if (hpFill != null) hpFill.fillAmount = health.Max > 0 ? (float)health.Current / health.Max : 0f;
            if (hpText != null) hpText.text = $"{health.Current} / {health.Max}";
            if (levelsText != null)
                levelsText.text = $"Güç {player.Stats.Level(StatType.Power)} · Hız {player.Stats.Level(StatType.Speed)} · Sağlık {player.Stats.Level(StatType.Health)}";
        }

        void RefreshClock()
        {
            if (timeText != null) timeText.text = Clock(Mathf.FloorToInt(Services.Match.MatchTime));
            if (zoneText == null) return;
            float left = Services.Zone.SecondsToNextShrink;
            bool show = !float.IsInfinity(left) && left > 0f;
            zoneText.enabled = show;
            if (show) zoneText.text = "Alan daralıyor: " + Clock(Mathf.CeilToInt(left));
        }

        void RefreshAlive(Character player)
        {
            int alive = 0;
            foreach (var c in Services.Match.Characters) if (c != null && c.IsAlive) alive++;
            if (aliveText != null) aliveText.text = $"Kalan: {alive}";
            if (killsText != null) killsText.text = $"Eleme: {player.Kills}";
        }

        void RefreshReady(Character player)
        {
            if (readyText == null) return;
            _sb.Clear();
            foreach (var recipe in Recipes)
            {
                // Hazır = yaprak yetiyor ve en üst seviyede değil; güvende olmamak yalnızca soluklaştırır (UI-06).
                if (player.Crafting.CanCraft(recipe, out var fail) || fail == CraftFail.NotSafe)
                {
                    if (_sb.Length > 0) _sb.Append("   ");
                    _sb.Append(RecipeName(recipe)).Append(" hazır [").Append((int)recipe + 1).Append(']');
                }
            }
            readyText.text = _sb.ToString();
            readyText.color = player.IsSafe ? readyColor : readyUnsafeColor;
            if (readyBox != null)
            {
                bool any = _sb.Length > 0;
                readyBox.gameObject.SetActive(any);
                if (any) readyBox.sizeDelta = new Vector2(readyText.preferredWidth + readyText.fontSize * 1.5f, readyBox.sizeDelta.y);
            }
        }

        // ---------------------------------------------------------------- olaylar

        void OnCountdown(int secondsLeft)
        {
            if (countdownText == null) return;
            countdownText.enabled = secondsLeft > 0;
            countdownText.text = secondsLeft.ToString();
        }

        void OnMatchState(MatchState state)
        {
            if (countdownText != null && state != MatchState.Countdown) countdownText.enabled = false;
            if (state == MatchState.Ended) _frozen = true;
        }

        // FR-45: "Güç 2 → 3" veya "Can +40", crafting.notifySec boyunca.
        void OnCraftCompleted(Character c, RecipeId recipe, int newLevel)
        {
            if (!c.IsPlayer) return;
            Notify(recipe == RecipeId.Potion
                ? $"Can +{Services.Config.recipes.potionHeal}"
                : $"{RecipeName(recipe)} {newLevel - 1} → {newLevel}");
        }

        // FR-46: eksik yaprak ikonları ya da "Güvende değilsin" kısa süre kırmızı.
        void OnCraftRejected(Character c, RecipeId recipe, CraftFail fail)
        {
            if (!c.IsPlayer) return;
            float seconds = Services.Config.crafting.notifySec;
            if (fail == CraftFail.NotSafe)
            {
                _notSafeUntil = Time.unscaledTime + seconds;
                return;
            }
            if (fail != CraftFail.NotEnoughLeaves) return;
            var cost = c.Crafting.Cost(recipe);
            foreach (var slot in leafSlots)
                if (slot != null && c.Inventory.Missing(cost, slot.Type) > 0) slot.Flash(seconds);
        }

        // SRS 3.4: "Ulti açıldı [E]"
        void OnAbilityUnlocked(Character c, AbilityId ability)
        {
            if (!c.IsPlayer) return;
            Notify(ability switch
            {
                AbilityId.Dash => "Atılma açıldı [Boşluk]",
                AbilityId.Shield => "Kalkan açıldı [Q]",
                _ => "Ulti açıldı [E]",
            });
        }

        void Notify(string message)
        {
            if (notifyText == null) return;
            // Aynı anda gelen bildirimler alt alta (ör. F3 ile üç yetenek birden açılınca).
            if (Time.unscaledTime >= _notifyUntil) _notifications.Clear();
            _notifications.Add(message);
            notifyText.text = string.Join("\n", _notifications);
            _notifyUntil = Time.unscaledTime + Services.Config.crafting.notifySec;
        }

        // ---------------------------------------------------------------- yardımcılar

        // Yerleşim 640×360 sanat biriminde: kök, üst Canvas'ın yüksekliğine göre ölçeklenir (Canvas Scaler referansı ne olursa olsun).
        void FitToReference()
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

        static string Clock(int seconds)
        {
            int s = Mathf.Max(0, seconds);
            return $"{s / 60}:{s % 60:00}";
        }

        static string RecipeName(RecipeId recipe) => recipe switch
        {
            RecipeId.Power => "Güç",
            RecipeId.Speed => "Hız",
            RecipeId.Health => "Sağlık",
            _ => "İksir",
        };
    }
}
