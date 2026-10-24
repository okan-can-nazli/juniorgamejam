using System.Collections.Generic;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    public enum MapItemKind { GreatTree, ClusterTree, GroveTree, Rock, Spawn, OuterBush, InnerBush }

    public struct MapItem
    {
        public MapItemKind Kind;
        public Vector2 PosPx;       // merkeze göre, dünya px
        public float RadiusPx;      // çarpışma / kaplama yarıçapı
        public int Group;           // küme, koru, kaya, çalılık sırası

        public bool IsSolid => Kind == MapItemKind.GreatTree || Kind == MapItemKind.ClusterTree
                            || Kind == MapItemKind.GroveTree || Kind == MapItemKind.Rock;
        public bool IsBush => Kind == MapItemKind.OuterBush || Kind == MapItemKind.InnerBush;
        public bool IsTree => Kind == MapItemKind.ClusterTree || Kind == MapItemKind.GroveTree;
    }

    /// <summary>Küme veya koru dairesi (SRS 3.12'deki "küme yarıçapı" / "koru yarıçapı"); geçit kontrolü için.</summary>
    public struct MapGroup
    {
        public Vector2 CenterPx;
        public float RadiusPx;
    }

    public class MapLayout
    {
        public readonly List<MapItem> Items = new List<MapItem>();
        public readonly List<MapGroup> Groups = new List<MapGroup>();
        public int Attempts;          // kaçıncı denemede geçerli çıktı
        public bool UsedFallback;     // true: sapmasız temel yerleşim döndü
    }

    /// <summary>
    /// Haritanın yerleşimini üretir; sahneye hiçbir şey koymaz (onu MapGenerator / MapBuilder yapacak).
    /// Merkez değerler SRS 3.12'dedir; üstüne küçük, sınırlı sapma eklenir ve sonuç doğrulanır.
    /// Sabit kalanlar: ulu ağaç, merkez açıklığı, doğma noktaları.
    /// Aynı tohum + aynı ayar = aynı yerleşim (NFR-07). Rastgelelik yalnızca verilen Rng'den gelir.
    /// </summary>
    public static class MapLayoutGenerator
    {
        public static MapLayout Generate(MapGenSettings s, Rng rng)
        {
            int tries = Mathf.Max(1, s.MaxRetries);
            for (int attempt = 1; attempt <= tries; attempt++)
            {
                MapLayout layout = BuildRandom(s, rng);
                if (layout != null && Validate(s, layout) == null)
                {
                    layout.Attempts = attempt;
                    return layout;
                }
            }

            // Hiçbiri geçmedi: sapmasız yerleşim (Rng kullanmaz).
            MapLayout fallback = Build(s, rng, false);
            fallback.Attempts = tries;
            fallback.UsedFallback = true;
            string reason = Validate(s, fallback);
            if (reason != null)
                Debug.LogWarning("MapLayoutGenerator: sapmasız yerleşim de geçersiz (" + reason + "). balance.json 'map' değerlerini kontrol et.");
            return fallback;
        }

        // ---------------------------------------------------------------- üretim

        static float Jit(Rng rng, bool on, float amount) => on ? rng.Range(-amount, amount) : 0f;

        static Vector2 Polar(float radiusPx, float deg)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector2(radiusPx * Mathf.Cos(a), radiusPx * Mathf.Sin(a));
        }

        static MapLayout Build(MapGenSettings s, Rng rng, bool jitter)
        {
            var layout = new MapLayout();
            var items = layout.Items;

            items.Add(new MapItem { Kind = MapItemKind.GreatTree, PosPx = Vector2.zero, RadiusPx = s.GreatTreeTrunkRadiusPx });

            for (int i = 0; i < s.ClusterCount; i++)
            {
                float ang = s.ClusterStartDeg + i * s.ClusterStepDeg + Jit(rng, jitter, s.AngleJitterDeg);
                float rad = s.ClusterRingPx + Jit(rng, jitter, s.RadiusJitterPx);
                Vector2 center = Polar(rad, ang);
                float rot = jitter ? rng.Range(0f, 360f) : 90f;     // her kümenin dizilimi farklı yöne baksın
                for (int k = 0; k < s.ClusterTreeCount; k++)
                    AddTree(s, rng, jitter, items, MapItemKind.ClusterTree, i,
                            center + Polar(s.ClusterTreeOffsetPx, rot + k * s.ClusterTreeStepDeg));
                layout.Groups.Add(new MapGroup { CenterPx = center, RadiusPx = s.ClusterRadiusPx });
            }

            for (int i = 0; i < s.GroveAnglesDeg.Length; i++)
            {
                float ang = s.GroveAnglesDeg[i] + Jit(rng, jitter, s.AngleJitterDeg);
                float rad = s.GroveRingPx + Jit(rng, jitter, s.RadiusJitterPx);
                Vector2 center = Polar(rad, ang);
                float rot = jitter ? rng.Range(0f, 360f) : 0f;
                for (int k = 0; k < s.GroveCenterTrees; k++)
                    AddTree(s, rng, jitter, items, MapItemKind.GroveTree, i, center);
                for (int k = 0; k < s.GroveRingTrees; k++)
                    AddTree(s, rng, jitter, items, MapItemKind.GroveTree, i,
                            center + Polar(s.GroveRingOffsetPx, rot + k * s.GroveRingStepDeg));
                layout.Groups.Add(new MapGroup { CenterPx = center, RadiusPx = s.GroveRadiusPx });
            }

            float rockRadius = s.RockSizePx * 0.5f * 1.4142136f;    // kareyi çevreleyen daire
            for (int i = 0; i < s.RockAnglesDeg.Length; i++)
            {
                float ang = s.RockAnglesDeg[i] + Jit(rng, jitter, s.RockAngleJitterDeg);
                float rad = s.RockRingPx + Jit(rng, jitter, s.RockRadiusJitterPx);
                items.Add(new MapItem { Kind = MapItemKind.Rock, PosPx = Polar(rad, ang), RadiusPx = rockRadius, Group = i });
            }

            for (int i = 0; i < s.SpawnAnglesDeg.Length; i++)
                items.Add(new MapItem { Kind = MapItemKind.Spawn, PosPx = Polar(s.SpawnRingPx, s.SpawnAnglesDeg[i]), RadiusPx = 0f, Group = i });

            for (int i = 0; i < s.OuterBushCount; i++)
            {
                float ang = s.OuterBushStartDeg + i * s.OuterBushStepDeg + Jit(rng, jitter, s.BushAngleJitterDeg);
                float rad = s.OuterBushRingPx + Jit(rng, jitter, s.BushRadiusJitterPx);
                items.Add(new MapItem { Kind = MapItemKind.OuterBush, PosPx = Polar(rad, ang), RadiusPx = s.OuterBushRadiusPx, Group = i });
            }

            for (int i = 0; i < s.InnerBushAnglesDeg.Length; i++)
            {
                float ang = s.InnerBushAnglesDeg[i] + Jit(rng, jitter, s.BushAngleJitterDeg);
                float rad = s.InnerBushRingPx + Jit(rng, jitter, s.BushRadiusJitterPx);
                items.Add(new MapItem { Kind = MapItemKind.InnerBush, PosPx = Polar(rad, ang), RadiusPx = s.InnerBushRadiusPx, Group = i });
            }

            return layout;
        }

        static void AddTree(MapGenSettings s, Rng rng, bool jitter, List<MapItem> items, MapItemKind kind, int group, Vector2 pos)
        {
            pos.x += Jit(rng, jitter, s.TreeJitterPx);
            pos.y += Jit(rng, jitter, s.TreeJitterPx);
            items.Add(new MapItem { Kind = kind, PosPx = pos, RadiusPx = s.TreeTrunkRadiusPx, Group = group });
        }

        // ------------------------------------------------- sıralı rastgele yerleştirme
        // Merkez (ulu ağaç, kayalar, doğma noktaları) sabit. Küme, koru ve çalılıklar kendi dilimlerinde
        // rastgele yer seçer; her biri yerleşirken kurallara bakılır, olmazsa yalnız o öğe yeniden denenir.
        // Bir öğe PlaceTries denemede yerleşmezse null döner (Generate yeni deneme başlatır).

        static MapLayout BuildRandom(MapGenSettings s, Rng rng)
        {
            var layout = new MapLayout();
            List<MapItem> items = layout.Items;

            items.Add(new MapItem { Kind = MapItemKind.GreatTree, PosPx = Vector2.zero, RadiusPx = s.GreatTreeTrunkRadiusPx });

            float rockRadius = s.RockSizePx * 0.5f * 1.4142136f;
            for (int i = 0; i < s.RockAnglesDeg.Length; i++)
            {
                float ang = s.RockAnglesDeg[i] + Jit(rng, true, s.RockAngleJitterDeg);
                float rad = s.RockRingPx + Jit(rng, true, s.RockRadiusJitterPx);
                items.Add(new MapItem { Kind = MapItemKind.Rock, PosPx = Polar(rad, ang), RadiusPx = rockRadius, Group = i });
            }

            for (int i = 0; i < s.SpawnAnglesDeg.Length; i++)
                items.Add(new MapItem { Kind = MapItemKind.Spawn, PosPx = Polar(s.SpawnRingPx, s.SpawnAnglesDeg[i]), RadiusPx = 0f, Group = i });

            int tries = Mathf.Max(1, s.PlaceTries);

            for (int i = 0; i < s.ClusterCount; i++)
            {
                bool placed = false;
                for (int t = 0; t < tries && !placed; t++)
                {
                    int start = items.Count;
                    float ang = s.ClusterStartDeg + i * s.ClusterStepDeg + Jit(rng, true, s.AngleJitterDeg);
                    float rad = s.ClusterRingPx + Jit(rng, true, s.RadiusJitterPx);
                    Vector2 center = Polar(rad, ang);
                    float rot = rng.Range(0f, 360f);
                    for (int k = 0; k < s.ClusterTreeCount; k++)
                        AddTree(s, rng, true, items, MapItemKind.ClusterTree, i,
                                center + Polar(s.ClusterTreeOffsetPx, rot + k * s.ClusterTreeStepDeg));
                    placed = Commit(s, layout, start, true, center, s.ClusterRadiusPx);
                }
                if (!placed) return null;
            }

            for (int i = 0; i < s.GroveAnglesDeg.Length; i++)
            {
                bool placed = false;
                for (int t = 0; t < tries && !placed; t++)
                {
                    int start = items.Count;
                    float ang = s.GroveAnglesDeg[i] + Jit(rng, true, s.AngleJitterDeg);
                    float rad = s.GroveRingPx + Jit(rng, true, s.RadiusJitterPx);
                    Vector2 center = Polar(rad, ang);
                    float rot = rng.Range(0f, 360f);
                    for (int k = 0; k < s.GroveCenterTrees; k++)
                        AddTree(s, rng, true, items, MapItemKind.GroveTree, i, center);
                    for (int k = 0; k < s.GroveRingTrees; k++)
                        AddTree(s, rng, true, items, MapItemKind.GroveTree, i,
                                center + Polar(s.GroveRingOffsetPx, rot + k * s.GroveRingStepDeg));
                    placed = Commit(s, layout, start, true, center, s.GroveRadiusPx);
                }
                if (!placed) return null;
            }

            for (int i = 0; i < s.OuterBushCount; i++)
            {
                bool placed = false;
                for (int t = 0; t < tries && !placed; t++)
                {
                    int start = items.Count;
                    float ang = s.OuterBushStartDeg + i * s.OuterBushStepDeg + Jit(rng, true, s.BushAngleJitterDeg);
                    float rad = s.OuterBushRingPx + Jit(rng, true, s.BushRadiusJitterPx);
                    items.Add(new MapItem { Kind = MapItemKind.OuterBush, PosPx = Polar(rad, ang), RadiusPx = s.OuterBushRadiusPx, Group = i });
                    placed = Commit(s, layout, start, false, Vector2.zero, 0f);
                }
                if (!placed) return null;
            }

            for (int i = 0; i < s.InnerBushAnglesDeg.Length; i++)
            {
                bool placed = false;
                for (int t = 0; t < tries && !placed; t++)
                {
                    int start = items.Count;
                    float ang = s.InnerBushAnglesDeg[i] + Jit(rng, true, s.BushAngleJitterDeg);
                    float rad = s.InnerBushRingPx + Jit(rng, true, s.BushRadiusJitterPx);
                    items.Add(new MapItem { Kind = MapItemKind.InnerBush, PosPx = Polar(rad, ang), RadiusPx = s.InnerBushRadiusPx, Group = i });
                    placed = Commit(s, layout, start, false, Vector2.zero, 0f);
                }
                if (!placed) return null;
            }

            return layout;
        }

        /// <summary>items[start..] yeni eklenen aday öğelerdir. Hepsi kurallara uyuyorsa (ve grupsa geçit payı varsa) kalır, yoksa silinir.</summary>
        static bool Commit(MapGenSettings s, MapLayout layout, int start, bool isGroup, Vector2 center, float radius)
        {
            List<MapItem> items = layout.Items;
            bool ok = true;
            for (int k = start; k < items.Count && ok; k++)
                ok = ItemFits(s, items, k);

            if (ok && isGroup)
            {
                for (int g = 0; g < layout.Groups.Count; g++)
                {
                    if (Vector2.Distance(layout.Groups[g].CenterPx, center) < layout.Groups[g].RadiusPx + radius + s.MinPassagePx)
                    {
                        ok = false;
                        break;
                    }
                }
            }

            if (!ok)
            {
                items.RemoveRange(start, items.Count - start);
                return false;
            }
            if (isGroup) layout.Groups.Add(new MapGroup { CenterPx = center, RadiusPx = radius });
            return true;
        }

        // items[k], kendinden önceki öğelere ve tek başına kurallara uyuyor mu (Validate'in 1-7 numaralı kuralları).
        static bool ItemFits(MapGenSettings s, List<MapItem> items, int k)
        {
            MapItem a = items[k];
            float half = s.MapSizePx * 0.5f - s.BoundsMarginPx;

            if (Mathf.Abs(a.PosPx.x) + a.RadiusPx > half || Mathf.Abs(a.PosPx.y) + a.RadiusPx > half) return false;

            for (int j = 0; j < k; j++)
                if (items[j].Kind == MapItemKind.Spawn && Vector2.Distance(a.PosPx, items[j].PosPx) - a.RadiusPx < s.SpawnClearPx)
                    return false;

            if ((a.IsTree || a.IsBush) && a.PosPx.magnitude - a.RadiusPx < s.ClearingRadiusPx) return false;

            if (a.Kind == MapItemKind.GroveTree && s.GroveOutsideZonePx > 0f
                && a.PosPx.magnitude - a.RadiusPx <= s.GroveOutsideZonePx)
                return false;

            for (int j = 0; j < k; j++)
            {
                MapItem b = items[j];
                float d = Vector2.Distance(a.PosPx, b.PosPx);
                if (a.IsSolid && b.IsSolid && d - a.RadiusPx - b.RadiusPx < s.MinGapPx) return false;
                if (a.IsBush && b.IsSolid && d < b.RadiusPx + s.MinGapPx) return false;
                if (b.IsBush && a.IsSolid && d < a.RadiusPx + s.MinGapPx) return false;
                if (a.IsBush && b.IsBush && d < a.RadiusPx + b.RadiusPx) return false;
            }
            return true;
        }

        // ------------------------------------------------------------- doğrulama

        /// <summary>Kurallar sağlanıyorsa null; değilse ilk bozulan kuralın kısa açıklaması.</summary>
        public static string Validate(MapGenSettings s, MapLayout layout)
        {
            List<MapItem> items = layout.Items;
            float half = s.MapSizePx * 0.5f - s.BoundsMarginPx;

            for (int i = 0; i < items.Count; i++)
            {
                MapItem a = items[i];

                // 1) Harita dışına taşma
                if (Mathf.Abs(a.PosPx.x) + a.RadiusPx > half || Mathf.Abs(a.PosPx.y) + a.RadiusPx > half)
                    return "harita dışı: " + a.Kind + " " + a.PosPx;

                // 2) Doğma noktası çevresi boş
                if (a.Kind != MapItemKind.Spawn)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        if (items[j].Kind != MapItemKind.Spawn) continue;
                        if (Vector2.Distance(a.PosPx, items[j].PosPx) - a.RadiusPx < s.SpawnClearPx)
                            return "doğma noktası dolu: " + a.Kind + " " + a.PosPx;
                    }
                }

                // 3) Küme ve koru ağaçları merkez açıklığa girmesin
                if (a.IsTree && a.PosPx.magnitude - a.RadiusPx < s.ClearingRadiusPx)
                    return "açıklıkta ağaç: " + a.PosPx;

                // 3b) Çalılıklar da merkez açıklığa girmesin (merkez sabit kalır)
                if (a.IsBush && a.PosPx.magnitude - a.RadiusPx < s.ClearingRadiusPx)
                    return "açıklıkta çalılık: " + a.PosPx;

                // 4) Koru ağaçları ilk daralmadan sonra tamamen alan dışında kalmalı (SRS 3.12, FR-73)
                if (a.Kind == MapItemKind.GroveTree && s.GroveOutsideZonePx > 0f
                    && a.PosPx.magnitude - a.RadiusPx <= s.GroveOutsideZonePx)
                    return "koru ağacı alan içinde: " + a.PosPx;

                for (int j = i + 1; j < items.Count; j++)
                {
                    MapItem b = items[j];
                    float d = Vector2.Distance(a.PosPx, b.PosPx);

                    // 5) Gövde ve kayalar üst üste binmesin
                    if (a.IsSolid && b.IsSolid && d - a.RadiusPx - b.RadiusPx < s.MinGapPx)
                        return "çakışma: " + a.Kind + " / " + b.Kind;

                    // 6) Çalılık merkezi bir gövdenin içinde olmasın
                    if (a.IsBush && b.IsSolid && d < b.RadiusPx + s.MinGapPx)
                        return "çalılık gövdede: " + a.PosPx;
                    if (b.IsBush && a.IsSolid && d < a.RadiusPx + s.MinGapPx)
                        return "çalılık gövdede: " + b.PosPx;

                    // 7) Çalılıklar birbirine binmesin
                    if (a.IsBush && b.IsBush && d < a.RadiusPx + b.RadiusPx)
                        return "çalılık çakışması: " + a.PosPx + " / " + b.PosPx;
                }
            }

            // 8) Küme ve koru daireleri arasında en az MinPassagePx geçit kalsın (SRS: komşu kümeler arası ≈160 px)
            List<MapGroup> groups = layout.Groups;
            for (int i = 0; i < groups.Count; i++)
                for (int j = i + 1; j < groups.Count; j++)
                    if (Vector2.Distance(groups[i].CenterPx, groups[j].CenterPx) < groups[i].RadiusPx + groups[j].RadiusPx + s.MinPassagePx)
                        return "geçit çok dar: grup " + i + " / " + j;

            return null;
        }
    }
}
