using LeafBrawl.Core;
using LeafBrawl.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Duraklatma menüsü (FR-06, 6.4). PauseMenu.prefab kökünde.
    /// Esc'yi MatchManager yakalar; bu ekran yalnızca MatchStateChanged(Paused) ile açılır, diğer durumlarda kapanır.
    /// Menüdeyken Esc = Devam (MatchManager). Düğmeler yalnızca Services.Match ve SceneFlow'u çağırır.
    /// </summary>
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button menuButton;
        [SerializeField] Button soundButton;
        [SerializeField] TMP_Text soundLabel;

        // Yer tutucu: ses AudioManager'a bağlanınca (Furkan, NFR-10) bu değer oradan okunur ve oraya yazılır.
        bool _soundOn = true;

        void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(() => Services.Match.Resume());
            if (restartButton != null) restartButton.onClick.AddListener(SceneFlow.Restart);
            if (menuButton != null) menuButton.onClick.AddListener(SceneFlow.LoadMenu);
            if (soundButton != null) soundButton.onClick.AddListener(ToggleSound);
            RefreshSoundLabel();
        }

        void OnEnable() => GameEvents.MatchStateChanged += OnMatchState;
        void OnDisable() => GameEvents.MatchStateChanged -= OnMatchState;
        void Start() => OnMatchState(Services.Match.State);

        void OnMatchState(MatchState state)
        {
            if (panel != null) panel.SetActive(state == MatchState.Paused);
        }

        void ToggleSound()
        {
            _soundOn = !_soundOn;
            RefreshSoundLabel();
        }

        void RefreshSoundLabel()
        {
            if (soundLabel != null) soundLabel.text = _soundOn ? "Ses: Açık" : "Ses: Kapalı";
        }
    }
}
