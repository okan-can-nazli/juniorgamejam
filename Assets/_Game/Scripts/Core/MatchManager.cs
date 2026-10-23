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
    /// M0 sürümü: sahnedeki karakterleri toplar, maç hemen başlar; Esc duraklatır.
    /// Doğurma, geri sayım ve maç sonu Aşama 1'de gelir.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class MatchManager : MonoBehaviour, IMatch
    {
        [SerializeField] InputActionAsset actions;   // LeafBrawl.inputactions; Esc = Player/Pause

        InputAction _pause;

        readonly List<Character> _characters = new List<Character>();

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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        MatchState _stateBeforePause;

        void Awake()
        {
            Services.Match = this;
            var map = actions.FindActionMap("Player", true);
            _pause = map.FindAction("Pause", true);
            map.Enable();
        }

        void Start()
        {
            _characters.AddRange(FindObjectsByType<Character>());
            _characters.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (var c in _characters)
                if (c.IsPlayer) Player = c;
            SetState(MatchState.Playing);
        }

        void Update()
        {
            if (_pause.WasPressedThisFrame())
            {
                if (State == MatchState.Paused) Resume();
                else Pause();
            }
            if (State == MatchState.Playing) MatchTime += Time.deltaTime;
        }

        void SetState(MatchState state)
        {
            if (State == state) return;
            State = state;
            GameEvents.RaiseMatchStateChanged(state);
        }
    }
}
