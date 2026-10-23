using System.Collections.Generic;
using UnityEngine;

namespace LeafBrawl.Shared.Stubs
{
    /// <summary>WorldService yokken: boş dünya, SRS 3.12'deki 4 doğma noktası, harita dışı engelli.</summary>
    public class StubWorld : IWorld
    {
        public static readonly StubWorld Instance = new StubWorld();

        static readonly IReadOnlyList<IPickup> NoPickups = new IPickup[0];
        static readonly IReadOnlyList<ITree> NoTrees = new ITree[0];
        Vector2[] _spawnPoints;

        public IReadOnlyList<IPickup> PickupsNear(Vector2 pos, float radiusPx) => NoPickups;
        public IReadOnlyList<ITree> TreesNear(Vector2 pos, float radiusPx) => NoTrees;
        public ITree GreatTree => null;

        public IReadOnlyList<Vector2> SpawnPoints
        {
            get
            {
                if (_spawnPoints != null) return _spawnPoints;
                var map = Services.Config.map;
                _spawnPoints = new Vector2[map.spawnAnglesDeg.Length];
                for (int i = 0; i < _spawnPoints.Length; i++)
                {
                    float a = map.spawnAnglesDeg[i] * Mathf.Deg2Rad;
                    _spawnPoints[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Units.ToUnits(map.spawnRingPx);
                }
                return _spawnPoints;
            }
        }

        public bool IsBlocked(Vector2 pos, float radiusPx)
        {
            float half = Units.ToUnits(Services.Config.map.sizePx * 0.5f - radiusPx);
            return Mathf.Abs(pos.x) > half || Mathf.Abs(pos.y) > half;
        }
    }
}
