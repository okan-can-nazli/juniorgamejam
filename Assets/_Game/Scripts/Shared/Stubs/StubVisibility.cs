using LeafBrawl.Characters;
using UnityEngine;

namespace LeafBrawl.Shared.Stubs
{
    /// <summary>VisibilityService yokken: herkes herkesi görür, çalılık yok.</summary>
    public class StubVisibility : IVisibility
    {
        public static readonly StubVisibility Instance = new StubVisibility();

        public bool CanSee(Character viewer, Character target) => true;
        public bool IsInBush(Character c) => false;
        public Vector2? NearestBush(Vector2 from) => null;
    }
}
