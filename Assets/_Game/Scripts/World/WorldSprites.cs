using System;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Dünya görselleri (Furkan, Art/Sprites). Map.prefab'taki MapGenerator'da atanır; MapGenerator.Sprites ile okunur.
    /// Sprite'lar PPU 16'dır ve ölçeklenmeden çizilir (SRS 3.13). Bir alan boşsa kod yer tutucu şekli çizer.
    /// Birden çok çeşidi olan öğede çeşit, öğenin sırasına göre seçilir (Rng akışını kaydırmasın, NFR-07).
    /// </summary>
    [Serializable]
    public class WorldSprites
    {
        public Sprite border;           // harita sınırının dışı, döşenir
        public Sprite[] trunks;         // küme ve koru ağacı gövdesi
        public Sprite[] canopies;       // küme ve koru ağacı tacı
        public Sprite stump;            // kesilmiş ağaç (kütük)
        public Sprite greatTrunk;
        public Sprite greatCanopy;
        public Sprite[] rocks;
        public Sprite[] outerBushes;
        public Sprite[] innerBushes;
        public Sprite[] leaves;         // LeafType sırasıyla: K, S, Y, T
        public Sprite[] loot;           // ekranda 1, 2, 3 yaprak (SRS 3.10)
        public Sprite[] zoneClouds;     // alan sınırındaki yağmur bulutu, animasyon kareleri

        public static Sprite Pick(Sprite[] sprites, int index)
        {
            if (sprites == null || sprites.Length == 0) return null;
            return sprites[Mathf.Abs(index) % sprites.Length];
        }

        public Sprite Leaf(LeafType type) => leaves != null && (int)type < leaves.Length ? leaves[(int)type] : null;
    }
}
