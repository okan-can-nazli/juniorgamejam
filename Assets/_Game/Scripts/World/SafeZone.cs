using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Güvenli alan (ISafeZone; SRS 3.8, FR-70, FR-72). SafeZone.prefab üzerinde.
    /// - Yarıçap, Services.Match.MatchTime'a göre zone.phases tablosundan doğrusal hesaplanır (saat geri sayım bitince 0'dan başlar).
    /// - Her zone.tickSec'te alan dışındaki canlı karakterlere ApplyDamage(IsZone = true) uygulanır.
    /// - zone.suddenDeathStartSec'ten sonra yarıçap 0, hasar suddenDeathDamagePerSec.
    /// - Aşama numarası: zone.phases dizisinin indeksi; ani ölüm = phases.Length. Değişince ZonePhaseChanged.
    /// Konumlar Unity birimi, yarıçaplar px (sözleşme). Görsel SafeZoneView'da.
    /// </summary>
    public class SafeZone : MonoBehaviour, ISafeZone
    {
        int _phase;
        float _tickLeft;
        float _carry;   // tick hasarının tam sayıya sığmayan kalanı (ör. 7/sn → 3,5)

        public Vector2 Center => transform.position;
        public float RadiusPx => RadiusAt(Now);
        public float DamagePerSecond => DamageAt(Now);

        /// <summary>
        /// Alanın varacağı bir sonraki yarıçap: daralma sürüyorsa bu daralmanın bitiş yarıçapı,
        /// değilse bir sonraki daralan aşamanın bitiş yarıçapı; kalmadıysa son yarıçap (#13).
        /// </summary>
        public float NextRadiusPx
        {
            get
            {
                int next = ShrinkingIndex(Now);
                if (next < 0) next = NextShrinkIndex(Now);
                return next < 0 ? RadiusAt(float.MaxValue) : Services.Config.zone.phases[next].endRadiusPx;
            }
        }

        /// <summary>Bir sonraki daralmanın başlamasına kalan süre; kalmadıysa sonsuz.</summary>
        public float SecondsToNextShrink
        {
            get
            {
                int next = NextShrinkIndex(Now);
                return next < 0 ? float.PositiveInfinity : Services.Config.zone.phases[next].startSec - Now;
            }
        }

        static float Now => Services.Match.MatchTime;

        void Awake()
        {
            Services.Zone = this;
            _tickLeft = Services.Config.zone.tickSec;
            if (GetComponent<SafeZoneView>() == null) gameObject.AddComponent<SafeZoneView>();
        }

        void Update()
        {
            if (Services.Match.State != MatchState.Playing) return;   // geri sayım, duraklatma, maç sonu

            TrackPhase();

            _tickLeft -= Time.deltaTime;
            if (_tickLeft > 0f) return;
            _tickLeft += Services.Config.zone.tickSec;
            DamageOutside();
        }

        // ---------------------------------------------------------------- ISafeZone

        public bool IsInside(Vector2 pos) => Inside(pos, RadiusAt(Now));

        // Yarıçap yalnızca küçülür; bu yüzden pozisyon seconds sonra dışarıdaysa o ana kadar da bir noktada dışarıya düşmüştür.
        public bool WillBeOutsideWithin(Vector2 pos, float seconds) => !Inside(pos, RadiusAt(Now + seconds));

        bool Inside(Vector2 pos, float radiusPx)
        {
            float r = Units.ToUnits(radiusPx);
            return (pos - Center).sqrMagnitude <= r * r;
        }

        // ---------------------------------------------------------------- aşama tablosu

        static float RadiusAt(float t)
        {
            ZoneConfig zone = Services.Config.zone;
            if (t >= zone.suddenDeathStartSec) return 0f;

            foreach (ZonePhase p in zone.phases)
                if (t < p.endSec)
                    return Mathf.Lerp(p.startRadiusPx, p.endRadiusPx, Mathf.InverseLerp(p.startSec, p.endSec, t));

            return zone.phases[zone.phases.Length - 1].endRadiusPx;
        }

        static int PhaseAt(float t)
        {
            ZoneConfig zone = Services.Config.zone;
            if (t >= zone.suddenDeathStartSec) return zone.phases.Length;

            for (int i = 0; i < zone.phases.Length; i++)
                if (t < zone.phases[i].endSec) return i;

            return zone.phases.Length - 1;
        }

        static float DamageAt(float t)
        {
            ZoneConfig zone = Services.Config.zone;
            int phase = PhaseAt(t);
            return phase >= zone.phases.Length ? zone.suddenDeathDamagePerSec : zone.phases[phase].damagePerSec;
        }

        // t anında süren daralan aşama; yoksa -1.
        static int ShrinkingIndex(float t)
        {
            ZonePhase[] phases = Services.Config.zone.phases;
            for (int i = 0; i < phases.Length; i++)
                if (phases[i].startSec <= t && t < phases[i].endSec && phases[i].endRadiusPx < phases[i].startRadiusPx) return i;
            return -1;
        }

        // t'den sonra başlayan ilk daralan aşama (yarıçapı küçülen); yoksa -1.
        static int NextShrinkIndex(float t)
        {
            ZonePhase[] phases = Services.Config.zone.phases;
            for (int i = 0; i < phases.Length; i++)
                if (phases[i].startSec > t && phases[i].endRadiusPx < phases[i].startRadiusPx) return i;
            return -1;
        }

        // ---------------------------------------------------------------- aşama ve hasar

        void TrackPhase()
        {
            int phase = PhaseAt(Now);
            if (phase == _phase) return;
            _phase = phase;
            GameEvents.RaiseZonePhaseChanged(phase);
        }

        // FR-72: her tick'te saniyelik hasarın yarısı (dps × tickSec). Hasar tam sayı olmalı, kalan _carry'de birikir.
        void DamageOutside()
        {
            float dps = DamageAt(Now);
            if (dps <= 0f)
            {
                _carry = 0f;
                return;
            }

            _carry += dps * Services.Config.zone.tickSec;
            int amount = Mathf.FloorToInt(_carry);
            _carry -= amount;
            if (amount <= 0) return;

            foreach (Character c in Services.Match.Characters)
            {
                if (!c.IsAlive || IsInside(c.Position)) continue;
                c.ApplyDamage(new DamageInfo { Amount = amount, Source = null, IsZone = true });
            }
        }
    }
}
