using System;
using System.Reflection;
using UnityEngine;
using LeafBrawl.Shared;
using LeafBrawl.Characters;

namespace LeafBrawl.Presentation
{
    /// <summary>
    /// Karakter gövdesi, atkısı, ayak halkası, hasar parlaması ve
    /// karakterin üstündeki ince can barı ile üretim çubuğunu yönetir.
    /// SRS: 3.13, FR-11, FR-23, FR-51, FR-53, FR-63, FR-64, UI-07
    /// </summary>
    public class CharacterVisual : MonoBehaviour
    {
        private const string SortingLayerName = "Characters";
        private const int FrameCountPerRow = 6;

        [Header("Karakter Sprite Dizileri (18 Kare: 0-5 Aşağı, 6-11 Yukarı, 12-17 Yan)")]
        [SerializeField] private Sprite[] bodySprites;
        [SerializeField] private Sprite[] blueScarfSprites;
        [SerializeField] private Sprite[] purpleScarfSprites;
        [SerializeField] private Sprite[] whiteScarfSprites;
        [SerializeField] private Sprite[] blackScarfSprites;
        [SerializeField] private Sprite playerRingSprite;

        [Header("Üst İnce Can Barı Ayarları (FR-64)")]
        [SerializeField] private bool showHealthBarOnPlayer = true;
        [SerializeField] private bool showHealthBarOnBots = true;
        [SerializeField] private float barHeightOffset = 1.15f;
        [SerializeField] private float barWidthUnits = 0.95f;
        [SerializeField] private float barThicknessUnits = 0.12f;

        [Header("Animasyon Ayarları")]
        [SerializeField] private float idleFps = 4f;
        [SerializeField] private float walkFps = 8f;
        [SerializeField] private float moveThresholdSq = 0.01f;
        [SerializeField] private float defaultBushAlpha = 0.5f;

        private Character _character;
        private SpriteRenderer _bodyRenderer;
        private SpriteRenderer _scarfRenderer;
        private SpriteRenderer _playerRingRenderer;

        // İnce Can Barı Bileşenleri
        private Transform _barsRoot;
        private SpriteRenderer _hpBorderRenderer;
        private SpriteRenderer _hpFillRenderer;

        // Tarif Üretim Barı Bileşenleri (UI-07: 1,5 sn)
        private GameObject _craftBarGroup;
        private SpriteRenderer _craftBorderRenderer;
        private SpriteRenderer _craftFillRenderer;

        private static Sprite _unitPixelSprite;
        private float _animTimer;
        private float _hitFlashTimer;

        private void Awake()
        {
            _character = GetComponentInParent<Character>();
            EnsureUnitPixelSprite();
            EnsureCharacterRenderers();
            EnsureOverheadBars();
        }

        private void OnEnable()
        {
            GameEvents.Damaged += OnCharacterDamaged;
        }

        private void OnDisable()
        {
            GameEvents.Damaged -= OnCharacterDamaged;
        }

        private void LateUpdate()
        {
            if (_character == null)
            {
                _character = GetComponentInParent<Character>();
                if (_character == null) return;
            }

            UpdateVisuals(Time.deltaTime);
        }

        private void OnCharacterDamaged(Character victim, DamageInfo info)
        {
            if (victim != null && victim == _character)
            {
                // FR-23: Hasar alan karakter 0,1 sn beyaz yanıp söner
                _hitFlashTimer = 0.1f;
            }
        }

        private static void EnsureUnitPixelSprite()
        {
            if (_unitPixelSprite != null) return;
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            // Pivot sol-orta (0, 0.5f) yapıyoruz ki bar soldan sağa doğru incelip azalsın
            _unitPixelSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }

        private void EnsureCharacterRenderers()
        {
            _playerRingRenderer = GetOrCreateChildRenderer("PlayerRing", -1);
            _bodyRenderer       = GetOrCreateChildRenderer("Body", 0);
            _scarfRenderer      = GetOrCreateChildRenderer("Scarf", 1);

            if (playerRingSprite != null)
            {
                _playerRingRenderer.sprite = playerRingSprite;
            }
        }

