using System.Collections.Generic;
using LeafBrawl.Shared;
using LeafBrawl.Shared.Stubs;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Dünya servisi (IWorld): toplanabilirler, ağaçlar, doğma noktaları, engeller. Map.prefab üzerinde.
    /// Hiçbir şeyi kendi listesinde tutmaz; katmanlara göre fizik sorgusu yapar:
    /// Pickup katmanı (yaprak, ganimet), Tree katmanı (ağaçların "Vurus" collider'ı), Obstacle katmanı (gövde, kaya, sınır).
    /// MapGenerator ve ağaç/yaprak script'lerinden önce hazır olsun diye en erken çalışır.
    /// Konumlar Unity birimidir, yarıçaplar px (sözleşme).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class WorldService : MonoBehaviour, IWorld
    {
        MapGenerator _map;
        ITree _greatTree;
        int _pickupMask, _treeMask, _obstacleMask;

        void Awake()
        {
            _map = GetComponent<MapGenerator>();
            _pickupMask = LayerMask.GetMask("Pickup");
            _treeMask = LayerMask.GetMask("Tree");
            _obstacleMask = LayerMask.GetMask("Obstacle");
            Services.World = this;
        }

        /// <summary>Haritadaki doğma noktaları (Unity birimi). MapGenerator yoksa veya kurmadıysa stub'ın SRS düzeni.</summary>
        public IReadOnlyList<Vector2> SpawnPoints
        {
            get
            {
                if (_map != null && _map.SpawnPoints.Count > 0) return _map.SpawnPoints;
                return StubWorld.Instance.SpawnPoints;
            }
        }

        /// <summary>Ulu ağaç; üzerinde ITree bileşeni olan "UluAgac" nesnesi kurulana kadar null.</summary>
        public ITree GreatTree
        {
            get
            {
                if (_greatTree == null)
                {
                    Transform t = transform.Find("UluAgac");
                    if (t != null) _greatTree = t.GetComponent<ITree>();
                }
                return _greatTree;
            }
        }

        public IReadOnlyList<IPickup> PickupsNear(Vector2 pos, float radiusPx)
        {
            float r = Units.ToUnits(radiusPx);
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, r, _pickupMask);
            var result = new List<IPickup>(hits.Length);
            foreach (Collider2D hit in hits)
            {
                IPickup pickup = hit.GetComponentInParent<IPickup>();
                if (pickup != null && Vector2.Distance(pickup.Position, pos) <= r) result.Add(pickup);
            }
            return result;
        }

        /// <summary>Yalnızca kesilmemiş ağaçlar. Kütüğün Vurus collider'ı kapalı olacağı için zaten sorguya girmez; Felled yine de kontrol edilir.</summary>
        public IReadOnlyList<ITree> TreesNear(Vector2 pos, float radiusPx)
        {
            float r = Units.ToUnits(radiusPx);
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, r, _treeMask);
            var result = new List<ITree>(hits.Length);
            foreach (Collider2D hit in hits)
            {
                ITree tree = hit.GetComponentInParent<ITree>();
                if (tree != null && !tree.Felled && Vector2.Distance(tree.Position, pos) <= r) result.Add(tree);
            }
            return result;
        }

        /// <summary>Gövde, kaya veya harita dışı mı (yaprak ve ganimet düşme noktası, bot yol bulma için).</summary>
        public bool IsBlocked(Vector2 pos, float radiusPx)
        {
            float r = Units.ToUnits(radiusPx);
            float half = Units.ToUnits(Services.Config.map.sizePx) * 0.5f;
            if (Mathf.Abs(pos.x) + r > half || Mathf.Abs(pos.y) + r > half) return true;
            return Physics2D.OverlapCircle(pos, r, _obstacleMask) != null;
        }
    }
}
