using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.World
{
    /// <summary>
    /// Kesilebilir ağaç: küme ve koru ağaçları (SRS 3.5, FR-35–FR-37).
    /// Her vuruşta TreeHit yayınlar, ağaç sallanır ve hpBarSec saniye can çubuğu görünür.
    /// Can 0 olunca veya vuruş ulti ise devrilir: TreeFelled yayınlanır,
    /// gövde ve "Vurus" collider'ları kapanır (kütük engel değildir, Tree sorgusuna girmez).
    /// Devrilince saçılan yapraklar LeafSpawner'ın işi (TreeFelled'i dinler).
    /// </summary>
    public class ChoppableTree : MonoBehaviour, ITree
    {
        // Yer tutucu görseller (Furkan'ın sprite, efekt ve sesi gelince değişir).
        static readonly Color StumpColor = new Color32(70, 50, 30, 255);
        static readonly Color BarBackColor = new Color32(30, 30, 30, 220);
        static readonly Color BarFillColor = new Color32(200, 60, 50, 255);
        const float StumpScale = 0.5f;   // kütük gövdenin yarı boyunda
        const float ShakeSec = 0.15f;
        const float ShakeSpeed = 80f;
        const float ShakeAmplitudeUnits = 0.06f;
        const float BarWidthUnits = 1f;
        const float BarHeightUnits = 0.12f;
        const float BarGapUnits = 0.25f;

        int _hp;
        Transform _visual;      // "Gorsel": sallanır, kütükte renk değişir
        Transform _bar;         // lazy: ilk vuruşta kurulur
        Transform _barFill;
        float _barLeft;
        float _shakeLeft;

        public Vector2 Position => transform.position;
        public bool IsAlive => !Felled;
        public bool Choppable => true;
        public bool Felled { get; private set; }

        /// <summary>MapGenerator kurarken yazar: küme mi koru mu (yaprak aralığı) ve hangi küme/koru (başlangıç yaprakları).</summary>
        public MapItemKind Kind { get; set; }
        public int Group { get; set; }

        void Awake()
        {
            _hp = Services.Config.trees.choppableHp;
            _visual = transform.Find("Gorsel");
        }

        void Update()
        {
            if (_barLeft > 0f)
            {
                _barLeft -= Time.deltaTime;
                if (_barLeft <= 0f) _bar.gameObject.SetActive(false);
            }

            if (_shakeLeft > 0f)
            {
                _shakeLeft -= Time.deltaTime;
                float x = _shakeLeft > 0f ? Mathf.Sin(_shakeLeft * ShakeSpeed) * ShakeAmplitudeUnits : 0f;
                _visual.localPosition = new Vector3(x, 0f, 0f);
            }
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (Felled) return;

            _hp -= info.Amount;
            GameEvents.RaiseTreeHit(this, info.Source);

            if (_hp <= 0 || info.IsUlti)
            {
                Fell(info.Source);
                return;
            }

            _shakeLeft = ShakeSec;
            ShowBar();
        }

        void Fell(Character by)
        {
            Felled = true;

            GetComponent<Collider2D>().enabled = false;                           // gövde (Obstacle)
            transform.Find("Vurus").GetComponent<Collider2D>().enabled = false;   // balta hedefi (Tree)

            _shakeLeft = 0f;
            _barLeft = 0f;
            _visual.localPosition = Vector3.zero;
            if (_bar != null) _bar.gameObject.SetActive(false);

            // Yer tutucu kütük: gövdeden küçük, koyu renk, karakterlerin altında.
            _visual.localScale *= StumpScale;
            var sr = _visual.GetComponent<SpriteRenderer>();
            sr.color = StumpColor;
            sr.sortingLayerName = "Ground";
            sr.sortingOrder = 2;

            GameEvents.RaiseTreeFelled(this, by);
        }

        // ---------------------------------------------------------------- can çubuğu (FR-35)

        void ShowBar()
        {
            if (_bar == null) BuildBar();

            float ratio = Mathf.Clamp01((float)_hp / Services.Config.trees.choppableHp);
            _barFill.localScale = new Vector3(BarWidthUnits * ratio, BarHeightUnits, 1f);
            _barFill.localPosition = new Vector3(-BarWidthUnits * (1f - ratio) * 0.5f, 0f, 0f);   // soldan küçülür

            _bar.gameObject.SetActive(true);
            _barLeft = Services.Config.trees.hpBarSec;
        }

        void BuildBar()
        {
            float top = GetComponent<CircleCollider2D>().radius + BarGapUnits;
            _bar = new GameObject("CanCubugu").transform;
            _bar.SetParent(transform, false);
            _bar.localPosition = new Vector3(0f, top, 0f);

            Transform back = AddBarPart("Arka", BarBackColor, 0);
            back.localScale = new Vector3(BarWidthUnits, BarHeightUnits, 1f);
            _barFill = AddBarPart("Dolgu", BarFillColor, 1);
        }

        Transform AddBarPart(string name, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_bar, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MapGenerator.Square;
            sr.color = color;
            sr.sortingLayerName = "Effects";
            sr.sortingOrder = order;
            return go.transform;
        }
    }
}
