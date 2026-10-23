# Hamza – Aşama 1 durumu: balta ve basit bot

24 Ekim 2026 · Muhammed Hamza Karamanlı · dal `hamza/asama1-balta-bot`

Bu dosya, görev dosyamdaki ([HAMZA_gorevler.md](gorev_dagilimi/HAMZA_gorevler.md)) Aşama 1 işlerinin nerede olduğunu, hangi kararları aldığımı ve sırada ne olduğunu anlatır. Yalnızca kendi klasörlerim değişti: `Scripts/Combat/Axe.cs`, `Scripts/Bots/BotBrain.cs`.

## Ne yapıldı

### Balta – `Scripts/Combat/Axe.cs` (SRS 3.3, FR-20, FR-21, FR-22)

- `CurrentIntent.Swing` true ve bekleme bitmişse savurur. Basılı tutulunca bekleme bittikçe yeniden savrulur (FR-20).
- Üretim sırasında (`Crafting.IsCrafting`) savurma olmaz (SRS 3.7). Tab paneli açıkken `PlayerIntentSource` zaten `Swing = false` verir.
- Savurma başlayınca: `GameEvents.RaiseSwung`, `character.MarkSwing()` ("güvende" kuralı için), `SpeedMultiplier = 0,5` (FR-22).
- 0,12 sn sonra vuruş çözülür (FR-21):
  - Karakter merkezinden 60 px ve bakış yönünün ±50°'si içindeki **bütün** canlı rakiplere bir kez `ApplyDamage(Amount = Stats.Damage, Source = self)`. Her isabette `HitLanded` yayınlanır.
  - Merkezden hedefe çizilen çizgi `Obstacle` katmanına (gövde, kaya) çarpıyorsa o hedefe vuruş işlemez.
  - Aynı yay içinde `Tree` katmanındaki en yakın **bir** ağaca da `ApplyDamage` gönderilir (`axe.maxTreesPerSwing`). Ağacın ne yapacağı Okan'ın işi.
- Editörde (yalnızca Scene görünümü, Gizmos) savurma yayı ve menzil çizilir; savururken kırmızı.
- Bütün sayılar `balance.json` → `axe` bölümünden gelir.

### Basit bot – `Scripts/Bots/BotBrain.cs` (BOT-06, BOT-07, BOT-09; kısmen BOT-02, BOT-03, BOT-08)

- `BotBrain : IIntentSource`. Bot yalnızca `CharacterIntent` üretir; konum, can veya envantere doğrudan yazmaz (BOT-01).
- Her 0,2 sn'de bir karar verir (BOT-03). İki durum:
  - **Saldır:** 500 px içinde görünen rakip varsa. Hedef seçimi (yığın kademesi × 200) − mesafe (BOT-08). Hedefe yaklaşır, menzildeyken ve yay içindeyken savurur. Yeni hedefi ilk gördükten 0,25 sn sonra vurmaya başlar (BOT-09).
  - **Topla:** Rakip yoksa. En yakın yaprak/ganimet → yoksa alan içindeki en yakın kesilebilir ağacı keser → yoksa ulu ağaca gider (BOT-07).
- Yalnızca 650 px içini ve `Visibility.CanSee` true olan karakterleri algılar (BOT-02).
- Yol bulma yok; doğrudan yürür (görev dosyasındaki Aşama 1 kapsamı).

## Test durumu

- **Derleme:** Unity 6000.6.7f1'de (M0 dalının üstünde) projeyi açtım; Console'da hata ve uyarı yok.
- **Play mode:** Henüz denenmedi. Benim bilgisayarımda Unity MCP bağlantısı daha kurulmadı. İlk oyun içi denemeyi Yiğit yapacak; sonuçları bu dosyaya ekleyeceğim.

## Nasıl denenir

1. Dalı aç: `git fetch && git switch hamza/asama1-balta-bot`, projeyi Unity 6000.6.7f1 ile aç.
2. `Assets/_Game/Scenes/Sandbox/Sandbox_Hamza.unity` sahnesini aç. Sahnede yalnızca Player var: `Assets/_Game/Prefabs/Bots/Bot.prefab`'ı sahneye sürükle ve oyuncudan 4–5 birim uzağa koy (sahneyi kaydetmen gerekmez).
3. Play:
   - Bot seni 500 px (12,5 birim) içinde görünce yaklaşır ve vurur; sen sol tık veya L ile vurursun.
   - **KT-03:** Başlangıç seviyesinde bot 10. isabette ölmeli (10 hasar × 10 = 100 can).
   - Savururken yürüme yavaşlamalı; tuşu basılı tutunca 0,5 sn'de bir savrulmalı.
   - Scene görünümünde (Gizmos açık) savurma yayı görünür.
4. Bilinen durumlar:
   - Bot prefab'ı mor atkılıdır (geçici görselde mor daire); oyuncu mavi.
   - Stub dünya boş olduğu için (yaprak, ağaç yok) bot rakip görmediğinde yerinde durur. Okan'ın `WorldService`'i gelince toplamaya ve ağaç kesmeye başlar.

## Kararlar (inceleme için)

SRS'in açıkça söylemediği yerlerde şunları seçtim. İtiraz varsa söyleyin, değiştiririm.

