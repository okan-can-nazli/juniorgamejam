using LeafBrawl.Shared;

namespace LeafBrawl.World
{
    /// <summary>
    /// Harita üretiminin girdileri (SRS 3.12). Uzunluklar dünya px'tir.
    /// Merkez değerler balance.json "map" bölümünden gelir (<see cref="FromConfig"/>).
    /// Sapma ve doğrulama değerleri de aynı bölümden gelir (SRS 3.12, NFR-05).
    /// </summary>
    public class MapGenSettings
    {
        // ---- balance.json "map" ----
        public float MapSizePx, ClearingRadiusPx;
        public float GreatTreeTrunkRadiusPx;
        public float RockRingPx, RockSizePx;
        public float[] RockAnglesDeg;
        public int ClusterCount;
        public float ClusterRingPx, ClusterStartDeg, ClusterStepDeg, ClusterRadiusPx;
        public int ClusterTreeCount;
        public float ClusterTreeOffsetPx, ClusterTreeStepDeg;
        public float GroveRingPx, GroveRadiusPx;
        public float[] GroveAnglesDeg;
        public int GroveCenterTrees, GroveRingTrees;
        public float GroveRingOffsetPx, GroveRingStepDeg;
        public float TreeTrunkRadiusPx;
        public int OuterBushCount;
        public float OuterBushRingPx, OuterBushStartDeg, OuterBushStepDeg, OuterBushRadiusPx;
        public float InnerBushRingPx, InnerBushRadiusPx;
        public float[] InnerBushAnglesDeg;
        public float SpawnRingPx;
        public float[] SpawnAnglesDeg;

        /// <summary>balance.json "zone": ilk daralmanın bittiği yarıçap. Koru ağaçları bunun dışında kalmalı (SRS 3.12). 0 = kontrol yok.</summary>
        public float GroveOutsideZonePx;

        // ---- Sapma ve doğrulama (balance.json "map") ----
        public float AngleJitterDeg, RadiusJitterPx;             // küme ve koru: kendi dilimi içinde (küme adımı 45°)
        public float RockAngleJitterDeg, RockRadiusJitterPx;     // kayalar: 0 = sabit
        public float BushAngleJitterDeg, BushRadiusJitterPx;
        public float TreeJitterPx;                               // küme/koru içindeki her ağaç (x ve y ayrı)
        public int PlaceTries;                                   // tek bir küme/koru/çalılığın yerleşme denemesi
        public float BoundsMarginPx;                             // harita kenarından en az bu kadar içeride
        public float MinGapPx;                                   // gövde ve kaya arası en az boşluk
        public float MinPassagePx;                               // küme/koru daireleri arası en az geçit
        public float SpawnClearPx;                               // doğma noktası çevresi boş alan
        public int MaxRetries;

        public static MapGenSettings FromConfig(Balance b)
        {
            MapConfig m = b.map;
            return new MapGenSettings
            {
                MapSizePx = m.sizePx,
                ClearingRadiusPx = m.clearingRadiusPx,
                GreatTreeTrunkRadiusPx = m.greatTreeTrunkRadiusPx,
                RockRingPx = m.rockRingPx,
                RockSizePx = m.rockSizePx,
                RockAnglesDeg = m.rockAnglesDeg,
                ClusterCount = m.clusterCount,
                ClusterRingPx = m.clusterRingPx,
                ClusterStartDeg = m.clusterStartDeg,
                ClusterStepDeg = m.clusterStepDeg,
                ClusterRadiusPx = m.clusterRadiusPx,
                ClusterTreeCount = m.clusterTreeCount,
                ClusterTreeOffsetPx = m.clusterTreeOffsetPx,
                ClusterTreeStepDeg = m.clusterTreeStepDeg,
                GroveRingPx = m.groveRingPx,
                GroveRadiusPx = m.groveRadiusPx,
                GroveAnglesDeg = m.groveAnglesDeg,
                GroveCenterTrees = m.groveCenterTrees,
                GroveRingTrees = m.groveRingTrees,
                GroveRingOffsetPx = m.groveRingOffsetPx,
                GroveRingStepDeg = m.groveRingStepDeg,
                TreeTrunkRadiusPx = m.treeTrunkRadiusPx,
                OuterBushCount = m.outerBushCount,
                OuterBushRingPx = m.outerBushRingPx,
                OuterBushStartDeg = m.outerBushStartDeg,
                OuterBushStepDeg = m.outerBushStepDeg,
                OuterBushRadiusPx = m.outerBushRadiusPx,
                InnerBushRingPx = m.innerBushRingPx,
                InnerBushAnglesDeg = m.innerBushAnglesDeg,
                InnerBushRadiusPx = m.innerBushRadiusPx,
                SpawnRingPx = m.spawnRingPx,
                SpawnAnglesDeg = m.spawnAnglesDeg,
                AngleJitterDeg = m.angleJitterDeg,
                RadiusJitterPx = m.radiusJitterPx,
                RockAngleJitterDeg = m.rockAngleJitterDeg,
                RockRadiusJitterPx = m.rockRadiusJitterPx,
                BushAngleJitterDeg = m.bushAngleJitterDeg,
                BushRadiusJitterPx = m.bushRadiusJitterPx,
                TreeJitterPx = m.treeJitterPx,
                PlaceTries = m.placeTries,
                BoundsMarginPx = m.boundsMarginPx,
                MinGapPx = m.minGapPx,
                MinPassagePx = m.minPassagePx,
                SpawnClearPx = m.spawnClearPx,
                MaxRetries = m.maxRetries,
                GroveOutsideZonePx = FirstShrinkEndRadius(b.zone),
            };
        }

        // İlk gerçekten daralan aşamanın bitiş yarıçapı (3.8: 1700 → 1000).
        static float FirstShrinkEndRadius(ZoneConfig zone)
        {
            if (zone == null || zone.phases == null) return 0f;
            foreach (ZonePhase p in zone.phases)
                if (p.endRadiusPx < p.startRadiusPx) return p.endRadiusPx;
            return 0f;
        }
    }
}
