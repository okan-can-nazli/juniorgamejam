using System.Collections.Generic;
using UnityEngine;

namespace LeafBrawl.Shared
{
    /// <summary>
    /// Tek tohumdan rastgelelik (NFR-07). Herkes <c>Services.Rng</c> kullanır; UnityEngine.Random yasak.
    /// Aynı tohum + aynı çağrı sırası = aynı maç.
    /// </summary>
    public class Rng
    {
        readonly System.Random _random;

        public int Seed { get; }

        /// <param name="seed">0 ise saatten rastgele bir tohum seçilir.</param>
        public Rng(int seed)
        {
            Seed = seed != 0 ? seed : System.Environment.TickCount & int.MaxValue;
            _random = new System.Random(Seed);
        }

        /// <summary>[0, 1)</summary>
        public float Value => (float)_random.NextDouble();

        /// <summary>[min, max)</summary>
        public float Range(float min, float max) => min + (max - min) * Value;

        /// <summary>[min, max) — max dahil değil, UnityEngine.Random.Range(int,int) gibi.</summary>
        public int Range(int min, int max) => max <= min ? min : _random.Next(min, max);

        /// <summary>probability olasılıkla true (0–1).</summary>
        public bool Chance(float probability) => Value < probability;

        public T Pick<T>(IReadOnlyList<T> items) => items[Range(0, items.Count)];

        /// <summary>radius yarıçaplı dairenin içinde düzgün dağılmış bir nokta (birim girdiyle aynı).</summary>
        public Vector2 InsideCircle(float radius)
        {
            float angle = Range(0f, Mathf.PI * 2f);
            float r = radius * Mathf.Sqrt(Value);
            return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        }

        /// <summary>Listeyi yerinde karıştırır (Fisher–Yates).</summary>
        public void Shuffle<T>(IList<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}
