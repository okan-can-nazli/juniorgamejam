using System.Text;
using LeafBrawl.Characters;
using LeafBrawl.Core;
using LeafBrawl.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Sonuç ekranı (FR-07, FR-08, FR-85). ResultScreen.prefab kökünde.
    /// MatchStateChanged(Ended) gelince Services.Match.Result ile dolar ve açılır. Esc → ana menü MatchManager'da.
    /// </summary>
    public class ResultView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text rankText;      // "2. oldun"
        [SerializeField] TMP_Text statsText;     // süre, eleme, tehdit
        [SerializeField] TMP_Text rankingText;   // 1.–4. liste
        [SerializeField] Button restartButton;
        [SerializeField] Button menuButton;

        readonly StringBuilder _sb = new StringBuilder();

        void Awake()
        {
            if (restartButton != null) restartButton.onClick.AddListener(SceneFlow.Restart);
            if (menuButton != null) menuButton.onClick.AddListener(SceneFlow.LoadMenu);
            foreach (var button in new[] { restartButton, menuButton })
                if (button != null) button.onClick.AddListener(AudioManager.PlayClick);
        }

        void OnEnable() => GameEvents.MatchStateChanged += OnMatchState;
        void OnDisable() => GameEvents.MatchStateChanged -= OnMatchState;
        void Start() => OnMatchState(Services.Match.State);

        void OnMatchState(MatchState state)
        {
            var result = Services.Match.Result;
            bool show = state == MatchState.Ended && result != null;
            if (panel != null) panel.SetActive(show);
            if (show) Fill(result);
        }

        void Fill(MatchResult result)
        {
            if (rankText != null) rankText.text = result.PlayerRank == 1 ? "Kazandın! 1." : $"{result.PlayerRank}. oldun";
            if (statsText != null)
                statsText.text = $"Hayatta kalma: {Clock(result.PlayerSurvivalTime)}\nEleme: {result.PlayerKills}\nTehdit puanı: {result.PlayerThreatScore}";
            if (rankingText == null || result.Ranking == null) return;
            _sb.Clear();
            foreach (var entry in result.Ranking)
                _sb.Append(entry.Rank).Append(". ").Append(Name(entry.Character))
                   .Append("   ").Append(Clock(entry.SurvivalTime))
                   .Append("   eleme ").Append(entry.Kills)
                   .Append("   tehdit ").Append(entry.ThreatScore).Append('\n');
            rankingText.text = _sb.ToString();
        }

        static string Name(Character c)
        {
            if (c == null) return "?";
            if (c.IsPlayer) return "Sen";
            return c.Profile switch
            {
                BotProfile.Aggressive => "Saldırgan",
                BotProfile.Agile => "Çevik",
                BotProfile.Tough => "Dayanıklı",
                _ => "Bot",
            };
        }

        static string Clock(float seconds)
        {
            int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{s / 60}:{s % 60:00}";
        }
    }
}
