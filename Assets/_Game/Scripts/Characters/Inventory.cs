using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Characters
{
    /// <summary>
    /// Yaprak envanteri (SRS 3.6). Toplayan herkes yalnızca TryAdd çağırır.
    /// Ölümde envanter silinmez; Okan ganimeti hesaplarken okur.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        readonly int[] _counts = new int[4];
        Character _character;

        public int Count(LeafType type) => _counts[(int)type];

        public int Cap(LeafType type)
        {
            var inventory = Services.Config.inventory;
            return type switch
            {
                LeafType.Red => inventory.capRed,
                LeafType.Yellow => inventory.capYellow,
                LeafType.Green => inventory.capGreen,
                _ => inventory.capOrange,
            };
        }

        public bool IsFull(LeafType type) => Count(type) >= Cap(type);

        /// <summary>Sınıra sığdığı kadar ekler ve eklenen adedi döner (0 = dolu ya da ölü). LeafCollected yayınlar.</summary>
        public int TryAdd(LeafType type, int count)
        {
            if (count <= 0 || !_character.IsAlive) return 0;
            int added = Mathf.Min(count, Mathf.Max(0, Cap(type) - Count(type)));
            if (added == 0) return 0;
            _counts[(int)type] += added;
            GameEvents.RaiseLeafCollected(_character, type, added);
            return added;
        }

        public bool Has(LeafCost cost)
        {
            return Count(LeafType.Red) >= cost.red && Count(LeafType.Yellow) >= cost.yellow
                && Count(LeafType.Green) >= cost.green && Count(LeafType.Orange) >= cost.orange;
        }

        /// <summary>Eksik adet (0'ın altına inmez). Envanter kartı ve bot için.</summary>
        public int Missing(LeafCost cost, LeafType type) => Mathf.Max(0, cost.Get(type) - Count(type));

        /// <summary>Yalnızca Crafting çağırır: tarif tamamlanınca yaprakları düşer.</summary>
        internal void Spend(LeafCost cost)
        {
            _counts[(int)LeafType.Red] -= cost.red;
            _counts[(int)LeafType.Yellow] -= cost.yellow;
            _counts[(int)LeafType.Green] -= cost.green;
            _counts[(int)LeafType.Orange] -= cost.orange;
        }

        /// <summary>Yalnızca debug ve test: adedi doğrudan yazar, sınırı aşabilir (NFR-05).</summary>
        public void SetCount(LeafType type, int count) => _counts[(int)type] = Mathf.Max(0, count);

        void Awake() => _character = GetComponent<Character>();
    }
}
