using UnityEngine;

namespace LeafBrawl.Shared.Stubs
{
    /// <summary>SafeZone yokken: her yer alan içi, hasar 0.</summary>
    public class StubZone : ISafeZone
    {
        public static readonly StubZone Instance = new StubZone();

        public Vector2 Center => Vector2.zero;
        public float RadiusPx => 100000f;
        public float NextRadiusPx => 100000f;
        public float SecondsToNextShrink => float.PositiveInfinity;
        public float DamagePerSecond => 0f;
        public bool IsInside(Vector2 pos) => true;
        public bool WillBeOutsideWithin(Vector2 pos, float seconds) => false;
    }
}
