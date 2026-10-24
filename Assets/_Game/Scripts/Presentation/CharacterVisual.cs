using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Karakterin görseli (CharacterVisual.prefab, Character.prefab içinde). Yalnızca değer okur ve GameEvents dinler.
    /// - Baltalı gövde: yön (Facing) ve yürüme/bekleme (Velocity) animasyonu, atkı (Scarf), oyuncu halkası (FR-11, FR-63, 3.13).
    /// - Balta savurma: Swung gelince vuruş kareleri ve vuruş anında savurma yayı (SRS 3.3); ulti hazırken parıltı, hazırlanırken parlama (FR-28, 3.4).
    /// - Yaprak yığını: tehdit kademesine göre 6/12/18 yaprak (SRS 3.9, FR-60–FR-62).
    /// - Hasar parlaması 0,1 sn beyaz (FR-23); çalılıkta oyuncu yarı saydam, görünmeyen karakter tamamen gizli (FR-51, FR-53).
    /// - Karakter üstü can çubuğu (FR-64) ve tarif üretim çubuğu (UI-07).
    /// Kare düzeni (16×24 gövde/atkı, 24×24 baltalı): satır 0 aşağı, 1 yukarı, 2 yan (sol için aynalanır); 0–1 bekleme, 2–5 yürüme.
    /// </summary>
    public class CharacterVisual : MonoBehaviour
    {
        const string CharactersLayer = "Characters";
        const string PileLayer = "Pile";
        const string EffectsLayer = "Effects";
        const int FramesPerRow = 6;
        const int SwingFramesPerRow = 4;

        [Header("Baltalı gövde (18 kare) ve vuruş (12 kare: yön başına 4)")]
        [SerializeField] Sprite[] walkSprites;
        [SerializeField] Sprite[] swingSprites;

        [Header("Atkılar (18'er kare, gövdeyle aynı düzen)")]
        [SerializeField] Sprite[] blueScarfSprites;
        [SerializeField] Sprite[] purpleScarfSprites;
        [SerializeField] Sprite[] whiteScarfSprites;
        [SerializeField] Sprite[] blackScarfSprites;
        [SerializeField] Sprite playerRingSprite;

        [Header("Balta")]
        [SerializeField] Sprite swingArcSprite;
        [SerializeField] Color ultiReadyGlow = new Color(1f, 0.85f, 0.25f, 0.55f);
        [SerializeField] Color ultiWindupGlow = new Color(1f, 0.95f, 0.6f, 0.85f);
        [SerializeField] float glowPulseHz = 2f;
        [SerializeField] float glowScale = 1.12f;

        [Header("Yaprak yığını")]
        [SerializeField] Sprite[] pileLeafSprites;
        [SerializeField] float pileSwayDeg = 12f;
        [SerializeField] float pileSwayHz = 0.8f;

        [Header("Parlama (siluet malzemesi)")]
        [SerializeField] Material flashMaterial;

        [Header("Karakter üstü çubuklar (FR-64, UI-07)")]
        [SerializeField] bool showHealthBarOnPlayer = true;
        [SerializeField] bool showHealthBarOnBots = true;
        [SerializeField] float barHeightOffset = 1.7f;
        [SerializeField] float barWidthUnits = 0.95f;
        [SerializeField] float barThicknessUnits = 0.12f;

        [Header("Animasyon")]
        [SerializeField] float idleFps = 4f;
        [SerializeField] float walkFps = 8f;
        [SerializeField] float moveThresholdSq = 0.01f;

        Character _character;
        SpriteRenderer _ring, _glow, _body, _scarf, _flash, _arc;
        Transform _barsRoot, _pileRoot;
        SpriteRenderer _hpBorder, _hpFill, _craftBorder, _craftFill;
        GameObject _craftBarGroup;
        SpriteRenderer[] _pileLeaves = new SpriteRenderer[0];
        float[] _pileAngles = new float[0];

        static Sprite _unitPixel;
        float _animTimer;
        float _flashUntil = float.NegativeInfinity;
        float _swingStart = float.NegativeInfinity;
        Vector2 _swingFacing = Vector2.right;
        int _pileCount;
        float _pileFrom, _pileTo, _pileGrowStart = float.NegativeInfinity;

        /// <summary>Şu anki gövde karesi ve aynalama (VfxManager atılma izi için).</summary>
        public Sprite BodySprite => _body != null ? _body.sprite : null;
        public bool BodyFlipX => _body != null && _body.flipX;

        void Awake()
        {
            _character = GetComponentInParent<Character>();
            EnsureUnitPixel();
            _ring = Child("PlayerRing", CharactersLayer, -2);
            _glow = Child("Glow", CharactersLayer, -1);
            _body = Child("Body", CharactersLayer, 0);
            _scarf = Child("Scarf", CharactersLayer, 1);
            _flash = Child("Flash", CharactersLayer, 2);
            _arc = Child("SwingArc", EffectsLayer, 0);
            _ring.sprite = playerRingSprite;
            _arc.sprite = swingArcSprite;
            if (flashMaterial != null)
            {
                _glow.sharedMaterial = flashMaterial;
                _flash.sharedMaterial = flashMaterial;
            }
            _flash.color = Color.white;
            _glow.transform.localScale = Vector3.one * glowScale;
            EnsureBars();
            _pileRoot = new GameObject("Pile").transform;
            _pileRoot.SetParent(transform, false);
        }

        void OnEnable()
        {
            GameEvents.Damaged += OnDamaged;
            GameEvents.Swung += OnSwung;
            GameEvents.ThreatChanged += OnThreatChanged;
        }

        void OnDisable()
        {
            GameEvents.Damaged -= OnDamaged;
            GameEvents.Swung -= OnSwung;
            GameEvents.ThreatChanged -= OnThreatChanged;
        }

        void Start() => SetPileTier(_character.PileTier, true);

        void OnDamaged(Character victim, DamageInfo info)
        {
            if (victim == _character) _flashUntil = Time.time + Services.Config.character.hitFlashSec;   // FR-23
        }

        void OnSwung(Character c)
        {
            if (c != _character) return;
            _swingStart = Time.time;
            _swingFacing = c.Facing;
        }

        void OnThreatChanged(Character c, int score, PileTier tier)
        {
            if (c == _character) SetPileTier(tier, false);
        }

        void LateUpdate()
        {
            if (_character == null) return;
            bool visible = _character.IsAlive && _character.VisibleToPlayer;   // FR-51, FR-80
            _body.enabled = _scarf.enabled = visible;
            _ring.enabled = visible && _character.IsPlayer;
            _barsRoot.gameObject.SetActive(visible);
            _pileRoot.gameObject.SetActive(visible);
            if (!visible)
            {
                _glow.enabled = _flash.enabled = _arc.enabled = false;
                return;
            }

            float alpha = _character.IsPlayer && _character.IsInBush ? Services.Config.bushes.playerInBushAlpha : 1f;   // FR-53
            UpdateBody(alpha);
            UpdateGlowAndFlash(alpha);
            UpdateArc(alpha);
            UpdatePile(alpha);
            UpdateHealthBar(alpha);
            UpdateCraftBar();
        }

        // ---------------- Gövde, atkı, vuruş ----------------

        void UpdateBody(float alpha)
        {
            var axe = Services.Config.axe;
            float sinceSwing = Time.time - _swingStart;
            // Vuruş kareleri: hasar anına (hitDelaySec) kadar 0–1 (kaldırma), sonra 2 (vuruş) ve 3 (bitiş), her biri hitDelaySec.
            bool swinging = sinceSwing < axe.hitDelaySec * 3f;
            Vector2 facing = swinging ? _swingFacing : _character.Facing;
            if (facing.sqrMagnitude < 0.0001f) facing = Vector2.down;

            int row;
            bool flip = false;
            if (Mathf.Abs(facing.x) >= Mathf.Abs(facing.y)) { row = 2; flip = facing.x < 0f; }
            else row = facing.y > 0f ? 1 : 0;

            bool moving = _character.Velocity.sqrMagnitude > moveThresholdSq;
            _animTimer += Time.deltaTime * (moving ? walkFps : idleFps);
            int col = moving ? 2 + Mathf.FloorToInt(_animTimer) % 4 : Mathf.FloorToInt(_animTimer) % 2;

            Sprite body;
            if (swinging && swingSprites != null && swingSprites.Length >= 3 * SwingFramesPerRow)
            {
                int swingCol = sinceSwing < axe.hitDelaySec ? (sinceSwing < axe.hitDelaySec * 0.5f ? 0 : 1)
                    : sinceSwing < axe.hitDelaySec * 2f ? 2 : 3;
                body = swingSprites[row * SwingFramesPerRow + swingCol];
                col = 0;   // atkı vuruşta bekleme karesinde durur
            }
            else body = Pick(walkSprites, row * FramesPerRow + col);

            _body.sprite = body;
            _body.flipX = _scarf.flipX = _flash.flipX = _glow.flipX = flip;
            _scarf.sprite = Pick(ScarfSprites(_character.Scarf), row * FramesPerRow + col);
            _flash.sprite = _glow.sprite = body;

            var tint = new Color(1f, 1f, 1f, alpha);
            _body.color = _scarf.color = tint;
            _ring.color = tint;
        }

        // Hasar: 0,1 sn beyaz siluet (FR-23). Ulti hazır: altın parıltı nabzı (SRS 3.3); hazırlanırken: güçlü parlama (SRS 3.4).
        void UpdateGlowAndFlash(float alpha)
        {
            bool flashing = Time.time < _flashUntil;
            _flash.enabled = flashing && flashMaterial != null;
            if (_flash.enabled) _flash.color = new Color(1f, 1f, 1f, alpha);

            var abilities = _character.Abilities;
            bool windup = abilities != null && abilities.UltiWindingUp;
            bool ready = abilities != null && abilities.State(AbilityId.Ulti) == AbilityState.Ready;
            _glow.enabled = (windup || ready) && flashMaterial != null;
            if (!_glow.enabled) return;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * glowPulseHz * Mathf.PI * 2f);
            Color c = windup ? ultiWindupGlow : ultiReadyGlow;
            c.a *= (windup ? 1f : pulse) * alpha;
            _glow.color = c;
            // Ayaktaki pivottan büyütülen parıltı yukarı kayar; görselin ortasına göre hizala.
            if (_glow.sprite != null) _glow.transform.localPosition = new Vector3(0f, -(glowScale - 1f) * _glow.sprite.bounds.center.y, 0f);
        }

        // Savurma yayı: hasar anında (hitDelaySec) bakış yönünde görünür, bir hitDelaySec içinde söner (SRS 3.3).
        void UpdateArc(float alpha)
        {
            float t = Time.time - _swingStart - Services.Config.axe.hitDelaySec;
            float life = Services.Config.axe.hitDelaySec;
            _arc.enabled = swingArcSprite != null && t >= 0f && t < life;
            if (!_arc.enabled) return;
            float angle = Mathf.Atan2(_swingFacing.y, _swingFacing.x) * Mathf.Rad2Deg;
            _arc.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            _arc.color = new Color(1f, 1f, 1f, alpha * (1f - t / life));
        }

        // ---------------- Yaprak yığını (SRS 3.9) ----------------

        void SetPileTier(PileTier tier, bool instant)
        {
            var threat = Services.Config.threat;
            int count = threat.pileLeafCount[(int)tier];
            float radius = Units.ToUnits(Services.Config.character.radiusPx) * threat.pileRadiusScale[(int)tier];
            _pileFrom = instant ? radius : CurrentPileRadius();
            _pileTo = radius;
            _pileGrowStart = instant ? float.NegativeInfinity : Time.time;
            EnsurePileLeaves(count);
            _pileCount = count;
        }

        float CurrentPileRadius()
        {
            float grow = Services.Config.threat.pileGrowSec;
            float k = grow > 0f ? Mathf.Clamp01((Time.time - _pileGrowStart) / grow) : 1f;
            return Mathf.Lerp(_pileFrom, _pileTo, k);
        }

        void EnsurePileLeaves(int count)
        {
            if (count > _pileLeaves.Length)
            {
                var leaves = new SpriteRenderer[count];
                var angles = new float[count];
                for (int i = 0; i < count; i++)
                {
                    if (i < _pileLeaves.Length) { leaves[i] = _pileLeaves[i]; angles[i] = _pileAngles[i]; continue; }
                    var sr = new GameObject("Leaf" + i).AddComponent<SpriteRenderer>();
                    sr.transform.SetParent(_pileRoot, false);
                    sr.sortingLayerName = PileLayer;
                    sr.sortingOrder = i;
                    if (pileLeafSprites != null && pileLeafSprites.Length > 0) sr.sprite = pileLeafSprites[i % pileLeafSprites.Length];
                    leaves[i] = sr;
                }
                _pileLeaves = leaves;
                _pileAngles = angles;
            }
            for (int i = 0; i < _pileLeaves.Length; i++)
            {
                _pileLeaves[i].enabled = i < count;
                // Yapraklar halkaya eşit aralıkla, her kademede yeniden dağılır; çift sıralar yarım adım kaydırılır.
                if (i < count) _pileAngles[i] = 360f * i / count + (i % 2) * 180f / count;
            }
        }

        void UpdatePile(float alpha)
        {
            if (_pileCount == 0) return;
            float radius = CurrentPileRadius();
            float sway = Mathf.Sin(Time.time * pileSwayHz * Mathf.PI * 2f) * pileSwayDeg;
            var tint = new Color(1f, 1f, 1f, alpha);
            for (int i = 0; i < _pileCount; i++)
            {
                float a = (_pileAngles[i] + sway) * Mathf.Deg2Rad;
                _pileLeaves[i].transform.localPosition = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius * 0.6f, 0f);
                _pileLeaves[i].transform.localRotation = Quaternion.Euler(0f, 0f, _pileAngles[i] + sway * 2f);
                _pileLeaves[i].color = tint;
            }
        }

        // ---------------- Karakter üstü çubuklar ----------------

        void UpdateHealthBar(float alpha)
        {
            bool show = _character.IsPlayer ? showHealthBarOnPlayer : showHealthBarOnBots;   // FR-64
            _hpBorder.enabled = _hpFill.enabled = show;
            if (!show) return;
            float ratio = Mathf.Clamp01((float)_character.Health.Current / Mathf.Max(1, _character.Health.Max));
            float half = barWidthUnits * 0.5f, pad = 0.04f;
            _hpBorder.transform.localPosition = new Vector3(-half - pad, 0f, 0f);
            _hpBorder.transform.localScale = new Vector3(barWidthUnits + pad * 2f, barThicknessUnits + pad * 2f, 1f);
            _hpFill.transform.localPosition = new Vector3(-half, 0f, 0f);
            _hpFill.transform.localScale = new Vector3(barWidthUnits * ratio, barThicknessUnits, 1f);
            Color color = !_character.IsPlayer ? new Color(0.90f, 0.22f, 0.20f)
                : ratio > 0.5f ? new Color(0.22f, 0.85f, 0.27f)
                : ratio > 0.25f ? new Color(1f, 0.85f, 0.13f) : new Color(0.90f, 0.18f, 0.18f);
            color.a = alpha;
            _hpFill.color = color;
        }

        void UpdateCraftBar()
        {
            bool crafting = _character.Crafting.IsCrafting;
            if (_craftBarGroup.activeSelf != crafting) _craftBarGroup.SetActive(crafting);
            if (!crafting) return;
            float progress = Mathf.Clamp01(_character.Crafting.Progress01);
            float half = barWidthUnits * 0.5f, pad = 0.03f, thickness = barThicknessUnits * 0.8f;
            _craftBorder.transform.localPosition = new Vector3(-half - pad, 0f, 0f);
            _craftBorder.transform.localScale = new Vector3(barWidthUnits + pad * 2f, thickness + pad * 2f, 1f);
            _craftFill.transform.localPosition = new Vector3(-half, 0f, 0f);
            _craftFill.transform.localScale = new Vector3(barWidthUnits * progress, thickness, 1f);
        }

        // ---------------- Kurulum ----------------

        Sprite[] ScarfSprites(ScarfColor scarf) => scarf switch
        {
            ScarfColor.Purple => purpleScarfSprites,
            ScarfColor.White => whiteScarfSprites,
            ScarfColor.Black => blackScarfSprites,
            _ => blueScarfSprites,
        };

        static Sprite Pick(Sprite[] sprites, int index) =>
            sprites != null && index >= 0 && index < sprites.Length ? sprites[index] : null;

        SpriteRenderer Child(string childName, string layer, int order)
        {
            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = layer;
            sr.sortingOrder = order;
            return sr;
        }

        void EnsureBars()
        {
            _barsRoot = new GameObject("OverheadBars").transform;
            _barsRoot.SetParent(transform, false);
            _barsRoot.localPosition = new Vector3(0f, barHeightOffset, 0f);
            _hpBorder = Bar(_barsRoot, "HpBarBorder", 10, new Color(0.10f, 0.06f, 0.04f, 0.92f));
            _hpFill = Bar(_barsRoot, "HpBarFill", 11, Color.green);
            _craftBarGroup = new GameObject("CraftBarGroup");
            _craftBarGroup.transform.SetParent(_barsRoot, false);
            _craftBarGroup.transform.localPosition = new Vector3(0f, barThicknessUnits + 0.08f, 0f);
            _craftBorder = Bar(_craftBarGroup.transform, "CraftBorder", 10, new Color(0.10f, 0.06f, 0.04f, 0.92f));
            _craftFill = Bar(_craftBarGroup.transform, "CraftFill", 11, new Color(1f, 0.85f, 0.13f, 1f));
            _craftBarGroup.SetActive(false);
        }

        static SpriteRenderer Bar(Transform parent, string barName, int order, Color color)
        {
            var go = new GameObject(barName);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _unitPixel;
            sr.color = color;
            sr.sortingLayerName = CharactersLayer;
            sr.sortingOrder = order;
            return sr;
        }

        // Çubuklar için 1×1 beyaz piksel; pivot sol-orta, böylece çubuk soldan sağa dolar.
        static void EnsureUnitPixel()
        {
            if (_unitPixel != null) return;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _unitPixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }
    }
}