1. **Savurma süresi = bekleme süresi (0,5 sn).** SRS "savururken %50 hız" diyor ama savurmanın süresini yazmıyor. Tuş basılı tutulursa karakter sürekli yarı hızda yürür.
2. **Vuruş yönü hasar anındaki bakış yönüdür** (savurma başlangıcından 0,12 sn sonra), savurmanın başladığı andaki yön değil.
3. **Menzil:** Karakterlere merkezden merkeze 60 px. Ağaçlara gövdenin **kenarına** göre 60 px; çünkü ulu ağacın gövdesi 40 px, karakter yarıçapı 20 px, merkezden ölçülse ulu ağaca hiç vurulamazdı.
4. **Ağaca vururken** hedef ağacın kendi gövdesi engel sayılmaz; araya giren başka gövde veya kaya sayılır.
5. **`HitLanded` yalnızca karakter isabetlerinde** yayınlanır. Ağaç isabetini Okan'ın `TreeHit` / `GreatTreeHit` olayları bildirir; böylece isabet ve ağaç sesi (NFR-10) iki kez çalmaz.
6. **Çalılıktaki (görünmeyen) karakter de** yay içindeyse hasar alır. SRS vuruşu görünürlüğe bağlamıyor; görünmezlik yalnızca botun algısını etkiler.
7. **Envanteri dolu olduğu için toplanamayan yaprak:** `IPickup` yaprağın türünü vermediği için bot bunu bilemiyor. Geçici çözüm: botun toplama yarıçapında `leaves.fallMaxSec`'ten uzun süre kalan yaprak bir daha seçilmez. `IPickup`'a tür eklenirse bu kaldırılır.
8. **Bot saldırırken** hedefe (menzil − karakter yarıçapı) = 40 px'e kadar yaklaşır, sonra yerinde vurur.

## Diğerlerine notlar

- **Okan:** Balta ağaçları `Tree` katmanındaki collider'lardan `GetComponentInParent<ITree>()` ile bulur. Ağacın `Tree` katmanındaki collider'ı, `ITree` bileşeniyle aynı nesnede veya onun altında olmalı. Bot, ağaçları ve yaprakları `Services.World.TreesNear` / `PickupsNear` ile bulur.
- **Yiğit:** `MatchManager` botlara `Setup(profil, atkı)` çağırınca bot profilleri (Aşama 2) ve atkı renkleri devreye girer.
- **Furkan:** Balta görseli için `GameEvents.Swung` (savurma başladı) ve `GameEvents.HitLanded` (karakter isabeti) hazır.

## Kalan işler

### Aşama 1'den kalan

- [ ] Play mode'da ilk deneme ve KT-03 doğrulaması
- [ ] `Sandbox_Hamza.unity`'ye kalıcı olarak bir Bot eklenmesi
- [ ] Hamza'nın bilgisayarında Unity MCP bağlantısı

### Aşama 2 – Yetenekler (`AbilityController`)

- [ ] Kilit açma: `LevelChanged` dinlenir; Hız 5 → Atılma, Sağlık 5 → Kalkan, Güç 5 → Ulti `Ready`, `AbilityUnlocked` yayınlanır (3.4)
- [ ] Atılma: hareket yönünde (yoksa bakış yönünde) `Motor.Dash(yön, 160, 0,15)`, bekleme 3 sn (FR-26)
- [ ] Kalkan: maçta bir kez, 6 sn; ilk balta/ulti hasarını emer (`IDamageInterceptor`), alan hasarını emmez (FR-27)
- [ ] Ulti: maçta bir kez; 0,4 sn hazırlanma, 120 px içindeki bütün rakipler ve kesilebilir ağaçlar (FR-28)
- [ ] Kullanılmış tek seferlik yetenek tekrar çalışmaz; üretimde ve geri sayımda yetenek yok (FR-29)
- [ ] Ulti hazırlanırken hız 0: `SpeedMultiplier`'ı balta ile tek yerden yönetmek

### Aşama 2 – Botlar

- [ ] Algılama (`BotPerception`): 650 px, `Visibility.CanSee`, alan durumu (BOT-02, BOT-03)
- [ ] 4 durumlu makine: Alana dön > Kaç > Saldır > Topla; Kaç'tan çıkış %50 can veya rakip 500 px dışında; kaçarken 60 px içindekine vur (BOT-03–BOT-07)
- [ ] Kaç: `Visibility.NearestBush`; atılma açıksa rakip 200 px'ten yakınken (BOT-05)
- [ ] Alana dön: `Zone.WillBeOutsideWithin(pos, 10)` (BOT-04)
- [ ] Tarif: güvendeyken önce can iksiri (%50 altı), sonra profil sırası; üretimde dur (BOT-10, 5.1)
- [ ] Yetenekler: kalkan, ulti, atılma kullanımı (BOT-13, BOT-06)
- [ ] Yol bulma: 20 px'lik `NavGrid` + A*, `TreeFelled`'da hücreleri aç, karede en fazla 1 bot hesaplar (BOT-11)
- [ ] Takılma: 2 sn'de 20 px'ten az ilerleyen bot 200 px içinde rastgele noktaya (BOT-11)
- [ ] 3 profil (`Character.Profile`) (5.1)
- [ ] Debug: botun üstünde durum adı ve hedef çizgisi, F4 ile aç/kapa

### Aşama 3 – Ayar ve test

- [ ] KT-13: 3 bot × 5 maç (tarif, ağaç kesme, alan dışında ≤ 3 sn, takılma ≤ 2 sn)
- [ ] KT-17: kalkan ve ulti
- [ ] NFR-06: 10 maç üst üste donma/takılma yok
- [ ] Bot performansı: 3 bot birlikte karede 1 ms'nin altında
- [ ] Okan'la denge turu (`bots`, `axe` değerleri)

## Sözleşme önerisi

- `IPickup`'a yaprağın türünü (veya "bu karakter toplayabilir mi") veren bir üye eklenmesi. Botun dolu envanterle toplanamayacak yaprağa yürümesini önler (BOT-07). Ayrı bir issue olarak açılacak.
