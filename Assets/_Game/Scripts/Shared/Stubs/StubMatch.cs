using System.Collections.Generic;
using LeafBrawl.Characters;
using UnityEngine;

namespace LeafBrawl.Shared.Stubs
{
    /// <summary>MatchManager yokken: maç hep Playing, saat sahne açılışından sayar, karakterler sahnedekilerdir.</summary>
    public class StubMatch : IMatch
    {
        public static readonly StubMatch Instance = new StubMatch();

        readonly List<Character> _characters = new List<Character>();
        int _cachedFrame = -1;

        public MatchState State => MatchState.Playing;
        public float MatchTime => Time.timeSinceLevelLoad;
        public MatchResult Result => null;

        public IReadOnlyList<Character> Characters
        {
            get
            {
                if (_cachedFrame != Time.frameCount)
                {
                    _cachedFrame = Time.frameCount;
                    _characters.Clear();
                    _characters.AddRange(Object.FindObjectsByType<Character>());
                }
                return _characters;
            }
        }

        public Character Player
        {
            get
            {
                foreach (var c in Characters)
                    if (c.IsPlayer) return c;
                return null;
            }
        }

        public void Pause() { }
        public void Resume() { }
        public void Restart() { }
    }
}
