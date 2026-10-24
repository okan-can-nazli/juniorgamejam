using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Ölüm ganimeti (SRS 3.10, FR-81). Map.prefab üzerinde. Died olayında kurbanın envanterinden
    /// K, S, Y'nin loot.rygRatio'sunu (aşağı yuvarla) ve T'nin loot.orangeRatio'sunu (yukarı yuvarla)
    /// öldüğü noktaya tek bir LootPile olarak bırakır. Kalan yapraklar yok olur. Toplam 0 ise yığın olmaz.
    /// </summary>
    public class LootSystem : MonoBehaviour
    {
        // Yuvarlama payı: float 0,35 gibi sayıları tam tutmaz (20 × 0,35 = 6,9999 çıkabilir). Oyun değeri değil.
        const float RoundGuard = 0.0001f;

        Transform _piles;
        Rng _rng;   // ganimet noktası akışı: ölüm zamanlaması Services.Rng'yi kaydırmasın (NFR-07)

        void OnEnable() => GameEvents.Died += OnDied;
        void OnDisable() => GameEvents.Died -= OnDied;

        void Start()
        {
            _piles = new GameObject("Ganimetler").transform;
            _piles.SetParent(transform, false);

            // Tohumdan türetilir, Services.Rng'den çekiliş yapmaz: Start sırasından bağımsız, aynı tohum aynı akış.
            _rng = new Rng(unchecked(Services.Rng.Seed + 1));
        }

        void OnDied(Character victim, Character killer)
        {
            var loot = Services.Config.loot;
            var counts = new int[4];   // LeafType sırasıyla: Red, Yellow, Green, Orange
            int total = 0;

            for (int i = 0; i < counts.Length; i++)
            {
                var type = (LeafType)i;
                int have = victim.Inventory.Count(type);
                counts[i] = type == LeafType.Orange
                    ? Mathf.CeilToInt(have * loot.orangeRatio - RoundGuard)
                    : Mathf.FloorToInt(have * loot.rygRatio + RoundGuard);
                total += counts[i];
            }

            if (total == 0) return;

            Vector2 position = FindDropPoint(victim.Position);
            LootPile.Create(_piles, position, counts);
            GameEvents.RaiseLootDropped(position, total);
        }

        // Öldüğü nokta geçerliyse orası; değilse çevresinde geçerli bir nokta (FR-34).
        // Arama yarıçapı her denemede karakter yarıçapı kadar büyür.
        Vector2 FindDropPoint(Vector2 position)
        {
            float clearPx = Services.Config.character.radiusPx;
            if (!Services.World.IsBlocked(position, clearPx)) return position;

            for (int i = 1; i <= Services.Config.leaves.maxDropTries; i++)
            {
                Vector2 candidate = position + _rng.InsideCircle(Units.ToUnits(clearPx * i));
                if (!Services.World.IsBlocked(candidate, clearPx)) return candidate;
            }
            return position;
        }
    }
}
