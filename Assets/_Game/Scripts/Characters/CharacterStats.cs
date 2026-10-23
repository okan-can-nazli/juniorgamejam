using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Characters
{
    /// <summary>Güç, Hız, Sağlık seviyeleri ve tablo değerleri (SRS 3.1). Değerler formülle değil tablodan okunur.</summary>
    public class CharacterStats : MonoBehaviour
    {
        readonly int[] _levels = { 1, 1, 1 };
        Character _character;

        public int MaxLevel => Services.Config.levels.maxLevel;
        public int Level(StatType stat) => _levels[(int)stat];

        public int Damage => Services.Config.levels.damage[Level(StatType.Power) - 1];
        public float SpeedPx => Services.Config.levels.speedPx[Level(StatType.Speed) - 1];
        public int MaxHp => Services.Config.levels.maxHp[Level(StatType.Health) - 1];
        /// <summary>Kaç saniyede 1 HP yenilenir (SRS 3.1).</summary>
        public float RegenSecondsPerHp => Services.Config.regen.secondsPerHp[Level(StatType.Health) - 1];

        /// <summary>
        /// Seviyeyi değiştirir ve LevelChanged yayınlar. Oyunda tarif çağırır; başka yerden yalnızca debug ve test için.
        /// Sağlık artınca mevcut can da aynı miktarda artar.
        /// </summary>
        public void SetLevel(StatType stat, int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);
            if (_levels[(int)stat] == level) return;
            int oldMaxHp = MaxHp;
            _levels[(int)stat] = level;
            if (stat == StatType.Health) _character.Health.OnMaxHpChanged(oldMaxHp, MaxHp);
            GameEvents.RaiseLevelChanged(_character, stat, level);
            _character.RecalculateThreat();
        }

        void Awake() => _character = GetComponent<Character>();
    }
}
