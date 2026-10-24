using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LeafBrawl.Core;
using LeafBrawl.Shared;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Tasarım görselindeki (Sol Üst Ana Ekran) düzeni birebir kurar:
    /// - Ortada LEAF / BRAWL logosu ve yaprak süslü ahşap butonlar (BAŞLA, KILAVUZ, KONTROLLER, ÇIKIŞ)
    /// - Sol altta görsel [W][A][S][D] tuş dizilimli "Kontroller" ahşap kutusu
    /// - Sağda 1-2-3 numaralı ve yaprak ikonlu "Nasıl Oynanır?" parşömen panosu
    /// - Ayrıca Kılavuz (6 sayfa ileri/geri) ve Kontroller tam ekran pencereleri (Esc ile kapanır).
    /// SRS: FR-01, FR-02, FR-03, 6.4, NFR-03
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("İsteğe Bağlı Özel Görseller (Boşsa kod otomatik çizer)")]
        [SerializeField] private Sprite customBackgroundSprite;
        [SerializeField] private Sprite customLogoSprite;
        [SerializeField] private Sprite[] guidePageSprites = new Sprite[6];
        [SerializeField] private Font customFont;

        private GameObject _controlsModal;
        private GameObject _guideModal;

        // 6 Sayfalık Kılavuz (FR-03) Bileşenleri
        private int _currentGuidePage;
        private Text _guideTitleText;
        private Text _guideBodyText;
        private Text _guidePageIndicatorText;
        private Image _guideIllustrationImage;
        private Button _prevPageButton;
        private Button _nextPageButton;

        // Prosedürel üretilen Pixel-Art Sprite'lar
        private Sprite _procBgSprite;
        private Sprite _procWoodBtnSprite;
        private Sprite _procParchmentSprite;
        private Sprite _procDarkBoxSprite;
        private Sprite _procLeafRed;
        private Sprite _procLeafYellow;
        private Sprite _procLeafGreen;
        private Sprite _procLeafOrange;
        private Sprite[] _procGuideSprites;

        private struct GuidePageData
        {
            public string Title;
            public string Body;
        }

        // SRS v1.4 Bölüm 3: 6 sayfa, her sayfada en fazla 3 cümle (10 sn alan uyarısı yok)
        private static readonly GuidePageData[] GuidePages = new GuidePageData[]
        {
            new GuidePageData
            {
                Title = "1. Yaprak Toplama ve Ağaç Kesme",
                Body = "Ağaçlardan belirli aralıklarla Kırmızı (K), Sarı (S), Yeşil (Y) ve Turuncu (T) yapraklar düşer; yaklaşınca otomatik toplanır.\n" +
                       "Merkezdeki ulu ağaç kesilemez ancak en sık ve en yüksek turuncu olasılığıyla yaprak üretir.\n" +
                       "Küme ve koru ağaçlarını baltayla keserek 6–10 adet ekstra yaprak elde edebilirsin; yerinde engel olmayan kütük kalır."
            },
            new GuidePageData
            {
                Title = "2. Tarifler, Seviyeler ve \"Güvende\" Kuralı",
                Body = "Topladığın yapraklarla 1, 2, 3 tuşlarına basarak Güç, Hız ve Sağlık geliştirmelerini 5. seviyeye kadar yükseltebilirsin.\n" +
                       "Tarif üretimi yalnızca karakter \"güvendeyken\" (güvenli alanın içindeyken ve son 3 sn hasar almamış, balta savurmamışken) başlatılabilir.\n" +
                       "Üretim 1,5 sn sürer; bu sürede hareket edebilirsin ancak hasar alırsan üretim iptal olur ve yaprakların harcanmaz."
            },
            new GuidePageData
            {
                Title = "3. Can Yenilenmesi ve Can İksiri",
                Body = "Karakter 5 sn boyunca hasar almazsa canı, mevcut Sağlık seviyesine göre kendiliğinden yavaşça yenilenmeye başlar.\n" +
                       "Savaşta hızlı iyileşmek için 2 Yeşil (Y) ve 1 Turuncu (T) yaprak harcayarak 4 tuşuyla Can İksiri üretebilirsin.\n" +
                       "Can iksiri üretildiği anda içilir, mevcut canı 40 artırır ve tehdit puanını yükseltmez."
            },
            new GuidePageData
            {
                Title = "4. 5. Seviye Yetenekleri",
                Body = "Maç başında özel yetenekler kapalıdır; bir geliştirme 5. seviyeye ulaştığında ona özel yetenek açılır.\n" +
                       "Hız 5'te Boşluk tuşuyla tekrar kullanılabilir Atılma (Dash); Sağlık 5'te Q tuşuyla maçta 1 kez ilk vuruşu emen Kalkan açılır.\n" +
                       "Güç 5'te E tuşuyla maçta 1 kez kullanılan Ulti açılır; kısa hazırlıktan sonra çevrendeki rakipleri eler ve ağaçları keser."
            },
            new GuidePageData
            {
                Title = "5. Çalılıklar ve Yaprak Yığını",
                Body = "Haritadaki çalılıkların içine giren karakter ve yaprak yığını, aynı çalılıkta olmayan bütün karakterlere görünmez olur.\n" +
                       "Çalılıktaki karakter balta savurduğunda veya hasar aldığında 1 sn boyunca herkese görünür hale gelir.\n" +
                       "Geliştirme seviyelerin arttıkça tehdit puanın yükselir ve karakterinin etrafındaki yaprak yığını büyür."
            },
            new GuidePageData
            {
                Title = "6. Daralan Alan ve Elenme",
                Body = "Güvenli alan harita merkezinde sabit bir dairedir ve maç ilerledikçe merkeze doğru daralır.\n" +
                       "Alan dışındaki karakterler her 0,5 sn'de bir hasar alır ve gövdesi alan dışında kalan ağaçlar yaprak üretmeyi bırakır.\n" +
                       "Yeniden doğma yoktur; ölen karakter ganimet yığınını bırakarak elenir ve hayatta kalan son karakter maçı kazanır."
            }
        };

        private void Awake()
        {
            EnsureCameraAndEventSystem();
            GenerateProceduralMenuSprites();
            BuildMockupMenuScreen();
        }

        private void Update()
        {
            // SRS 6.4: Kontroller ve Kılavuz ekranlarında Esc ana menüye döner
            if (WasEscapePressedThisFrame())
            {
                if ((_controlsModal != null && _controlsModal.activeSelf) ||
                    (_guideModal != null && _guideModal.activeSelf))
                {
                    CloseModals();
                }
            }
        }

        private void OnStartClicked() => SceneFlow.StartMatch();

        private void OnOpenGuideClicked()
        {
            if (_controlsModal != null) _controlsModal.SetActive(false);
            if (_guideModal != null) _guideModal.SetActive(true);
            SetGuidePage(0);
        }

        private void OnOpenControlsClicked()
        {
            if (_guideModal != null) _guideModal.SetActive(false);
            if (_controlsModal != null) _controlsModal.SetActive(true);
        }

        private void OnQuitClicked()
        {
            SceneFlow.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void CloseModals()
        {
            if (_controlsModal != null) _controlsModal.SetActive(false);
            if (_guideModal != null) _guideModal.SetActive(false);
        }

        private void ChangeGuidePage(int delta)
        {
            SetGuidePage(Mathf.Clamp(_currentGuidePage + delta, 0, GuidePages.Length - 1));
        }

        private void SetGuidePage(int index)
        {
            _currentGuidePage = Mathf.Clamp(index, 0, GuidePages.Length - 1);
            GuidePageData page = GuidePages[_currentGuidePage];

            if (_guideTitleText != null) _guideTitleText.text = page.Title;
            if (_guideBodyText != null) _guideBodyText.text = page.Body;
            if (_guidePageIndicatorText != null) _guidePageIndicatorText.text = $"Sayfa {_currentGuidePage + 1} / {GuidePages.Length}";

            if (_guideIllustrationImage != null)
            {
                Sprite s = (guidePageSprites != null && _currentGuidePage < guidePageSprites.Length && guidePageSprites[_currentGuidePage] != null)
                    ? guidePageSprites[_currentGuidePage]
                    : _procGuideSprites[_currentGuidePage];
                _guideIllustrationImage.sprite = s;
            }

            if (_prevPageButton != null) _prevPageButton.interactable = _currentGuidePage > 0;
            if (_nextPageButton != null) _nextPageButton.interactable = _currentGuidePage < GuidePages.Length - 1;
        }

        // =====================================================================
        // SOL ÜSTTEKİ TASARIM GÖRSELİNİN BİREBİR İNŞASI (640x360 Canvas)
        // =====================================================================
        private void BuildMockupMenuScreen()
        {
            Font font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObj = new GameObject("MainMenu_Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(640f, 360f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 1. ARKA PLAN (Altın/Krem Sonbahar Gökyüzü + Köşelerden Sarkan Sonbahar Yaprakları)
            GameObject bgObj = CreatePanel(canvasObj.transform, "AutumnBackground", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            bgObj.GetComponent<Image>().sprite = customBackgroundSprite != null ? customBackgroundSprite : _procBgSprite;

            // 2. ORTA BÖLÜM: DEV "LEAF / BRAWL" LOGOSU VE AHŞAP BUTONLAR
            if (customLogoSprite != null)
            {
                GameObject logoImg = CreatePanel(canvasObj.transform, "Logo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-10f, 92f), new Vector2(220f, 95f), Color.white);
                logoImg.GetComponent<Image>().sprite = customLogoSprite;
            }
            else
            {
                // Logo Arka Yaprak Süsleri
                CreateIcon(canvasObj.transform, "LogoLeafL", _procLeafOrange, new Vector2(-88f, 108f), new Vector2(28f, 28f));
                CreateIcon(canvasObj.transform, "LogoLeafR", _procLeafRed,    new Vector2(68f, 108f),  new Vector2(28f, 28f));
                CreateIcon(canvasObj.transform, "LogoLeafB", _procLeafGreen,  new Vector2(-92f, 75f),  new Vector2(22f, 22f));

                // Üst Satır: "LEAF" (Altın Sarısı, Koyu Kahve Kalın Konturlu)
                Text leafTitle = CreateStyledText(canvasObj.transform, "Title_LEAF", "LEAF", 44, new Vector2(-10f, 114f), new Vector2(260f, 56f), font, new Color(1f, 0.84f, 0.28f), new Color(0.28f, 0.11f, 0.05f));
                leafTitle.fontStyle = FontStyle.Bold;

                // Alt Satır: "BRAWL" (Kızıl-Turuncu, Koyu Kahve Kalın Konturlu)
                Text brawlTitle = CreateStyledText(canvasObj.transform, "Title_BRAWL", "BRAWL", 40, new Vector2(-10f, 72f), new Vector2(280f, 52f), font, new Color(0.93f, 0.32f, 0.16f), new Color(0.28f, 0.11f, 0.05f));
                brawlTitle.fontStyle = FontStyle.Bold;
            }

            // Ortadaki Ahşap Butonlar (Tasarım görselindeki gibi yanları yapraklı ahşap tabelalar)
            CreateWoodenButton(canvasObj.transform, "Btn_Start",    "BAŞLA",      new Vector2(-10f, 12f),  new Vector2(156f, 32f), font, true,  OnStartClicked);
            CreateWoodenButton(canvasObj.transform, "Btn_Guide",    "KILAVUZ",    new Vector2(-10f, -26f), new Vector2(156f, 32f), font, false, OnOpenGuideClicked);
            CreateWoodenButton(canvasObj.transform, "Btn_Controls", "KONTROLLER", new Vector2(-10f, -64f), new Vector2(156f, 32f), font, false, OnOpenControlsClicked);
            CreateWoodenButton(canvasObj.transform, "Btn_Quit",     "ÇIKIŞ",      new Vector2(-10f, -102f),new Vector2(156f, 32f), font, false, OnQuitClicked);

            // 3. SOL ALT KART: "Kontroller" ve Görsel [W][A][S][D] Tuş Kutusu (Resimdeki Sol Alt Kutu)
            BuildLeftControlsPreviewCard(canvasObj.transform, font);

            // 4. SAĞ KART: "Nasıl Oynanır?" Parşömen Panosu (Resimdeki Sağ Kutu: 1, 2, 3 ve Yapraklar)
            BuildRightHowToPlayCard(canvasObj.transform, font);

            // 5. AÇILIR TAM EKRAN MODALLAR (Kontroller & 6 Sayfalık Kılavuz - FR-02, FR-03, KT-18)
            BuildFullControlsModal(canvasObj.transform, font);
            BuildFullGuideModal(canvasObj.transform, font);
        }

        private void BuildLeftControlsPreviewCard(Transform parent, Font font)
        {
            GameObject box = CreatePanel(parent, "LeftControlsCard", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(88f, 102f), new Vector2(148f, 168f), Color.white);
            box.GetComponent<Image>().sprite = _procDarkBoxSprite;

            Text header = CreateStyledText(box.transform, "Header", "Kontroller", 13, new Vector2(0f, 66f), new Vector2(130f, 22f), font, new Color(1f, 0.92f, 0.75f), new Color(0.15f, 0.07f, 0.03f));
            header.fontStyle = FontStyle.Bold;

            // Görsel [W] / [A][S][D] Tuş Dizilimi
            CreateKeycap(box.transform, "W", new Vector2(-18f, 38f), new Vector2(24f, 22f), font);
            CreateKeycap(box.transform, "A", new Vector2(-44f, 13f), new Vector2(24f, 22f), font);
            CreateKeycap(box.transform, "S", new Vector2(-18f, 13f), new Vector2(24f, 22f), font);
            CreateKeycap(box.transform, "D", new Vector2(8f,   13f), new Vector2(24f, 22f), font);

            // [Sol Tık] balta
            CreateKeycap(box.transform, "Sol Tık", new Vector2(-28f, -18f), new Vector2(54f, 20f), font, 9);
            Text atkLbl = CreateText(box.transform, "LblAtk", "balta", 10, new Vector2(28f, -18f), new Vector2(50f, 20f), font, TextAnchor.MiddleLeft);
            atkLbl.color = new Color(1f, 0.90f, 0.72f);

            // [Boşluk] atılma
            CreateKeycap(box.transform, "Boşluk", new Vector2(-28f, -44f), new Vector2(54f, 20f), font, 9);
            Text dashLbl = CreateText(box.transform, "LblDash", "atılma", 10, new Vector2(28f, -44f), new Vector2(50f, 20f), font, TextAnchor.MiddleLeft);
            dashLbl.color = new Color(1f, 0.90f, 0.72f);

            // [1-4] tarif
            CreateKeycap(box.transform, "1-4", new Vector2(-28f, -68f), new Vector2(54f, 18f), font, 9);
            Text craftLbl = CreateText(box.transform, "LblCraft", "tarif", 10, new Vector2(28f, -68f), new Vector2(50f, 18f), font, TextAnchor.MiddleLeft);
            craftLbl.color = new Color(1f, 0.90f, 0.72f);
        }

        private void BuildRightHowToPlayCard(Transform parent, Font font)
        {
            GameObject board = CreatePanel(parent, "RightHowToPlayCard", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-96f, 10f), new Vector2(162f, 246f), Color.white);
            board.GetComponent<Image>().sprite = _procParchmentSprite;

            // Koyu ahşap üst başlık şeridi
            GameObject topBar = CreatePanel(board.transform, "TopWoodBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(148f, 26f), new Color(0.42f, 0.22f, 0.11f, 1f));
            Text title = CreateStyledText(topBar.transform, "Title", "Nasıl Oynanır?", 12, Vector2.zero, new Vector2(140f, 24f), font, new Color(1f, 0.93f, 0.78f), new Color(0.18f, 0.08f, 0.03f));
            title.fontStyle = FontStyle.Bold;

            // --- Madde (1): Kırmızı & Yeşil Yaprak ---
            CreateNumberBadge(board.transform, "1", new Vector2(0f, 68f), font);
            CreateIcon(board.transform, "LeafK", _procLeafRed,   new Vector2(-42f, 40f), new Vector2(26f, 26f));
            CreateIcon(board.transform, "LeafY", _procLeafGreen, new Vector2(-10f, 40f), new Vector2(24f, 24f));
            Text t1 = CreateText(board.transform, "Txt1", "Kırmızı (Güç)\nYeşil (Sağlık)", 9, new Vector2(36f, 40f), new Vector2(72f, 32f), font, TextAnchor.MiddleLeft);
            t1.color = new Color(0.28f, 0.15f, 0.07f);
            t1.fontStyle = FontStyle.Bold;

            // --- Madde (2): Sarı İnce Uzun & Turuncu İksir ---
            CreateNumberBadge(board.transform, "2", new Vector2(0f, 6f), font);
            CreateIcon(board.transform, "LeafS", _procLeafYellow, new Vector2(-42f, -22f), new Vector2(26f, 26f));
            CreateIcon(board.transform, "LeafT", _procLeafOrange, new Vector2(-10f, -22f), new Vector2(26f, 26f));
            Text t2 = CreateText(board.transform, "Txt2", "İnce uzun (Hız)\nTuruncu (İksir)", 9, new Vector2(36f, -22f), new Vector2(72f, 32f), font, TextAnchor.MiddleLeft);
            t2.color = new Color(0.28f, 0.15f, 0.07f);
            t2.fontStyle = FontStyle.Bold;

            // --- Madde (3): Tarif ve Savaş ---
            CreateNumberBadge(board.transform, "3", new Vector2(0f, -56f), font);
            CreateIcon(board.transform, "LeafT2", _procLeafOrange, new Vector2(-42f, -84f), new Vector2(26f, 26f));
            CreateIcon(board.transform, "LeafK2", _procLeafRed,    new Vector2(-10f, -84f), new Vector2(26f, 26f));
            Text t3 = CreateText(board.transform, "Txt3", "Güvendeyken\nTarif Üret [1-4]", 9, new Vector2(36f, -84f), new Vector2(72f, 32f), font, TextAnchor.MiddleLeft);
            t3.color = new Color(0.28f, 0.15f, 0.07f);
            t3.fontStyle = FontStyle.Bold;

            // Karta tıklayınca da 6 sayfalık Kılavuz açılır
            Button boardBtn = board.AddComponent<Button>();
            boardBtn.transition = Selectable.Transition.None;
            boardBtn.onClick.AddListener(OnOpenGuideClicked);
        }

        private void BuildFullControlsModal(Transform canvasTr, Font font)
        {
            _controlsModal = CreatePanel(canvasTr, "Modal_Controls", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.04f, 0.02f, 0.85f));
            GameObject box = CreatePanel(_controlsModal.transform, "ControlsBoard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 310f), Color.white);
            box.GetComponent<Image>().sprite = _procParchmentSprite;

            CreateStyledText(box.transform, "Title", "TÜM KONTROLLER (SRS 6.1)", 16, new Vector2(0f, 126f), new Vector2(400f, 28f), font, new Color(1f, 0.86f, 0.40f), new Color(0.22f, 0.10f, 0.04f)).fontStyle = FontStyle.Bold;

            string fullControls =
                "<b>W / A / S / D</b>   :   8 yönde hareket\n" +
                "<b>Fare İmleci</b>   :   Bakış ve balta vuruş yönü\n" +
                "<b>Sol Tık veya L</b>   :   Balta savurma (basılı tutulursa sürekli)\n" +
                "<b>Boşluk</b>   :   Atılma / Dash (Hız 5. seviyede açılır)\n" +
                "<b>Q</b>   :   Kalkan (Sağlık 5. seviyede açılır, maçta 1 kez)\n" +
                "<b>E</b>   :   Ulti (Güç 5. seviyede açılır, maçta 1 kez)\n" +
                "<b>1 / 2 / 3</b>   :   Güç / Hız / Sağlık geliştirmesini üret\n" +
                "<b>4</b>   :   Can İksiri üret (+40 Can)\n" +
                "<b>Tab</b>   :   Envanter ve Tarif panelini aç / kapat\n" +
                "<b>Esc</b>   :   Maçı duraklat / Ana menüye dön";

            Text body = CreateText(box.transform, "Body", fullControls, 11, new Vector2(0f, 4f), new Vector2(390f, 195f), font, TextAnchor.MiddleLeft);
            body.color = new Color(0.22f, 0.11f, 0.05f);
            body.lineSpacing = 1.22f;

            CreateWoodenButton(box.transform, "Btn_CloseCtrl", "ANA MENÜ [Esc]", new Vector2(0f, -122f), new Vector2(160f, 30f), font, false, CloseModals);
            _controlsModal.SetActive(false);
        }

        private void BuildFullGuideModal(Transform canvasTr, Font font)
        {
            _guideModal = CreatePanel(canvasTr, "Modal_Guide", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.04f, 0.02f, 0.85f));
            GameObject box = CreatePanel(_guideModal.transform, "GuideBoard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 316f), Color.white);
            box.GetComponent<Image>().sprite = _procParchmentSprite;

            _guideTitleText = CreateStyledText(box.transform, "GuideTitle", "", 15, new Vector2(0f, 126f), new Vector2(430f, 28f), font, new Color(1f, 0.86f, 0.40f), new Color(0.22f, 0.10f, 0.04f));
            _guideTitleText.fontStyle = FontStyle.Bold;

            GameObject imgFrame = CreatePanel(box.transform, "ImgFrame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 52f), new Vector2(180f, 96f), new Color(0.25f, 0.14f, 0.07f, 1f));
            GameObject imgInner = CreatePanel(imgFrame.transform, "Illustration", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(172f, 88f), Color.white);
            _guideIllustrationImage = imgInner.GetComponent<Image>();

            _guideBodyText = CreateText(box.transform, "GuideBody", "", 10, new Vector2(0f, -44f), new Vector2(420f, 84f), font, TextAnchor.MiddleCenter);
            _guideBodyText.color = new Color(0.22f, 0.11f, 0.05f);
            _guideBodyText.fontStyle = FontStyle.Bold;
            _guideBodyText.lineSpacing = 1.22f;

            _prevPageButton = CreateWoodenButton(box.transform, "Btn_Prev", "◀ GERİ", new Vector2(-152f, -122f), new Vector2(96f, 30f), font, false, () => ChangeGuidePage(-1));
            _guidePageIndicatorText = CreateStyledText(box.transform, "PageNum", "Sayfa 1 / 6", 11, new Vector2(-46f, -122f), new Vector2(92f, 26f), font, new Color(1f, 0.92f, 0.70f), new Color(0.20f, 0.09f, 0.03f));
            _nextPageButton = CreateWoodenButton(box.transform, "Btn_Next", "İLERİ ▶", new Vector2(46f, -122f), new Vector2(96f, 30f), font, false, () => ChangeGuidePage(1));
            CreateWoodenButton(box.transform, "Btn_CloseGuide", "KAPAT [Esc]", new Vector2(162f, -122f), new Vector2(112f, 30f), font, false, CloseModals);

            _guideModal.SetActive(false);
        }

        // =====================================================================
        // PROSEDÜREL PIXEL-ART GÖRSEL ÜRETİCİ (Hiç dış dosya olmasa bile çalışır)
        // =====================================================================
        private void GenerateProceduralMenuSprites()
        {
            // 1. Sonbahar Arka Planı (Altın/Krem Gökyüzü + Köşelerde Sonbahar Yaprak Kümeleri)
            Texture2D bgTex = new Texture2D(160, 90, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < 90; y++)
            {
                float t = y / 89f;
                Color sky = Color.Lerp(new Color(0.95f, 0.72f, 0.38f), new Color(0.98f, 0.85f, 0.58f), t);
                for (int x = 0; x < 160; x++)
                {
                    float cornerDistTL = Vector2.Distance(new Vector2(x, y), new Vector2(0, 90));
                    float cornerDistTR = Vector2.Distance(new Vector2(x, y), new Vector2(160, 90));
                    float cornerDistBL = Vector2.Distance(new Vector2(x, y), new Vector2(0, 0));
                    float cornerDistBR = Vector2.Distance(new Vector2(x, y), new Vector2(160, 0));

                    if (cornerDistTL < 28f || cornerDistTR < 28f || cornerDistBL < 24f || cornerDistBR < 24f || y < 5 || y > 85)
                    {
                        Color foliage = ((x + y) % 3 == 0) ? new Color(0.75f, 0.25f, 0.14f) : new Color(0.88f, 0.45f, 0.18f);
                        bgTex.SetPixel(x, y, foliage);
                    }
                    else
                    {
                        bgTex.SetPixel(x, y, sky);
                    }
                }
            }
            bgTex.Apply();
            _procBgSprite = Sprite.Create(bgTex, new Rect(0, 0, 160, 90), new Vector2(0.5f, 0.5f), 16f);

            // 2. Ahşap Buton, Parşömen Pano ve Koyu Kontroller Kutusu
            _procWoodBtnSprite   = CreateFramedBoxSprite(64, 18, new Color(0.22f, 0.10f, 0.04f), new Color(0.46f, 0.25f, 0.13f), new Color(0.62f, 0.36f, 0.19f));
            _procParchmentSprite = CreateFramedBoxSprite(80, 110, new Color(0.24f, 0.12f, 0.05f), new Color(0.95f, 0.86f, 0.70f), new Color(0.55f, 0.32f, 0.16f));
            _procDarkBoxSprite   = CreateFramedBoxSprite(64, 72, new Color(0.16f, 0.08f, 0.04f), new Color(0.32f, 0.18f, 0.10f, 0.94f), new Color(0.52f, 0.30f, 0.16f));

            // 3. 4 Yaprak Şekli (Kırmızı Üç Uçlu, Sarı İnce Uzun, Yeşil Yuvarlak, Turuncu Yıldız)
            _procLeafRed    = CreateLeafIconSprite(new Color(0.90f, 0.18f, 0.18f), 0);
            _procLeafYellow = CreateLeafIconSprite(new Color(1.00f, 0.82f, 0.12f), 1);
            _procLeafGreen  = CreateLeafIconSprite(new Color(0.55f, 0.82f, 0.22f), 2);
            _procLeafOrange = CreateLeafIconSprite(new Color(1.00f, 0.48f, 0.10f), 3);

            // 4. 6 Kılavuz Görseli Yedeği
            _procGuideSprites = new Sprite[6];
            Color[] accents = {
                new Color(0.90f, 0.25f, 0.18f), new Color(0.95f, 0.78f, 0.18f), new Color(0.35f, 0.85f, 0.30f),
                new Color(0.35f, 0.78f, 0.98f), new Color(0.80f, 0.38f, 0.18f), new Color(0.65f, 0.40f, 0.85f)
            };
            for (int i = 0; i < 6; i++)
            {
                _procGuideSprites[i] = CreateFramedBoxSprite(64, 36, new Color(0.20f, 0.10f, 0.04f), accents[i], new Color(0.95f, 0.86f, 0.70f));
            }
        }

        private static Sprite CreateFramedBoxSprite(int w, int h, Color outline, Color fill, Color innerBorder)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if ((x == 0 || x == w - 1) && (y == 0 || y == h - 1))
                        tex.SetPixel(x, y, Color.clear);
                    else if (x <= 1 || x >= w - 2 || y <= 1 || y >= h - 2)
                        tex.SetPixel(x, y, outline);
                    else if (x == 2 || x == w - 3 || y == 2 || y == h - 3)
                        tex.SetPixel(x, y, innerBorder);
                    else
                        tex.SetPixel(x, y, fill);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Sprite CreateLeafIconSprite(Color mainCol, int shapeType)
        {
            Texture2D tex = new Texture2D(12, 12, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            Color darkOut = new Color(0.18f, 0.08f, 0.04f);
            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    tex.SetPixel(x, y, Color.clear);
                    float dx = x - 5.5f, dy = y - 5.5f;
                    bool inside = shapeType switch
                    {
                        1 => Mathf.Abs(dx - dy) < 2.2f && (dx * dx + dy * dy) < 24f, // İnce uzun çapraz
                        2 => (dx * dx + dy * dy) <= 18f,                             // Yuvarlak
                        _ => (dx * dx + dy * dy) <= 16f || (Mathf.Abs(dx) < 2f && dy > 0f && dy < 5f) // Sivri/Yıldız
                    };
                    if (inside)
                    {
                        bool edge = (dx * dx + dy * dy) > 11f;
                        tex.SetPixel(x, y, edge ? darkOut : mainCol);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 12, 12), new Vector2(0.5f, 0.5f), 16f);
        }

        // --- YARDIMCI UI METOTLARI ---
        private void CreateKeycap(Transform parent, string label, Vector2 pos, Vector2 size, Font font, int fontSize = 10)
        {
            GameObject key = CreatePanel(parent, $"Key_{label}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, new Color(0.22f, 0.12f, 0.06f, 1f));
            Outline ol = key.AddComponent<Outline>();
            ol.effectColor = new Color(0.85f, 0.72f, 0.52f, 0.9f);
            ol.effectDistance = new Vector2(1f, -1f);

            Text txt = CreateText(key.transform, "Txt", label, fontSize, Vector2.zero, size, font, TextAnchor.MiddleCenter);
            txt.color = new Color(1f, 0.95f, 0.85f);
            txt.fontStyle = FontStyle.Bold;
        }

        private void CreateNumberBadge(Transform parent, string num, Vector2 pos, Font font)
        {
            GameObject badge = CreatePanel(parent, $"Badge_{num}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(18f, 18f), new Color(0.45f, 0.24f, 0.12f, 1f));
            Text t = CreateText(badge.transform, "Num", num, 10, Vector2.zero, new Vector2(18f, 18f), font, TextAnchor.MiddleCenter);
            t.color = new Color(1f, 0.92f, 0.75f);
            t.fontStyle = FontStyle.Bold;
        }

        private static void CreateIcon(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size)
        {
            GameObject go = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, Color.white);
            go.GetComponent<Image>().sprite = sprite;
        }

        private Button CreateWoodenButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Font font, bool withSideLeaves, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, Color.white);
            btnObj.GetComponent<Image>().sprite = _procWoodBtnSprite;

            if (withSideLeaves)
            {
                CreateIcon(btnObj.transform, "LeafL", _procLeafGreen,  new Vector2(-size.x * 0.5f + 4f, 2f), new Vector2(16f, 16f));
                CreateIcon(btnObj.transform, "LeafR", _procLeafOrange, new Vector2( size.x * 0.5f - 4f, 2f), new Vector2(16f, 16f));
            }

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(1f, 0.92f, 0.75f, 1f);
            cb.pressedColor     = new Color(0.80f, 0.70f, 0.55f, 1f);
            cb.disabledColor    = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            Text txt = CreateStyledText(btnObj.transform, "Label", label, 13, Vector2.zero, size, font, new Color(1f, 0.94f, 0.80f), new Color(0.20f, 0.08f, 0.02f));
            txt.fontStyle = FontStyle.Bold;
            return btn;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos, Vector2 size, Font font, TextAnchor align)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Text txt = go.GetComponent<Text>();
            txt.font = font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = align;
            txt.supportRichText = true;
            return txt;
        }

        private static Text CreateStyledText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos, Vector2 size, Font font, Color textColor, Color outlineColor)
        {
            Text txt = CreateText(parent, name, content, fontSize, anchoredPos, size, font, TextAnchor.MiddleCenter);
            txt.color = textColor;
            Outline ol = txt.gameObject.AddComponent<Outline>();
            ol.effectColor = outlineColor;
            ol.effectDistance = new Vector2(1.5f, -1.5f);
            Shadow sh = txt.gameObject.AddComponent<Shadow>();
            sh.effectColor = outlineColor;
            sh.effectDistance = new Vector2(0f, -2f);
            return txt;
        }

        private static void EnsureCameraAndEventSystem()
        {
            if (Camera.main == null && FindAnyObjectByType<Camera>() == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                Camera cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.95f, 0.76f, 0.45f, 1f);
                cam.orthographic = true;
            }

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                Type newInputModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (newInputModule != null) esObj.AddComponent(newInputModule);
                else esObj.AddComponent<StandaloneInputModule>();
            }
        }

        private static bool WasEscapePressedThisFrame()
        {
            try
            {
                Type kbType = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
                if (kbType != null)
                {
                    object currentKb = kbType.GetProperty("current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                    if (currentKb != null)
                    {
                        object escKey = kbType.GetProperty("escapeKey", BindingFlags.Public | BindingFlags.Instance)?.GetValue(currentKb);
                        if (escKey != null)
                        {
                            PropertyInfo pressedProp = escKey.GetType().GetProperty("wasPressedThisFrame", BindingFlags.Public | BindingFlags.Instance);
                            if (pressedProp != null && (bool)pressedProp.GetValue(escKey)) return true;
                        }
                    }
                }
            }
            catch { }

            try { return Input.GetKeyDown(KeyCode.Escape); }
            catch { return false; }
        }
    }
}