        private void EnsureOverheadBars()
        {
            Transform existing = transform.Find("OverheadBars");
            if (existing == null)
            {
                GameObject rootObj = new GameObject("OverheadBars");
                rootObj.transform.SetParent(transform, false);
                _barsRoot = rootObj.transform;
            }
            else
            {
                _barsRoot = existing;
            }
            _barsRoot.localPosition = new Vector3(0f, barHeightOffset, 0f);

            // 1. İnce Can Barı (Arka Çerçeve + İç Dolgu)
            _hpBorderRenderer = GetOrCreateBarRenderer(_barsRoot, "HpBarBorder", 10, new Color(0.10f, 0.06f, 0.04f, 0.92f));
            _hpFillRenderer   = GetOrCreateBarRenderer(_barsRoot, "HpBarFill",   11, new Color(0.22f, 0.85f, 0.27f, 1f));

            // 2. Üstte 1,5 sn Dolan Tarif Üretim Çubuğu (UI-07)
            Transform craftTr = _barsRoot.Find("CraftBarGroup");
            if (craftTr == null)
            {
                _craftBarGroup = new GameObject("CraftBarGroup");
                _craftBarGroup.transform.SetParent(_barsRoot, false);
                _craftBarGroup.transform.localPosition = new Vector3(0f, barThicknessUnits + 0.08f, 0f);
            }
            else
            {
                _craftBarGroup = craftTr.gameObject;
            }

            _craftBorderRenderer = GetOrCreateBarRenderer(_craftBarGroup.transform, "CraftBorder", 10, new Color(0.10f, 0.06f, 0.04f, 0.92f));
            _craftFillRenderer   = GetOrCreateBarRenderer(_craftBarGroup.transform, "CraftFill",   11, new Color(1.00f, 0.85f, 0.13f, 1f));
        }

