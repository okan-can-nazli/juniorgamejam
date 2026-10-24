using LeafBrawl.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeafBrawl.Core
{
    /// <summary>
    /// Sahne geçişleri (FR-01, FR-06, FR-08). Furkan'ın menü, duraklatma ve sonuç ekranı düğmeleri yalnızca bunları çağırır.
    /// Build Settings: MainMenu (0), Match (1).
    /// </summary>
    public static class SceneFlow
    {
        const string MainMenuScene = "MainMenu";
        const string MatchScene = "Match";

        /// <summary>Ana menüdeki "Başla" (FR-04).</summary>
        public static void StartMatch() => Load(MatchScene);

        /// <summary>Duraklatma ve sonuç ekranındaki "Yeniden başlat". Sandbox sahnesinde de çalışır.</summary>
        public static void Restart() => Services.Match.Restart();

        /// <summary>Duraklatma ve sonuç ekranındaki "Ana menü"; sonuç ekranında Esc de bunu çağırır (FR-08).</summary>
        public static void LoadMenu() => Load(MainMenuScene);

        /// <summary>Ana menüdeki "Çıkış".</summary>
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Duraklatma veya maç sonu timeScale'i 0 bırakır; yeni sahne normal hızla açılsın.
        static void Load(string scene)
        {
            Time.timeScale = Services.DebugConfig.timeScale;
            SceneManager.LoadScene(scene);
        }
    }
}
