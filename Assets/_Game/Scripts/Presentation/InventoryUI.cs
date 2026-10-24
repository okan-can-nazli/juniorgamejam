using LeafBrawl.Characters;
using LeafBrawl.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Envanter paneli (FR-40–FR-43, FR-46). InventoryPanel.prefab kökünde, HUD.prefab'ın içinde (640×360 sanat birimi).
    /// Tab → GameEvents.InventoryToggled ile açılır/kapanır (Yiğit'in PlayerIntentSource'u yayınlar; panel açıkken saldırı yok).
    /// Sol: 4 yaprak yuvası "adet / sınır". Sağ: 4 tarif kartı (Güç, Hız, Sağlık, Can iksiri); her kart sonraki seviyeyi,
    /// gereken / sahip olunan yaprakları, eksikleri, "En üst seviye" ve "Güvende değilsin" durumunu gösterir.
    /// Karta tıklamak Player.Crafting.Request çağırır (Presentation'ın oyuna dokunduğu tek yer).
    /// Arayüz Awake'te, prefab'daki sprite'larla kurulur; yalnızca değer okur ve GameEvents dinler.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Envanter/ görselleri")]
        [SerializeField] Sprite frameSprite;          // spr_envanter_cerceve 260×180
        [SerializeField] Sprite leafBoxSprite;        // spr_envanter_yaprak_kutusu (9-slice)
        [SerializeField] Sprite cardActiveSprite;     // spr_tarif_karti_aktif (9-slice)
        [SerializeField] Sprite cardPassiveSprite;    // spr_tarif_karti_pasif (9-slice)
        [SerializeField] Sprite craftButtonSprite;    // spr_buton_uret_yesil
        [SerializeField] Sprite[] leafIcons;          // K, S, Y, T (spr_yaprak_ikon_*)

        [Header("Renkler")]
        [SerializeField] Color lightText = new Color(1f, 0.93f, 0.8f);
        [SerializeField] Color darkText = new Color(0.16f, 0.08f, 0.03f);
        [SerializeField] Color missingColor = new Color(0.85f, 0.15f, 0.12f);
        [SerializeField] Color capColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] float rejectFlashSec = 0.6f;

        static readonly LeafType[] Leaves = { LeafType.Red, LeafType.Yellow, LeafType.Green, LeafType.Orange };
        static readonly string[] LeafLetters = { "K", "S", "Y", "T" };
        static readonly RecipeId[] Recipes = { RecipeId.Power, RecipeId.Speed, RecipeId.Health, RecipeId.Potion };
        static readonly string[] RecipeNames = { "Güç", "Hız", "Sağlık", "Can iksiri" };

        class Card
        {
            public RecipeId Recipe;
            public Image Background;
            public TMP_Text Title, Cost, Status, ButtonLabel;
            public Image Button;
        }

        GameObject _window;
        readonly TMP_Text[] _leafCounts = new TMP_Text[4];
        readonly Card[] _cards = new Card[4];
        float _rejectUntil = float.NegativeInfinity;
        RecipeId _rejectRecipe;
        CraftFail _rejectFail;

        void Awake()
        {
            Build();
            _window.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.InventoryToggled += OnToggled;
            GameEvents.CraftRejected += OnRejected;
        }

        void OnDisable()
        {
            GameEvents.InventoryToggled -= OnToggled;
            GameEvents.CraftRejected -= OnRejected;
        }

        void OnToggled(bool open) => _window.SetActive(open);

        // FR-46: reddedilen tarifin eksik yaprakları ya da "Güvende değilsin" kısa süre kırmızı.
        void OnRejected(Character c, RecipeId recipe, CraftFail fail)
        {
            if (c == null || !c.IsPlayer) return;
            _rejectUntil = Time.unscaledTime + rejectFlashSec;
            _rejectRecipe = recipe;
            _rejectFail = fail;
        }

        void Update()
        {
            if (!_window.activeSelf) return;
            var player = Services.Match.Player;
            if (player == null) return;

            for (int i = 0; i < Leaves.Length; i++)
            {
                int count = player.Inventory.Count(Leaves[i]), cap = player.Inventory.Cap(Leaves[i]);
                _leafCounts[i].text = $"{count} / {cap}";
                _leafCounts[i].color = count >= cap ? capColor : lightText;
            }

            bool flashing = Time.unscaledTime < _rejectUntil;
            foreach (var card in _cards) UpdateCard(card, player, flashing && card.Recipe == _rejectRecipe);
        }

        void UpdateCard(Card card, Character player, bool rejected)
        {
            var crafting = player.Crafting;
            bool canCraft = crafting.CanCraft(card.Recipe, out var fail);
            bool maxLevel = fail == CraftFail.MaxLevel;
            int index = (int)card.Recipe;

            if (card.Recipe == RecipeId.Potion)
                card.Title.text = $"{RecipeNames[index]} (+{Services.Config.recipes.potionHeal} can)";
            else
            {
                int level = player.Stats.Level(Crafting.ToStat(card.Recipe));
                card.Title.text = maxLevel ? $"{RecipeNames[index]} {level}" : $"{RecipeNames[index]} {level} → {level + 1}";
            }

            // Gereken / sahip olunan; eksik olan kırmızı (FR-41). Reddedilince eksikler kalın (FR-46).
            var cost = crafting.Cost(card.Recipe);
            var sb = new System.Text.StringBuilder();
            if (!maxLevel)
                for (int i = 0; i < Leaves.Length; i++)
                {
                    int need = cost.Get(Leaves[i]);
                    if (need <= 0) continue;
                    int have = player.Inventory.Count(Leaves[i]);
                    bool missing = player.Inventory.Missing(cost, Leaves[i]) > 0;
                    if (sb.Length > 0) sb.Append("  ");
                    if (missing) sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(missingColor)}>{(rejected ? "<b>" : "")}");
                    sb.Append($"{LeafLetters[i]} {Mathf.Min(have, need)}/{need}");
                    if (missing) sb.Append(rejected ? "</b></color>" : "</color>");
                }
            card.Cost.text = sb.ToString();

            // FR-42: en üst seviye / güvende değil / üretilebilir.
            bool notSafe = !maxLevel && fail == CraftFail.NotSafe;
            card.Status.text = maxLevel ? "En üst seviye" : notSafe ? "Güvende değilsin" : crafting.IsCrafting && crafting.Current == card.Recipe ? "Üretiliyor" : "";
            card.Status.color = notSafe && rejected ? missingColor : darkText;

            card.Background.sprite = canCraft ? cardActiveSprite : cardPassiveSprite;
            card.Background.color = canCraft ? Color.white : new Color(1f, 1f, 1f, 0.8f);
            // Uyarı varken buton yerine uyarı yazısı; yoksa buton (üretilemiyorsa soluk).
            bool showButton = card.Status.text.Length == 0;
            card.Button.enabled = card.ButtonLabel.enabled = showButton;
            card.Button.color = canCraft ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        }

        // FR-43: karta tıklayınca üretim başlar (başlamazsa Crafting CraftRejected yayınlar).
        void OnCardClicked(RecipeId recipe)
        {
            var player = Services.Match.Player;
            if (player == null || !player.IsAlive) return;
            AudioManager.PlayClick();
            player.Crafting.Request(recipe);
        }

        // ---------------- Kurulum (640×360 sanat birimi) ----------------

        float _sliceScale = 1f;

        void Build()
        {
            // 9-slice kenarları sanat pikseliyle çizilsin: Canvas'ın referans PPU'su / sprite PPU'su (ör. 100 / 16).
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null && frameSprite != null) _sliceScale = canvas.referencePixelsPerUnit / frameSprite.pixelsPerUnit;

            _window = NewImage(transform, "Pencere", frameSprite, Vector2.zero, new Vector2(260, 180)).gameObject;
            NewText(_window.transform, "Baslik", "Envanter ve Tarifler", new Vector2(0, 80), new Vector2(150, 12), 7, lightText, TextAlignmentOptions.Center);
            NewText(_window.transform, "Ipucu", "Kapatmak için Tab", new Vector2(0, -95), new Vector2(160, 10), 6, lightText, TextAlignmentOptions.Center);

            // Sol sütun: yaprak yuvaları
            for (int i = 0; i < Leaves.Length; i++)
            {
                var box = NewImage(_window.transform, "Yaprak_" + LeafLetters[i], leafBoxSprite, new Vector2(-69, 51 - i * 38), new Vector2(92, 32), Image.Type.Sliced);
                box.pixelsPerUnitMultiplier = _sliceScale;
                NewImage(box.transform, "Ikon", leafIcons != null && i < leafIcons.Length ? leafIcons[i] : null, new Vector2(-30, 0), new Vector2(16, 16)).preserveAspect = true;
                _leafCounts[i] = NewText(box.transform, "Adet", "0 / 0", new Vector2(10, 0), new Vector2(60, 14), 8, lightText, TextAlignmentOptions.Center);
            }

            // Sağ sütun: tarif kartları
            for (int i = 0; i < Recipes.Length; i++)
            {
                var recipe = Recipes[i];
                var bg = NewImage(_window.transform, "Kart_" + recipe, cardActiveSprite, new Vector2(57, 51 - i * 38), new Vector2(124, 35), Image.Type.Sliced);
                bg.pixelsPerUnitMultiplier = _sliceScale;
                var button = bg.gameObject.AddComponent<Button>();
                button.targetGraphic = bg;
                button.onClick.AddListener(() => OnCardClicked(recipe));
                var card = new Card { Recipe = recipe, Background = bg };
                card.Title = NewText(bg.transform, "Baslik", "", new Vector2(-19, 5), new Vector2(74, 9), 6.5f, darkText, TextAlignmentOptions.Left);
                card.Cost = NewText(bg.transform, "Maliyet", "", new Vector2(-19, -5), new Vector2(74, 9), 6f, darkText, TextAlignmentOptions.Left);
                card.Button = NewImage(bg.transform, "Uret", craftButtonSprite, new Vector2(37, 0), new Vector2(40, 14));
                card.Button.raycastTarget = false;   // tıklama kartın kendisinde
                card.ButtonLabel = NewText(card.Button.transform, "Yazi", $"Üret [{i + 1}]", Vector2.zero, new Vector2(40, 12), 5.5f, Color.white, TextAlignmentOptions.Center);
                card.Status = NewText(bg.transform, "Durum", "", new Vector2(37, 0), new Vector2(42, 22), 5.5f, darkText, TextAlignmentOptions.Center);
                card.Status.textWrappingMode = TextWrappingModes.Normal;
                _cards[i] = card;
            }
        }

        static Image NewImage(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size, Image.Type type = Image.Type.Simple)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null && sprite.border != Vector4.zero ? type : Image.Type.Simple;
            image.color = sprite != null ? Color.white : new Color(0.3f, 0.2f, 0.1f, 0.9f);
            return image;
        }

        static TMP_Text NewText(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, Color color, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
