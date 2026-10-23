using LeafBrawl.Combat;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Characters
{
    /// <summary>
    /// Oyuncu ve botların ortak kökü. Diğer sistemler karakterle yalnızca buradaki herkese açık üyeler üzerinden konuşur.
    /// Konumlar (Vector2) Unity birimidir; mesafe karşılaştırırken Units.ToUnits(px) kullan.
    /// </summary>
    [DefaultExecutionOrder(-50)]   // CurrentIntent, savaş ve bot kodundan önce güncellenir
    [RequireComponent(typeof(Rigidbody2D), typeof(CharacterMotor), typeof(CharacterStats))]
    [RequireComponent(typeof(Health), typeof(Inventory), typeof(Crafting))]
    public class Character : MonoBehaviour, IDamageable
    {
        static int _nextId;

        [SerializeField] bool isPlayer;
        [SerializeField] ScarfColor scarf = ScarfColor.Blue;

        IIntentSource _intentSource;
        Rigidbody2D _body;

        // ---- Kimlik (Yiğit yazar) ----
        public int Id { get; private set; }
        public bool IsPlayer => isPlayer;
        public BotProfile? Profile { get; private set; }
        public ScarfColor Scarf => scarf;

        // ---- Konum ve durum (Yiğit yazar) ----
        public Vector2 Position => _body != null ? _body.position : (Vector2)transform.position;
        /// <summary>Birim/sn.</summary>
        public Vector2 Velocity => Motor.Velocity;
        /// <summary>Bakış yönü (birim vektör), Aim'den güncellenir.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;
        public bool IsAlive => Health.IsAlive;
        /// <summary>Bu karedeki girdi. Geri sayımda, duraklatmada, maç bitince ve ölüyken sıfırdır.</summary>
        public CharacterIntent CurrentIntent { get; private set; }

        // ---- Parçalar ----
        public CharacterMotor Motor { get; private set; }
        public CharacterStats Stats { get; private set; }
        public Health Health { get; private set; }
        public Inventory Inventory { get; private set; }
        public Crafting Crafting { get; private set; }
        /// <summary>Hamza (CharacterCombat.prefab içinde).</summary>
        public AbilityController Abilities { get; private set; }

        // ---- Tehdit, güvende, eleme (Yiğit yazar) ----
        public int ThreatScore { get; private set; }
        public PileTier PileTier { get; private set; }
        public int Kills { get; private set; }

        /// <summary>Alan içinde + son 3 sn hasar yok + son 3 sn savurma yok (SRS 1.3, 3.7).</summary>
        public bool IsSafe
        {
            get
            {
                if (!IsAlive) return false;
                var crafting = Services.Config.crafting;
                float now = Time.time;
                return Services.Zone.IsInside(Position)
                    && now - Health.LastDamageTime >= crafting.safeNoDamageSec
                    && now - LastSwingTime >= crafting.safeNoSwingSec;
            }
        }

        // ---- Savaş (Hamza yazar) ----
        public float LastSwingTime { get; private set; } = float.NegativeInfinity;
        /// <summary>Savurma anında çağrılır ("güvende" kuralı için).</summary>
        public void MarkSwing() => LastSwingTime = Time.time;
        /// <summary>Hareket hızı çarpanı: savururken 0,5; ulti hazırlanırken 0.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        // ---- Görünürlük (Okan yazar) ----
        public bool VisibleToPlayer { get; set; } = true;
        public bool IsInBush { get; set; }
        public bool IsRevealed { get; set; }

        public void ApplyDamage(DamageInfo info) => Health.ApplyDamage(info);

        /// <summary>MatchManager doğurduğunda çağırır. Oyuncu her zaman mavi atkılıdır.</summary>
        public void Setup(bool player, BotProfile? profile, ScarfColor scarfColor)
        {
            isPlayer = player;
            Profile = player ? null : profile;
            scarf = player ? ScarfColor.Blue : scarfColor;
        }

        void Awake()
        {
            Id = ++_nextId;
            _body = GetComponent<Rigidbody2D>();
            Motor = GetComponent<CharacterMotor>();
            Stats = GetComponent<CharacterStats>();
            Health = GetComponent<Health>();
            Inventory = GetComponent<Inventory>();
            Crafting = GetComponent<Crafting>();
            Abilities = GetComponentInChildren<AbilityController>(true);
            _intentSource = GetComponent<IIntentSource>();
        }

        void Start()
        {
            if (isPlayer) ApplyDebugStart();
            RecalculateThreat();
        }

        void Update()
        {
            bool canAct = IsAlive && _intentSource != null && Services.Match.State == MatchState.Playing;
            CurrentIntent = canAct ? _intentSource.ReadIntent(this) : default;
            if (CurrentIntent.Aim.sqrMagnitude > 0.0001f)
                Facing = CurrentIntent.Aim.normalized;
        }

        // debug.json: oyuncunun başlangıç seviyeleri ve envanteri (NFR-05).
        void ApplyDebugStart()
        {
            var debug = Services.DebugConfig;
            Stats.SetLevel(StatType.Power, debug.startLevels.power);
            Stats.SetLevel(StatType.Speed, debug.startLevels.speed);
            Stats.SetLevel(StatType.Health, debug.startLevels.health);
            foreach (LeafType type in System.Enum.GetValues(typeof(LeafType)))
                if (debug.startInventory.Get(type) > 0)
                    Inventory.SetCount(type, debug.startInventory.Get(type));
        }

        internal void AddKill() => Kills++;

        /// <summary>(Güç−1)+(Hız−1)+(Sağlık−1) → kademe (SRS 3.9). Seviye değişince CharacterStats çağırır.</summary>
        internal void RecalculateThreat()
        {
            int score = Stats.Level(StatType.Power) + Stats.Level(StatType.Speed) + Stats.Level(StatType.Health) - 3;
            var threat = Services.Config.threat;
            var tier = score >= threat.largeMinScore ? PileTier.Large
                : score >= threat.mediumMinScore ? PileTier.Medium
                : score >= threat.smallMinScore ? PileTier.Small
                : PileTier.None;
            if (score == ThreatScore && tier == PileTier) return;
            ThreatScore = score;
            PileTier = tier;
            GameEvents.RaiseThreatChanged(this, score, tier);
        }

        // Ölen karakter haritadan kalkar (FR-80): çarpışmaz, hareket etmez. Görseli IsAlive'a bakarak gizlenir.
        internal void OnDied()
        {
            CurrentIntent = default;
            if (_body != null) _body.simulated = false;
            foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetIds() => _nextId = 0;
    }
}
