using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Characters
{
    /// <summary>
    /// Can, hasar, ölüm ve can yenilenmesi (SRS 3.2, FR-24, FR-25, FR-84).
    /// Hasar veren herkes yalnızca Character.ApplyDamage çağırır.
    /// </summary>
    public class Health : MonoBehaviour
    {
        Character _character;
        IDamageInterceptor[] _interceptors;
        float _regenTimer;

        public int Current { get; private set; }
        public int Max => _character.Stats.MaxHp;
        public bool IsAlive { get; private set; } = true;
        /// <summary>Son hasarın Time.time değeri; hiç hasar almadıysa -sonsuz.</summary>
        public float LastDamageTime { get; private set; } = float.NegativeInfinity;

        // Sıralama için (FR-83): ölümden önceki can, ölüm anı ve katil.
        public int HpBeforeDeath { get; private set; }
        public float DeathTime { get; private set; } = -1f;
        public int DeathFrame { get; private set; } = -1;
        public Character Killer { get; private set; }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            int before = Current;
            Current = Mathf.Min(Max, Current + amount);
            if (Current > before) GameEvents.RaiseHealed(_character, Current - before);
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (!IsAlive || info.Amount <= 0) return;

            // Önce kalkan gibi emiciler (Hamza'nın AbilityController'ı çocuklarda durur).
            _interceptors ??= GetComponentsInChildren<IDamageInterceptor>(true);
            foreach (var interceptor in _interceptors)
                if (interceptor.TryAbsorb(info)) return;

            int before = Current;
            Current = Mathf.Max(0, Current - info.Amount);
            LastDamageTime = Time.time;
            _regenTimer = 0f;
            _character.Crafting.Cancel();   // FR-44: hasar üretimi iptal eder
            GameEvents.RaiseDamaged(_character, info);

            if (Current == 0) Die(info, before);
        }

        void Die(DamageInfo info, int hpBefore)
        {
            IsAlive = false;
            HpBeforeDeath = hpBefore;
            DeathTime = Services.Match.MatchTime;
            DeathFrame = Time.frameCount;
            // Son vuruşu yapan katildir; alan hasarında katil yok (FR-84).
            Killer = info.IsZone ? null : info.Source;
            if (Killer != null && Killer != _character) Killer.AddKill();
            _character.OnDied();
            GameEvents.RaiseDied(_character, Killer);
        }

        /// <summary>Sağlık seviyesi değişince CharacterStats çağırır: artış mevcut cana da eklenir (SRS 3.1).</summary>
        internal void OnMaxHpChanged(int oldMax, int newMax)
        {
            if (!IsAlive) return;
            Current = Mathf.Clamp(Current + Mathf.Max(0, newMax - oldMax), 0, newMax);
        }

        void Awake()
        {
            _character = GetComponent<Character>();
            Current = _character.GetComponent<CharacterStats>().MaxHp;
        }

        // 5 sn hasar almayan karakter Sağlık seviyesine göre yenilenir (FR-25).
        void Update()
        {
            if (!IsAlive || Services.Match.State != MatchState.Playing) return;
            if (Current >= Max || Time.time - LastDamageTime < Services.Config.regen.delayAfterDamageSec)
            {
                _regenTimer = 0f;
                return;
            }
            _regenTimer += Time.deltaTime;
            float secondsPerHp = _character.Stats.RegenSecondsPerHp;
            while (_regenTimer >= secondsPerHp && Current < Max)
            {
                _regenTimer -= secondsPerHp;
                Heal(1);
            }
        }
    }
}
