using LeafBrawl.Characters;
using UnityEngine;

namespace LeafBrawl.Shared
{
    public struct DamageInfo
    {
        public int Amount;
        public Character Source;   // alan hasarında null
        public bool IsUlti;
        public bool IsZone;
    }

    public interface IDamageable
    {
        Vector2 Position { get; }
        bool IsAlive { get; }
        void ApplyDamage(DamageInfo info);
    }

    /// <summary>Karakterin çocuklarında durur (Kalkan). true: hasar tamamen emildi.</summary>
    public interface IDamageInterceptor
    {
        bool TryAbsorb(DamageInfo info);
    }

    public interface ITree : IDamageable
    {
        bool Choppable { get; }
        bool Felled { get; }
    }

    public interface IPickup
    {
        Vector2 Position { get; }
        bool IsLoot { get; }
        /// <summary>Yaprağın türü. Ganimet yığını (IsLoot) birden çok tür taşır; orada anlamı yoktur, yok sayılır.</summary>
        LeafType Type { get; }
    }
}
