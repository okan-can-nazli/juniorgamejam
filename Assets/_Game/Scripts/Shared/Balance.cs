using System;

namespace LeafBrawl.Shared
{
    // balance.json'ın birebir C# karşılığı (NFR-05). JsonUtility ile okunur: alan adları dosyadakiyle aynı olmalı.
    // Uzunluklar dünya px (Units.ToUnits ile çevir), süreler saniye. Dizilerde [0] = 1. seviye.
    // Bölüm sahipleri: levels..match → Yiğit · axe, abilities, bots → Hamza · trees..map → Okan.

    [Serializable]
    public class Balance
    {
        public LevelsConfig levels;
        public RegenConfig regen;
        public InventoryConfig inventory;
        public RecipesConfig recipes;
        public CraftingConfig crafting;
        public ThreatConfig threat;
        public CharacterConfig character;
        public CameraConfig camera;
        public MatchConfig match;
        public AxeConfig axe;
        public AbilitiesConfig abilities;
        public BotsConfig bots;
        public TreesConfig trees;
        public LeavesConfig leaves;
        public ZoneConfig zone;
        public LootConfig loot;
        public BushesConfig bushes;
        public MapConfig map;
    }

    /// <summary>Dört yaprak türünün adedi: tarif maliyeti, başlangıç envanteri.</summary>
    [Serializable]
    public struct LeafCost
    {
        public int red, yellow, green, orange;

        public int Get(LeafType type) => type switch
        {
            LeafType.Red => red,
            LeafType.Yellow => yellow,
            LeafType.Green => green,
            _ => orange,
        };
    }

    // ---- Yiğit ----

    [Serializable]
    public class LevelsConfig
    {
        public int maxLevel;
        public int[] damage;
        public float[] speedPx;
        public int[] maxHp;
    }

    [Serializable]
    public class RegenConfig
    {
        public float delayAfterDamageSec;
        public float[] secondsPerHp;
    }

    [Serializable]
    public class InventoryConfig
    {
        public int capRed, capYellow, capGreen, capOrange;
        public float pickupRadiusPx;
    }

    [Serializable]
    public class RecipesConfig
    {
        public LeafCost[] power;    // [0] = 1 → 2 geçişi
        public LeafCost[] speed;
        public LeafCost[] health;
        public LeafCost potion;
        public int potionHeal;
    }

    [Serializable]
    public class CraftingConfig
    {
        public float durationSec;
        public float safeNoDamageSec;
        public float safeNoSwingSec;
        public float notifySec;
    }

    [Serializable]
    public class ThreatConfig
    {
        public int smallMinScore, mediumMinScore, largeMinScore;
        public int[] pileLeafCount;      // PileTier sırasıyla: None, Small, Medium, Large
        public float[] pileRadiusScale;
        public float pileGrowSec;
    }

    [Serializable]
    public class CharacterConfig
    {
        public float radiusPx;
        public float hitFlashSec;
        public float deathEffectSec;
    }

    [Serializable]
    public class CameraConfig
    {
        public int referenceWidth, referenceHeight;
        public int assetsPixelsPerUnit;
    }

    [Serializable]
    public class MatchConfig
    {
        public float countdownSec;
        public string[] botScarves;     // ScarfColor adları
        public string[] botProfiles;    // BotProfile adları
    }

    // ---- Hamza ----

    [Serializable]
    public class AxeConfig
    {
        public float rangePx;
        public float arcDeg;
        public float cooldownSec;
        public float hitDelaySec;
        public float swingSpeedMultiplier;
        public int maxTreesPerSwing;
    }

    [Serializable]
    public class AbilitiesConfig
    {
        public int unlockLevel;
        public float dashDistancePx, dashDurationSec, dashCooldownSec;
        public float shieldDurationSec;
        public float ultiWindupSec, ultiRadiusPx;
    }

