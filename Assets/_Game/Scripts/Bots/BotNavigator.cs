using System.Collections.Generic;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Bots
{
    /// <summary>
    /// Bir botun hedefe giden hareket yönü (BOT-11). Hedef düz görünüyorsa doğrudan yürür; değilse NavGrid'den A* yolu ister.
    /// Yol hesabını bütün botlar arasında karede en fazla 1 bot yapar; bir bot en sık decisionIntervalSec'te bir yol yeniler.
    /// Takılma: stuckCheckSec boyunca stuckMinMovePx'ten az ilerleyen bot unstuckRadiusPx içinde rastgele bir noktaya yönelir.
    /// </summary>
    public class BotNavigator
    {
        static int _lastPathFrame = -1;

        readonly List<Vector2> _path = new List<Vector2>();
        int _index;
        Vector2 _pathGoal;
        bool _hasPath;
        float _nextRepathTime;

        // Takılma
        Vector2 _checkpoint;
        float _checkpointTime = -1f;
        Vector2? _unstuckGoal;
        float _unstuckUntil;

        /// <summary>Takılmadan kurtulmak için rastgele noktaya gidiliyor mu (debug için).</summary>
        public bool Unsticking => _unstuckGoal.HasValue;
        public Vector2? CurrentWaypoint { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => _lastPathFrame = -1;

        /// <summary>pos'tan goal'a gitmek için bu karedeki yön (birim vektör; varılmışsa sıfır).</summary>
        public Vector2 Steer(Vector2 pos, Vector2 goal)
        {
            var bots = Services.Config.bots;
            var grid = NavGrid.Get();

            if (_unstuckGoal.HasValue)
            {
                if (Time.time > _unstuckUntil || (_unstuckGoal.Value - pos).sqrMagnitude < grid.CellSize * grid.CellSize)
                    _unstuckGoal = null;
                else
                    goal = _unstuckGoal.Value;
            }

            Vector2 target = goal;
            if (grid.LineClear(pos, goal))
            {
                _hasPath = false;
            }
            else
            {
                bool goalMoved = !_hasPath || (goal - _pathGoal).sqrMagnitude > grid.CellSize * grid.CellSize;
                if (goalMoved && Time.time >= _nextRepathTime && Time.frameCount != _lastPathFrame)
                {
                    _lastPathFrame = Time.frameCount;
                    _nextRepathTime = Time.time + bots.decisionIntervalSec;
                    _hasPath = grid.FindPath(pos, goal, _path);
                    _pathGoal = goal;
                    _index = 0;
                }
                if (_hasPath)
                {
                    float reach = grid.CellSize * 0.5f;
                    while (_index < _path.Count - 1 && ((_path[_index] - pos).sqrMagnitude < reach * reach || grid.LineClear(pos, _path[_index + 1])))
                        _index++;
                    if (_index < _path.Count) target = _path[_index];
                }
            }

            CurrentWaypoint = target;
            Vector2 toTarget = target - pos;
            // Son noktaya yarım hücreden yakınsa varılmış sayılır (yerinde titreyip takılma sanılmasın).
            bool last = !_hasPath || _index >= _path.Count - 1;
            float arrive = grid.CellSize * 0.5f;
            if (last && toTarget.sqrMagnitude < arrive * arrive) return Vector2.zero;
            return toTarget.normalized;
        }

        /// <summary>
        /// Her karede çağrılır (BOT-11). Bot hareket etmeye çalışırken stuckCheckSec boyunca stuckMinMovePx'ten az
        /// ilerlediyse unstuckRadiusPx içinde rastgele, açık bir noktaya yönelir.
        /// </summary>
        public void TrackStuck(Vector2 pos, bool tryingToMove)
        {
            var bots = Services.Config.bots;
            if (!tryingToMove || _checkpointTime < 0f)
            {
                _checkpoint = pos;
                _checkpointTime = Time.time;
                return;
            }
            if (Time.time - _checkpointTime < bots.stuckCheckSec) return;

            bool stuck = Units.ToPx((pos - _checkpoint).magnitude) < bots.stuckMinMovePx;
            _checkpoint = pos;
            _checkpointTime = Time.time;
            if (!stuck || _unstuckGoal.HasValue) return;

            var grid = NavGrid.Get();
            Vector2 random = pos + Services.Rng.InsideCircle(Units.ToUnits(bots.unstuckRadiusPx));
            _unstuckGoal = grid.NearestOpen(random);
            _unstuckUntil = Time.time + bots.stuckCheckSec;
            _hasPath = false;
        }
    }
}
