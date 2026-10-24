using System.Collections;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Ses efektleri (NFR-10). AudioManager.prefab üzerinde; yalnızca GameEvents dinler, başka hiçbir kod AudioSource çalmaz.
    /// Oyuncuya ait sesler (tarif, yaprak toplama, yetenek açılması) yalnızca oyuncu için; dünya sesleri (balta, isabet,
    /// ağaç, ölüm, yetenekler) yalnızca görünen (VisibleToPlayer) ve ekranda olan karakterler için çalar.
    /// Ses aç/kapa: AudioManager.Muted (duraklatma menüsündeki düğme).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] SfxLibrary library;

        static AudioManager _instance;
        static bool _muted;
        AudioSource _source;
        float _nextLeafTime;

        /// <summary>Ses kapalı mı (NFR-10). Sahne değişse de korunur.</summary>
        public static bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                AudioListener.volume = value ? 0f : 1f;
            }
        }

        /// <summary>Arayüz düğmeleri çağırır (duraklatma, sonuç, envanter kartı).</summary>
        public static void PlayClick()
        {
            if (_instance != null && _instance.library != null) _instance.Play(_instance.library.uiClick);
        }

        void Awake()
        {
            _instance = this;
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            AudioListener.volume = _muted ? 0f : 1f;
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        void OnEnable()
        {
            GameEvents.Swung += OnSwung;
            GameEvents.HitLanded += OnHitLanded;
            GameEvents.Died += OnDied;
            GameEvents.TreeHit += OnTreeHit;
            GameEvents.GreatTreeHit += OnGreatTreeHit;
            GameEvents.TreeFelled += OnTreeFelled;
            GameEvents.LeafCollected += OnLeafCollected;
            GameEvents.CraftStarted += OnCraftStarted;
            GameEvents.CraftCompleted += OnCraftCompleted;
            GameEvents.CraftRejected += OnCraftRejected;
            GameEvents.AbilityUnlocked += OnAbilityUnlocked;
            GameEvents.AbilityUsed += OnAbilityUsed;
            GameEvents.ShieldBroken += OnShieldBroken;
            GameEvents.ZonePhaseChanged += OnZonePhase;
            GameEvents.CountdownTick += OnCountdown;
        }

        void OnDisable()
        {
            GameEvents.Swung -= OnSwung;
            GameEvents.HitLanded -= OnHitLanded;
            GameEvents.Died -= OnDied;
            GameEvents.TreeHit -= OnTreeHit;
            GameEvents.GreatTreeHit -= OnGreatTreeHit;
            GameEvents.TreeFelled -= OnTreeFelled;
            GameEvents.LeafCollected -= OnLeafCollected;
            GameEvents.CraftStarted -= OnCraftStarted;
            GameEvents.CraftCompleted -= OnCraftCompleted;
            GameEvents.CraftRejected -= OnCraftRejected;
            GameEvents.AbilityUnlocked -= OnAbilityUnlocked;
            GameEvents.AbilityUsed -= OnAbilityUsed;
            GameEvents.ShieldBroken -= OnShieldBroken;
            GameEvents.ZonePhaseChanged -= OnZonePhase;
            GameEvents.CountdownTick -= OnCountdown;
        }

        // ---------------- Dünya sesleri (görünen ve ekrandaki karakterler) ----------------

        void OnSwung(Character c) { if (Audible(c)) Play(library.swing); }
        void OnHitLanded(Character attacker, IDamageable target, int damage) { if (Audible(attacker) || Audible(target as Character)) Play(library.hit); }
        void OnDied(Character victim, Character killer) { if (Audible(victim)) Play(library.death); }
        void OnTreeHit(ITree tree, Character by) { if (by == null || Audible(by)) Play(library.treeHit); }
        void OnGreatTreeHit(Character by) { if (by == null || Audible(by)) Play(library.greatTreeHit); }
        void OnTreeFelled(ITree tree, Character by) { if (OnScreen(tree.Position)) Play(library.treeFelled); }

        void OnAbilityUsed(Character c, AbilityId ability)
        {
            if (!Audible(c)) return;
            if (ability == AbilityId.Dash) Play(library.dash);
            else if (ability == AbilityId.Shield) Play(library.shieldUp);
            else
            {
                Play(library.ultiWindup);
                StartCoroutine(UltiBlast(c));
            }
        }

        // Ulti patlaması hazırlanma bitince (abilities.ultiWindupSec); karakter bu arada ölürse çalmaz.
        IEnumerator UltiBlast(Character c)
        {
            yield return new WaitForSeconds(Services.Config.abilities.ultiWindupSec);
            if (c != null && c.IsAlive && Audible(c)) Play(library.ultiBlast);
        }

        void OnShieldBroken(Character c) { if (Audible(c)) Play(library.shieldBroken); }

        // ---------------- Oyuncu sesleri ----------------

        void OnLeafCollected(Character c, LeafType type, int count)
        {
            if (c == null || !c.IsPlayer || Time.time < _nextLeafTime) return;
            _nextLeafTime = Time.time + 1f / Mathf.Max(1f, library.leafSoundsPerSecond);
            Play(library.leafCollected);
        }

        void OnCraftStarted(Character c, RecipeId recipe) { if (c != null && c.IsPlayer) Play(library.craftStarted); }
        void OnCraftCompleted(Character c, RecipeId recipe, int newLevel) { if (c != null && c.IsPlayer) Play(library.craftCompleted); }
        void OnCraftRejected(Character c, RecipeId recipe, CraftFail fail) { if (c != null && c.IsPlayer) Play(library.craftRejected); }
        void OnAbilityUnlocked(Character c, AbilityId ability) { if (c != null && c.IsPlayer) Play(library.abilityUnlocked); }

        // ---------------- Maç ----------------

        // Alan daralmasının başlaması (v1.4: ZoneWarning yerine ZonePhaseChanged). 0. aşama maç başıdır, çalmaz.
        void OnZonePhase(int phase) { if (phase > 0) Play(library.zoneShrink); }

        void OnCountdown(int secondsLeft) => Play(secondsLeft > 0 ? library.countdownTick : library.countdownGo);

        // ---------------- Yardımcılar ----------------

        void Play(AudioClip clip)
        {
            if (clip == null || library == null || _muted) return;
            _source.PlayOneShot(clip, library.volume);
        }

        static bool Audible(Character c) => c != null && c.VisibleToPlayer && OnScreen(c.Position);

        static bool OnScreen(Vector2 pos)
        {
            var camera = Camera.main;
            if (camera == null) return true;
            Vector3 v = camera.WorldToViewportPoint(pos);
            return v.x >= 0f && v.x <= 1f && v.y >= 0f && v.y <= 1f;
        }
    }
}