        private SpriteRenderer GetOrCreateChildRenderer(string childName, int sortingOrder)
        {
            Transform tr = transform.Find(childName);
            if (tr == null)
            {
                GameObject go = new GameObject(childName);
                go.transform.SetParent(transform, false);
                tr = go.transform;
            }
            SpriteRenderer sr = tr.GetComponent<SpriteRenderer>();
            if (sr == null) sr = tr.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = SortingLayerName;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        private SpriteRenderer GetOrCreateBarRenderer(Transform parent, string name, int sortingOrder, Color color)
        {
            Transform tr = parent.Find(name);
            if (tr == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent, false);
                tr = go.transform;
            }
            SpriteRenderer sr = tr.GetComponent<SpriteRenderer>();
            if (sr == null) sr = tr.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = _unitPixelSprite;
            sr.color = color;
            sr.sortingLayerName = SortingLayerName;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        private void UpdateVisuals(float dt)
        {
            // 1. Görünürlük ve Ölüm Kontrolü (FR-51, FR-64, FR-80)
            bool visible = _character.IsAlive && _character.VisibleToPlayer;
            _bodyRenderer.enabled = visible;
            _scarfRenderer.enabled = visible;
            _playerRingRenderer.enabled = visible && _character.IsPlayer;

            if (_barsRoot != null)
            {
                _barsRoot.gameObject.SetActive(visible);
            }

            if (!visible) return;

            // 2. Yön (Facing) ve Hareket (Velocity) Animasyonu
            Vector2 facing = _character.Facing.sqrMagnitude > 0.0001f ? _character.Facing : Vector2.down;
            int dirRow;
            bool flipX = false;

            if (Mathf.Abs(facing.x) >= Mathf.Abs(facing.y))
            {
                dirRow = 2; // Yan
                flipX = facing.x < -0.001f;
            }
            else if (facing.y > 0f)
            {
                dirRow = 1; // Yukarı
            }
            else
            {
                dirRow = 0; // Aşağı
            }

            bool isMoving = _character.Velocity.sqrMagnitude > moveThresholdSq;
            _animTimer += dt * (isMoving ? walkFps : idleFps);

            int col = isMoving ? 2 + (Mathf.FloorToInt(_animTimer) % 4) : (Mathf.FloorToInt(_animTimer) % 2);
            int idx = dirRow * FrameCountPerRow + col;

            _bodyRenderer.flipX = flipX;
            if (bodySprites != null && idx >= 0 && idx < bodySprites.Length)
            {
                _bodyRenderer.sprite = bodySprites[idx];
            }

            _scarfRenderer.flipX = flipX;
            Sprite[] scarfArr = GetScarfSpriteArray(_character.Scarf);
            if (scarfArr != null && idx >= 0 && idx < scarfArr.Length)
            {
                _scarfRenderer.sprite = scarfArr[idx];
            }

            // 3. Çalılık Yarı Saydamlığı (FR-53) ve Hasar Parlaması (FR-23)
            if (_hitFlashTimer > 0f)
            {
                _hitFlashTimer -= dt;
            }

            float alpha = (_character.IsPlayer && _character.IsInBush) ? ReadPlayerInBushAlpha() : 1f;
            Color tint = (_hitFlashTimer > 0f) ? new Color(1f, 0.35f, 0.35f, alpha) : new Color(1f, 1f, 1f, alpha);

            _bodyRenderer.color = tint;
            _scarfRenderer.color = tint;
            if (_playerRingRenderer.enabled)
            {
                _playerRingRenderer.color = new Color(1f, 1f, 1f, alpha);
            }

            // 4. Karakter Üstü İnce Can Barı Güncellemesi (Hasar yedikçe azalır - FR-64)
            UpdateOverheadHealthBar(alpha);

            // 5. Karakter Üstü 1,5 sn Tarif Üretim Çubuğu (UI-07)
            UpdateOverheadCraftBar();
        }

        private void UpdateOverheadHealthBar(float alpha)
        {
            bool showHp = _character.IsPlayer ? showHealthBarOnPlayer : showHealthBarOnBots;
            _hpBorderRenderer.enabled = showHp;
            _hpFillRenderer.enabled = showHp;
            if (!showHp || _character.Health == null) return;

            float maxHp = Mathf.Max(1, _character.Health.Max);
            float curHp = Mathf.Clamp(_character.Health.Current, 0, maxHp);
            float ratio = curHp / maxHp;

            float halfW = barWidthUnits * 0.5f;
            float borderPad = 0.04f;

            // Arka Siyah Çerçeve
            _hpBorderRenderer.transform.localPosition = new Vector3(-halfW - borderPad, 0f, 0f);
            _hpBorderRenderer.transform.localScale = new Vector3(barWidthUnits + borderPad * 2f, barThicknessUnits + borderPad * 2f, 1f);

            // İç Can Dolgusu (Hasar yedikçe ratio azalır ve bar incelir/kısalır)
            _hpFillRenderer.transform.localPosition = new Vector3(-halfW, 0f, 0f);
            _hpFillRenderer.transform.localScale = new Vector3(barWidthUnits * ratio, barThicknessUnits, 1f);

            // Oyuncunun barı yeşil (canı %30 altına inince sarı/kırmızı), rakiplerin barı kırmızı-turuncu
            Color barColor;
            if (_character.IsPlayer)
            {
                barColor = ratio > 0.5f
                    ? new Color(0.22f, 0.85f, 0.27f, alpha)
                    : (ratio > 0.25f ? new Color(1f, 0.85f, 0.13f, alpha) : new Color(0.90f, 0.18f, 0.18f, alpha));
            }
            else
            {
                barColor = new Color(0.90f, 0.22f, 0.20f, alpha);
            }
            _hpFillRenderer.color = barColor;
        }

        private void UpdateOverheadCraftBar()
        {
            bool isCrafting = _character.Crafting != null && _character.Crafting.IsCrafting;
            if (_craftBarGroup.activeSelf != isCrafting)
            {
                _craftBarGroup.SetActive(isCrafting);
            }
            if (!isCrafting) return;

            float progress = Mathf.Clamp01(_character.Crafting.Progress01);
            float halfW = barWidthUnits * 0.5f;
            float borderPad = 0.03f;

            _craftBorderRenderer.transform.localPosition = new Vector3(-halfW - borderPad, 0f, 0f);
            _craftBorderRenderer.transform.localScale = new Vector3(barWidthUnits + borderPad * 2f, (barThicknessUnits * 0.8f) + borderPad * 2f, 1f);

            _craftFillRenderer.transform.localPosition = new Vector3(-halfW, 0f, 0f);
            _craftFillRenderer.transform.localScale = new Vector3(barWidthUnits * progress, barThicknessUnits * 0.8f, 1f);
        }

        private Sprite[] GetScarfSpriteArray(ScarfColor scarf)
        {
            switch (scarf)
            {
                case ScarfColor.Blue:   return blueScarfSprites;
                case ScarfColor.Purple: return purpleScarfSprites;
                case ScarfColor.White:  return whiteScarfSprites;
                case ScarfColor.Black:  return blackScarfSprites;
                default:                return blueScarfSprites;
            }
        }

        private float ReadPlayerInBushAlpha()
        {
            try
            {
                object config = Services.Config;
                if (config == null) return defaultBushAlpha;
                Type ct = config.GetType();
                object bushes = ct.GetField("bushes", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(config)
                             ?? ct.GetProperty("bushes", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(config);
                if (bushes == null) return defaultBushAlpha;
                Type bt = bushes.GetType();
                object val = bt.GetField("playerInBushAlpha", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(bushes)
                          ?? bt.GetProperty("playerInBushAlpha", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(bushes);
                if (val != null) return Convert.ToSingle(val);
            }
            catch { }
            return defaultBushAlpha;
        }
    }
}
