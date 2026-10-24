using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Güvenli alanın yer tutucu görseli (FR-71): sınır çizgisi ve dışarıda yarı saydam örtü.
    /// İkisi de halka şeklinde birer mesh; yarıçap değişince köşeleri yeniden yazılır.
    /// SafeZone Awake'te bu bileşeni kendisi ekler. Furkan'ın görseli gelince değişir.
    /// </summary>
    public class SafeZoneView : MonoBehaviour
    {
        const int Segments = 128;
        const float LineWidthUnits = 0.12f;
        static readonly Color VeilColor = new Color(0.75f, 0.1f, 0.1f, 0.3f);
        static readonly Color LineColor = new Color(1f, 0.35f, 0.25f, 0.95f);

        SafeZone _zone;
        Material _material;
        Mesh _veil, _line;
        readonly Vector3[] _vertices = new Vector3[Segments * 2];
        float _lastRadius = -1f;

        void Awake()
        {
            _zone = GetComponent<SafeZone>();
            _material = new Material(Shader.Find("Sprites/Default"));
            _veil = AddRing("Orte", VeilColor, 0);
            _line = AddRing("Sinir", LineColor, 1);
        }

        void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }

        void LateUpdate()
        {
            float radius = Units.ToUnits(_zone.RadiusPx);
            if (Mathf.Approximately(radius, _lastRadius)) return;
            _lastRadius = radius;

            float outer = Units.ToUnits(Services.Config.map.sizePx) * 2f;   // haritanın ve kameranın dışına taşar
            SetRing(_veil, radius, outer);
            SetRing(_line, Mathf.Max(0f, radius - LineWidthUnits * 0.5f), radius + LineWidthUnits * 0.5f);
        }

        // Halka: iç ve dış çember üzerinde Segments çift köşe. Renk ve üçgenler sabit, yalnızca köşeler değişir.
        Mesh AddRing(string name, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var mesh = new Mesh { name = name };
            mesh.vertices = _vertices;

            var colors = new Color[_vertices.Length];
            for (int i = 0; i < colors.Length; i++) colors[i] = color;
            mesh.colors = colors;

            var triangles = new int[Segments * 6];
            for (int i = 0; i < Segments; i++)
            {
                int a = i * 2, b = a + 1;
                int c = (i + 1) % Segments * 2, d = c + 1;
                triangles[i * 6 + 0] = a; triangles[i * 6 + 1] = b; triangles[i * 6 + 2] = c;
                triangles[i * 6 + 3] = c; triangles[i * 6 + 4] = b; triangles[i * 6 + 5] = d;
            }
            mesh.triangles = triangles;

            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _material;
            renderer.sortingLayerName = "ZoneOverlay";
            renderer.sortingOrder = order;
            return mesh;
        }

        void SetRing(Mesh mesh, float inner, float outer)
        {
            for (int i = 0; i < Segments; i++)
            {
                float angle = Mathf.PI * 2f * i / Segments;
                var dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                _vertices[i * 2] = dir * inner;
                _vertices[i * 2 + 1] = dir * outer;
            }
            mesh.vertices = _vertices;
            mesh.RecalculateBounds();
        }
    }
}
