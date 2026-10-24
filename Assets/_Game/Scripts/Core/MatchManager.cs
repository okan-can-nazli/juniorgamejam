using System;
using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LeafBrawl.Core
{
    /// <summary>
    /// Maç akışı (IMatch). Systems.prefab üzerinde durur ve Awake'te Services.Match olarak kaydolur.
    /// Match sahnesinde oyuncu + debug.botCount botu doğma noktalarına dağıtır; sandbox'ta sahnedeki karakterleri kullanır.
    /// 3 sn geri sayım → Playing → oyuncu ölünce veya tek karakter kalınca Ended (FR-04, FR-05, FR-07, FR-85).
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class MatchManager : MonoBehaviour, IMatch
    {
        [SerializeField] InputActionAsset actions;   // LeafBrawl.inputactions; Esc = Player/Pause
        [Tooltip("Açık: oyuncu ve botlar doğma noktalarında doğar (Match). Kapalı: sahnedeki karakterler kullanılır (sandbox).")]
        [SerializeField] bool spawnCharacters;
        [SerializeField] Character playerPrefab;
        [SerializeField] Character botPrefab;

        readonly List<Character> _characters = new List<Character>();
        readonly List<Character> _deathOrder = new List<Character>();
        InputActionAsset _actions;   // bileşene ait kopya (PlayerIntentSource'unkinden bağımsız)
        InputAction _pause;
        MatchState _stateBeforePause;
        float _countdownLeft;
        int _lastTick;
        bool _endCheckPending;

        public MatchState State { get; private set; } = MatchState.Countdown;
        public float MatchTime { get; private set; }
        public IReadOnlyList<Character> Characters => _characters;
        public Character Player { get; private set; }
        public MatchResult Result { get; private set; }

        public void Pause()
        {
            if (State == MatchState.Paused || State == MatchState.Ended) return;
            _stateBeforePause = State;
            Time.timeScale = 0f;
            SetState(MatchState.Paused);
        }

        public void Resume()
        {
            if (State != MatchState.Paused) return;
            Time.timeScale = Services.DebugConfig.timeScale;
            SetState(_stateBeforePause);
        }

        public void Restart()
        {
            Time.timeScale = Services.DebugConfig.timeScale;
            var scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            // Sandbox sahneleri Build Settings'te yok; editörde yolundan yükle.
            if (scene.buildIndex < 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(scene.buildIndex);
        }

        void Awake()
        {
            Services.Match = this;
            _actions = Instantiate(actions);
            var map = _actions.FindActionMap("Player", true);
            _pause = map.FindAction("Pause", true);
            map.Enable();
        }

        void OnEnable() => GameEvents.Died += OnDied;
        void OnDisable() => GameEvents.Died -= OnDied;
        void OnDestroy() => Destroy(_actions);

        void Start()
        {
            if (spawnCharacters) SpawnCharacters();
            else _characters.AddRange(FindObjectsByType<Character>());
            _characters.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (var c in _characters)
                if (c.IsPlayer) Player = c;

            _countdownLeft = Services.Config.match.countdownSec;
            _lastTick = Mathf.CeilToInt(_countdownLeft);
            GameEvents.RaiseMatchStateChanged(MatchState.Countdown);
            GameEvents.RaiseCountdownTick(_lastTick);
        }

        // FR-04: oyuncu ve botlar doğma noktalarına rastgele dağıtılır; botlara profil ve atkı rengi verilir.
        void SpawnCharacters()
        {
            var rng = Services.Rng;
            var points = new List<Vector2>(Services.World.SpawnPoints);
            rng.Shuffle(points);
            var match = Services.Config.match;
            var profiles = new List<string>(match.botProfiles);
            rng.Shuffle(profiles);

            int botCount = Mathf.Min(Services.DebugConfig.botCount, points.Count - 1, match.botScarves.Length);
            var player = Instantiate(playerPrefab, points[0], Quaternion.identity);
            player.name = "Player";
            player.Setup(true, null, ScarfColor.Blue);
            _characters.Add(player);

            for (int i = 0; i < botCount; i++)
            {
                var bot = Instantiate(botPrefab, points[i + 1], Quaternion.identity);
                var profile = (BotProfile)Enum.Parse(typeof(BotProfile), profiles[i % profiles.Count]);
                var scarf = (ScarfColor)Enum.Parse(typeof(ScarfColor), match.botScarves[i]);
                bot.name = $"Bot_{profile}";
                bot.Setup(false, profile, scarf);
                _characters.Add(bot);
            }
        }

        void Update()
        {
            if (_pause.WasPressedThisFrame())
            {
                if (State == MatchState.Ended) SceneFlow.LoadMenu();   // FR-08: sonuç ekranında Esc ana menüye döner
                else if (State == MatchState.Paused) Resume();
                else Pause();
            }

            if (State == MatchState.Countdown) TickCountdown();
            else if (State == MatchState.Playing) MatchTime += Time.deltaTime;
        }

        // FR-05: geri sayımda kimse hareket edemez (Character intent'i sıfırlar); maç saati sonra başlar.
        void TickCountdown()
        {
            _countdownLeft -= Time.deltaTime;
            int tick = Mathf.Max(0, Mathf.CeilToInt(_countdownLeft));
            if (tick != _lastTick)
            {
                _lastTick = tick;
                GameEvents.RaiseCountdownTick(tick);
            }
            if (_countdownLeft <= 0f)
            {
                MatchTime = 0f;
                SetState(MatchState.Playing);
            }
        }

        void OnDied(Character victim, Character killer)
        {
            if (!_characters.Contains(victim) || _deathOrder.Contains(victim)) return;
            _deathOrder.Add(victim);
            _endCheckPending = true;   // aynı karede başka ölümler de olabilir (FR-83)
        }

        // Bitişe karenin sonunda karar ver: aynı karede ölenlerin hepsi kaydedilmiş olur.
        void LateUpdate()
        {
            if (!_endCheckPending || State == MatchState.Ended) return;
            _endCheckPending = false;

            int alive = 0;
            foreach (var c in _characters)
                if (c.IsAlive) alive++;
            bool playerOut = Player != null && !Player.IsAlive;
            if (playerOut || alive <= 1) EndMatch();
        }

        // FR-85: maç ekranda donar, botlar arka planda sürmez.
        void EndMatch()
        {
            Result = BuildResult();
            Time.timeScale = 0f;
            SetState(MatchState.Ended);
        }

        // FR-82–FR-84: ölüm sırası; aynı karede ölenlerde ölümden önceki canı yüksek olan üstte, eşitse aynı sıra.
        // Hayatta kalanlar en üstte ortak sırayı paylaşır (oyuncu elenince maç durur, aralarında karar verilmez).
        MatchResult BuildResult()
        {
            var ordered = new List<Character>(_characters);
            ordered.Sort(CompareStanding);

            var ranking = new List<RankEntry>(ordered.Count);
            var result = new MatchResult { Ranking = ranking };
            for (int i = 0; i < ordered.Count; i++)
            {
                var c = ordered[i];
                int rank = i > 0 && CompareStanding(ordered[i - 1], c) == 0 ? ranking[i - 1].Rank : i + 1;
                var entry = new RankEntry
                {
                    Character = c,
                    Rank = rank,
                    SurvivalTime = c.IsAlive ? MatchTime : c.Health.DeathTime,
                    Kills = c.Kills,
                    ThreatScore = c.ThreatScore,
                };
                ranking.Add(entry);
                if (c == Player)
                {
                    result.PlayerRank = entry.Rank;
                    result.PlayerSurvivalTime = entry.SurvivalTime;
                    result.PlayerKills = entry.Kills;
                    result.PlayerThreatScore = entry.ThreatScore;
                }
            }
            return result;
        }

        // Negatif: a daha üst sırada.
        static int CompareStanding(Character a, Character b)
        {
            if (a.IsAlive != b.IsAlive) return a.IsAlive ? -1 : 1;
            if (a.IsAlive) return 0;
            if (a.Health.DeathFrame != b.Health.DeathFrame) return b.Health.DeathFrame.CompareTo(a.Health.DeathFrame);
            return b.Health.HpBeforeDeath.CompareTo(a.Health.HpBeforeDeath);
        }

        void SetState(MatchState state)
        {
            if (State == state) return;
            State = state;
            GameEvents.RaiseMatchStateChanged(state);
        }
    }
}