    [Serializable]
    public class BotsConfig
    {
        public float perceptionRadiusPx;
        public float decisionIntervalSec;
        public float enemyNearPx;
        public float zoneLookaheadSec;
        public float fleeHpRatio, fleeExitHpRatio, fleeDashEnemyPx;
        public float attackDashMinPx, attackDashMaxPx;
        public float targetTierWeight;
        public float firstSwingDelaySec;
        public float potionHpRatio;
        public float stuckCheckSec, stuckMinMovePx, unstuckRadiusPx;
        public float shieldHpRatio, shieldEnemyPx, ultiEnemyPx;
        public float navCellPx;
        public string[] priorityAggressive;   // StatType adları
        public string[] priorityAgile;
        public string[] priorityTough;
    }

    // ---- Okan ----

    [Serializable]
    public class TreesConfig
    {
        public float greatSpawnIntervalSec, greatOrangeChance;
        public float clusterSpawnIntervalSec, clusterOrangeChance;
        public float groveSpawnIntervalSec, groveOrangeChance;
        public int choppableHp;
        public int felledLeavesMin, felledLeavesMax;
        public float felledOrangeChance;
        public float dropRadiusPx;
        public float hpBarSec;
    }

    [Serializable]
    public class LeavesConfig
    {
        public float lifetimeSec, blinkBeforeSec;
        public int maxOnMap;
        public float fallMaxSec;
        public int startPerCluster, startAtGreatTree, startAtGreatTreeOrange;
        public int maxDropTries;
    }

    [Serializable]
    public class ZonePhase
    {
        public float startSec, endSec;
        public float startRadiusPx, endRadiusPx;
        public float damagePerSec;
    }

    [Serializable]
    public class ZoneConfig
    {
        public float tickSec;
        public float warningSec;
        public ZonePhase[] phases;
        public float suddenDeathStartSec;
        public float suddenDeathDamagePerSec;
    }

    [Serializable]
    public class LootConfig
    {
        public float rygRatio;       // K, S, Y: aşağı yuvarlanır
        public float orangeRatio;    // T: yukarı yuvarlanır
        public float lifetimeSec;
        public int oneLeafMaxTotal, twoLeavesMaxTotal;
    }

    [Serializable]
    public class BushesConfig
    {
        public float revealSec;
        public float playerInBushAlpha;
    }

    [Serializable]
    public class MapConfig
    {
        public float sizePx;
        public float clearingRadiusPx;
        public float greatTreeTrunkRadiusPx, greatTreeCanopyRadiusPx;
        public float rockRingPx;
        public float[] rockAnglesDeg;
        public float rockSizePx;
        public int clusterCount;
        public float clusterRingPx, clusterStartDeg, clusterStepDeg, clusterRadiusPx;
        public int clusterTreeCount;
        public float clusterTreeOffsetPx, clusterTreeStepDeg;
        public float groveRingPx;
        public float[] groveAnglesDeg;
        public float groveRadiusPx;
        public int groveCenterTrees, groveRingTrees;
        public float groveRingOffsetPx, groveRingStepDeg;
        public float treeTrunkRadiusPx;
        public int outerBushCount;
        public float outerBushRingPx, outerBushStartDeg, outerBushStepDeg, outerBushRadiusPx;
        public float innerBushRingPx;
        public float[] innerBushAnglesDeg;
        public float innerBushRadiusPx;
        public float spawnRingPx;
        public float[] spawnAnglesDeg;

        // Her maç tohumdan sapma ve doğrulama (SRS 3.12)
        public float angleJitterDeg, radiusJitterPx;          // küme ve koru merkezi
        public float rockAngleJitterDeg, rockRadiusJitterPx;  // kayalar (0 = sabit)
        public float bushAngleJitterDeg, bushRadiusJitterPx;
        public float treeJitterPx;                            // küme/koru içindeki her ağaç (x ve y ayrı)
        public int placeTries;                                // tek bir küme/koru/çalılığın yerleşme denemesi
        public int maxRetries;                                // bütün haritanın yeniden üretim sınırı
        public float boundsMarginPx, minGapPx, minPassagePx, spawnClearPx;
    }
}
