using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Bots
{
    /// <summary>
    /// Botun bir karardaki algısı (BOT-02, BOT-03). Yalnızca perceptionRadiusPx içindeki ve görünen (çalılıkta gizli olmayan)
    /// karakterleri ve yaprakları bilir. Ağaç konumlarını ve güvenli alanı her zaman bilir.
    /// </summary>
    public class BotPerception
    {
        public readonly List<Character> Enemies = new List<Character>();
        public readonly List<IPickup> Pickups = new List<IPickup>();
        public readonly List<ITree> Trees = new List<ITree>();

        /// <summary>Bot alan dışında.</summary>
        public bool OutsideZone { get; private set; }
        /// <summary>"Alan tehlikede": alan dışında veya zoneLookaheadSec içinde dışarıda kalacak (BOT-04).</summary>
        public bool ZoneDanger { get; private set; }

        public void Refresh(Character self)
        {
            var bots = Services.Config.bots;
            float perception = Units.ToUnits(bots.perceptionRadiusPx);
            Vector2 pos = self.Position;

            Enemies.Clear();
            foreach (var other in Services.Match.Characters)
            {
                if (other == null || other == self || !other.IsAlive) continue;
                if ((other.Position - pos).sqrMagnitude > perception * perception) continue;
                if (!Services.Visibility.CanSee(self, other)) continue;   // görünmeyen hiçbir şekilde kullanılmaz
                Enemies.Add(other);
            }

            Pickups.Clear();
            Pickups.AddRange(Services.World.PickupsNear(pos, bots.perceptionRadiusPx));

            // Ağaçların konumu her zaman bilinir; arama 650 px içiyle sınırlı (BOT-07).
            Trees.Clear();
            Trees.AddRange(Services.World.TreesNear(pos, bots.perceptionRadiusPx));

            var zone = Services.Zone;
            OutsideZone = !zone.IsInside(pos);
            ZoneDanger = OutsideZone || zone.WillBeOutsideWithin(pos, bots.zoneLookaheadSec);
        }

        /// <summary>maxDistance (birim) içindeki en yakın görünen rakip; yoksa null.</summary>
        public Character NearestEnemy(Vector2 from, float maxDistance)
        {
            Character best = null;
            float bestDistance = maxDistance * maxDistance;
            foreach (var enemy in Enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                float d = (enemy.Position - from).sqrMagnitude;
                if (d <= bestDistance) { bestDistance = d; best = enemy; }
            }
            return best;
        }
    }
}
