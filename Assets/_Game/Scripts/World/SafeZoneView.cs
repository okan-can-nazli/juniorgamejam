using System.Collections.Generic;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Güvenli alanın görseli (FR-71): dışarıda yarı saydam örtü ve sınır. Örtü halka şeklinde bir mesh;
    /// yarıçap değişince köşeleri yeniden yazılır. Sınır, Furkan'ın yağmur bulutu sprite'ları varsa çember boyunca dizilen
    /// animasyonlu bulutlardır (ölçeklenmez); yoksa ince bir çizgi. SafeZone Awake'te bu bileşeni kendisi ekler.
    /// </summary>
    public class SafeZoneView : MonoBehaviour
    {
        const int Segments = 128;
        const float LineWidthUnits = 0.12f;
        const float CloudSpacing = 0.75f;      // bulutlar genişliklerinin bu katı aralıkla dizilir (üst üste biner)
        const float CloudFrameSec = 0.2f;      // yağmur animasyonu kare süresi (yalnızca görsel)
        static readonly Color VeilColor = new Color(0.75f, 0.1f, 0.1f, 0.3f);
        static readonly Color LineColor = new Color(1f, 0.35f, 0.25f, 0.95f);

        SafeZone _zone;
        Material _material;
        Mesh _veil, _line;
        readonly Vector3[] _vertices = new Vector3[Segments * 2];
        float _lastRadius = -1f;
        readonly List<SpriteRenderer> _clouds = new List<SpriteRenderer>();
        int _activeClouds;
        bool _cloudsReady;

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
            Sprite[] clouds = MapGenerator.Sprites.zoneClouds;
            bool useClouds = clouds != null && clouds.Length > 0;
            if (!_cloudsReady)
            {
                _cloudsReady = true;
                transform.Find("Sinir").gameObject.SetActive(!useClouds);   // bulut varsa çizgi gerekmez
            }

            float radius = Units.ToUnits(_zone.RadiusPx);
            if (!Mathf.Approximately(radius, _lastRadius))
            {
                _lastRadius = radius;
                float outer = Units.ToUnits(Services.Config.map.sizePx) * 2f;   // haritanın ve kameranın dışına taşar
                SetRing(_veil, radius, outer);
                SetRing(_line, Mathf.Max(0f, radius - LineWidthUnits * 0.5f), radius + LineWidthUnits * 0.5f);
                if (useClouds) PlaceClouds(clouds[0], radius);
            }
            if (useClouds) AnimateClouds(clouds);
        }

        // Bulutlar çember boyunca, genişliklerinin CloudSpacing katı aralıkla; yarıçap küçüldükçe sayıları azalır.
        void PlaceClouds(Sprite sample, float radius)
        {
            float spacing = sample.bounds.size.x * CloudSpacing;
            int count = radius <= 0f ? 0 : Mathf.Max(1, Mathf.CeilToInt(2f * Mathf.PI * radius / spacing));
            while (_clouds.Count < count)
            {
                var go = new GameObject("Bulut");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "ZoneOverlay";
                sr.sortingOrder = 2;
                _clouds.Add(sr);
            }
            for (int i = 0; i < _clouds.Count; i++)
            {
                bool on = i < count;
                _clouds[i].gameObject.SetActive(on);
                if (!on) continue;
                float angle = Mathf.PI * 2f * i / count;
                _clouds[i].transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            }
            _activeClouds = count;
        }

        // Komşu bulutlar farklı karede başlar, böylece yağmur hep birlikte titremez.
        void AnimateClouds(Sprite[] frames)
        {
            int frame = Mathf.FloorToInt(Time.time / CloudFrameSec);
            for (int i = 0; i < _activeClouds; i++)
                _clouds[i].sprite = frames[(frame + i) % frames.Length];
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
