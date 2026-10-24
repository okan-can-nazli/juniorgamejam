using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Bots
{
    /// <summary>
    /// Botların ortak yol bulma ızgarası (BOT-11). Harita bots.navCellPx'lik hücrelere bölünür; bir hücre,
    /// merkezine karakter yarıçapında bir daire sığmıyorsa (World.IsBlocked) kapalıdır. A* 8 yönlüdür, köşe kesmez.
    /// Izgara ilk ihtiyaçta, o anki dünyadan kurulur; kesilen ağacın çevresi TreeFelled ile yeniden değerlendirilir.
    /// </summary>
    public class NavGrid
    {
        static NavGrid _instance;
        static IWorld _builtFor;

        readonly float _cell;        // birim
        readonly int _size;          // hücre sayısı (kare)
        readonly float _origin;      // sol alt köşe (birim)
        readonly float _radiusPx;
        readonly bool[] _blocked;
        readonly List<Vector2> _dirty = new List<Vector2>();

        // A* çalışma alanı (yeniden kullanılır)
        readonly float[] _g;
        readonly int[] _parent;
        readonly int[] _stamp;
        readonly bool[] _closed;
        int _search;
        readonly MinHeap _open;

        /// <summary>Dünya değiştiyse (yeni sahne, yeni harita) ızgarayı yeniden kurar.</summary>
        public static NavGrid Get()
        {
            var world = Services.World;
            if (_instance != null && ReferenceEquals(_builtFor, world)) return _instance;
            _instance?.Dispose();
            _builtFor = world;
            _instance = new NavGrid();
            return _instance;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay()
        {
            _instance = null;
            _builtFor = null;
        }

        public float CellSize => _cell;

        NavGrid()
        {
            var config = Services.Config;
            _cell = Units.ToUnits(config.bots.navCellPx);
            float mapUnits = Units.ToUnits(config.map.sizePx);
            _size = Mathf.CeilToInt(mapUnits / _cell);
            _origin = -mapUnits * 0.5f;
            _radiusPx = config.character.radiusPx;

            int count = _size * _size;
            _blocked = new bool[count];
            _g = new float[count];
            _parent = new int[count];
            _stamp = new int[count];
            _closed = new bool[count];
            _open = new MinHeap(count);

            var world = Services.World;
            for (int y = 0; y < _size; y++)
                for (int x = 0; x < _size; x++)
                    _blocked[y * _size + x] = world.IsBlocked(Center(x, y), _radiusPx);

            GameEvents.TreeFelled += OnTreeFelled;
        }

        void Dispose() => GameEvents.TreeFelled -= OnTreeFelled;

        // Ağacın collider'ları olaydan sonra kapanabilir; çevresi bir sonraki sorguda yeniden değerlendirilir.
        void OnTreeFelled(ITree tree, Character by) => _dirty.Add(tree.Position);

        void ApplyDirty()
        {
            if (_dirty.Count == 0) return;
            var map = Services.Config.map;
            float reach = Units.ToUnits(Mathf.Max(map.treeTrunkRadiusPx, map.greatTreeTrunkRadiusPx) + _radiusPx) + _cell;
            int r = Mathf.CeilToInt(reach / _cell);
            var world = Services.World;
            foreach (var p in _dirty)
            {
                ToCell(p, out int cx, out int cy);
                for (int y = cy - r; y <= cy + r; y++)
                    for (int x = cx - r; x <= cx + r; x++)
                        if (InBounds(x, y)) _blocked[y * _size + x] = world.IsBlocked(Center(x, y), _radiusPx);
            }
            _dirty.Clear();
        }

        public bool IsOpen(Vector2 pos)
        {
            ToCell(pos, out int x, out int y);
            return InBounds(x, y) && !_blocked[y * _size + x];
        }

        /// <summary>a'dan b'ye düz çizgi kapalı hücreye girmiyor mu (yarım hücre adımlarla).</summary>
        public bool LineClear(Vector2 a, Vector2 b)
        {
            ApplyDirty();
            float length = Vector2.Distance(a, b);
            int steps = Mathf.CeilToInt(length / (_cell * 0.5f));
            for (int i = 1; i <= steps; i++)
                if (!IsOpen(Vector2.Lerp(a, b, (float)i / steps))) return false;
            return true;
        }

        /// <summary>pos'a en yakın açık hücrenin merkezi (halka halka arar). Hiç yoksa pos.</summary>
        public Vector2 NearestOpen(Vector2 pos)
        {
            ApplyDirty();
            int index = NearestOpenIndex(pos);
            return index < 0 ? pos : Center(index % _size, index / _size);
        }

        /// <summary>A* yolu: from → to. Yol hücre merkezlerinden oluşur ve düz çizgiyle kısaltılır; son nokta hedeftir.</summary>
        public bool FindPath(Vector2 from, Vector2 to, List<Vector2> path)
        {
            path.Clear();
            ApplyDirty();
            int start = NearestOpenIndex(from);
            int goal = NearestOpenIndex(to);
            if (start < 0 || goal < 0) return false;

            _search++;
            _open.Clear();
            Visit(start, 0f, -1);
            _open.Push(start, Heuristic(start, goal));

            while (_open.Count > 0)
            {
                int current = _open.Pop();
                if (_closed[current] && _stamp[current] == _search) continue;
                _closed[current] = true;
                if (current == goal) break;

                int cx = current % _size, cy = current / _size;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = cx + dx, ny = cy + dy;
                        if (!InBounds(nx, ny) || _blocked[ny * _size + nx]) continue;
                        // Köşe kesme yok: çapraz adımda iki yan hücre de açık olmalı.
                        if (dx != 0 && dy != 0 && (_blocked[cy * _size + nx] || _blocked[ny * _size + cx])) continue;
                        int next = ny * _size + nx;
                        if (_stamp[next] == _search && _closed[next]) continue;
                        float cost = _g[current] + (dx != 0 && dy != 0 ? 1.41421356f : 1f);
                        if (_stamp[next] == _search && cost >= _g[next]) continue;
                        Visit(next, cost, current);
                        _open.Push(next, cost + Heuristic(next, goal));
                    }
            }

            if (_stamp[goal] != _search) return false;

            for (int i = goal; i != -1; i = _parent[i]) path.Add(Center(i % _size, i / _size));
            path.Reverse();
            if (path.Count > 0) path.RemoveAt(0);   // başlangıç hücresi
            path.Add(to);
            Smooth(from, path);
            return true;
        }

        // Görüş çizgisi açık olan ara noktaları atla.
        void Smooth(Vector2 from, List<Vector2> path)
        {
            int write = 0;
            Vector2 anchor = from;
            int i = 0;
            while (i < path.Count)
            {
                int farthest = i;
                while (farthest + 1 < path.Count && LineClear(anchor, path[farthest + 1])) farthest++;
                path[write++] = path[farthest];
                anchor = path[farthest];
                i = farthest + 1;
            }
            path.RemoveRange(write, path.Count - write);
        }

        void Visit(int index, float g, int parent)
        {
            _stamp[index] = _search;
            _closed[index] = false;
            _g[index] = g;
            _parent[index] = parent;
        }

        float Heuristic(int a, int b)
        {
            int dx = Mathf.Abs(a % _size - b % _size), dy = Mathf.Abs(a / _size - b / _size);
            return Mathf.Max(dx, dy) + 0.41421356f * Mathf.Min(dx, dy);
        }

        int NearestOpenIndex(Vector2 pos)
        {
            ToCell(pos, out int cx, out int cy);
            cx = Mathf.Clamp(cx, 0, _size - 1);
            cy = Mathf.Clamp(cy, 0, _size - 1);
            for (int r = 0; r < _size; r++)
            {
                int best = -1;
                float bestDistance = float.MaxValue;
                for (int y = cy - r; y <= cy + r; y++)
                    for (int x = cx - r; x <= cx + r; x++)
                    {
                        if (Mathf.Max(Mathf.Abs(x - cx), Mathf.Abs(y - cy)) != r || !InBounds(x, y)) continue;
                        if (_blocked[y * _size + x]) continue;
                        float d = (Center(x, y) - pos).sqrMagnitude;
                        if (d < bestDistance) { bestDistance = d; best = y * _size + x; }
                    }
                if (best >= 0) return best;
            }
            return -1;
        }

        Vector2 Center(int x, int y) => new Vector2(_origin + (x + 0.5f) * _cell, _origin + (y + 0.5f) * _cell);

        void ToCell(Vector2 pos, out int x, out int y)
        {
            x = Mathf.FloorToInt((pos.x - _origin) / _cell);
            y = Mathf.FloorToInt((pos.y - _origin) / _cell);
        }

        bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < _size && y < _size;

        /// <summary>A* açık listesi için ikili yığın.</summary>
        class MinHeap
        {
            readonly int[] _items;
            readonly float[] _keys;
            public int Count { get; private set; }

            public MinHeap(int capacity)
            {
                // Bir hücre birden çok kez itilebilir (daha iyi yol bulununca); kapasite bunu karşılar.
                _items = new int[capacity * 8];
                _keys = new float[capacity * 8];
            }

            public void Clear() => Count = 0;

            public void Push(int item, float key)
            {
                if (Count == _items.Length) return;
                int i = Count++;
                _items[i] = item;
                _keys[i] = key;
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (_keys[parent] <= _keys[i]) break;
                    Swap(i, parent);
                    i = parent;
                }
            }

            public int Pop()
            {
                int top = _items[0];
                Count--;
                _items[0] = _items[Count];
                _keys[0] = _keys[Count];
                int i = 0;
                while (true)
                {
                    int left = i * 2 + 1, right = left + 1, smallest = i;
                    if (left < Count && _keys[left] < _keys[smallest]) smallest = left;
                    if (right < Count && _keys[right] < _keys[smallest]) smallest = right;
                    if (smallest == i) break;
                    Swap(i, smallest);
                    i = smallest;
                }
                return top;
            }

            void Swap(int a, int b)
            {
                (_items[a], _items[b]) = (_items[b], _items[a]);
                (_keys[a], _keys[b]) = (_keys[b], _keys[a]);
            }
        }
    }
}
