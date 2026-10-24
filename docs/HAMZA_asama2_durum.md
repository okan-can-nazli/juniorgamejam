# Hamza – Aşama 2 durumu: 5. seviye yetenekleri

24 Ekim 2026 · Muhammed Hamza Karamanlı · dal `hamza/asama2-yetenekler`

Görev dosyamdaki ([HAMZA_gorevler.md](gorev_dagilimi/HAMZA_gorevler.md)) Aşama 2 › Yetenekler bölümü. Aşama 1 için bkz. [HAMZA_asama1_durum.md](HAMZA_asama1_durum.md). Yalnızca kendi dosyalarım değişti: `Scripts/Combat/AbilityController.cs`, `Scripts/Combat/Axe.cs`, `Scenes/Sandbox/Sandbox_Hamza.unity`.

## Ne yapıldı

### `AbilityController` (SRS 3.4, FR-26–FR-29)

- **Kilit açma:** `GameEvents.LevelChanged` dinlenir. Hız 5 → Atılma, Sağlık 5 → Kalkan, Güç 5 → Ulti `Ready` olur ve `AbilityUnlocked` yayınlanır. Başlangıç seviyesi zaten 5 ise (`debug.json`) `Start`'ta da açılır.
- **Atılma (Boşluk):** Hareket yönünde (hareket yoksa bakış yönünde) `Motor.Dash(yön, 160 px, 0,15 sn)`. 3 sn bekleme, tekrar kullanılır. `AbilityUsed(Dash)`.
- **Kalkan (Q):** Maçta bir kez, 6 sn açık. `IDamageInterceptor` olarak rakipten gelen **ilk** vuruşu (balta veya ulti) emer, kırılır ve `ShieldBroken` yayınlar. Alan hasarını emmez. Süre dolarsa kırılmadan kapanır.
- **Ulti (E):** Maçta bir kez. 0,4 sn hazırlanma: `AbilityUsed(Ulti)` yayınlanır, karakter durur ve savuramaz. Ardından 120 px içindeki bütün rakipler ölür, 120 px içindeki bütün kesilebilir ağaçlara `IsUlti` hasarı gider. Kalkanlı rakibin kalkanı kırılır, rakip yaşar.
- **FR-29:** Kullanılmış tek seferlik yetenek tekrar çalışmaz. Üretim sırasında (SRS 3.7) ve ulti hazırlanırken hiçbir yetenek çalışmaz. Geri sayımda intent zaten sıfırdır.
- **HUD için:** `State(AbilityId)` → Locked / Ready / Cooldown / Used; `CooldownLeft(AbilityId)` atılmanın kalan beklemesi.
- **Görsel için (Furkan):** `character.Abilities.ShieldActive` (kalkan halkası, FR-27) ve `character.Abilities.UltiWindingUp` (hazırlanırken parlama). Ulti hazırken balta parlaması için `State(AbilityId.Ulti) == Ready`.

### `Axe` güncellemesi

- Ulti hazırlanırken savurma yok ve hız çarpanı 0. `SpeedMultiplier`'ı artık yalnızca `Axe` yazar (ulti 0, savurma 0,5, diğer 1); böylece iki sistem birbirinin değerini ezmez.

### Sandbox

- `Sandbox_Hamza.unity`'ye kalıcı bir **Bot** eklendi (oyuncudan 200 px sağda). Play'e basınca bot oyuncuya yürüyüp vurur.

## Test (Unity 6000.6.7f1, Unity MCP ile, Play mode)

Sandbox'ta geçici bir test karakteri ve iki hedefle otomatik test çalıştırıldı. **16/16 geçti:**

| Test | Sonuç |
| --- | --- |
| Seviye 5'te üç yetenek açılıyor | Geçti |
| Atılma ~160 px ilerliyor | Geçti (**178 px**, aşağıdaki nota bakın) |
| Atılma sonrası 3 sn bekleme; beklemede atılma çalışmıyor | Geçti |
| Kalkan açılıyor; alan hasarını emmiyor (160 → 155) | Geçti |
| Kalkan ilk vuruşu emip kırılıyor; sonraki vuruş işliyor | Geçti |
| Kalkan ikinci kez açılmıyor | Geçti |
| Ulti hazırlanırken karakter duruyor, hedef henüz yaşıyor | Geçti |
| Ulti 120 px içindekini öldürüyor, dışındakine dokunmuyor | Geçti |
| Ulti eleme sayısına yazılıyor (FR-28) | Geçti |
| Ulti ikinci kez çalışmıyor | Geçti |

Console'da hata yok. Test betiği geçiciydi; commit'lenmedi.

## Kararlar (inceleme için)

1. **Ulti hasarı = hedefin kalan canı** (görev dosyasında `int.MaxValue` yazıyor). Sonuç aynı (hedef ölür), ama FR-23'teki hasar sayısı ekranda "2147483647" yerine anlamlı bir sayı gösterir. Ağaçlara `int.MaxValue` + `IsUlti` gönderilir.
2. **Kalkan yalnızca rakipten gelen hasarı emer** (`Source != null` ve `IsZone == false`).
3. **`CooldownLeft`** yalnızca atılma için anlamlı; kalkan ve ulti tek seferlik olduğu için 0 döner. Kalkanın kalan süresi gerekirse eklenebilir.

## Bulunan bir sorun (Yiğit'in alanı)

**Atılma 160 yerine ~178 px gidiyor.** `CharacterMotor.FixedUpdate` atılma süresini `_dashTimeLeft -= Time.fixedDeltaTime` ile sayıyor. 0,15 sn / (1/60 sn) = 9 adım olmalı, ama kayan nokta yuvarlamasıyla 10. adım da atılıyor: 160 × 10/9 ≈ 178 px. Öneri: adım sayısını baştan hesaplamak (`Mathf.RoundToInt(süre / fixedDeltaTime)`) ya da son adımda kalan mesafe kadar ilerlemek. Issue olarak açılacak.

## Kalan işler (Aşama 2 – Botlar)

- [ ] 4 durumlu bot makinesi: Alana dön > Kaç > Saldır > Topla (BOT-03–BOT-07)
- [ ] Bot tarif üretimi ve profiller (BOT-10, 5.1)
- [ ] Bot yetenek kullanımı: kalkan, ulti, atılma (BOT-13, BOT-06)
- [ ] Yol bulma ve takılma (BOT-11)
- [ ] Debug: durum adı ve hedef çizgisi (F4)
