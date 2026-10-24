using LeafBrawl.Shared;
using UnityEditor;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Menü: Leaf Brawl → Haritayı Kur.
    /// 1) Map.prefab'ta MapGenerator yoksa ekler.
    /// 2) Açık sahnede bir MapGenerator varsa haritayı önizleme olarak kurar (kaydedilmez).
    /// Harita her maç tohumdan değiştiği için prefab'a sabit öğe gömülmez; asıl kurulum Play'de MapGenerator.Awake'te olur.
    /// </summary>
    public static class MapBuilder
    {
        const string PrefabPath = "Assets/_Game/Prefabs/World/Map.prefab";

        [MenuItem("Leaf Brawl/Haritayı Kur")]
        static void Run()
        {
            EnsureGeneratorOnPrefab();

            MapGenerator generator = Object.FindAnyObjectByType<MapGenerator>();
            if (generator == null)
            {
                Debug.Log("[Harita] Map.prefab hazır. Önizleme için açık sahneye Map.prefab'ı koyup komutu tekrar çalıştır.");
                return;
            }

            MapGenSettings settings = MapGenSettings.FromConfig(Services.Config);
            MapLayout layout = MapLayoutGenerator.Generate(settings, new Rng(Services.DebugConfig.seed));   // seed 0 = her seferinde farklı
            generator.Build(layout, settings);

            Debug.Log($"[Harita] Önizleme kuruldu: {layout.Items.Count} öğe, {layout.Attempts}. denemede" +
                      (layout.UsedFallback ? " (YEDEK düzen)" : "") + ". Sahneye kaydedilmez; Play'de her maç yeniden kurulur.");
        }

        static void EnsureGeneratorOnPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (root.GetComponent<MapGenerator>() != null) return;
                root.AddComponent<MapGenerator>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[Harita] Map.prefab'a MapGenerator eklendi.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
