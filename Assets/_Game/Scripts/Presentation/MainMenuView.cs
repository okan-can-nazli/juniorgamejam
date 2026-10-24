using System;
using LeafBrawl.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Ana menü (FR-01), Kontroller (FR-02) ve Kılavuz (FR-03) ekranları; MainMenu.unity'deki MainMenu nesnesinde.
    /// Düğmeler yalnızca SceneFlow'u çağırır. Kontroller ve Kılavuz'da Esc ana menüye döner (6.4).
    /// Kılavuz sayfalarının metni ve görseli Inspector'dan (pages) değiştirilir; kod değişmez.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Serializable]
        public class GuidePage
        {
            public string title;
            [TextArea(2, 5)] public string body;   // en fazla 3 cümle (FR-03)
            public Sprite image;                   // boşsa yer tutucu görünür
        }

        [SerializeField] InputActionAsset actions;   // LeafBrawl.inputactions; Esc = Player/Pause

        [Header("Ana menü")]
        [SerializeField] GameObject mainPanel;
        [SerializeField] Button startButton;
        [SerializeField] Button guideButton;
        [SerializeField] Button controlsButton;
        [SerializeField] Button quitButton;

        [Header("Kontroller")]
        [SerializeField] GameObject controlsPanel;
        [SerializeField] Button controlsBackButton;

        [Header("Kılavuz")]
        [SerializeField] GameObject guidePanel;
        [SerializeField] TMP_Text guideTitle;
        [SerializeField] TMP_Text guideBody;
        [SerializeField] TMP_Text guidePageLabel;   // "2 / 6"
        [SerializeField] Image guideImage;
        [SerializeField] Button guidePrevButton;
        [SerializeField] Button guideNextButton;
        [SerializeField] Button guideBackButton;
        [SerializeField] GuidePage[] pages;

        InputActionAsset _actions;   // bileşene ait kopya (PlayerIntentSource ve MatchManager'daki gibi)
        InputAction _back;
        int _page;

        void Awake()
        {
            _actions = Instantiate(actions);
            _back = _actions.FindActionMap("Player", true).FindAction("Pause", true);

            startButton.onClick.AddListener(SceneFlow.StartMatch);
            guideButton.onClick.AddListener(ShowGuide);
            controlsButton.onClick.AddListener(() => Show(controlsPanel));
            quitButton.onClick.AddListener(SceneFlow.Quit);
            controlsBackButton.onClick.AddListener(ShowMain);
            guideBackButton.onClick.AddListener(ShowMain);
            guidePrevButton.onClick.AddListener(() => SetPage(_page - 1));
            guideNextButton.onClick.AddListener(() => SetPage(_page + 1));
            ShowMain();
        }

        void OnEnable() => _back.Enable();
        void OnDisable() => _back.Disable();
        void OnDestroy() => Destroy(_actions);

        // 6.4: Kontroller ve Kılavuz'da Esc ana menüye döner; ana menüde bir şey yapmaz.
        void Update()
        {
            if (_back.WasPressedThisFrame() && !mainPanel.activeSelf) ShowMain();
        }

        public void ShowMain() => Show(mainPanel);

        public void ShowGuide()
        {
            Show(guidePanel);
            SetPage(0);
        }

        void Show(GameObject panel)
        {
            mainPanel.SetActive(panel == mainPanel);
            controlsPanel.SetActive(panel == controlsPanel);
            guidePanel.SetActive(panel == guidePanel);
        }

        void SetPage(int index)
        {
            if (pages == null || pages.Length == 0) return;
            _page = Mathf.Clamp(index, 0, pages.Length - 1);
            var page = pages[_page];
            guideTitle.text = page.title;
            guideBody.text = page.body;
            guidePageLabel.text = $"{_page + 1} / {pages.Length}";
            if (guideImage != null)
            {
                guideImage.sprite = page.image;
                guideImage.color = page.image != null ? Color.white : new Color(1f, 1f, 1f, 0.15f);   // görsel yoksa yer tutucu kutu
            }
            guidePrevButton.interactable = _page > 0;
            guideNextButton.interactable = _page < pages.Length - 1;
        }
    }
}
