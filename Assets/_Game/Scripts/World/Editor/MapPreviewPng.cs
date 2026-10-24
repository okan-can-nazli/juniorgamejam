using System.IO;
using LeafBrawl.Shared;
using UnityEditor;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>Menü: Leaf Brawl → Harita Önizleme PNG. 4 tohumdan yerleşimi 2x2 tek resme çizer (arkadaşlara göstermek için).</summary>
    public static class MapPreviewPng
    {
        const int Cell = 600;
        static readonly int[] Seeds = { 1, 2, 3, 4 };

        static readonly Color32 Grass = new Color32(86, 140, 70, 255);
        static readonly Color32 Clearing = new Color32(150, 120, 80, 255);
        static readonly Color32 ZoneRing = new Color32(255, 255, 255, 255);
        static readonly Color32 Bush = new Color32(40, 100, 50, 255);
        static readonly Color32 Tree = new Color32(20, 70, 30, 255);
        static readonly Color32 Great = new Color32(10, 40, 15, 255);
        static readonly Color32 Rock = new Color32(130, 130, 130, 255);
        static readonly Color32 Spawn = new Color32(230, 50, 50, 255);

        [MenuItem("Leaf Brawl/Harita Önizleme PNG")]
        static void Run()
        {
            MapGenSettings s = MapGenSettings.FromConfig(Services.Config);
            int size = Cell * 2;
            var pixels = new Color32[size * size];

            for (int i = 0; i < Seeds.Length; i++)
            {
                MapLayout layout = MapLayoutGenerator.Generate(s, new Rng(Seeds[i]));
                DrawMap(pixels, size, (i % 2) * Cell, (i / 2) * Cell, s, layout);
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "MapPreview"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "harita_onizleme.png");
            File.WriteAllBytes(path, png);
            Debug.Log("[Harita Önizleme] Yazıldı: " + path + " (tohumlar 1-4; sol üst=1, sağ üst=2, sol alt=3, sağ alt=4)");
        }

        static void DrawMap(Color32[] px, int size, int ox, int oy, MapGenSettings s, MapLayout layout)
        {
            float scale = Cell / s.MapSizePx;
            float c = Cell * 0.5f;

            Fill(px, size, ox, oy, 0, 0, 0, Grass, true);                       // zemin
            Circle(px, size, ox, oy, c, c, s.ClearingRadiusPx * scale, Clearing);
            if (s.GroveOutsideZonePx > 0f)
                Ring(px, size, ox, oy, c, c, s.GroveOutsideZonePx * scale, ZoneRing);

            foreach (MapItem it in layout.Items)                                // önce çalılık, üstüne gövdeler
                if (it.IsBush) Circle(px, size, ox, oy, c + it.PosPx.x * scale, c + it.PosPx.y * scale, it.RadiusPx * scale, Bush);
            foreach (MapItem it in layout.Items)
            {
                float x = c + it.PosPx.x * scale, y = c + it.PosPx.y * scale, r = Mathf.Max(2f, it.RadiusPx * scale);
                if (it.Kind == MapItemKind.Rock) Circle(px, size, ox, oy, x, y, r, Rock);
                else if (it.Kind == MapItemKind.GreatTree) Circle(px, size, ox, oy, x, y, r, Great);
                else if (it.IsTree) Circle(px, size, ox, oy, x, y, r, Tree);
                else if (it.Kind == MapItemKind.Spawn) Circle(px, size, ox, oy, x, y, 5f, Spawn);
            }
        }

        static void Fill(Color32[] px, int size, int ox, int oy, int _a, int _b, int _c, Color32 col, bool all)
        {
            for (int y = 0; y < Cell; y++)
                for (int x = 0; x < Cell; x++)
                    px[(oy + y) * size + ox + x] = col;
        }

        static void Circle(Color32[] px, int size, int ox, int oy, float cx, float cy, float r, Color32 col)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r)), x1 = Mathf.Min(Cell - 1, Mathf.CeilToInt(cx + r));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r)), y1 = Mathf.Min(Cell - 1, Mathf.CeilToInt(cy + r));
            float r2 = r * r;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2)
                        px[(oy + y) * size + ox + x] = col;
        }

        static void Ring(Color32[] px, int size, int ox, int oy, float cx, float cy, float r, Color32 col)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r - 2)), x1 = Mathf.Min(Cell - 1, Mathf.CeilToInt(cx + r + 2));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r - 2)), y1 = Mathf.Min(Cell - 1, Mathf.CeilToInt(cy + r + 2));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (Mathf.Abs(d - r) < 1f) px[(oy + y) * size + ox + x] = col;
                }
        }
    }
}
