using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Combat
{
    /// <summary>
    /// 5. seviye yetenekleri (SRS 3.4, FR-26–FR-29). CharacterCombat.prefab üzerinde, karakterin çocuğunda durur.
    /// Hız 5 → Atılma (tekrar kullanılır), Sağlık 5 → Kalkan (maçta 1 kez), Güç 5 → Ulti (maçta 1 kez).
    /// Kalkan IDamageInterceptor olarak Health tarafından sorulur.
    /// </summary>
    public class AbilityController : MonoBehaviour, IDamageInterceptor
    {
        Character _self;
        readonly bool[] _unlocked = new bool[3];
        bool _shieldUsed, _ultiUsed;
        float _dashReadyTime = float.NegativeInfinity;
        float _shieldEndTime = float.NegativeInfinity;
        float _ultiFireTime;

        /// <summary>Kalkan açık mı (FR-27: karakterin etrafında halka olarak görünür).</summary>
        public bool ShieldActive { get; private set; }

        /// <summary>Ulti hazırlanıyor mu (0,4 sn; karakter parlar, hareket edemez, savuramaz).</summary>
        public bool UltiWindingUp { get; private set; }

        public AbilityState State(AbilityId ability)
        {
            if (!_unlocked[(int)ability]) return AbilityState.Locked;
            return ability switch
            {
                AbilityId.Dash => Time.time < _dashReadyTime ? AbilityState.Cooldown : AbilityState.Ready,
                AbilityId.Shield => _shieldUsed ? AbilityState.Used : AbilityState.Ready,
                _ => _ultiUsed ? AbilityState.Used : AbilityState.Ready,
            };
        }

        /// <summary>Kalan bekleme süresi (sn). Yalnızca atılmanın beklemesi vardır; kalkan ve ulti tek seferliktir.</summary>
        public float CooldownLeft(AbilityId ability)
        {
            if (ability != AbilityId.Dash || !_unlocked[(int)ability]) return 0f;
            return Mathf.Max(0f, _dashReadyTime - Time.time);
        }

        void Awake() => _self = GetComponentInParent<Character>();
        void OnEnable() => GameEvents.LevelChanged += OnLevelChanged;
        void OnDisable() => GameEvents.LevelChanged -= OnLevelChanged;

        // Başlangıç seviyesi zaten 5 olabilir (debug.json); olay kaçırılmasın diye bir kez de burada bakılır.
        void Start()
        {
            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
                OnLevelChanged(_self, stat, _self.Stats.Level(stat));
        }

        void OnLevelChanged(Character character, StatType stat, int newLevel)
        {
            if (character != _self || newLevel < Services.Config.abilities.unlockLevel) return;
            var ability = stat switch
            {
                StatType.Speed => AbilityId.Dash,
                StatType.Health => AbilityId.Shield,
                _ => AbilityId.Ulti,
            };
            if (_unlocked[(int)ability]) return;
            _unlocked[(int)ability] = true;
            GameEvents.RaiseAbilityUnlocked(_self, ability);
        }

        void Update()
        {
            if (!_self.IsAlive)
            {
                ShieldActive = false;
                UltiWindingUp = false;
                return;
            }

            if (ShieldActive && Time.time >= _shieldEndTime) ShieldActive = false;   // süre doldu, kırılmadan kapanır
            if (UltiWindingUp && Time.time >= _ultiFireTime) FireUlti();

            // FR-29: üretim sırasında ve ulti hazırlanırken yetenek yok. Geri sayımda intent zaten sıfırdır.
            if (_self.Crafting.IsCrafting || UltiWindingUp) return;
            var intent = _self.CurrentIntent;
            if (intent.Dash) TryDash(intent);
            if (intent.Shield) TryShield();
            if (intent.Ulti) TryUlti();
        }

        // FR-26: hareket yönünde (hareket yoksa baktığı yöne) 0,15 sn'de 160 px; 3 sn bekleme.
        void TryDash(CharacterIntent intent)
        {
            if (State(AbilityId.Dash) != AbilityState.Ready) return;
            var abilities = Services.Config.abilities;
            Vector2 direction = intent.Move.sqrMagnitude > 0.0001f ? intent.Move : _self.Facing;
            _self.Motor.Dash(direction, abilities.dashDistancePx, abilities.dashDurationSec);
            _dashReadyTime = Time.time + abilities.dashCooldownSec;
            GameEvents.RaiseAbilityUsed(_self, AbilityId.Dash);
        }

        // FR-27: maçta bir kez, 6 sn açık; gelen ilk vuruşu emer.
        void TryShield()
        {
            if (State(AbilityId.Shield) != AbilityState.Ready) return;
            _shieldUsed = true;
            ShieldActive = true;
            _shieldEndTime = Time.time + Services.Config.abilities.shieldDurationSec;
            GameEvents.RaiseAbilityUsed(_self, AbilityId.Shield);
        }

        // FR-28: maçta bir kez. Önce hazırlanma (AbilityUsed burada yayınlanır, görsel için), sonra patlama.
        void TryUlti()
        {
            if (State(AbilityId.Ulti) != AbilityState.Ready) return;
            _ultiUsed = true;
            UltiWindingUp = true;
            _ultiFireTime = Time.time + Services.Config.abilities.ultiWindupSec;
            GameEvents.RaiseAbilityUsed(_self, AbilityId.Ulti);
        }

        // 120 px içindeki bütün rakipler canına bakılmaksızın ölür, bütün kesilebilir ağaçlar kesilir (SRS 3.4).
        void FireUlti()
        {
            UltiWindingUp = false;
            float radiusPx = Services.Config.abilities.ultiRadiusPx;
            float radius = Units.ToUnits(radiusPx);
            Vector2 origin = _self.Position;

            foreach (var target in Services.Match.Characters)
            {
                if (target == null || target == _self || !target.IsAlive) continue;
                if ((target.Position - origin).sqrMagnitude > radius * radius) continue;
                // Hasar = hedefin kalan canı: tam öldürür ve ekrandaki hasar sayısı anlamlı kalır.
                target.ApplyDamage(new DamageInfo { Amount = Mathf.Max(1, target.Health.Current), Source = _self, IsUlti = true });
            }

            foreach (var tree in Services.World.TreesNear(origin, radiusPx))
            {
                if (!tree.Choppable || tree.Felled) continue;
                tree.ApplyDamage(new DamageInfo { Amount = int.MaxValue, Source = _self, IsUlti = true });
            }
        }

        // Kalkan açıkken rakipten gelen ilk vuruşu (balta veya ulti) emer ve kırılır. Alan hasarını emmez.
        public bool TryAbsorb(DamageInfo info)
        {
            if (!ShieldActive || info.IsZone || info.Source == null) return false;
            ShieldActive = false;
            GameEvents.RaiseShieldBroken(_self);
            return true;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (_self == null || !Application.isPlaying || State(AbilityId.Ulti) == AbilityState.Locked) return;
            if (State(AbilityId.Ulti) == AbilityState.Used && !UltiWindingUp) return;
            Gizmos.color = UltiWindingUp ? new Color(1f, 0.85f, 0.2f, 0.9f) : new Color(1f, 0.85f, 0.2f, 0.3f);
            const int segments = 32;
            float radius = Units.ToUnits(Services.Config.abilities.ultiRadiusPx);
            Vector3 center = _self.Position;
            Vector3 previous = center + Vector3.right * radius;
            for (int i = 1; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                Vector3 next = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
#endif
    }
}
