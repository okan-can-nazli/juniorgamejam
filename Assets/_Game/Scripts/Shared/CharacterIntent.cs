using LeafBrawl.Characters;
using UnityEngine;

namespace LeafBrawl.Shared
{
    /// <summary>Oyuncu ve bot AYNI girdiyi üretir. Karakter bunu her karede IIntentSource'tan okur.</summary>
    public struct CharacterIntent
    {
        public Vector2 Move;        // birim vektör veya sıfır
        public Vector2 Aim;         // bakış yönü (birim vektör)
        public bool Swing;          // basılı tutuldukça true
        public bool Dash, Shield, Ulti;   // yalnızca basıldığı karede true
        public RecipeId? Craft;     // bu karede istenen tarif, yoksa null
    }

    /// <summary>Uygulayanlar: PlayerIntentSource (Yiğit), BotBrain (Hamza). Karakter kökünde durur.</summary>
    public interface IIntentSource
    {
        CharacterIntent ReadIntent(Character self);
    }
}
