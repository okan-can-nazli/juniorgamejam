using UnityEngine;

namespace LeafBrawl.Presentation
{
    /// <summary>Olay → ses eşlemesi (NFR-10). Audio/SfxLibrary.asset; AudioManager okur. Sesler CC0 (Kenney), bkz. CREDITS.md.</summary>
    [CreateAssetMenu(menuName = "Leaf Brawl/Sfx Library", fileName = "SfxLibrary")]
    public class SfxLibrary : ScriptableObject
    {
        [Header("Savaş")]
        public AudioClip swing;            // Swung
        public AudioClip hit;              // HitLanded
        public AudioClip death;            // Died

        [Header("Ağaç ve yaprak")]
        public AudioClip treeHit;          // TreeHit
        public AudioClip greatTreeHit;     // GreatTreeHit ("tok")
        public AudioClip treeFelled;       // TreeFelled
        public AudioClip leafCollected;    // LeafCollected (yalnızca oyuncu)

        [Header("Tarif (yalnızca oyuncu)")]
        public AudioClip craftStarted;
        public AudioClip craftCompleted;
        public AudioClip craftRejected;

        [Header("Yetenekler")]
        public AudioClip abilityUnlocked;  // yalnızca oyuncu
        public AudioClip dash;
        public AudioClip shieldUp;
        public AudioClip shieldBroken;
        public AudioClip ultiWindup;
        public AudioClip ultiBlast;

        [Header("Maç ve arayüz")]
        public AudioClip zoneShrink;       // ZonePhaseChanged (alan daralması başlıyor)
        public AudioClip countdownTick;    // CountdownTick 3, 2, 1
        public AudioClip countdownGo;      // CountdownTick 0
        public AudioClip uiClick;          // düğme

        [Header("Ayarlar")]
        [Range(0f, 1f)] public float volume = 0.8f;
        [Tooltip("Yaprak toplama sesi saniyede en fazla bu kadar çalar.")]
        public float leafSoundsPerSecond = 6f;
    }
}
