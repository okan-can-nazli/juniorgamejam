using System;

namespace LeafBrawl.Shared
{
    /// <summary>
    /// debug.json (test ayarları, NFR-05). Varsa gitignore'lu debug.local.json'daki değerler geçerlidir.
    /// Demo yapısında debug.json varsayılan değerlerde kalır.
    /// </summary>
    [Serializable]
    public class DebugSettings
    {
        public int seed;                    // 0 = her maç farklı tohum
        public int botCount = 3;            // 0–3
        public LeafCost startInventory;     // yalnızca oyuncu; envanter sınırını aşabilir
        public StartLevels startLevels = new StartLevels();   // yalnızca oyuncu
        public float timeScale = 1f;        // test için maçı hızlandırır
        public bool showOverlay;
    }

    [Serializable]
    public class StartLevels
    {
        public int power = 1, speed = 1, health = 1;
    }
}
