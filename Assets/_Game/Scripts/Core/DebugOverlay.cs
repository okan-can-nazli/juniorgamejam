using LeafBrawl.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeafBrawl.Core
{
    /// <summary>
    /// debug.json "showOverlay" açıkken FPS sayacı (KT-14, NFR-01) ve sahne açılış süreleri (NFR-03). Yapıda da çalışır.
    /// Varsayılan kapalıdır; açmak için kişisel debug.local.json'a { "showOverlay": true } yazılır.
    /// Sahneye konmaz: oyun açılınca kendini kurar ve sahne değişse de kalır (ana menüde Systems.prefab yok).
    /// Ölçümler Console'a ve yapıda Player.log'a da yazılır, ekrana bakmadan okunabilsin.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        // Ölçüm aralıkları; oyun değeri değil.
        const float SampleSec = 0.5f;   // ekrandaki FPS bu sürenin ortalaması
        const float LogSec = 10f;       // log'a bu aralıkla ortalama, en düşük FPS ve en uzun kare yazılır

        float _sampleTime, _logTime;
        int _sampleFrames, _logFrames;
        float _fps, _logMinFps = float.MaxValue, _logMaxFrameMs;
        float _sceneLoadStarted;
        bool _skipFrame = true;   // sahnenin yüklendiği uzun kare ölçüme girmesin
        GUIStyle _style;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (!Services.DebugConfig.showOverlay) return;
            var go = new GameObject("DebugOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<DebugOverlay>();
            Debug.Log($"[Overlay] Açılış: '{SceneManager.GetActiveScene().name}' {Time.realtimeSinceStartup:F2} sn'de açıldı (NFR-03)");
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        // Sahne geçişi (ör. "Başla" → Match): eski sahnenin kapanmasından yenisinin açılmasına kadar geçen süre.
        void OnSceneUnloaded(Scene scene) => _sceneLoadStarted = Time.realtimeSinceStartup;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            float took = _sceneLoadStarted > 0f ? Time.realtimeSinceStartup - _sceneLoadStarted : 0f;
            Debug.Log($"[Overlay] Sahne '{scene.name}' {took:F2} sn'de açıldı (NFR-03)");
            _sampleTime = _logTime = 0f;
            _sampleFrames = _logFrames = 0;
            _logMinFps = float.MaxValue;
            _logMaxFrameMs = 0f;
            _skipFrame = true;
        }

        void Update()
        {
            if (_skipFrame)
            {
                _skipFrame = false;
                return;
            }
            float dt = Time.unscaledDeltaTime;
            _sampleTime += dt;
            _sampleFrames++;
            _logTime += dt;
            _logFrames++;
            _logMaxFrameMs = Mathf.Max(_logMaxFrameMs, dt * 1000f);

            if (_sampleTime >= SampleSec)
            {
                _fps = _sampleFrames / _sampleTime;
                _logMinFps = Mathf.Min(_logMinFps, _fps);
                _sampleTime = 0f;
                _sampleFrames = 0;
            }

            if (_logTime >= LogSec)
            {
                Debug.Log($"[Overlay] FPS ort={_logFrames / _logTime:F0} en düşük={_logMinFps:F0} en uzun kare={_logMaxFrameMs:F1} ms " +
                          $"({Screen.width}×{Screen.height}, durum={Services.Match.State})");
                _logTime = 0f;
                _logFrames = 0;
                _logMinFps = float.MaxValue;
                _logMaxFrameMs = 0f;
            }
        }

        void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(14, Screen.height / 40) };
            GUI.Label(new Rect(8, 8, 400, 40), $"FPS {_fps:F0}", _style);
        }
    }
}
