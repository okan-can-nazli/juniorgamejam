using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Combat
{
    /// <summary>STUB (sahibi Hamza): 5. seviye yetenekleri (SRS 3.4, FR-26–FR-29). CharacterCombat.prefab üzerinde.</summary>
    public class AbilityController : MonoBehaviour
    {
        public AbilityState State(AbilityId ability) => AbilityState.Locked;
        public float CooldownLeft(AbilityId ability) => 0f;
    }
}
