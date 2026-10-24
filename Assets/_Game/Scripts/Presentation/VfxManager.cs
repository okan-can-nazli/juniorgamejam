using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using TMPro;
using UnityEngine;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Dünya efektleri (HUD.prefab kökünde). Yalnızca GameEvents dinler ve değer okur; oyuna yazmaz.
    /// Hasar sayısı (FR-23), ölümde yaprak dağılması (FR-80), tarif birleşmesi (FR-45), atılma izi ve kalkan halkası (3.4, FR-27),
    /// ulti patlama halkası (FR-28), ağaç vuruşu ve devrilme yaprakları (FR-35, FR-36).
    /// Hasar parlaması, ulti parıltısı ve üretim çubuğu CharacterVisual'dadır. Görünmeyen karakterin efekti çizilmez (FR-51).
    /// Süreler ve yarıçaplar balance.json'dan; aşağıdaki alanlar yalnızca görsel ayarlardır.
    /// </summary>
    public class VfxManager : MonoBehaviour
    {
        const string EffectsLayer = "Effects";

        [SerializeField] Sprite[] leafSprites;          // spr_yigin_yaprak_0..5
        [SerializeField] Material flashMaterial;        // Mat_SpriteFlash (siluet)

        [Header("Hasar sayısı")]
        [SerializeField] float numberRiseUnits = 0.8f;
        [SerializeField] float numberLifeSec = 0.7f;
        [SerializeField] float numberSize = 3f;
        [SerializeField] float numberHeightUnits = 1.8f;
        [SerializeField] Color numberColor = new Color(1f, 0.95f, 0.85f);

        [Header("Yaprak patlamaları")]
        [SerializeField] int deathLeaves = 14;
        [SerializeField] int craftLeaves = 8;
        [SerializeField] int treeHitLeaves = 3;
        [SerializeField] int treeFellLeaves = 12;
        [SerializeField] float burstSpeed = 2.5f;
        [SerializeField] float smallBurstLifeSec = 0.5f;

        [Header("Atılma izi, kalkan, ulti")]
        [SerializeField] int dashGhosts = 4;
        [SerializeField] Color dashGhostColor = new Color(0.85f, 0.95f, 1f, 0.5f);
        [SerializeField] Color shieldColor = new Color(0.55f, 0.85f, 1f, 0.8f);
        [SerializeField] float shieldRadiusUnits = 0.75f;
        [SerializeField] Color ultiColor = new Color(1f, 0.85f, 0.3f, 0.9f);
        [SerializeField] float ultiRingLifeSec = 0.35f;

        class Effect
        {
            public Transform Root;
            public SpriteRenderer Sprite;
            public TextMeshPro Text;
            public Vector3 Velocity;
            public float Spin;
            public float Born, Life;
            public float StartScale, EndScale;
            public Color Color;
            public Character Follow;          // karakter görünmezse gizlenir
            public System.Func<bool> Alive;   // false olunca biter (kalkan)
            public Vector3 Target;            // birleşme: hedefe doğru
            public bool Converge;
        }

        readonly List<Effect> _effects = new List<Effect>();
        readonly Stack<Effect> _spritePool = new Stack<Effect>();
        static Sprite _ringSprite;

        void Awake() => _ringSprite ??= CreateRingSprite();

        void OnEnable()
        {
            GameEvents.Damaged += OnDamaged;
            GameEvents.Died += OnDied;
            GameEvents.CraftCompleted += OnCraftCompleted;
            GameEvents.AbilityUsed += OnAbilityUsed;
            GameEvents.ShieldBroken += OnShieldBroken;
            GameEvents.TreeHit += OnTreeHit;
            GameEvents.TreeFelled += OnTreeFelled;
        }

        void OnDisable()
        {
            GameEvents.Damaged -= OnDamaged;
            GameEvents.Died -= OnDied;
            GameEvents.CraftCompleted -= OnCraftCompleted;
            GameEvents.AbilityUsed -= OnAbilityUsed;
            GameEvents.ShieldBroken -= OnShieldBroken;
            GameEvents.TreeHit -= OnTreeHit;
            GameEvents.TreeFelled -= OnTreeFelled;
        }

        // ---------------- Olaylar ----------------

        // FR-23: hasar sayısı (ulti için hedefin kalan canı).
        void OnDamaged(Character victim, DamageInfo info)
        {
            if (victim == null || !victim.VisibleToPlayer) return;
            var go = new GameObject("HasarSayisi");
            var text = go.AddComponent<TextMeshPro>();
            text.text = info.Amount.ToString();
            text.fontSize = numberSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = numberColor;
            text.outlineWidth = 0.25f;
            text.outlineColor = new Color32(30, 18, 10, 255);
            text.sortingLayerID = SortingLayer.NameToID(EffectsLayer);
            text.sortingOrder = 20;
            go.transform.position = (Vector3)victim.Position + Vector3.up * numberHeightUnits;
            _effects.Add(new Effect { Root = go.transform, Text = text, Born = Time.time, Life = numberLifeSec,
                Velocity = Vector3.up * numberRiseUnits / numberLifeSec, StartScale = 1f, EndScale = 1f, Color = numberColor });
        }

        // FR-80: ölen karakterin yerine character.deathEffectSec süren yaprak dağılması.
        void OnDied(Character victim, Character killer)
        {
            if (victim == null || !victim.VisibleToPlayer) return;
            Burst(victim.Position, deathLeaves, Services.Config.character.deathEffectSec, burstSpeed);
        }

        // FR-45: üretim tamamlanınca yapraklar karaktere doğru birleşir.
        void OnCraftCompleted(Character c, RecipeId recipe, int newLevel)
        {
            if (c == null || !c.VisibleToPlayer) return;
            float radius = Units.ToUnits(Services.Config.character.radiusPx) * 2f;
            for (int i = 0; i < craftLeaves; i++)
            {
                float a = Mathf.PI * 2f * i / craftLeaves;
                var e = Leaf((Vector3)c.Position + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * radius, smallBurstLifeSec, i);
                e.Converge = true;
                e.Target = c.Position;
                e.Follow = c;
                e.EndScale = 0.3f;
            }
        }

        void OnAbilityUsed(Character c, AbilityId ability)
        {
            if (c == null) return;
            if (ability == AbilityId.Dash) DashTrail(c);
            else if (ability == AbilityId.Shield) ShieldRing(c);
            else if (ability == AbilityId.Ulti) StartCoroutine(UltiBlast(c));
        }

        // FR-27: kalkan kırılınca halka genişleyip söner.
        void OnShieldBroken(Character c)
        {
            if (c == null || !c.VisibleToPlayer) return;
            Ring(c.Position, shieldRadiusUnits, shieldRadiusUnits * 1.8f, smallBurstLifeSec, shieldColor, c);
        }

        // FR-35: vurulan ağaçtan birkaç yaprak; FR-36: devrilen ağaçtan çok yaprak.
        void OnTreeHit(ITree tree, Character by) => Burst(tree.Position, treeHitLeaves, smallBurstLifeSec, burstSpeed * 0.6f);
        void OnTreeFelled(ITree tree, Character by) => Burst(tree.Position, treeFellLeaves, Services.Config.character.deathEffectSec, burstSpeed);

        // ---------------- Efekt kurucular ----------------

        // 3.4: atılma süresince gövdenin solan kopyaları.
        void DashTrail(Character c)
        {
            var visual = c.GetComponentInChildren<CharacterVisual>();
            if (visual == null) return;
            StartCoroutine(DashTrailRoutine(c, visual));
        }

        System.Collections.IEnumerator DashTrailRoutine(Character c, CharacterVisual visual)
        {
            float duration = Services.Config.abilities.dashDurationSec;
            for (int i = 0; i < dashGhosts; i++)
            {
                if (c == null || !c.IsAlive) yield break;
                if (c.VisibleToPlayer && visual.BodySprite != null)
                {
                    var e = SpriteEffect(c.Position, visual.BodySprite, dashGhostColor, duration * 2f);
                    e.Sprite.flipX = visual.BodyFlipX;
                    e.Sprite.sharedMaterial = flashMaterial;
                    e.Sprite.sortingLayerName = "Characters";
                    e.Sprite.sortingOrder = -3;
                }
                yield return new WaitForSeconds(duration / dashGhosts);
            }
        }

        // FR-27: kalkan açıkken karakterin etrafında halka.
        void ShieldRing(Character c)
        {
            var abilities = c.Abilities;
            var e = SpriteEffect(c.Position, _ringSprite, shieldColor, float.PositiveInfinity);
            e.Follow = c;
            e.Alive = () => c != null && c.IsAlive && abilities != null && abilities.ShieldActive;
            float scale = shieldRadiusUnits * 2f / _ringSprite.bounds.size.x;
            e.StartScale = e.EndScale = scale;
            e.Root.localScale = Vector3.one * scale;
        }

        // FR-28: hazırlanma bitince ultiRadiusPx'e genişleyen patlama halkası.
        System.Collections.IEnumerator UltiBlast(Character c)
        {
            yield return new WaitForSeconds(Services.Config.abilities.ultiWindupSec);
            if (c == null || !c.IsAlive || !c.VisibleToPlayer) yield break;
            float radius = Units.ToUnits(Services.Config.abilities.ultiRadiusPx);
            Ring(c.Position, radius * 0.3f, radius, ultiRingLifeSec, ultiColor, null);
            Burst(c.Position, treeFellLeaves, ultiRingLifeSec * 2f, burstSpeed * 1.5f);
        }

        void Ring(Vector2 pos, float fromRadius, float toRadius, float life, Color color, Character follow)
        {
            var e = SpriteEffect(pos, _ringSprite, color, life);
            float unit = 2f / _ringSprite.bounds.size.x;
            e.StartScale = fromRadius * unit;
            e.EndScale = toRadius * unit;
            e.Follow = follow;
        }

        void Burst(Vector2 pos, int count, float life, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float a = Mathf.PI * 2f * (i + 0.5f * (i % 2)) / count;
                var e = Leaf(pos, life, i);
                e.Velocity = new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * speed * (0.6f + 0.4f * ((i * 7) % 5) / 4f);
                e.Spin = (i % 2 == 0 ? 1f : -1f) * 360f;
            }
        }

        Effect Leaf(Vector3 pos, float life, int index)
        {
            Sprite sprite = leafSprites != null && leafSprites.Length > 0 ? leafSprites[index % leafSprites.Length] : null;
            return SpriteEffect(pos, sprite, Color.white, life);
        }

        Effect SpriteEffect(Vector3 pos, Sprite sprite, Color color, float life)
        {
            var e = _spritePool.Count > 0 ? _spritePool.Pop() : NewSpriteEffect();
            e.Root.gameObject.SetActive(true);
            e.Root.position = pos;
            e.Root.rotation = Quaternion.identity;
            e.Root.localScale = Vector3.one;
            e.Sprite.sprite = sprite;
            e.Sprite.color = color;
            e.Sprite.flipX = false;
            e.Sprite.sharedMaterial = _defaultMaterial;
            e.Sprite.sortingLayerName = EffectsLayer;
            e.Sprite.sortingOrder = 0;
            e.Color = color;
            e.Born = Time.time;
            e.Life = life;
            e.Velocity = Vector3.zero;
            e.Spin = 0f;
            e.StartScale = e.EndScale = 1f;
            e.Follow = null;
            e.Alive = null;
            e.Converge = false;
            _effects.Add(e);
            return e;
        }

        Material _defaultMaterial;

        Effect NewSpriteEffect()
        {
            var go = new GameObject("Efekt");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            _defaultMaterial ??= sr.sharedMaterial;
            return new Effect { Root = go.transform, Sprite = sr };
        }

        // ---------------- Güncelleme ----------------

        void LateUpdate()
        {
            float now = Time.time;
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var e = _effects[i];
                bool done = e.Root == null || (e.Alive != null ? !e.Alive() : now - e.Born >= e.Life);
                if (done) { Recycle(i); continue; }

                float k = float.IsInfinity(e.Life) ? 0f : Mathf.Clamp01((now - e.Born) / e.Life);
                float dt = Time.deltaTime;
                if (e.Converge) e.Root.position = Vector3.Lerp(e.Root.position, e.Follow != null ? (Vector3)e.Follow.Position : e.Target, k);
                else if (e.Follow != null && e.Alive != null) e.Root.position = e.Follow.Position;
                else e.Root.position += e.Velocity * dt;
                if (e.Spin != 0f) e.Root.Rotate(0f, 0f, e.Spin * dt);
                if (e.StartScale != e.EndScale || e.Converge) e.Root.localScale = Vector3.one * Mathf.Lerp(e.StartScale, e.EndScale, k);

                bool hidden = e.Follow != null && !e.Follow.VisibleToPlayer;
                Color c = e.Color;
                c.a *= hidden ? 0f : 1f - (e.Alive != null ? 0f : k);
                if (e.Sprite != null) e.Sprite.color = c;
                if (e.Text != null) e.Text.color = c;
            }
        }

        void Recycle(int index)
        {
            var e = _effects[index];
            _effects.RemoveAt(index);
            if (e.Root == null) return;
            if (e.Text != null) { Destroy(e.Root.gameObject); return; }
            e.Root.gameObject.SetActive(false);
            _spritePool.Push(e);
        }

        // Halka: 32×32 piksellik, 1 piksel kalınlığında daire (PPU 16 → 2 birim çap).
        static Sprite CreateRingSprite()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            float r = size * 0.5f - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f));
                    pixels[y * size + x] = Mathf.Abs(d - r) < 0.75f ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
