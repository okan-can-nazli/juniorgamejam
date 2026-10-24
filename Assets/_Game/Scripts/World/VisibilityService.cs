using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Çalılık ve görüş (IVisibility; SRS 3.11, FR-50, FR-51, BOT-02, BOT-05). Map.prefab üzerinde.
    /// Çalılıklar MapGenerator.Layout'tan okunur (her maç sapmalı konumlar). Bir karakter, merkezi
    /// çalılığın yarıçapı içindeyse o çalılıktadır; bu fizik olayıyla değil her karede mesafeyle bulunur.
    /// Konumlar Unity birimi (sözleşme).
    /// </summary>
    public class VisibilityService : MonoBehaviour, IVisibility
    {
        struct Bush
        {
            public Vector2 Center;   // Unity birimi
            public float Radius;     // Unity birimi
        }

        readonly List<Bush> _bushes = new List<Bush>();
        readonly Dictionary<Character, int> _bushOf = new Dictionary<Character, int>();   // karakter → çalılık sırası, yoksa -1
        MapGenerator _map;

        void Awake()
        {
            _map = GetComponent<MapGenerator>();
            Services.Visibility = this;
        }

        // MapGenerator Awake'te (-100) kurduğu için Layout burada hazır.
        void Start()
        {
            if (_map == null || _map.Layout == null) return;
            foreach (MapItem item in _map.Layout.Items)
            {
                if (!item.IsBush) continue;
                _bushes.Add(new Bush
                {
                    Center = new Vector2(Units.ToUnits(item.PosPx.x), Units.ToUnits(item.PosPx.y)),
                    Radius = Units.ToUnits(item.RadiusPx),
                });
            }
        }

        void Update()
        {
            IMatch match = Services.Match;

            // Önce herkesin çalılığı bulunur, sonra görünürlük hesaplanır (CanSee ikisine de bakar).
            foreach (Character c in match.Characters)
            {
                int bush = c.IsAlive ? FindBush(c.Position) : -1;
                _bushOf[c] = bush;
                c.IsInBush = bush >= 0;
            }

            Character player = match.Player;
            foreach (Character c in match.Characters)
                c.VisibleToPlayer = player == null || CanSee(player, c);
        }

        // ---------------------------------------------------------------- IVisibility

        public bool IsInBush(Character c) => _bushOf.TryGetValue(c, out int bush) && bush >= 0;

        /// <summary>FR-51: hedef çalılıkta değilse görünür; çalılıktaysa yalnızca aynı çalılıktaki izleyene veya görünür süresindeyse.</summary>
        public bool CanSee(Character viewer, Character target)
        {
            if (viewer == target) return true;
            if (!_bushOf.TryGetValue(target, out int targetBush) || targetBush < 0) return true;
            if (target.IsRevealed) return true;
            return _bushOf.TryGetValue(viewer, out int viewerBush) && viewerBush == targetBush;
        }

        /// <summary>BOT-05: en yakın çalılığın merkezi (Unity birimi). Merkezi alan dışında kalan çalılıklar sayılmaz.</summary>
        public Vector2? NearestBush(Vector2 from)
        {
            Vector2? best = null;
            float bestSqr = float.MaxValue;
            foreach (Bush bush in _bushes)
            {
                if (!Services.Zone.IsInside(bush.Center)) continue;
                float sqr = (bush.Center - from).sqrMagnitude;
                if (sqr >= bestSqr) continue;
                bestSqr = sqr;
                best = bush.Center;
            }
            return best;
        }

        // ---------------------------------------------------------------- yardımcı

        // Konumun içinde olduğu çalılığın sırası; yoksa -1. Çalılıklar üst üste binmez (SRS 3.12 kural 3).
        int FindBush(Vector2 pos)
        {
            for (int i = 0; i < _bushes.Count; i++)
            {
                Bush bush = _bushes[i];
                if ((pos - bush.Center).sqrMagnitude <= bush.Radius * bush.Radius) return i;
            }
            return -1;
        }
    }
}
