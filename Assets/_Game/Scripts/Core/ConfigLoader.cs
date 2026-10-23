using System;
using System.Collections;
using System.IO;
using System.Reflection;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Core
{
    /// <summary>
    /// balance.json ve debug.json'ı oyunun başında okur ve Services'e koyar (NFR-05, NFR-07).
    /// Systems.prefab üzerinde durur; her şeyden önce çalışır.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class ConfigLoader : MonoBehaviour
    {
        const string BalanceFile = "balance.json";
        const string DebugFile = "debug.json";
        const string DebugLocalFile = "debug.local.json";

        void Awake()
        {
            Services.Config = LoadBalance();
            Services.DebugConfig = LoadDebug();
            Services.Rng = new Rng(Services.DebugConfig.seed);
            Time.timeScale = Services.DebugConfig.timeScale;
            Debug.Log($"[ConfigLoader] Ayarlar yüklendi. Tohum: {Services.Rng.Seed}");
        }

        public static Balance LoadBalance()
        {
            var balance = new Balance();
            if (TryRead(BalanceFile, out string json))
            {
                try { JsonUtility.FromJsonOverwrite(json, balance); }
                catch (Exception e) { Debug.LogError($"[ConfigLoader] {BalanceFile} okunamadı: {e.Message}"); }
            }
            ReportMissing(balance, BalanceFile, "");
            return balance;
        }

        public static DebugSettings LoadDebug()
        {
            var settings = new DebugSettings();
            if (TryRead(DebugFile, out string json))
            {
                try { JsonUtility.FromJsonOverwrite(json, settings); }
                catch (Exception e) { Debug.LogError($"[ConfigLoader] {DebugFile} okunamadı: {e.Message}"); }
            }
            // Kişisel deneme ayarları: yalnızca dosyada yazan alanlar değişir.
            string localPath = Path.Combine(Application.streamingAssetsPath, DebugLocalFile);
            if (File.Exists(localPath))
            {
                try
                {
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(localPath), settings);
                    Debug.Log($"[ConfigLoader] {DebugLocalFile} kullanılıyor.");
                }
                catch (Exception e) { Debug.LogError($"[ConfigLoader] {DebugLocalFile} okunamadı: {e.Message}"); }
            }
            settings.botCount = Mathf.Clamp(settings.botCount, 0, 3);
            if (settings.timeScale <= 0f) settings.timeScale = 1f;
            return settings;
        }

        static bool TryRead(string fileName, out string text)
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"[ConfigLoader] {path} bulunamadı.");
                text = null;
                return false;
            }
            text = File.ReadAllText(path);
            return true;
        }

        // JsonUtility eksik alanı sessizce boş bırakır; eksik bölüm ve dizileri adıyla yaz.
        static void ReportMissing(object node, string file, string path)
        {
            foreach (var field in node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldPath = path.Length == 0 ? field.Name : path + "." + field.Name;
                object value = field.GetValue(node);
                if (value == null || (value is ICollection list && list.Count == 0))
                    Debug.LogError($"[ConfigLoader] {file}: '{fieldPath}' eksik veya boş.");
                else if (value is Array array)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        object item = array.GetValue(i);
                        if (item != null && IsConfigClass(item.GetType()))
                            ReportMissing(item, file, $"{fieldPath}[{i}]");
                    }
                }
                else if (IsConfigClass(field.FieldType))
                    ReportMissing(value, file, fieldPath);
            }
        }

        static bool IsConfigClass(Type type) => type.IsClass && type != typeof(string) && !type.IsArray;
    }
}
