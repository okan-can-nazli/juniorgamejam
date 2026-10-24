using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Ulu ağaç (SRS 3.5, FR-35): kesilemez. Vuruşta yalnızca GreatTreeHit yayınlar, can düşmez.
    /// MapGenerator "UluAgac" nesnesine takar.
    /// </summary>
    public class GreatTree : MonoBehaviour, ITree
    {
        public Vector2 Position => transform.position;
        public bool IsAlive => true;
        public bool Choppable => false;
        public bool Felled => false;

        public void ApplyDamage(DamageInfo info)
        {
            GameEvents.RaiseGreatTreeHit(info.Source);
        }
    }
}
