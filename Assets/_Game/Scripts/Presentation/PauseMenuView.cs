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

        void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(() => Services.Match.Resume());
            if (restartButton != null) restartButton.onClick.AddListener(SceneFlow.Restart);
            if (menuButton != null) menuButton.onClick.AddListener(SceneFlow.LoadMenu);
            if (soundButton != null) soundButton.onClick.AddListener(ToggleSound);
            foreach (var button in new[] { resumeButton, restartButton, menuButton, soundButton })
                if (button != null) button.onClick.AddListener(AudioManager.PlayClick);
            RefreshSoundLabel();
        }

        void OnEnable() => GameEvents.MatchStateChanged += OnMatchState;
        void OnDisable() => GameEvents.MatchStateChanged -= OnMatchState;
        void Start() => OnMatchState(Services.Match.State);

        void OnMatchState(MatchState state)
        {
            if (panel != null) panel.SetActive(state == MatchState.Paused);
        }

        // NFR-10: ses aç/kapa AudioManager.Muted'ı değiştirir.
        void ToggleSound()
        {
            AudioManager.Muted = !AudioManager.Muted;
            RefreshSoundLabel();
        }

        void RefreshSoundLabel()
        {
            if (soundLabel != null) soundLabel.text = AudioManager.Muted ? "Ses: Kapalı" : "Ses: Açık";
        }
    }
}
