using LeafBrawl.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LeafBrawl.Core
{
    /// <summary>
    /// Yalnızca editörde test kısayolları (KT-08). Systems.prefab üzerinde durur; yapıda hiçbir şey yapmaz.
    /// F2: oyuncunun envanterine her türden 10 yaprak (sınıra kadar). F3: oyuncunun bütün seviyeleri +1.
    /// </summary>
    public class DebugHotkeys : MonoBehaviour
    {
        const int LeavesPerPress = 10;

#if UNITY_EDITOR
        void Update()
        {
            var keyboard = Keyboard.current;
            var player = Services.Match.Player;
            if (keyboard == null || player == null || !player.IsAlive) return;

            if (keyboard.f2Key.wasPressedThisFrame)
            {
                foreach (LeafType type in System.Enum.GetValues(typeof(LeafType)))
                    player.Inventory.TryAdd(type, LeavesPerPress);
                Debug.Log("[Debug] F2: envantere 10'ar yaprak eklendi.");
            }

            if (keyboard.f3Key.wasPressedThisFrame)
            {
                foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
                    player.Stats.SetLevel(stat, player.Stats.Level(stat) + 1);
                Debug.Log("[Debug] F3: bütün seviyeler +1.");
            }
        }
#endif
    }
}
