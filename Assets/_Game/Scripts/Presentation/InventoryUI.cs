using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using LeafBrawl.Shared;
using LeafBrawl.Characters;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Tasarım görselindeki:
    /// 1) Alt orta HUD Yaprak Sayacı Çubuğunu (UI-04: K, S, Y, T adet/sınır) ve "Güç hazır [1]" kutusunu (UI-06),
    /// 2) Tab ile açılan "ENVANTER VE TARİFLER" penceresini (FR-40–FR-46: Solda 4x2 Yaprak & Şekil kutuları,
    ///    sağda parşömen Tarif Kartları ve yeşil ÜRET butonları) birebir kurar ve günceller.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("İsteğe Bağlı Yaprak İkonları (Boşsa otomatik çizilir)")]
        [SerializeField] private Sprite redLeafSprite;
        [SerializeField] private Sprite yellowLeafSprite;
        [SerializeField] private Sprite greenLeafSprite;
        [SerializeField] private Sprite orangeLeafSprite;
        [SerializeField] private Font customFont;

        private GameObject _inventoryWindow;
        private bool _isOpen;

        // 1. Alt Orta HUD Yaprak Sayacı (UI-04 & UI-06)
        private readonly Text[] _bottomCountTexts = new Text[4];
        private readonly Image[] _bottomSlotBgs = new Image[4];
        private Image _bottomProgressFill;
        private GameObject _readyBannerObj;
        private Text _readyBannerText;

        // 2. Tab "ENVANTER VE TARİFLER" Penceresi Sol Sütun (4 Yaprak x 2 Kutu)
        private readonly Text[] _leftIconCountTexts = new Text[4];
        private readonly Text[] _leftShapeCountTexts = new Text[4];
        private readonly Image[] _leftSlotImages = new Image[4];

        // 3. Sağ Sütun Tarif Kartları (Güç, Hız, Sağlık, Can İksiri)
        private readonly RecipeCardUI[] _recipeCards = new RecipeCardUI[4];
        private Text _safeStatusBanner;

        // Prosedürel Pixel-Art Sprite'lar
        private Sprite _woodBoxSprite;
        private Sprite _slotBoxSprite;
        private Sprite _parchmentActiveSprite;
        private Sprite _parchmentDimSprite;
        private Sprite _greenBtnSprite;
        private Sprite[] _leafSprites;

        private float _rejectFlashTimer;
        private CraftFail _lastRejectReason;

        private class RecipeCardUI
        {
            public RecipeId Recipe;
            public Image CardBg;
            public Text TitleText;
            public Text FormulaText;
            public Text EffectText;
            public Button CraftBtn;
            public Image CraftBtnBg;
            public Text CraftBtnLabel;
        }

        private static readonly LeafType[] LeafOrder = { LeafType.Red, LeafType.Yellow, LeafType.Green, LeafType.Orange };
        private static readonly string[] LeafCodes = { "K", "S", "Y", "T" };
        private static readonly string[] LeafShapeNames = { "Sivri", "İnce\nuzun", "Yuvarlak", "Yıldız\ndamarlı" };

        private void Awake()
        {
            GenerateProceduralSprites();
            BuildAllInventoryUI();
            SetWindowOpen(false);
        }

        private void OnEnable()
        {
            GameEvents.InventoryToggled += OnInventoryToggled;
            GameEvents.CraftRejected += OnCraftRejected;
        }

        private void OnDisable()
        {
            GameEvents.InventoryToggled -= OnInventoryToggled;
            GameEvents.CraftRejected -= OnCraftRejected;
        }

        private void Update()
        {
            if (_rejectFlashTimer > 0f)
            {
                _rejectFlashTimer -= Time.unscaledDeltaTime;
            }

            // Eğer sahnede Tab olayı dışarıdan tetiklenmediyse yedek Tab kontrolü
            if (WasTabPressedThisFrame())
            {
                SetWindowOpen(!_isOpen);
            }

            Character player = Services.Match != null ? Services.Match.Player : null;
            if (player == null)
            {
                player = FindAnyObjectByType<Character>();
                if (player == null) return;
            }

            UpdateBottomHudLeafBar(player);
            if (_isOpen)
            {
                UpdateTabInventoryWindow(player);
            }
        }

        private void OnInventoryToggled(bool open)
        {
            SetWindowOpen(open);
        }

        private void OnCraftRejected(Character c, RecipeId recipe, CraftFail reason)
        {
            Character player = Services.Match != null ? Services.Match.Player : null;
            if (player == null || c == player)
            {
                // FR-46: Eksik yaprak veya Güvende değilsin kırmızı yanıp söner
                _rejectFlashTimer = 0.7f;
                _lastRejectReason = reason;
            }
        }

        public void SetWindowOpen(bool open)
        {
            _isOpen = open;
            if (_inventoryWindow != null)
            {
                _inventoryWindow.SetActive(open);
            }
        }

        // =====================================================================
        // 1. ALT ORTA HUD YAPRAK SAYACI GÜNCELLEMESİ (K: 12/40 S: 8/40 Y: 3/40 T: 2/5)
        // =====================================================================
        private void UpdateBottomHudLeafBar(Character player)
        {
            if (player.Inventory == null) return;

            bool flashLeaves = _rejectFlashTimer > 0f && _lastRejectReason == CraftFail.NotEnoughLeaves;
            int totalCount = 0;
            int totalCap = 0;

            for (int i = 0; i < 4; i++)
            {
                LeafType lt = LeafOrder[i];
                int count = player.Inventory.Count(lt);
                int cap = Mathf.Max(1, player.Inventory.Cap(lt));
                totalCount += count;
                totalCap += cap;

                bool atCap = count >= cap;
                if (_bottomCountTexts[i] != null)
                {
                    _bottomCountTexts[i].text = $"{LeafCodes[i]}: {count}/{cap}";
                    // UI-04: Sınırdaki türün adedi farklı renkte
                    _bottomCountTexts[i].color = atCap
                        ? new Color(1f, 0.78f, 0.15f)
                        : new Color(0.98f, 0.94f, 0.86f);
                }

                if (_bottomSlotBgs[i] != null)
                {
                    _bottomSlotBgs[i].color = flashLeaves
                        ? new Color(0.95f, 0.20f, 0.20f, 1f)
                        : Color.white;
                }
            }

            if (_bottomProgressFill != null && totalCap > 0)
            {
                _bottomProgressFill.fillAmount = Mathf.Clamp01((float)totalCount / totalCap);
            }

            // UI-06: "Güç hazır [1]" Bildirim Kutusu
            if (player.Crafting != null && _readyBannerObj != null && _readyBannerText != null)
            {
                if (_rejectFlashTimer > 0f && _lastRejectReason == CraftFail.NotSafe)
                {
                    _readyBannerObj.SetActive(true);
                    _readyBannerText.text = "Güvende değilsin!";
                    _readyBannerText.color = new Color(1f, 0.30f, 0.30f, 1f);
                }
                else
                {
                    List<string> readyNames = new List<string>();
                    RecipeId[] ids = { RecipeId.Power, RecipeId.Speed, RecipeId.Health, RecipeId.Potion };
                    string[] labels = { "Güç hazır [1]", "Hız hazır [2]", "Sağlık hazır [3]", "İksir hazır [4]" };

                    for (int i = 0; i < 4; i++)
                    {
                        bool can = player.Crafting.CanCraft(ids[i], out CraftFail fail);
                        if (can || (fail == CraftFail.NotSafe && HasEnoughLeaves(player, ids[i])))
                        {
                            readyNames.Add(labels[i]);
                        }
                    }

                    bool hasAny = readyNames.Count > 0;
                    _readyBannerObj.SetActive(hasAny);
                    if (hasAny)
                    {
                        _readyBannerText.text = string.Join("  ·  ", readyNames);
                        float alpha = player.IsSafe ? 1f : 0.45f; // Güvende değilse soluk (UI-06)
                        _readyBannerText.color = new Color(1f, 0.95f, 0.75f, alpha);
                    }
                }
            }
        }

        // =====================================================================
        // 2. TAB "ENVANTER VE TARİFLER" PENCERESİ GÜNCELLEMESİ (Sağdaki Resim)
        // =====================================================================
        private void UpdateTabInventoryWindow(Character player)
        {
            if (player.Inventory == null) return;

            bool flashLeaves = _rejectFlashTimer > 0f && _lastRejectReason == CraftFail.NotEnoughLeaves;

            // Sol Sütun: 4 Yaprak Yuvası ve Şekil Kutuları (12/40 vb.)
            for (int i = 0; i < 4; i++)
            {
                LeafType lt = LeafOrder[i];
                int count = player.Inventory.Count(lt);
                int cap = Mathf.Max(1, player.Inventory.Cap(lt));
                bool atCap = count >= cap;

                Color numCol = atCap ? new Color(1f, 0.80f, 0.20f) : new Color(0.98f, 0.93f, 0.84f);

                if (_leftIconCountTexts[i] != null)
                {
                    _leftIconCountTexts[i].text = $"{count}/{cap}";
                    _leftIconCountTexts[i].color = numCol;
                }

                if (_leftShapeCountTexts[i] != null)
                {
                    _leftShapeCountTexts[i].text = $"{LeafShapeNames[i]}\n<b>{count}/{cap}</b>";
                    _leftShapeCountTexts[i].color = numCol;
                }

                if (_leftSlotImages[i] != null)
                {
                    _leftSlotImages[i].color = flashLeaves ? new Color(1f, 0.35f, 0.35f) : Color.white;
                }
            }

            // Üst Güvende Uyarısı (FR-42, FR-46)
            if (_safeStatusBanner != null)
            {
                if (!player.IsSafe)
                {
                    _safeStatusBanner.text = "⚠ GÜVENDE DEĞİLSİN (Alan dışı veya son 3 sn savaş/hasar)";
                    _safeStatusBanner.color = (_rejectFlashTimer > 0f) ? Color.red : new Color(1f, 0.50f, 0.35f);
                }
                else
                {
                    _safeStatusBanner.text = "✔ Güvendesin — Karta tıklayarak veya [1-4] ile üretebilirsin";
                    _safeStatusBanner.color = new Color(0.55f, 0.95f, 0.55f);
                }
            }

            // Sağ Sütun: 4 Tarif Kartı (Güç, Hız, Sağlık, Can İksiri)
            for (int i = 0; i < 4; i++)
            {
                RecipeCardUI card = _recipeCards[i];
                if (card == null) continue;

                CraftFail fail = CraftFail.NotEnoughLeaves;
                bool canCraft = player.Crafting != null && player.Crafting.CanCraft(card.Recipe, out fail);

                if (card.Recipe == RecipeId.Potion)
                {
                    int haveY = player.Inventory.Count(LeafType.Green);
                    int haveT = player.Inventory.Count(LeafType.Orange);
                    card.TitleText.text = "CAN İKSİRİ";
                    card.FormulaText.text = FormatReqPart("Y", haveY, 2) + " + " + FormatReqPart("T", haveT, 1);
                    card.EffectText.text = $"Mevcut Can +40 ({player.Health.Current}/{player.Health.Max})";
                    SetupCardButtonAndVisual(card, canCraft, false, fail, "ÜRET [4]");
                }
                else
                {
                    StatType st = RecipeToStat(card.Recipe);
                    int lvl = player.Stats != null ? player.Stats.Level(st) : 1;
                    bool isMax = lvl >= 5;

                    string statTitle = card.Recipe switch
                    {
                        RecipeId.Power  => "GÜÇ GELİŞTİRMESİ",
                        RecipeId.Speed  => "HIZ GELİŞTİRMESİ",
                        _               => "SAĞLIK GELİŞTİRMESİ"
                    };
                    card.TitleText.text = $"{statTitle} (Sv.{lvl})";

                    if (isMax)
                    {
                        card.FormulaText.text = "Maksimum Seviye (5/5)";
                        card.EffectText.text = "5. Seviye Yeteneği Açık!";
                        SetupCardButtonAndVisual(card, false, true, CraftFail.MaxLevel, "En üst seviye");
                    }
                    else
                    {
                        GetUpgradeCost(card.Recipe, lvl, out int reqK, out int reqS, out int reqY);
                        int haveK = player.Inventory.Count(LeafType.Red);
                        int haveS = player.Inventory.Count(LeafType.Yellow);
                        int haveY = player.Inventory.Count(LeafType.Green);

                        card.FormulaText.text = $"{FormatReqPart("K", haveK, reqK)} + {FormatReqPart("S", haveS, reqS)} + {FormatReqPart("Y", haveY, reqY)}";
                        card.EffectText.text = GetEffectDescription(card.Recipe, lvl);
                        SetupCardButtonAndVisual(card, canCraft, false, fail, $"ÜRET [{i + 1}]");
                    }
                }
            }
        }

        private void SetupCardButtonAndVisual(RecipeCardUI card, bool canCraft, bool isMax, CraftFail fail, string readyLabel)
        {
            if (isMax)
            {
                card.CardBg.sprite = _parchmentActiveSprite;
                card.CardBg.color = new Color(1f, 0.90f, 0.55f, 0.95f);
                card.CraftBtn.interactable = false;
                card.CraftBtnBg.color = new Color(0.75f, 0.58f, 0.18f, 1f);
                card.CraftBtnLabel.text = "En üst seviye"; // FR-42
            }
            else if (canCraft)
            {
                card.CardBg.sprite = _parchmentActiveSprite;
                card.CardBg.color = Color.white;
                card.CraftBtn.interactable = true;
                card.CraftBtnBg.color = Color.white;
                card.CraftBtnLabel.text = readyLabel;
            }
            else
            {
                // FR-42: Üretilemeyen kart soluk görünür ama gizlenmez
                card.CardBg.sprite = _parchmentDimSprite;
                card.CardBg.color = new Color(0.82f, 0.78f, 0.72f, 0.78f);
                card.CraftBtn.interactable = false;
                card.CraftBtnBg.color = new Color(0.45f, 0.35f, 0.28f, 0.85f);
                card.CraftBtnLabel.text = (fail == CraftFail.NotSafe) ? "Güvende değilsin" : "Eksik Yaprak";
            }
        }

        private static string FormatReqPart(string code, int have, int req)
        {
            int missing = Mathf.Max(0, req - have);
            if (missing > 0)
                return $"<color=#B81D13><b>{req}{code}</b> ({have}/{req})</color>";
            return $"<color=#1E6B18><b>{req}{code}</b> ({have}/{req})</color>";
        }

        private static string GetEffectDescription(RecipeId recipe, int currentLvl)
        {
            return recipe switch
            {
                RecipeId.Power  => $"Güç Seviyesi {currentLvl} → {currentLvl + 1} (Hasar +3)",
                RecipeId.Speed  => $"Hız Seviyesi {currentLvl} → {currentLvl + 1} (Hız Artışı)",
                RecipeId.Health => $"Sağlık Seviyesi {currentLvl} → {currentLvl + 1} (Maks Can +15)",
                _               => ""
            };
        }

        private static void GetUpgradeCost(RecipeId recipe, int currentLevel, out int k, out int s, out int y)
        {
            int pri = currentLevel switch { 1 => 3, 2 => 8, 3 => 15, 4 => 25, _ => 0 };
            int sec = currentLevel switch { 1 => 1, 2 => 4, 3 => 8, 4 => 15, _ => 0 };
            switch (recipe)
            {
                case RecipeId.Power:  k = pri; s = sec; y = sec; break;
                case RecipeId.Speed:  s = pri; k = sec; y = sec; break;
                case RecipeId.Health: y = pri; k = sec; s = sec; break;
                default:              k = 0;   s = 0;   y = 0;   break;
            }
        }

        private static bool HasEnoughLeaves(Character p, RecipeId r)
        {
            if (r == RecipeId.Potion)
                return p.Inventory.Count(LeafType.Green) >= 2 && p.Inventory.Count(LeafType.Orange) >= 1;
            StatType st = RecipeToStat(r);
            int lvl = p.Stats != null ? p.Stats.Level(st) : 1;
            if (lvl >= 5) return false;
            GetUpgradeCost(r, lvl, out int rk, out int rs, out int ry);
            return p.Inventory.Count(LeafType.Red) >= rk &&
                   p.Inventory.Count(LeafType.Yellow) >= rs &&
                   p.Inventory.Count(LeafType.Green) >= ry;
        }

        private static StatType RecipeToStat(RecipeId r) => r switch
        {
            RecipeId.Power  => StatType.Power,
            RecipeId.Speed  => StatType.Speed,
            _               => StatType.Health
        };

        private void OnClickCraft(RecipeId recipe)
        {
            Character player = Services.Match != null ? Services.Match.Player : FindAnyObjectByType<Character>();
            if (player != null && player.Crafting != null)
            {
                // FR-43: Envanter kartına tıklanınca yalnızca Crafting.Request çağrılır
                player.Crafting.Request(recipe);
            }
        }

        // =====================================================================
        // TASARIM GÖRSELİNDEKİ ARAYÜZÜN OTOMATİK İNŞASI (640x360 Canvas)
        // =====================================================================
        private void BuildAllInventoryUI()
        {
            Font font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject cObj = new GameObject("Inventory_HUD_Canvas");
                cObj.transform.SetParent(transform, false);
                canvas = cObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 60;

                CanvasScaler scaler = cObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(640f, 360f);
                scaler.matchWidthOrHeight = 0.5f;
                cObj.AddComponent<GraphicRaycaster>();
            }

            // -----------------------------------------------------------------
            // A) ALT ORTA HUD YAPRAK SAYACI BARI (Ortadaki Resmin Alt Kısmı - UI-04 & UI-06)
            // -----------------------------------------------------------------
            GameObject hudBar = CreateBox(canvas.transform, "HUD_BottomLeafBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(236f, 36f), _woodBoxSprite);

            // Üstteki "Güç hazır [1]" Bildirim Kutusu (UI-06)
            _readyBannerObj = CreateBox(hudBar.transform, "ReadyBanner", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 13f), new Vector2(160f, 20f), _slotBoxSprite);
            _readyBannerText = CreateText(_readyBannerObj.transform, "Txt", "Güç hazır [1]", 9, Vector2.zero, new Vector2(154f, 18f), font, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.75f));
            _readyBannerText.fontStyle = FontStyle.Bold;

            // Yeşil Doluluk Çubuğu (Resimdeki yaprak barının üst iç şeridi)
            GameObject barBg = CreateBox(hudBar.transform, "ProgressBg", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -9f), new Vector2(216f, 8f), null);
            barBg.GetComponent<Image>().color = new Color(0.16f, 0.09f, 0.05f, 1f);

            GameObject barFill = CreateBox(barBg.transform, "ProgressFill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, null);
            _bottomProgressFill = barFill.GetComponent<Image>();
            _bottomProgressFill.color = new Color(0.45f, 0.85f, 0.20f, 1f);
            _bottomProgressFill.type = Image.Type.Filled;
            _bottomProgressFill.fillMethod = Image.FillMethod.Horizontal;

            // Alt 4 Yaprak Sayısı (K: 12/40  S: 8/40  Y: 3/40  T: 2/5)
            for (int i = 0; i < 4; i++)
            {
                float x = -81f + i * 54f;
                GameObject item = CreateBox(hudBar.transform, $"HudLeaf_{LeafCodes[i]}", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(x, 11f), new Vector2(52f, 16f), _slotBoxSprite);
                _bottomSlotBgs[i] = item.GetComponent<Image>();

                GameObject icon = CreateBox(item.transform, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(11f, 11f), _leafSprites[i]);
                _bottomCountTexts[i] = CreateText(item.transform, "Count", $"{LeafCodes[i]}: 0/40", 8, new Vector2(6f, 0f), new Vector2(38f, 15f), font, TextAnchor.MiddleCenter, Color.white);
                _bottomCountTexts[i].fontStyle = FontStyle.Bold;
            }

            // -----------------------------------------------------------------
            // B) TAB "ENVANTER VE TARİFLER" PENCERESİ (Sağdaki Resmin Birebir Düzeni)
            // -----------------------------------------------------------------
            _inventoryWindow = CreateBox(canvas.transform, "Window_InventoryAndRecipes", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(440f, 290f), _woodBoxSprite);

            // Üst Orta Ahşap Başlık Şeridi: "ENVANTER VE TARİFLER"
            GameObject topRibbon = CreateBox(_inventoryWindow.transform, "TopRibbon", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(230f, 28f), _slotBoxSprite);
            Text winTitle = CreateText(topRibbon.transform, "Title", "ENVANTER VE TARİFLER", 12, Vector2.zero, new Vector2(220f, 24f), font, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.78f));
            winTitle.fontStyle = FontStyle.Bold;

            // Sağ Üst Yuvarlak [X] Kapat Düğmesi
            GameObject closeBtnObj = CreateBox(_inventoryWindow.transform, "Btn_CloseX", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -14f), new Vector2(24f, 24f), _slotBoxSprite);
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => SetWindowOpen(false));
            Text xTxt = CreateText(closeBtnObj.transform, "X", "X", 12, Vector2.zero, new Vector2(24f, 24f), font, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.65f));
            xTxt.fontStyle = FontStyle.Bold;

            // Güvende Durum Bandı
            _safeStatusBanner = CreateText(_inventoryWindow.transform, "SafeBanner", "", 9, new Vector2(0f, 114f), new Vector2(400f, 16f), font, TextAnchor.MiddleCenter, Color.white);
            _safeStatusBanner.fontStyle = FontStyle.Bold;

            // --- SOL SÜTUN: YAPRAKLAR (4 Satır x 2 Kutu: Sol Kutu = İkon + Sayı, Sağ Kutu = Şekil + Sayı) ---
            GameObject leftCol = CreateBox(_inventoryWindow.transform, "LeftColumn_Leaves", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, -12f), new Vector2(164f, 232f), _slotBoxSprite);
            Text leftHeader = CreateText(leftCol.transform, "Header", "Yapraklar", 11, new Vector2(0f, 98f), new Vector2(150f, 20f), font, TextAnchor.MiddleCenter, new Color(1f, 0.90f, 0.70f));
            leftHeader.fontStyle = FontStyle.Bold;

            for (int i = 0; i < 4; i++)
            {
                float rowY = 58f - i * 48f;

                // Sol Kutu: Yaprak İkonu + "12/40"
                GameObject iconBox = CreateBox(leftCol.transform, $"LeafIconBox_{i}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-38f, rowY), new Vector2(68f, 42f), _woodBoxSprite);
                _leftSlotImages[i] = iconBox.GetComponent<Image>();
                CreateBox(iconBox.transform, "LeafIcon", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(20f, 20f), _leafSprites[i]);
                _leftIconCountTexts[i] = CreateText(iconBox.transform, "Count", "0/40", 9, new Vector2(0f, -12f), new Vector2(64f, 14f), font, TextAnchor.MiddleCenter, Color.white);
                _leftIconCountTexts[i].fontStyle = FontStyle.Bold;

                // Sağ Kutu: Şekil Adı ("Sivri", "İnce uzun", "Yuvarlak", "Yıldız damarlı") + "12/40"
                GameObject shapeBox = CreateBox(leftCol.transform, $"LeafShapeBox_{i}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(38f, rowY), new Vector2(72f, 42f), _woodBoxSprite);
                _leftShapeCountTexts[i] = CreateText(shapeBox.transform, "ShapeTxt", $"{LeafShapeNames[i]}\n0/40", 8, Vector2.zero, new Vector2(68f, 38f), font, TextAnchor.MiddleCenter, Color.white);
            }

            // --- SAĞ SÜTUN: TARİF KARTLARI (Parşömen Kartlar + Yeşil ÜRET [1..4] Butonları) ---
            GameObject rightCol = CreateBox(_inventoryWindow.transform, "RightColumn_Recipes", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-132f, -12f), new Vector2(244f, 232f), _slotBoxSprite);

            RecipeId[] allRecipes = { RecipeId.Power, RecipeId.Speed, RecipeId.Health, RecipeId.Potion };
            for (int i = 0; i < 4; i++)
            {
                float cardY = 82f - i * 55f;
                GameObject cardObj = CreateBox(rightCol.transform, $"RecipeCard_{i}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, cardY), new Vector2(228f, 50f), _parchmentActiveSprite);

                // Kartın tamamına tıklanınca da üretir (FR-43)
                Button cardWholeBtn = cardObj.AddComponent<Button>();
                RecipeId captured = allRecipes[i];
                cardWholeBtn.onClick.AddListener(() => OnClickCraft(captured));

                Text titleTxt = CreateText(cardObj.transform, "Title", "GÜÇ GELİŞTİRMESİ", 9, new Vector2(-36f, 14f), new Vector2(142f, 14f), font, TextAnchor.MiddleLeft, new Color(0.28f, 0.14f, 0.06f));
                titleTxt.fontStyle = FontStyle.Bold;

                Text formulaTxt = CreateText(cardObj.transform, "Formula", "3K + 1S + 1Y", 8, new Vector2(-36f, 0f), new Vector2(142f, 14f), font, TextAnchor.MiddleLeft, new Color(0.22f, 0.11f, 0.05f));
                Text effectTxt  = CreateText(cardObj.transform, "Effect", "Güç Seviyesi +1", 8, new Vector2(-36f, -13f), new Vector2(142f, 14f), font, TextAnchor.MiddleLeft, new Color(0.35f, 0.20f, 0.10f));

                // Sağdaki Yeşil "ÜRET [1]" Butonu
                GameObject greenBtnObj = CreateBox(cardObj.transform, "Btn_Craft", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-42f, 0f), new Vector2(72f, 26f), _greenBtnSprite);
                Button craftBtn = greenBtnObj.AddComponent<Button>();
                craftBtn.onClick.AddListener(() => OnClickCraft(captured));

                Text btnLbl = CreateText(greenBtnObj.transform, "Label", $"ÜRET [{i + 1}]", 8, Vector2.zero, new Vector2(68f, 24f), font, TextAnchor.MiddleCenter, Color.white);
                btnLbl.fontStyle = FontStyle.Bold;

                _recipeCards[i] = new RecipeCardUI
                {
                    Recipe = captured,
                    CardBg = cardObj.GetComponent<Image>(),
                    TitleText = titleTxt,
                    FormulaText = formulaTxt,
                    EffectText = effectTxt,
                    CraftBtn = craftBtn,
                    CraftBtnBg = greenBtnObj.GetComponent<Image>(),
                    CraftBtnLabel = btnLbl
                };
            }
        }

        // =====================================================================
        // PROSEDÜREL PIXEL-ART SPRITE ÜRETİCİ (Dış resim atanmasa bile çalışır)
        // =====================================================================
        private void GenerateProceduralSprites()
        {
            _woodBoxSprite         = MakeBoxSprite(32, 32, new Color(0.14f, 0.07f, 0.03f), new Color(0.55f, 0.31f, 0.16f), new Color(0.38f, 0.20f, 0.10f));
            _slotBoxSprite         = MakeBoxSprite(32, 32, new Color(0.12f, 0.06f, 0.03f), new Color(0.38f, 0.21f, 0.11f), new Color(0.26f, 0.14f, 0.08f));
            _parchmentActiveSprite = MakeBoxSprite(48, 24, new Color(1.00f, 0.88f, 0.35f), new Color(0.92f, 0.72f, 0.22f), new Color(0.96f, 0.88f, 0.74f));
            _parchmentDimSprite    = MakeBoxSprite(48, 24, new Color(0.22f, 0.12f, 0.06f), new Color(0.55f, 0.46f, 0.36f), new Color(0.76f, 0.68f, 0.56f));
            _greenBtnSprite        = MakeBoxSprite(32, 16, new Color(0.10f, 0.28f, 0.08f), new Color(0.38f, 0.88f, 0.32f), new Color(0.22f, 0.66f, 0.18f));

            _leafSprites = new Sprite[4];
            _leafSprites[0] = redLeafSprite    != null ? redLeafSprite    : MakeLeafSprite(new Color(0.90f, 0.18f, 0.18f), 0);
            _leafSprites[1] = yellowLeafSprite != null ? yellowLeafSprite : MakeLeafSprite(new Color(1.00f, 0.84f, 0.13f), 1);
            _leafSprites[2] = greenLeafSprite  != null ? greenLeafSprite  : MakeLeafSprite(new Color(0.58f, 0.84f, 0.22f), 2);
            _leafSprites[3] = orangeLeafSprite != null ? orangeLeafSprite : MakeLeafSprite(new Color(1.00f, 0.48f, 0.10f), 3);
        }

        private static Sprite MakeBoxSprite(int w, int h, Color outCol, Color borderCol, Color fillCol)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if ((x == 0 || x == w - 1) && (y == 0 || y == h - 1)) tex.SetPixel(x, y, Color.clear);
                    else if (x <= 1 || x >= w - 2 || y <= 1 || y >= h - 2) tex.SetPixel(x, y, outCol);
                    else if (x == 2 || x == w - 3 || y == 2 || y == h - 3) tex.SetPixel(x, y, borderCol);
                    else tex.SetPixel(x, y, fillCol);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Sprite MakeLeafSprite(Color col, int type)
        {
            Texture2D tex = new Texture2D(12, 12, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            Color outline = new Color(0.14f, 0.07f, 0.03f);
            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    tex.SetPixel(x, y, Color.clear);
                    float dx = x - 5.5f, dy = y - 5.5f;
                    bool inside = type switch
                    {
                        1 => Mathf.Abs(dx - dy) < 2.3f && (dx * dx + dy * dy) < 24f,
                        2 => (dx * dx + dy * dy) <= 18f,
                        _ => (dx * dx + dy * dy) <= 16f || (Mathf.Abs(dx) < 2f && dy > 0f)
                    };
                    if (inside)
                    {
                        tex.SetPixel(x, y, (dx * dx + dy * dy > 11f) ? outline : col);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 12, 12), new Vector2(0.5f, 0.5f), 16f);
        }

        private static GameObject CreateBox(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.GetComponent<Image>();
            if (sprite != null) img.sprite = sprite;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content, int size, Vector2 pos, Vector2 boxSize, Font font, TextAnchor align, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = boxSize;
            Text txt = go.GetComponent<Text>();
            txt.font = font;
            txt.text = content;
            txt.fontSize = size;
            txt.alignment = align;
            txt.color = color;
            txt.supportRichText = true;
            return txt;
        }

        private static bool WasTabPressedThisFrame()
        {
            try
            {
                Type kbType = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
                if (kbType != null)
                {
                    object currentKb = kbType.GetProperty("current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                    if (currentKb != null)
                    {
                        object tabKey = kbType.GetProperty("tabKey", BindingFlags.Public | BindingFlags.Instance)?.GetValue(currentKb);
                        if (tabKey != null)
                        {
                            PropertyInfo pressedProp = tabKey.GetType().GetProperty("wasPressedThisFrame", BindingFlags.Public | BindingFlags.Instance);
                            if (pressedProp != null && (bool)pressedProp.GetValue(tabKey)) return true;
                        }
                    }
                }
            }
            catch { }

            try { return Input.GetKeyDown(KeyCode.Tab); }
            catch { return false; }
        }
    }
}