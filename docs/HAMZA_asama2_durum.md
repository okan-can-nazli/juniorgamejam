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

# Aşama 2 – Botlar (dal `hamza/botlar`)

## Ne yapıldı

Dosyalar (`Scripts/Bots/`): `BotBrain.cs` (durum makinesi), `BotPerception.cs` (algı), `BotNavigator.cs` (yol izleme, takılma), `NavGrid.cs` (ızgara + A*).

- **Algı (BOT-02, BOT-03):** Her 0,2 sn'de bir; 650 px içindeki ve `Visibility.CanSee` true olan karakterler, 650 px içindeki yapraklar ve ağaçlar, alan durumu. Görünmeyen karakter hiçbir hesapta kullanılmaz.
- **Durum makinesi (BOT-03–BOT-07):** Öncelik Alana dön > Kaç > Saldır > Topla.
  - **Alana dön:** alan dışında veya 10 sn içinde dışarıda kalacaksa merkeze; alan dışındayken atılma açıksa kullanır.
  - **Kaç:** can %35 altında ve 500 px içinde görünen rakip varsa en yakın çalılığa (yoksa rakipten 500 px uzağa); rakip 200 px'ten yakınsa atılır; rakip 60 px'teyse kaçarken ona vurur. Can %50'ye çıkınca veya rakip 500 px dışına çıkınca çıkar.
  - **Saldır:** BOT-08 skoruyla hedef; yaklaşır, menzil ve yay içindeyken vurur (yeni hedefte 0,25 sn bekler); atılma açıksa hedef 160–300 px'teyken mesafeyi kapatır.
  - **Topla:** yaprak/ganimet → alan içindeki kesilebilir ağaç (keser) → ulu ağaç.
- **Tarif (BOT-10, 5.1):** Güvendeyken önce can iksiri (can %50 altı), değilse profil sırasındaki ilk üretilebilir geliştirme. Üretim sırasında yerinde durur.
- **Yetenekler (BOT-13):** Kalkan: can %50 altı ve 100 px'te rakip. Ulti: 120 px'te en az bir rakip. Atılma: Saldır, Kaç ve Alana dön'deki kurallarla.
- **Yol bulma (BOT-11):** 20 px'lik ızgara (`World.IsBlocked` ile), 8 yönlü A*, köşe kesmez, yol düz çizgiyle kısaltılır. Hedef düz görünüyorsa A* çalışmaz. Karede en fazla 1 bot yol hesaplar; bir bot en sık 0,2 sn'de bir yol yeniler. `TreeFelled` olayında kesilen ağacın çevresi yeniden değerlendirilir. Dünya değişirse (yeni sahne, yeni harita) ızgara yeniden kurulur.
- **Takılma (BOT-11):** 2 sn'de 20 px'ten az ilerleyen bot 200 px içinde rastgele (`Services.Rng`) açık bir noktaya yönelir.
- **Debug:** Editörde F4 ile Scene görünümünde her botun durumu (renkli çizgi, etiket) ve sıradaki yol noktası.
- Zone, Visibility ve World yalnızca `Services` üzerinden kullanılır; stub'larla da çalışır, Okan'ın gerçek servisleri gelince değişiklik gerekmez.

## Test (Unity, Play mode, geçici test betiği – commit'lenmedi)

Sandbox'ta birbirinden uzak 7 senaryo aynı anda, ardından alan + duvar senaryosu. **11/11 geçti:**

| Test | Sonuç |
| --- | --- |
| Ulti: 120 px içindeki rakip öldü | Geçti |
| Kalkan: can %44, 80 px'te rakip → kalkan açıldı | Geçti |
| Atılma: hedef 240 px'te → atıldı | Geçti |
| Kaç: can %30, rakip yakın → Kaç durumu; 500 px uzaklaştı | Geçti |
| Görüş: çalılıkta gizli (görünmeyen) rakibe saldırmadı | Geçti |
| Saldır: hedefe yürüdü, 10 vuruşta öldürdü, eleme bota yazıldı | Geçti |
| Tarif: önce can iksiri (40 → 80), sonra Güç 1 → 2 | Geçti |
| Alana dön: alan dışındaki bot Alana dön durumuna geçti | Geçti |
| A*: önündeki duvarın etrafından dolaşıp alana girdi | Geçti |

Test edilemeyenler: gerçek çalılık ve harita (Okan'ın servisleri henüz stub), takılma kurtarma, kaçarken vurma, 3 botun karede 1 ms bütçesi (Profiler, Aşama 3).

## Kararlar (inceleme için)

1. **Tarif yalnızca Topla ve Kaç durumunda** denenir; Saldır'da hedefe yürürken 1,5 sn durmasın diye. Alana dön'de bot zaten güvende değildir.
2. **Profili olmayan bot** (sandbox'a elle konan) Saldırgan önceliğini kullanır. Maçta `MatchManager` profili verir.
3. **Çalılık yoksa** (stub) kaçan bot rakipten 500 px uzaktaki noktaya gider.
4. **Yol yenileme** en sık `decisionIntervalSec`'te bir; hedef bir hücreden az kıpırdadıysa yenilenmez.

## Kalan işler

- [ ] Okan'ın `WorldService`, `SafeZone`, `VisibilityService`'i gelince gerçek haritada deneme (KT-13)
- [ ] Aşama 3: KT-13 (3 bot × 5 maç), KT-17, NFR-06, bot performansı, denge turu
