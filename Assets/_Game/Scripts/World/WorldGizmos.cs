using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LeafBrawl.World
{
    /// <summary>
    /// Editör debug çizimi (Gizmos; Okan görev dosyası "Debug"): Play sırasında Scene görünümünde
    /// ağaç canları, yaprak düşme yarıçapı, çalılık sınırları ve güvenli alan yarıçapı.
    /// MapGenerator Awake'te kendisi ekler. Oyun mantığına dokunmaz, yalnızca okur.
    /// </summary>
    public class WorldGizmos : MonoBehaviour
    {
        static readonly Color DropColor = new Color(1f, 0.85f, 0.2f, 0.35f);
        static readonly Color BushColor = new Color(0.2f, 0.9f, 0.4f, 0.9f);
        static readonly Color ZoneColor = new Color(1f, 0.3f, 0.2f, 0.9f);

        MapGenerator _map;

        void Awake() => _map = GetComponent<MapGenerator>();

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            DrawTrees();
            DrawBushes();
            DrawZone();
            DrawCharacters();
        }

        // Karakter canı (Character.Health, sözleşmedeki herkese açık yüz). Yalnızca okunur.
        void DrawCharacters()
        {
#if UNITY_EDITOR
            foreach (Character c in Services.Match.Characters)
            {
                if (c == null || !c.IsAlive) continue;
                Handles.Label(c.Position + Vector2.up * 1.1f, c.name + "  " + c.Health.Current + " / " + c.Health.Max);
            }
#endif
        }

        // Her ağacın yaprak düşme yarıçapı; kesilebilir ağaçta kalan can da yazılır.
        void DrawTrees()
        {
            float dropRadius = Units.ToUnits(Services.Config.trees.dropRadiusPx);

            Gizmos.color = DropColor;
            foreach (ChoppableTree tree in GetComponentsInChildren<ChoppableTree>())
            {
                if (tree.Felled) continue;
                Gizmos.DrawWireSphere(tree.Position, dropRadius);
#if UNITY_EDITOR
                Handles.Label(tree.Position + Vector2.up * 0.6f, tree.Hp + " / " + Services.Config.trees.choppableHp);
#endif
            }

            GreatTree great = GetComponentInChildren<GreatTree>();
            if (great != null) Gizmos.DrawWireSphere(great.Position, dropRadius);
        }

        void DrawBushes()
        {
            if (_map == null || _map.Layout == null) return;

            Gizmos.color = BushColor;
            foreach (MapItem item in _map.Layout.Items)
            {
                if (!item.IsBush) continue;
                Gizmos.DrawWireSphere(
                    new Vector3(Units.ToUnits(item.PosPx.x), Units.ToUnits(item.PosPx.y), 0f),
                    Units.ToUnits(item.RadiusPx));
            }
        }

        void DrawZone()
        {
            ISafeZone zone = Services.Zone;
            Gizmos.color = ZoneColor;
            Gizmos.DrawWireSphere(zone.Center, Units.ToUnits(zone.RadiusPx));
        }
    }
}
