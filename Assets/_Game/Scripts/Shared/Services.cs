using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Core;
using LeafBrawl.Shared.Stubs;
using UnityEngine;

namespace LeafBrawl.Shared
{
    /// <summary>
    /// Servis bulucu. Sistemler kendini Awake'te kaydeder (ör. <c>Services.Zone = this;</c>).
    /// Kayıtlı değilse Stubs/ altındaki sahte sürüm döner, böylece her sahne tek başına çalışır.
    /// </summary>
    public static class Services
    {
        static Balance _config;
        static DebugSettings _debugConfig;
        static Rng _rng;
        static IMatch _match;
        static ISafeZone _zone;
        static IVisibility _visibility;
        static IWorld _world;

        /// <summary>balance.json (Yiğit, ConfigLoader). Sahnede ConfigLoader yoksa ilk erişimde yüklenir.</summary>
        public static Balance Config
        {
            get => _config ??= ConfigLoader.LoadBalance();
            set => _config = value;
        }

        /// <summary>debug.json + debug.local.json (Yiğit, ConfigLoader).</summary>
        public static DebugSettings DebugConfig
        {
            get => _debugConfig ??= ConfigLoader.LoadDebug();
            set => _debugConfig = value;
        }

        /// <summary>Tek tohumdan rastgelelik (NFR-07). UnityEngine.Random kullanılmaz.</summary>
        public static Rng Rng
        {
            get => _rng ??= new Rng(DebugConfig.seed);
            set => _rng = value;
        }

        /// <summary>Yiğit (MatchManager). Stub: hep Playing, sahnedeki karakterler.</summary>
        public static IMatch Match
        {
            get => IsSet(_match) ? _match : StubMatch.Instance;
            set => _match = value;
        }

        /// <summary>Okan (SafeZone). Stub: her yer alan içi, hasar 0.</summary>
        public static ISafeZone Zone
        {
            get => IsSet(_zone) ? _zone : StubZone.Instance;
            set => _zone = value;
        }

        /// <summary>Okan (VisibilityService). Stub: herkes herkesi görür, çalılık yok.</summary>
        public static IVisibility Visibility
        {
            get => IsSet(_visibility) ? _visibility : StubVisibility.Instance;
            set => _visibility = value;
        }

        /// <summary>Okan (WorldService). Stub: boş dünya, 4 doğma noktası.</summary>
        public static IWorld World
        {
            get => IsSet(_world) ? _world : StubWorld.Instance;
            set => _world = value;
        }

        // Kaydı yapan MonoBehaviour yok edildiyse (sahne değişti) stub'a düş.
        static bool IsSet(object service) => service is Object unityObject ? unityObject != null : service != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay()
        {
            _config = null; _debugConfig = null; _rng = null;
            _match = null; _zone = null; _visibility = null; _world = null;
        }
    }

    public interface IMatch
    {
        MatchState State { get; }
        float MatchTime { get; }                                   // geri sayım bitince 0'dan başlar
        IReadOnlyList<Character> Characters { get; }
        Character Player { get; }
        MatchResult Result { get; }                                // Ended olunca dolu
        void Pause();
        void Resume();
        void Restart();
    }

    public interface ISafeZone
    {
        Vector2 Center { get; }
        float RadiusPx { get; }
        float NextRadiusPx { get; }
        float SecondsToNextShrink { get; }
        float DamagePerSecond { get; }
        bool IsInside(Vector2 pos);
        bool WillBeOutsideWithin(Vector2 pos, float seconds);
    }

    public interface IVisibility
    {
        bool CanSee(Character viewer, Character target);           // çalılık + 1 sn görünme kuralları
        bool IsInBush(Character c);
        Vector2? NearestBush(Vector2 from);
    }

    public interface IWorld
    {
        IReadOnlyList<IPickup> PickupsNear(Vector2 pos, float radiusPx);
        IReadOnlyList<ITree> TreesNear(Vector2 pos, float radiusPx);  // yalnızca kesilmemiş ağaçlar
        ITree GreatTree { get; }
        IReadOnlyList<Vector2> SpawnPoints { get; }
        bool IsBlocked(Vector2 pos, float radiusPx);               // gövde / kaya / harita dışı
    }
}
