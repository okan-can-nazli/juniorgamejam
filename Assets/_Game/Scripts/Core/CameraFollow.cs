using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Core
{
    /// <summary>Kamera oyuncuyu ortalar; oyuncu elenince durur (FR-13). Pixel Perfect Camera ile aynı nesnede durur.</summary>
    public class CameraFollow : MonoBehaviour
    {
        void LateUpdate()
        {
            var player = Services.Match.Player;
            if (player == null || !player.IsAlive) return;
            Vector3 target = player.transform.position;
            transform.position = new Vector3(target.x, target.y, transform.position.z);
        }
    }
}
