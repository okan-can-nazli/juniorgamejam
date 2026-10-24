# Leaf Brawl – Yazılım Gereksinimleri Spesifikasyonu (Demo v1.4)

24 Ekim 2026 · Yiğit Ahmet Hosta · **v1.4: tek geçerli sürüm.** Ayrı ayrı yayımlanan v1.1.1 (alan uyarısı kaldırıldı) ve v1.2'yi (her maçta sapmalı harita) birleştirir ve geliştirmede alınan kararları işler (değişiklikler Bölüm 11.3'te). v1.2: Bölüm 11.2. v1.1: 23 Ekim 2026, v1.0'a gelen ekip geri bildirimleri (Bölüm 11).

## 1. Giriş

Bu belge, Leaf Brawl'ın ilk ve tek demosunu tanımlar: 1 insan oyuncu ile 3 botun yerel bilgisayarda, internet bağlantısı olmadan oynadığı tek haritalı, yaklaşık 5 dakikalık eleme maçı. Belge sonraki sürümleri öngörmez; burada yazmayan her şey demonun dışındadır.

### 1.1 Amaç

Demonun tek amacı şu soruyu yanıtlamaktır: Oyuncu birkaç dakika içinde yaprak toplama, ağaç kesme, geliştirme yapma ve bu gelişimi baltayla savaşta kullanma döngüsünü eğlenceli buluyor mu? Belge geliştirici, tasarımcı ve test eden kişi için ortak ölçüttür.

### 1.2 Kapsam

- Tek insan oyuncu, 3 bilgisayar kontrollü rakip (bot), toplam 4 karakter.
- Tek sonbahar haritası düzeni; her maçta tohumdan küçük ve sınırlı sapmalarla üretilir (3.12). Pixel art görseller.
- Yeniden doğma yok; ölen karakter elenir.
- Yakın dövüş silahı olarak balta; mermi ve menzilli saldırı yok.
- 4 yaprak türü, 3 geliştirme (en fazla 5. seviye) ve can iksiri, 5. seviyede açılan 3 yetenek, çalılıklar, kesilebilir ağaçlar, daralan güvenli alan.
- Yalnızca yerel çalışma: sunucu, çevrim içi bağlantı, hesap, yayınlama (deploy) yok.

### 1.3 Tanımlar ve kısaltmalar

| Terim | Anlamı |
| --- | --- |
| Oyuncu | Klavye ve fareyle oynayan tek insan kullanıcı |
| Bot | Oyun tarafından yönetilen rakip karakter |
| Karakter | Oyuncu veya bot; haritadaki savaşan birim |
| K / S / Y / T | Kırmızı / Sarı / Yeşil / Turuncu yaprak |
| Geliştirme | Güç, Hız veya Sağlık; her biri 1. seviyeden başlar, en fazla 5. seviyeye çıkar |
| Tarif | Yaprak harcayarak bir geliştirmeyi 1 seviye yükselten veya can iksiri üreten işlem |
| Tehdit puanı | Karakterin üç geliştirmede 1. seviyenin üstüne çıktığı seviyelerin toplamı (0–12). Can iksiri sayılmaz |
| Yaprak yığını | Tehdit puanını gösteren, karakterin etrafındaki yaprak görseli |
| Güvende | Karakter güvenli alanın içinde ve son 3 sn içinde hasar almamış, balta savurmamış |
| Çalılık | İçine giren karakteri diğerlerine görünmez yapan, içinden geçilebilen bitki alanı |
| Kütük | Kesilen ağaçtan geriye kalan, engel olmayan ve yaprak üretmeyen görsel |
| Güvenli alan | Dışında karakterin hasar aldığı, zamanla küçülen daire |
| FR / NFR | Fonksiyonel / fonksiyonel olmayan gereksinim |
| px | Dünya birimi. 40 px = 1 Unity birimi (bkz. 3.13) |

### 1.4 Referanslar

- *Leaf Brawl – Ana Oyun Konsepti* (ekip belgesi). Bu SRS, konseptin MVP bölümünü tek oyunculu demoya indirger.
- *Leaf Brawl SRS (Demo v1.0) – Geri Bildirim*, Muhammed Hamza Karamanlı (repo: `Leaf_Brawl_SRS_Geri_Bildirim.pdf`).
- Okan Can Nazlı'nın notları (repo: `feedback_okan.md`) ve *Düzeltilmesi Gerekenler – Yiğit'in Buldukları* (repo: `docs/`).
- IEEE 830 / ISO/IEC/IEEE 29148 yapısı esas alınmıştır.

## 2. Genel Tanım

Demo, tek bir bilgisayarda açılıp kapanan, kendi içinde tamamlanmış bir oyundur. Dış sisteme bağımlılığı yoktur.

### 2.1 Ürün perspektifi

- Bağımsız, çevrimdışı bir uygulamadır. Ağ isteği, sunucu, veritabanı veya kullanıcı hesabı içermez.
- Botlar aynı süreç içinde, oyunun kendi döngüsünde çalışır.
- Teslim biçimi: Windows veya macOS için Unity masaüstü yapısı. Herkese açık bir adrese yüklenmez.

### 2.2 Temel işlevler

1. Ana menüden maç başlatma; kılavuzu ve kontrolleri okuma.
2. Karakteri hareket ettirme ve baltayla yakın dövüş saldırısı yapma.
3. Ağaçlardan düşen yaprakları toplama; ağaçları baltayla keserek daha fazla yaprak elde etme.
4. Güvendeyken tarif üreterek Güç, Hız veya Sağlık geliştirmesini bir seviye yükseltme ya da can iksiri üretme.
5. Bir geliştirme 5. seviyeye ulaşınca açılan özel yeteneği kullanma: Güç → ulti, Hız → atılma, Sağlık → kalkan.
6. Çalılıklarda saklanma; botlarla savaşma; tehdit puanına göre büyüyen yaprak yığınını görme.
7. Daralan güvenli alan içinde hayatta kalma; son kalan karakter kazanır.
8. Sonuç ekranında sıralamayı görüp maçı yeniden başlatma.

### 2.3 Kullanıcı

Tek kullanıcı profili vardır: demoyu deneyen oyuncu (ekip üyesi, jüri veya test katılımcısı). Üstten görünümlü aksiyon oyunlarına aşina olduğu varsayılır. Oynanış sırasında yönlendiren bir eğitim bölümü yoktur; oyunun nasıl oynandığı ana menüdeki **Kılavuz** ekranında, tuşlar **Kontroller** ekranında anlatılır.

### 2.4 Kısıtlar

- Platform: macOS veya Windows dizüstü/masaüstü bilgisayar, klavye + fare. Dokunmatik ve gamepad desteklenmez.
- Motor: **Unity (2D)**, dil C#. Unity sürümü ekip tarafından tek sürüme sabitlenir. Geliştirme Claude + Unity MCP ile yapılır.
- Ortak repo kuralları: Unity'ye uygun `.gitignore`; Asset Serialization = Force Text; Visible Meta Files. Ana sahneyi aynı anda yalnızca bir kişi düzenler; iş prefab'lara bölünür.
- Görseller pixel art'tır; ölçek kuralları 3.13'tedir.
- Tüm görsel ve sesler ekibin kendi üretimi veya lisansı açık (CC0 vb.) varlıklar olmalıdır. Brawl Stars, Clash Royale ve Minecraft'a ait hiçbir görsel, ses, kod, isim veya arayüz düzeni kullanılmaz.
- Arayüz dili Türkçedir.

### 2.5 Varsayımlar

- Bölüm 3'teki sayısal değerler ilk denge tahminidir; demo testlerinde değiştirilebilir ve bu nedenle tek bir ayar dosyasında tutulur (NFR-05).
- Bir maç tek oturumda oynanır; ilerleme, kayıt veya skor saklanmaz.

## 3. Oyun Kuralları ve Sayısal Değerler

Bütün karakterler (oyuncu ve botlar) aynı kurallara ve aynı başlangıç değerlerine tabidir. Botlara gizli bonus verilmez. Aşağıdaki değerler ilk denge tahminidir.

### 3.1 Geliştirme seviyeleri

Her karakter Güç, Hız ve Sağlık geliştirmelerinde 1. seviyeden başlar. Her tarif ilgili geliştirmeyi 1 seviye yükseltir. Hiçbir geliştirme 5. seviyeyi geçemez. Değerler formülle değil, doğrudan bu tablodan okunur.

| Seviye | Güç: vuruş hasarı | Hız (px/sn) | Sağlık: maksimum can | Can yenilenmesi |
| --- | --- | --- | --- | --- |
| 1 | 10 | 220 | 100 | 10 sn'de 1 HP |
| 2 | 13 | 238 | 115 | 9 sn'de 1 HP |
| 3 | 16 | 255 | 130 | 8 sn'de 1 HP |
| 4 | 19 | 273 | 145 | 7 sn'de 1 HP |
| 5 | 22 | 290 | 160 | 6 sn'de 1 HP |

- Sağlık seviyesi yükseldiğinde maksimum can ve mevcut can aynı miktarda (+15) artar.
- 5. seviyeye ulaşan geliştirme kendi yeteneğini açar (3.4).

### 3.2 Can yenilenmesi ve can iksiri

- Karakter **5 sn boyunca hasar almazsa** canı, Sağlık seviyesine göre 3.1'deki hızla yenilenmeye başlar. Hasar alındığında 5 sn'lik bekleme yeniden başlar. Can maksimumu geçmez.
- Ayrıca **can iksiri** tarifi vardır (3.7). İksir üretildiği anda içilir ve mevcut canı 40 artırır (maksimumu geçmez). İksir envanterde saklanmaz.

### 3.3 Balta (saldırı)

Oyunda mermi ve menzilli saldırı yoktur. Her karakterin tek silahı yakın dövüş baltasıdır. Balta hem rakiplere hem ağaçlara vurur.

| Özellik | Değer |
| --- | --- |
| Tuş | Sol fare tuşu veya L. Basılı tutulursa bekleme bittikçe yeniden savrulur |
| Vuruş yönü | Karakterin baktığı yön (fare imleci) |
| Menzil | Karakter merkezinden 60 px |
| Yay açısı | 100° |
| Bekleme | 0,5 sn |
| Savurma başlangıcından hasara | 0,12 sn |
| Savururken hareket hızı | Normal hızın %50'si |
| Hasar | Güç seviyesine göre 3.1 |

- Tek savuruş, yay içindeki **bütün** karakterlere ve en fazla 1 ağaca vurur; her hedefe en fazla bir kez.
- Karakter merkezinden hedefe çizilen çizgi bir ağaç gövdesinden veya kayadan geçiyorsa vuruş o hedefe işlemez.
- Dost ateşi kavramı yoktur; balta savuran dışındaki herkese hasar verir.
- Karakter çarpışma yarıçapı 20 px'tir. Balta görseli seviyeyle değişmez; Güç 5'te ulti hazırken balta parlar.

### 3.4 5. seviye yetenekleri

Maç başında hiçbir karakterin özel yeteneği yoktur; atılma da kapalıdır. Bir geliştirme 5. seviyeye ulaşınca ona özel yetenek açılır ve ekranda "Ulti açıldı [E]" biçiminde bildirim çıkar.

| Geliştirme | Yetenek | Tuş | Kullanım | Etkisi |
| --- | --- | --- | --- | --- |
| Hız 5 | Atılma (dash) | Boşluk | Tekrar kullanılabilir, bekleme 3 sn | Hareket yönünde (hareket yoksa baktığı yöne) 0,15 sn'de 160 px ilerler; hasar almayı engellemez |
| Sağlık 5 | Kalkan | Q | Maçta 1 kez | 6 sn açık kalır. Bu sürede bir rakipten gelen **ilk vuruşu** (balta veya ulti) tamamen emer ve kırılır. Alan hasarını engellemez |
| Güç 5 | Ulti | E | Maçta 1 kez | 0,4 sn hazırlanma (karakter parlar, hareket edemez), ardından karakter merkezli 120 px yarıçaplı alandaki bütün rakipler canına bakılmaksızın ölür ve bütün kesilebilir ağaçlar kesilir. Ulti rakibe kalan canı kadar hasar verir; ekranda görünen hasar sayısı bu değerdir. Kalkanı açık rakibin kalkanı kırılır, rakip hasar almaz |

### 3.5 Ağaçlar ve yaprak kaynakları

Yapraklar **yalnızca ağaçlardan** düşer. Ağaçlar iki yolla yaprak verir: belirli aralıklarla kendiliğinden ve baltayla kesildiklerinde.

| Ağaç | Adet | Kendiliğinden üretim | Turuncu olasılığı | Kesilebilir mi |
| --- | --- | --- | --- | --- |
| Ulu ağaç (merkez) | 1 | 5 sn'de 1 yaprak | %25 | Hayır |
| Küme ağacı | 8 küme × 3 = 24 | Her ağaç 45 sn'de 1 yaprak (küme başına ≈15 sn'de 1) | %5 | Evet |
| Koru ağacı (köşe koruları) | 4 koru × 5 = 20 | Her ağaç 60 sn'de 1 yaprak (koru başına 12 sn'de 1) | %2 | Evet |

- Turuncu olmayan yaprak K, S, Y arasında eşit olasılıkla seçilir.
- Yaprak, ağacın gövdesinin 120 px çevresinde rastgele bir noktaya düşer. Nokta bir gövdenin, kayanın içinde veya harita dışındaysa geçerli bir nokta bulunana kadar yeniden seçilir.
- Maç başında her ağaç kümesinin çevresinde 3, ulu ağacın çevresinde 2 yaprak hazır bulunur (ulu ağacın yanındakilerden biri turuncu).
- Yerdeki bir yaprak 45 sn sonra kaybolur. Haritada aynı anda en fazla 80 yaprak bulunur; sınır doluyken ağaçlar kendiliğinden üretmez.
- **Ağaç kesme:** Kesilebilir her ağacın canı 400'dür. Balta ağaca karakterlere verdiği hasarı verir; böylece Güç 1'deki bir oyuncu bir ağacı yaklaşık 20 sn'de (40 vuruş) keser, Güç 5'teki oyuncu yaklaşık 9 sn'de keser.
- Kesilen ağaç, gövdesinin 120 px çevresine **6–10 adet** (rastgele) yaprak saçar; her birinin turuncu olma olasılığı %10'dur. Bu yapraklar 80 yaprak sınırına sayılmaz.
- Kesilen ağacın yerinde **kütük** kalır. Kütük engel değildir, yaprak üretmez ve maç boyunca yeniden büyümez.

### 3.6 Yapraklar ve envanter

- Karakter bir yaprağa 32 px yaklaşınca yaprak otomatik toplanır.
- Envanter sınırı: K, S, Y için 40'ar; T için 5. Sınırdaki tür toplanmaz, yerde kalır.

### 3.7 Tarifler

Bir geliştirmenin bir sonraki seviyesi için gereken yaprak sayısı seviye yükseldikçe artar.

| Geçiş | Güç geliştirmesi | Hız geliştirmesi | Sağlık geliştirmesi |
| --- | --- | --- | --- |
| 1 → 2 | 3 K + 1 S + 1 Y | 3 S + 1 K + 1 Y | 3 Y + 1 K + 1 S |
| 2 → 3 | 8 K + 4 S + 4 Y | 8 S + 4 K + 4 Y | 8 Y + 4 K + 4 S |
| 3 → 4 | 10 K + 6 S + 6 Y | 10 S + 6 K + 6 Y | 10 Y + 6 K + 6 S |
| 4 → 5 | 18 K + 10 S + 10 Y | 18 S + 10 K + 10 Y | 18 Y + 10 K + 10 S |

| Tarif | Gereken yapraklar | Sonuç |
| --- | --- | --- |
| Can iksiri | 2 Y + 1 T | Mevcut can +40 (maksimumu geçmez). Sınırsız üretilebilir; tehdit puanını artırmaz |

- Turuncu yaprak (T) geliştirme tariflerinde kullanılmaz; yalnızca can iksirinde kullanılır.
- **Tarif yalnızca karakter güvendeyken üretilebilir** (güvenli alanın içinde, son 3 sn içinde hasar almamış ve balta savurmamış).
- Üretim **1,5 sn** sürer. Bu sürede karakter hareket edebilir ama saldıramaz ve yetenek kullanamaz. Karakter bu sürede hasar alırsa üretim iptal olur. Yapraklar üretim tamamlandığında düşülür.
- 5. seviyedeki geliştirmenin tarifi üretilemez.

### 3.8 Güvenli alan aşamaları

Alan, harita merkezinde sabit bir dairedir. Dışındaki karakter saniyede aşağıdaki hasarı alır. Daralma sırasında yarıçap doğrusal azalır. Maç saati, başlangıçtaki 3 sn'lik geri sayım bittiğinde 0:00'dan başlar.

| Aşama | Zaman | Yarıçap (px) | Dışarıda hasar |
| --- | --- | --- | --- |
| Başlangıç | 0:00–1:30 | 1700 (tüm harita) | — |
| 1. daralma | 1:30–2:15 | 1700 → 1000 | 4 / sn |
| Bekleme | 2:15–3:00 | 1000 | 4 / sn |
| 2. daralma | 3:00–3:45 | 1000 → 500 | 7 / sn |
| 3. daralma | 3:45–4:30 | 500 → 150 | 10 / sn |
| Son daralma | 4:30–5:00 | 150 → 0 | 15 / sn |
| Ani ölüm | 5:00'ten sonra | 0 | 50 / sn |

Daralmadan önce yeni sınır yerde gösterilmez; alan doğrudan daralır. 1. daralma bittiğinde köşe koruları alan dışında kalır. Ani ölüm aşamasında en yüksek canlı karakter (160) 3,2 sn'de ölür; bu nedenle maç en geç yaklaşık 5:04'te biter. Aynı anda ölenler FR-83'e göre sıralanır.

### 3.9 Tehdit puanı ve yaprak yığını

Tehdit puanı = karakterin Güç, Hız ve Sağlık geliştirmelerinde 1. seviyenin üstüne çıktığı seviyelerin toplamıdır (en az 0, en fazla 12). Örnek: Güç 3, Hız 1, Sağlık 2 olan karakterin tehdit puanı (3−1) + (1−1) + (2−1) = 3'tür. Can iksiri tehdit puanını değiştirmez.

| Tehdit puanı | Yığın kademesi | Görsel |
| --- | --- | --- |
| 0–1 | Yok | Yalnızca karakter |
| 2–3 | Küçük | 6 yaprak, karakter yarıçapının 1,2 katı |
| 4–6 | Orta | 12 yaprak, 1,4 kat |
| 7–12 | Büyük | 18 yaprak, 1,6 kat |

- Yığın karışık sonbahar yapraklarından oluşur; renk veya parıltıyla ek bilgi vermez.
- Yığın karakterin yön okunu ve silueti kapatmaz; karakter yığının üstünde çizilir.

### 3.10 Ölüm ganimeti

Ölen karakterin envanterinden K, S, Y türlerinin her birinin %35'i (aşağı yuvarlanır) ve T'nin %60'ı (yukarı yuvarlanır) öldüğü noktada **tek bir ganimet yığını** olarak yere bırakılır. Kalan yapraklar yok olur. Ganimet 80 yaprak sınırına sayılmaz ve 45 sn sonra kaybolur.

Görüntü kirliliğini önlemek için ganimet tek tek yaprak olarak gösterilmez:

| Yığındaki toplam yaprak | Ekranda görünen |
| --- | --- |
| 1–10 | 1 yaprak |
| 11–20 | Üst üste 2 yaprak |
| 21 ve üzeri | Üst üste 3 yaprak |

Karakter yığına 32 px yaklaştığında yığındaki **gerçek adetler** envantere eklenir. Envanter sınırına sığmayan yapraklar yığında kalır. Envanter ve HUD her zaman gerçek adetleri gösterir.

### 3.11 Çalılıklar

- Haritada 12 çalılık vardır (3.12). Çalılık engel değildir; içinden normal hızla geçilir.
- Çalılıktaki karakter, **aynı çalılıkta olmayan** bütün karakterlere görünmezdir. Bu kural yığın kademesinden bağımsızdır ve botlar için de geçerlidir.
- Çalılıktaki karakter balta savurduğunda veya hasar aldığında 1 sn boyunca herkese görünür olur.
- Ağaç taçları yalnızca görseldir; saklanma sağlamaz. Taç altına giren karakter varsa taç yarı saydam çizilir.

### 3.12 Harita düzeni

![Harita düzeni (merkez değerler) · 2400×2400 px, ulu ağaç, 8 ağaç kümesi, 4 köşe korusu, 12 çalılık](img/harita_duzeni_v1.1.png)

*Görsel, sapma uygulanmamış merkez düzeni gösterir. Her maçın gerçek haritası aşağıdaki kurallarla bu düzenin çevresinde üretilir.*

Harita düzeni elle tasarlanmıştır, ama konumlar **merkez değerlerdir**: maç başında harita, maçın tohumundan (NFR-07) bu değerlerin çevresinde küçük ve sınırlı sapmalarla yeniden üretilir. Böylece her maç biraz farklı bir harita sunar, adalet bozulmaz. Görsel merkez düzeni gösterir. Bütün ölçüler merkeze göredir.

| Öğe | Adet | Konum | Boyut |
| --- | --- | --- | --- |
| Merkez açıklığı | 1 | Merkez | Yarıçap 350 px; çimenli zemin, engel yok |
| Ulu ağaç | 1 | Merkez | Gövde yarıçapı 40 px, taç yarıçapı 160 px |
| Kaya | 4 | Merkeze 280 px, çaprazlarda (45°, 135°, 225°, 315°) | 60 × 60 px |
| Ağaç kümesi | 8 | Merkezleri 600 px halkada, 45° aralıkla | Küme yarıçapı 150 px; 3 ağaç, gövde yarıçapı 18 px; komşu kümeler arası geçit ≈ 160 px |
| Köşe korusu | 4 | Merkeze 1270 px, çaprazlarda | Koru yarıçapı 220 px; 5 ağaç, gövde yarıçapı 18 px |
| Dış çalılık | 8 | 850 px halkada, kümelerin arasında (22,5° + 45° aralıkla) | Yarıçap 60 px |
| İç çalılık | 4 | 450 px halkada (22,5°, 112,5°, 202,5°, 292,5°) | Yarıçap 40 px |
| Doğma noktası | 4 | Merkeze 1000 px, eksenlerde (0°, 90°, 180°, 270°) | — |

Köşe koruları ilk daralma bitince (1270 − 220 = 1050 > 1000) tamamen alan dışında kalır; sapmadan sonra da bu, doğrulama kuralı (6) ile korunur. Merkez düzende dış çalılıklar 2. daralmanın ilk saniyelerinde, iç çalılıklar 3. daralmanın başında alan dışına düşer. Sapmayla bu anlar kayar: dış çalılıklar (merkeze 730–970 px) 2. daralma içinde, maç saatiyle yaklaşık 3:03–3:24 arasında; iç çalılıklar (330–570 px) yaklaşık 3:39–4:07 arasında alan dışına düşer.

**Her maçta üretim (sapma ve doğrulama):**

| Sabit kalanlar | Sapan öğeler |
| --- | --- |
| Ulu ağaç, merkez açıklığı, 4 kaya, 4 doğma noktası; bütün öğe sayıları | Küme ve koru merkezleri, çalılıklar, küme ve koru içindeki her ağaç; kümelerin ve korunun iç dizilimi rastgele döndürülür |

- Sapma payları: küme ve koru merkezi ±20° ve ±150 px (kümeler kendi 45°'lik diliminde kalır); çalılık ±18° ve ±120 px; küme ve koru içindeki ağaçlar her eksende ±15 px. Kayalar merkez açıklığın parçasıdır ve sabit kalır. Değerler `balance.json` `map` bölümünde tutulur (NFR-05).
- Üretimden sonra şu kurallar doğrulanır: (1) hiçbir öğe harita kenarından 40 px'ten fazla taşmaz; (2) gövde ve kayalar arasında en az 20 px boşluk kalır; (3) çalılık merkezi bir gövdeye 20 px'ten yakın olmaz ve çalılıklar üst üste binmez; (4) doğma noktalarının 150 px çevresi boştur; (5) küme ve koru ağaçları ile çalılıklar merkez açıklığa girmez; (6) koru ağaçları ilk daralma bitince tamamen alan dışında kalır; (7) küme ve koru daireleri arasında en az 80 px geçit kalır (tablodaki ≈160 px merkez değerdir). Öğeler sırayla yerleştirilir; kuralı bozan öğe yalnızca kendisi yeniden denenir (en fazla 30 deneme). Bir öğe yerleşemezse bütün harita yeniden üretilir (en fazla 50 deneme); hiçbiri tutmazsa tablodaki merkez düzen kullanılır.
- Aynı tohum aynı haritayı verir. Botlar, yaprak üretimi ve görseller konumları sabit varsaymaz; ağaç, çalılık ve doğma noktalarını `Services.World` ve `Services.Visibility` üzerinden okur.

### 3.13 Ölçek ve pixel art

- Bölüm 3'teki bütün uzunluklar **dünya px** cinsindendir ve ayar dosyasında bu birimle tutulur. Kod bunları 1 Unity birimi = 40 dünya px ile çevirir.
- Karakter sprite'ı 16 × 16 pikseldir; Pixels Per Unit = 16. Böylece bir karakter 1 Unity birimi (40 dünya px) genişliğindedir ve 20 px çarpışma yarıçapıyla uyuşur. Bütün sprite'lar aynı PPU değerini kullanır.
- Kamerada Pixel Perfect Camera kullanılır; referans çözünürlük 640 × 360 sanat pikselidir (40 × 22,5 birim = 1600 × 900 dünya px, FR-13). 1920 × 1080'de 3 kat, 2560 × 1440'ta 4 kat büyütülür.
- 16:10 ekranlarda (ör. MacBook) siyah bant kullanılmaz; dikeyde biraz daha fazla alan görünür. Bu fark tek oyunculu demoda kabul edilir.

## 4. Fonksiyonel Gereksinimler

Tüm gereksinimler demo için zorunludur. Her gereksinim Bölüm 9'daki kabul testleriyle doğrulanır.

### 4.1 Ekran ve maç akışı

![Ekran akışı · 6 ekran](img/ekran_akisi_v1.1.png)

Demoda 6 ekran vardır; ana menüdeki "Çıkış" uygulamayı kapatır. Duraklatma ve sonuç ekranları hem yeniden başlatmaya hem ana menüye geçiş verir.

| No | Gereksinim |
| --- | --- |
| FR-01 | Uygulama açıldığında ana menü gösterilir: "Başla", "Kılavuz", "Kontroller", "Çıkış". |
| FR-02 | "Kontroller" ekranı bütün tuşları tek sayfada gösterir. |
| FR-03 | "Kılavuz" ekranı oyunun nasıl oynandığını ileri/geri düğmeleriyle gezilen en fazla 6 sayfada anlatır: yaprak toplama ve ağaç kesme; tarifler, seviyeler ve "güvende" kuralı; can yenilenmesi ve can iksiri; 5. seviye yetenekleri; çalılıklar; daralan alan ve elenme. Her sayfada bir başlık, en fazla 3 cümle ve bir görsel bulunur. |
| FR-04 | "Başla" ile 1 oyuncu ve 3 bot, haritanın 4 doğma noktasına rastgele dağıtılarak maç başlar. |
| FR-05 | Maç başında 3 sn geri sayım olur; bu sürede hiçbir karakter hareket edemez ve saldıramaz. Maç saati, alan zamanlaması ve hayatta kalma süresi geri sayım bitince başlar. |
| FR-06 | Esc tuşu maçı duraklatır (tüm zamanlayıcılar durur). Duraklatma menüsü: "Devam", "Yeniden başlat", "Ana menü". Duraklatma menüsündeyken Esc "Devam" ile aynıdır. |
| FR-07 | Oyuncu elendiğinde veya son hayatta kalan olduğunda sonuç ekranı açılır. |
| FR-08 | Sonuç ekranı oyuncunun sırasını (1.–4.), hayatta kalma süresini, elediği rakip sayısını ve tehdit puanını gösterir; "Yeniden başlat" ve "Ana menü" seçenekleri sunar. Sonuç ekranında Esc ana menüye döner. |

### 4.2 Hareket ve kamera

| No | Gereksinim |
| --- | --- |
| FR-10 | Oyuncu W/A/S/D ile 8 yönde hareket eder; çapraz hareket hızı düz hareketle aynıdır. |
| FR-11 | Karakter fare imlecine doğru döner; yönü bir okla görünür. Baltanın vuruş yönü bu yöndür. |
| FR-12 | Karakterler ağaç gövdelerinden, kayalardan ve harita sınırından geçemez; çalılıklardan, kütüklerden ve birbirlerinin içinden geçebilir. |
| FR-13 | Kamera oyuncuyu ortalar ve 16:9 ekranda 1600 × 900 px'lik dünya alanı gösterir (3.13). Oyuncu elendiğinde kamera durur. |

### 4.3 Savaş ve yetenekler

| No | Gereksinim |
| --- | --- |
| FR-20 | Sol tık veya L, baltayı karakterin baktığı yöne savurur; bekleme süresi dolmadan yeni savuruş yapılmaz. Tuş basılı tutulursa bekleme bittikçe yeniden savrulur. |
| FR-21 | Savuruşun hasar anında 60 px menzil ve 100° yay içindeki bütün karakterler, savuranın güncel vuruş hasarı kadar can kaybeder. Gövde veya kaya arkasındaki hedefe vuruş işlemez (3.3). |
| FR-22 | Savururken karakter normal hızının %50'siyle hareket eder. Savurma ve isabet için ayrı ses çalar. |
| FR-23 | Hasar alan karakter 0,1 sn beyaz yanıp söner ve üstünde hasar sayısı belirir. |
| FR-24 | Can 0'a düştüğünde karakter ölür (FR-80). |
| FR-25 | 5 sn hasar almayan karakterin canı 3.1'deki hızla yenilenir; hasar alınca sayaç sıfırlanır. |
| FR-26 | Hız 5'e ulaşan karakter Boşluk ile atılır (3.4). Hız 5'ten önce Boşluk hiçbir şey yapmaz. |
| FR-27 | Sağlık 5'e ulaşan karakter Q ile kalkanı maçta bir kez açar; kalkan 6 sn içinde gelen ilk vuruşu emer ve kırılır. Açık kalkan karakterin etrafında halka olarak görünür. |
| FR-28 | Güç 5'e ulaşan karakter E ile ultiyi maçta bir kez kullanır (3.4). Ulti ile ölen her rakip, ultiyi kullananın eleme sayısına yazılır. |
| FR-29 | Kullanılmış tek seferlik yetenek tekrar kullanılamaz; tuşuna basılırsa hiçbir şey olmaz. |

### 4.4 Ağaçlar ve yaprak toplama

| No | Gereksinim |
| --- | --- |
| FR-30 | Ağaçlar 3.5'teki aralık ve olasılıklarla kendiliğinden yaprak üretir. |
| FR-31 | Yaprak düşerken kısa bir süzülme animasyonu (en fazla 0,6 sn) oynar; yere değmeden toplanamaz. |
| FR-32 | Karakter 32 px yakınındaki yaprağı otomatik toplar; envanter sınırı doluysa toplamaz. |
| FR-33 | Kaybolmasına 5 sn kalan yaprak yanıp söner. |
| FR-34 | Yaprak veya ganimet geçersiz bir noktaya (gövde, kaya, harita dışı) düşmez; konum yeniden seçilir. |
| FR-35 | Baltayla vurulan kesilebilir ağaç her vuruşta sallanır, ses çalar ve üstünde 2 sn boyunca can çubuğu görünür. Ulu ağaca vurulduğunda farklı bir "tok" ses çalar ve hiçbir şey olmaz. |
| FR-36 | Canı 0'a düşen ağaç devrilme efektiyle kütüğe dönüşür ve 3.5'teki kesme ödülünü saçar. |
| FR-37 | Kütük engel değildir, yaprak üretmez ve maç boyunca kalır. |

### 4.5 Envanter ve tarif üretimi

| No | Gereksinim |
| --- | --- |
| FR-40 | Tab tuşu envanter panelini açıp kapatır. Panel açıkken oyun durmaz; karakter hareket edebilir, saldıramaz. Oyuncu elendiğinde veya maç bittiğinde panel kendiliğinden kapanır. |
| FR-41 | Panel 4 yaprak yuvasını (adet / sınır) ve 4 tarif kartını (Güç, Hız, Sağlık, Can iksiri) gösterir. Geliştirme kartları mevcut seviyeyi ve bir sonraki seviyenin maliyetini; her kart gereken, sahip olunan ve eksik yaprak miktarlarını yazar. |
| FR-42 | Üretilebilir kart vurgulanır; üretilemeyen kart soluk görünür ama gizlenmez. 5. seviyedeki geliştirmenin kartında "En üst seviye" yazar. Karakter güvende değilse kartlarda "Güvende değilsin" uyarısı görünür. |
| FR-43 | Bir tarif paneldeki kartına tıklanarak veya panel kapalıyken 1 (Güç), 2 (Hız), 3 (Sağlık), 4 (Can iksiri) tuşuyla başlatılır. |
| FR-44 | Tarif yalnızca karakter güvendeyken başlatılabilir (3.7). Üretim 1,5 sn sürer ve karakterin üstünde dolan bir üretim çubuğuyla gösterilir. Bu sürede hasar alınırsa üretim iptal olur ve yaprak harcanmaz. |
| FR-45 | Üretim tamamlandığında gereken yapraklar envanterden düşülür; geliştirme 1 seviye artar ve tehdit puanı 1 artar ya da iksir içilir. Yaprak birleşme efekti ve ses çalar; ekranda 3 sn "Güç 2 → 3" veya "Can +40" biçiminde bildirim görünür. |
| FR-46 | Yeterli yaprak yokken veya karakter güvende değilken 1/2/3/4 tuşuna basılırsa üretim başlamaz; eksik yaprak ikonları ya da "Güvende değilsin" yazısı kısa süre kırmızıyla işaretlenir. |

### 4.6 Çalılık ve görüş

| No | Gereksinim |
| --- | --- |
| FR-50 | Haritada 3.12'deki düzene göre (sapma payıyla) 12 çalılık bulunur. |
| FR-51 | Çalılıktaki karakter ve yaprak yığını, aynı çalılıkta olmayan karakterlere görünmez. Bu kural botlar için de geçerlidir. |
| FR-52 | Çalılıktaki karakter balta savurduğunda veya hasar aldığında 1 sn boyunca herkese görünür olur. |
| FR-53 | Oyuncu çalılıktayken kendi karakteri yarı saydam çizilir. |
| FR-54 | Ağaç taçları saklanma sağlamaz; altında karakter varken taç yarı saydam çizilir. |

### 4.7 Yaprak yığını ve karakter tanıma

| No | Gereksinim |
| --- | --- |
| FR-60 | Her karakterin yığın kademesi tehdit puanına göre 3.9'daki gibi güncellenir. |
| FR-61 | Kademe arttığında yığın 0,3 sn içinde büyür. |
| FR-62 | Yığın karakterle birlikte hareket eder, hafifçe salınır ve oyuncunun kontrolünü veya görüşünü engellemez. |
| FR-63 | Karakterlerin üzerinde isim yazmaz. Her karakterin farklı renkte bir atkısı vardır (oyuncu mavi; botlar mor, beyaz, siyah; yaprak renkleri kullanılmaz). Oyuncu ayrıca ayak altındaki halkadan tanınır. |
| FR-64 | Her rakibin üstünde küçük bir can çubuğu görünür. Rakip görünmezken çubuğu da görünmez. |

### 4.8 Güvenli alan

| No | Gereksinim |
| --- | --- |
| FR-70 | Güvenli alan 3.8'deki zamanlamayla daralır. |
| FR-71 | Alan dışı ekranda renkli bir örtüyle gösterilir; oyuncu alan dışındayken ekran kenarları kırmızılaşır. |
| FR-72 | Alan dışındaki karaktere her 0,5 sn'de, saniyelik hasarın yarısı kadar hasar uygulanır. |
| FR-73 | Gövdesi alan dışında kalan ağaçlar yaprak üretmeyi bırakır. |

### 4.9 Ölüm, elenme ve sıralama

| No | Gereksinim |
| --- | --- |
| FR-80 | Ölen karakter haritadan kaldırılır ve yeniden doğmaz. Yerine 1 sn süren bir yaprak dağılma efekti oynar. |
| FR-81 | Ölüm ganimeti 3.10'a göre tek yığın olarak bırakılır ve toplanır. |
| FR-82 | Sıralama ölüm sırasına göredir: ilk ölen 4., son kalan 1. olur. |
| FR-83 | Aynı karede birden fazla karakter ölürse, ölümden önceki canı yüksek olan üst sıraya yazılır; can eşitse aynı sırayı paylaşırlar. Son iki karakter aynı anda ölürse aynı kural kazananı belirler. |
| FR-84 | Bir rakibin son vuruşunu yapan karakterin eleme sayısı 1 artar. Alan hasarıyla ölenlerde eleme kimseye yazılmaz. |
| FR-85 | Oyuncu elendiğinde maç ekranda donar ve sonuç ekranı açılır; kalan botların maçı arka planda sürdürülmez. |

## 5. Bot Gereksinimleri

Demoda 3 bot bulunur ve tek zorluk seviyesi vardır. Botlar oyuncuyla aynı kuralları kullanır; tek farkları girdilerini klavye yerine karar mantığından almalarıdır. Bot mantığı 4 durumlu basit bir durum makinesidir.

![Bot durum makinesi · 4 durum](img/bot_durum_makinesi_v1.1.png)

"Alan tehlikede" = bot alan dışında veya 10 sn içinde alan dışında kalacak. "Rakip görünür" = 500 px içinde görünen (çalılıkta gizli olmayan) rakip var. Can eşikleri BOT-05 ve BOT-06'da tanımlıdır.

### 5.1 Bot profilleri

| Bot | Geliştirme önceliği |
| --- | --- |
| Saldırgan | Güç → Sağlık → Hız |
| Çevik | Hız → Güç → Sağlık |
| Dayanıklı | Sağlık → Güç → Hız |

### 5.2 Davranış kuralları

| No | Gereksinim |
| --- | --- |
| BOT-01 | Bot, oyuncuyla aynı seviye, bekleme süresi, envanter sınırı, tarif ve "güvende" kurallarına tabidir; gizli bonus almaz. |
| BOT-02 | Bot yalnızca 650 px içindeki karakterleri ve yaprakları algılar; FR-51'e göre görünmeyen karakteri algılayamaz. Ağaçların konumunu, kesilip kesilmediğini ve güvenli alanı her zaman bilir. |
| BOT-03 | Bot her 0,2 sn'de bir karar verir; durum seçimi öncelik sırasıyla Alana dön > Kaç > Saldır > Topla'dır. |
| BOT-04 | **Alana dön:** Bot alan dışındaysa veya 10 sn içinde alan dışında kalacaksa merkeze doğru gider; atılma açıksa alan dışındayken kullanır. |
| BOT-05 | **Kaç:** Canı %35'in altındaysa ve 500 px içinde rakip varsa rakipten uzaklaşıp en yakın çalılığa gider; atılma açıksa rakip 200 px'ten yakınken kullanır. Rakip balta menzilindeyse (60 px) kaçarken de ona vurur. Canı %50'ye çıkınca veya rakip 500 px dışına çıkınca Kaç'tan çıkar. |
| BOT-06 | **Saldır:** Canı %35 veya üstündeyse ve rakip görünürse hedefe yaklaşır, hedef menzile ve yaya girince vurur. Atılma açıksa hedef 160–300 px uzaktayken mesafeyi kapatmak için kullanır. |
| BOT-07 | **Topla:** Diğer durumlarda en yakın görünen yaprağa veya ganimet yığınına gider. Görünen yaprak yoksa alan içindeki 650 px içinde en yakın kesilebilir ağaca gidip onu keser; böyle bir ağaç yoksa ulu ağaca gider. Gidilecek yaprak, ganimet veya ağaç alanın içinde olmalı ve 10 sn içinde alan dışında kalmamalıdır; aksi hâlde bot Alana dön ile Topla arasında gidip gelir. |
| BOT-08 | Hedef seçimi: görünen rakipler arasında (yığın kademesi × 200) − mesafe değeri en yüksek olan seçilir (Yok = 0, Küçük = 1, Orta = 2, Büyük = 3). Botlar oyuncu ile botu ayırt etmez. Böylece botlar kendiliğinden en tehlikeli karaktere yönelir. |
| BOT-09 | Vuruş: Bot yeni bir hedefi ilk gördükten 0,25 sn sonra vurmaya başlar ve yalnızca hedef 60 px menzil ve 100° yay içindeyken savurur. |
| BOT-10 | Tarif: Bot güvendeyken önce canı %50'nin altındaysa ve yaprağı yetiyorsa can iksiri üretir; değilse profil sırasındaki ilk üretilebilir geliştirmeyi üretir. Tarifi yalnızca Topla ve Kaç durumlarında başlatır; Saldır ve Alana dön sırasında üretmez. Üretim sırasında yerinde durur. |
| BOT-11 | Yol bulma: Bot engellerin etrafından dolaşır. 2 sn boyunca 20 px'ten az ilerlerse 200 px içinde rastgele bir noktaya yönelerek takılmadan çıkar. |
| BOT-12 | Botlar arasında ittifak, iletişim veya ortak hedef paylaşımı yoktur; aynı karaktere yönelmeleri yalnızca BOT-08'in sonucudur. |
| BOT-13 | Yetenekler: Kalkan açıksa bot canı %50'nin altındayken 100 px içinde rakip varsa kalkanı açar. Ulti açıksa 120 px içinde en az bir rakip varken kullanır. |

## 6. Kullanıcı Arayüzü ve Kontroller

Maç ekranında yalnızca aşağıdaki 7 HUD öğesi bulunur; hiçbiri oyun alanının %15'inden fazlasını kaplamaz.

### 6.1 Kontroller

| Tuş | Eylem |
| --- | --- |
| W / A / S / D | Hareket |
| Fare | Bakış ve vuruş yönü |
| Sol tık veya L | Balta savurma (basılı tut: sürekli) |
| Boşluk | Atılma (Hız 5'te açılır) |
| Q | Kalkan (Sağlık 5'te açılır, maçta 1 kez) |
| E | Ulti (Güç 5'te açılır, maçta 1 kez) |
| 1 / 2 / 3 | Güç / Hız / Sağlık geliştirmesini üret |
| 4 | Can iksiri üret |
| Tab | Envanter panelini aç / kapat |
| Esc | Duraklat |

### 6.2 HUD öğeleri

| No | Konum | İçerik |
| --- | --- | --- |
| UI-01 | Sol üst | Can çubuğu ve "85 / 115" biçiminde can / maksimum can; altında "Güç 2 · Hız 1 · Sağlık 2" biçiminde seviyeler |
| UI-02 | Üst orta | Maç süresi ve "Alan daralıyor: 0:12" biçiminde bir sonraki daralmaya kalan süre |
| UI-03 | Sağ üst | Hayatta kalan karakter sayısı ("Kalan: 3") ve oyuncunun eleme sayısı |
| UI-04 | Alt orta | 4 yaprak ikonu ve adetleri (K, S, Y, T); sınırdaki türün adedi farklı renkte |
| UI-05 | Sağ alt | Balta, atılma, kalkan ve ulti ikonları. Kilitli yetenek gri ve kilit işaretlidir ("Seviye 5"); kullanılmış tek seferlik yetenek üstü çizili görünür; bekleme süreleri dairesel dolumla ve kalan saniyeyle gösterilir |
| UI-06 | Yaprak çubuğunun üstü | Tarif üretilebilir olduğunda küçük bildirim: "Güç hazır \[1\]"; birden fazlaysa yan yana. Karakter güvende değilse bildirimler soluk görünür |
| UI-07 | Karakterin üstü | Tarif üretilirken 1,5 sn'de dolan üretim çubuğu |

### 6.3 Yaprak görselleri

Yapraklar renk körlüğüne karşı renkle birlikte şekille de ayırt edilir. HUD, envanter, yerdeki yaprak ve ganimet aynı şekilleri kullanır.

| Yaprak | Renk | Şekil |
| --- | --- | --- |
| K (güç) | Kırmızı | Sivri, üç uçlu |
| S (hız) | Sarı | İnce ve uzun |
| Y (sağlık) | Yeşil | Yuvarlak |
| T (iksir) | Turuncu | Yıldız damarlı, parlayan kenarlı |

### 6.4 Diğer ekranlar

- Ana menü, kontroller, kılavuz, duraklatma ve sonuç ekranları tek sütunlu, en fazla 4 düğmeli basit panellerdir.
- Tüm düğmeler fareyle tıklanabilir. Esc, kontroller ve kılavuz ekranlarında ana menüye, sonuç ekranında ana menüye döner; duraklatma menüsünde "Devam" ile aynıdır.

## 7. Fonksiyonel Olmayan Gereksinimler

| No | Alan | Gereksinim |
| --- | --- | --- |
| NFR-01 | Performans | 4 karakter, 80 yaprak ve bütün efektler ekrandayken Apple M1 veya Intel i5 (8. nesil, tümleşik grafik) düzeyinde bir dizüstü bilgisayarda 1920×1080 çözünürlükte en az 60 FPS. |
| NFR-02 | Tepki süresi | Tuşa basış ile ekrandaki hareket arasında en fazla 1 kare gecikme. |
| NFR-03 | Açılış | Uygulama açılışından ana menüye 10 sn, "Başla"dan geri sayıma 3 sn içinde. |
| NFR-04 | Çevrimdışı çalışma | Uygulama hiçbir ağ isteği yapmaz; internet kapalıyken tüm işlevler çalışır. Hesap, telemetri, reklam, analiz ve kayıt dosyası yoktur. |
| NFR-05 | Ayarlanabilirlik | Bölüm 3'teki tüm sayısal değerler `Assets/StreamingAssets/balance.json` dosyasında tutulur; böylece derlenmiş yapıda da kod değişmeden düzenlenebilir. Ayrı bir `debug.json` dosyasında test için tohum, bot sayısı (0–3) ve oyuncunun başlangıç envanteri ile seviyeleri ayarlanabilir; test başlangıç envanteri envanter sınırlarını aşabilir. Demo yapısında bu ayarlar varsayılan değerlerdedir. |
| NFR-06 | Kararlılık | 10 ardışık maçta çökme, donma veya botun 2 sn'den uzun takılı kalması olmaz. |
| NFR-07 | Tekrarlanabilirlik | Rastgelelik tek bir tohum (seed) değerinden üretilir; ayar dosyasında sabit tohum verildiğinde aynı maç başlangıcı, harita yerleşimi ve yaprak üretimi tekrarlanır (hata ayıklama için). |
| NFR-08 | Erişilebilirlik | Yapraklar şekille ayrılır (6.3). HUD yazıları 1080p'de en az 18 px; metin ile arka plan kontrastı en az 4.5:1. |
| NFR-09 | Özgünlük | Tüm karakter, yaprak, harita, arayüz, ses ve kod ekibe aittir veya açık lisanslıdır; lisanslar bir `CREDITS` dosyasında listelenir. Referans oyunlardan hiçbir varlık veya isim kullanılmaz. |
| NFR-10 | Ses | Balta savurma, isabet, ağaca vurma, ağaç devrilmesi, yaprak toplama, tarif üretimi, yetenek kullanımı, ölüm ve alan daralmasının başlaması için birer kısa ses efekti; müzik zorunlu değildir. Duraklatma menüsünde ses açma/kapama düğmesi bulunur. |
| NFR-11 | Dağıtım | Demo tek bir klasör veya çalıştırılabilir dosya olarak, kurulum gerektirmeden açılır. Herhangi bir sunucuya veya mağazaya yüklenmez. |

## 8. Kapsam Dışı

Aşağıdakiler demoda yoktur ve demo için tasarım, kod veya altyapı hazırlığı yapılmaz.

| Konu | Demodaki karşılığı |
| --- | --- |
| Çevrim içi çok oyunculu, eşleştirme, sunucu, deploy | Yok; yalnızca yerel tek oyuncu |
| Aynı bilgisayarda ikinci insan oyuncu | Yok |
| 10 oyunculu maç | Yok; maç sabit 4 karakterlidir |
| Birden çok bot zorluk seviyesi | Yok; tek seviye |
| İzleyici modu | Yok; oyuncu elenince sonuç ekranı açılır |
| Menzilli saldırı, mermi, nişan alma | Yok; tek silah balta |
| Dengeli geliştirme, Kaçış tarifleri | Yok; 3 geliştirme ve can iksiri |
| Kesilen ağacın yeniden büyümesi | Yok; kütük maç boyunca kalır |
| Aynı alanda uzun kalınca yaprak üretiminin azalması | Yok; pasif oyunu alan daralması çözer |
| Zamanlı etkinlikler, özel ağaç türleri | Yok |
| Mini harita, ekran dışı yığın göstergesi | Yok |
| Birden çok karakter modeli veya harita | Yok; tek karakter modeli (atkı rengiyle ayrılır), tek harita düzeni (her maç küçük sapmayla) |
| Seviyeyle değişen balta görseli | Yok; tek balta görseli |
| Hesap, ilerleme kaydı, skor tablosu, kozmetik, mağaza | Yok |
| Gamepad, dokunmatik, mobil | Yok |
| Oynanış sırasında yönlendiren eğitim bölümü | Yok; ana menüde Kılavuz ve Kontroller ekranları var |
| Ayar menüsü (çözünürlük, tuş atama) | Yok; yalnızca ses aç/kapa |

## 9. Kabul Kriterleri

Demo, aşağıdaki 18 testin tümü geçtiğinde tamamlanmış sayılır. Testler internet bağlantısı kapalı bir bilgisayarda yapılır.

### 9.1 Kabul testleri

| No | Adımlar | Beklenen sonuç | İlgili |
| --- | --- | --- | --- |
| KT-01 | Uygulamayı aç, "Başla"ya bas | 3 sn geri sayımdan sonra oyuncu ve 3 bot farklı doğma noktalarında hareket eder; maç saati 0:00'dan başlar | FR-01, FR-04, FR-05 |
| KT-02 | 30 sn boyunca W/A/S/D ve fareyle dolaş | Karakter 8 yönde 220 px/sn hızla hareket eder; gövde ve kayalardan geçemez, çalılık ve kütüklerden geçer | FR-10–FR-12 |
| KT-03 | Bir botu baltayla 10 kez vur | Başlangıç seviyesiyle bot 10. isabette ölür; yere tek bir ganimet yığını bırakılır | FR-20–FR-24, FR-81 |
| KT-04 | Ulu ağacın yanında 60 sn yaprak topla | Yapraklar otomatik toplanır, HUD adetleri doğru artar, sınırdaki tür toplanmaz | FR-30–FR-32, UI-04 |
| KT-05 | Güvendeyken 3 K + 1 S + 1 Y ile 1'e bas | 1,5 sn üretim çubuğu dolar; Güç 1 → 2 olur, "Güç 1 → 2" bildirimi çıkar, 5 yaprak düşer, vuruş hasarı 13 olur | FR-43–FR-45, UI-07 |
| KT-06 | Yaprak yetmezken 2'ye bas; ardından yeterli yaprakla hasar alır almaz 1'e bas | İki durumda da hiçbir seviye değişmez; eksik yaprak ikonları veya "Güvende değilsin" yazısı kırmızıyla işaretlenir | FR-44, FR-46 |
| KT-07 | Tab ile paneli aç | 4 kart görünür; üretilebilir olan vurgulu, diğerleri soluk ama okunur; karakter hareket eder, saldırmaz | FR-40–FR-42 |
| KT-08 | Hata ayıklama ayarıyla bol yaprakla başla, Güç, Hız ve Sağlık'ı 5'e çıkar | Yığın tehdit puanı 2'de Küçük, 4'te Orta, 7'de Büyük olur; yön oku görünür kalır; her geliştirme 5'te kendi yeteneğini açar ve kartında "En üst seviye" yazar | FR-26–FR-28, FR-42, FR-60–FR-62 |
| KT-09 | Bir çalılığa gir, botun 100 px yakınından geçmesini bekle; sonra balta savur | Bot çalıdaki oyuncuyu hedef almaz; savuruştan sonra 1 sn görünür olan oyuncuyu hedef alır | FR-51, FR-52, BOT-02 |
| KT-10 | Maçı 1:30'a kadar bir köşe korusunda bekle | Alan daralır, dışarıda 4 hasar/sn alınır, ekran kenarları kırmızılaşır; koru ağaçları yaklaşık 2:00'den itibaren, en geç 2:15'te üretmeyi bırakır | FR-70–FR-73 |
| KT-11 | Oyuncu ölünceye kadar oyna | Oyuncu yeniden doğmaz; sonuç ekranı doğru sırayı, süreyi, eleme sayısını ve tehdit puanını gösterir | FR-07, FR-08, FR-80–FR-85 |
| KT-12 | Hiç saldırmadan alan içinde hayatta kalmaya çalış | Maç en geç 5:05'te tek kazanan veya FR-83'e göre sıralamayla biter | FR-70, FR-83 |
| KT-13 | 3 botu 5 maç boyunca izle | Her bot en az 1 tarif üretir ve en az 1 ağaç keser; alan dışında 3 sn'den uzun kalmaz, 2 sn'den uzun takılmaz; canı %35'in altına düşen bot dibindeki rakibe vurur | BOT-01–BOT-13 |
| KT-14 | Ağı kapat, 10 maç üst üste oyna, FPS sayacını aç | Çökme yok, FPS 60'ın altına düşmez, ağ isteği yapılmaz | NFR-01, NFR-04, NFR-06 |
| KT-15 | Güç 1'de bir küme ağacını baltayla kes | Ağaç yaklaşık 20 sn'de (40 vuruş) devrilir, 6–10 yaprak saçar, yerinde içinden geçilebilen bir kütük kalır | FR-35–FR-37 |
| KT-16 | Hasar al, sonra 5 sn hasar almadan bekle | Sağlık 1'de can 5. sn'den sonra 10 sn'de 1 artar; yeniden hasar alınca yenilenme durur ve sayaç baştan başlar | FR-25 |
| KT-17 | Hata ayıklama ayarıyla oyuncuyu Sağlık 5, bir botu Güç 5 yap; kalkanı aç ve botun ultisine yakalan | Kalkan kırılır, oyuncunun canı değişmez; Q ve E ikinci kez çalışmaz | FR-27–FR-29, BOT-13 |
| KT-18 | Ana menüden Kılavuz'u aç, bütün sayfaları gez, Esc'ye bas | En fazla 6 sayfa ileri/geri gezilir; Esc ana menüye döner | FR-01, FR-03 |

### 9.2 Demo değerlendirmesi

Kabul testleri geçtikten sonra en az 5 kişi demoyu 3'er maç oynar ve her maçtan sonra şu soruları 1–5 arasında puanlar:

1. Yaprak toplamak, ağaç kesmek ve tarif üretmek eğlenceli miydi?
2. Hangi geliştirmeyi yükselteceğini seçmek oynayışını değiştirdi mi?
3. Yaprak yığınına bakarak tehlikeli rakibi tanıyabildin mi?
4. Maç süresi ve alan daralması uygun hızda mıydı?
5. Bir maç daha oynamak ister misin?

Ortalama puan 3,5 veya üzerindeyse döngü eğlenceli kabul edilir. Bu eşik ekip tarafından değiştirilebilir; karşılanmazsa Bölüm 3'teki değerler ayar dosyasından düzenlenerek test tekrarlanır.

## 10. Geliştirme Aşamaları

Ekipte oyun geliştirme deneyimi az olduğu için SRS'teki her şey üç aşamada yapılır; her aşamanın sonunda oynanabilir bir sürüm bulunur. Görev dağılımı bu aşamalara göre yapılır.

| Aşama | İçerik | Bitiş ölçütü |
| --- | --- | --- |
| 1 – Çekirdek | Unity projesi ve repo kuralları, Pixel Perfect kamera ve hareket, balta, can, ölüm ve can yenilenmesi, ağaçlar, yaprak toplama ve ağaç kesme, 3 geliştirme ve can iksiri (1–4 tuşları, "güvende" kuralı), güvenli alan, basit bot (Topla + Saldır), `balance.json` | Oyuncu 1 botla 5 dakikalık bir maç oynayabiliyor |
| 2 – Asıl oyun | 3 bot profili ve 4 durumlu makine, yol bulma, çalılıklar, 5. seviye yetenekleri, yaprak yığını kademeleri, ganimet yığını, rakip can çubuğu, sıralama ve sonuç ekranı, HUD'un tamamı | 1 oyuncu + 3 botla tam maç |
| 3 – Cila | Ana menü, Kılavuz ve Kontroller ekranları, envanter paneli (Tab), efektler ve sesler, tohumla tekrarlanabilirlik (NFR-07), kabul testleri | KT-01–KT-18 geçiyor |

Envanter paneli (FR-40–FR-42) Aşama 3'e bırakılabilir: 1–4 tuşları ve UI-06 bildirimi tarif üretmek için yeterlidir.

## 11. Değişiklik Kaydı

v1.0 → v1.1. Kaynaklar: **O** = Okan'ın notları, **H** = Hamza'nın geri bildirim belgesi, **Y** = Yiğit'in düzeltme listesi.

| Konu | v1.1'deki karar | Kaynak | Bölüm |
| --- | --- | --- | --- |
| Motor | Unity 2D, C#; masaüstü yapısı; tarayıcı yapısı kalktı; repo kuralları eklendi | H, Y | 2.1, 2.4 |
| Pixel art ölçeği | 1 birim = 40 px, karakter 16×16, PPU 16, Pixel Perfect 640×360; 16:10'da bant yok | H | 1.3, 3.13 |
| Saldırı | Mermi tamamen kalktı; tek silah balta (sol tık / L), 60 px, 100° yay, 0,5 sn | H, Y | 3.3, 4.3, 5, 6, 9 |
| Hasar formülü | "7 + 3 × Güç" formülü kalktı; değerler seviye tablosundan okunur | O | 3.1 |
| Seviye sınırı | Güç, Hız, Sağlık 1'den başlar, en fazla 5; "en fazla tarif" kavramı yerine seviye | O, Y | 3.1 |
| Atılma | Başlangıç yeteneği değil; yalnızca Hız 5'te açılır | O, Y | 2.2, 3.4 |
| 5. seviye yetenekleri | Hız → atılma, Sağlık → kalkan, Güç → ulti | O, Y | 3.4, 4.3 |
| Yaprak kaynakları | Yalnızca ağaçlar; merkez kaynağı "ulu ağaç" oldu | O | 3.5 |
| Ağaç kesme | Ağaç canı 400 (Güç 1 ≈ 20 sn); 6–10 yaprak; kütük kalır | H, Y | 3.5, 4.4 |
| Can yenilenmesi | 5 sn hasar almayınca Sağlık seviyesine göre yenilenir | Y | 3.2, FR-25 |
| Can iksiri | Yeni tarif: 2 Y + 1 T → +40 can | O | 3.2, 3.7 |
| Tarif maliyeti | Seviyeyle artar; T geliştirmelerden çıktı; envanter sınırı K/S/Y 40 | Y, H | 3.6, 3.7 |
| Tarif üretimi | Yalnızca güvendeyken; 1,5 sn sürer; bildirim 3 sn | O | 3.7, FR-44, FR-45 |
| Tehdit puanı | Üretilen tarif sayısı değil, kazanılan seviyelerin toplamı (0–12); kademeler yeniden ayarlandı | O, Y | 1.3, 3.9 |
| Yığın görseli | Baskın renk ve turuncu parıltı kalktı | O | 3.9, 5.1 |
| Saklanma | Taç altı saklanma yerine çalılıklar; saldıran/hasar alan 1 sn görünür | Y, H | 3.11, 4.6 |
| Ölüm ganimeti | Tek yığın; 1/2/3 yaprakla gösterilir, gerçek adet toplanır | Y | 3.10 |
| Maç sonu | 5:00'ten sonra ani ölüm (50/sn); maç en geç ≈ 5:04 | H | 3.8, KT-12 |
| Bot kaçışı | Kaç durumundaki bot dibindeki rakibe vurur; %50 canda Kaç'tan çıkar | H | BOT-05 |
| Karakter tanıma | Rakip can çubuğu ve atkı rengi eklendi | H | FR-63, FR-64 |
| Harita ölçüleri | Küme, koru, kaya, açıklık, çalılık ölçüleri tablolandı | H | 3.12 |
| Küçük eksikler | Geçersiz düşme noktası, geri sayım/maç saati, Esc davranışları, KT-08 ve KT-10 düzeltildi | H | FR-05, FR-06, FR-08, FR-34, 9 |
| Kılavuz | Ana menüye oynanış kılavuzu eklendi | Y | FR-01, FR-03, 2.3, 8 |
| Geliştirme sırası | 3 aşamalı geliştirme planı eklendi | H | 10 |

### 11.1 Ekibin onaylaması gereken yorumlar

Geri bildirimlerde açık bırakılan bazı noktalar şu şekilde karara bağlandı. İtiraz edilmezse v1.1 bu hâliyle geçerlidir.

1. **Tehdit puanı:** Okan'ın "bu açıklama çok yanlış" notu, tehdit puanının üretilen tarif sayısı yerine kazanılan seviyeler olarak tanımlanması gerektiği şeklinde yorumlandı; can iksiri bu yüzden puana sayılmaz.
2. **"Güvende" tanımı:** Okan'ın "safedayken craft olsun" notu, güvenli alanın içinde olmak ve son 3 sn hasar almamış/savurmamış olmak şeklinde tanımlandı.
3. **Atılma tekrar kullanılabilir:** Kalkan ve ulti maçta bir kez kullanılır; atılma Hız 5'te açılır ama 3 sn beklemeyle tekrar kullanılabilir. Tek kullanımlık atılma yakın dövüşte işe yaramayacağı için bu seçildi.
4. **Can yenilenmesi ve iksir birlikte var:** Yenilenme yavaştır (en hızlı 6 sn'de 1 HP); asıl iyileşme iksirle olur.
5. **Turuncu yaprağın rolü:** T yalnızca can iksirinde kullanılır. Bu, v1.0'daki turuncu yaprak darboğazını da ortadan kaldırır.
6. **Ulti:** 120 px içindeki bütün rakipleri canına bakmadan öldürür ve ağaçları keser; kalkan ultiyi bir kez durdurur.
7. **Ara maliyetler:** Tarif tablolarındaki 2 → 3 ve 3 → 4 maliyetleri ile Hız/Sağlık tabloları, Yiğit'in 1 → 2 ve 4 → 5 örneklerine göre dolduruldu.
8. **Rastgele harita:** Okan'ın önerisi (her maç küçük sapmalı harita) Yiğit ve Okan tarafından konuşulup onaylandı ve v1.2'ye girdi (11.2). Hamza ve Furkan'ın itirazı yoksa geçerlidir; bot ve görsel kodu konumları sabit varsaymamalı, `Services.World` üzerinden okumalıdır.

### 11.2 Değişiklik Kaydı (v1.1 → v1.2)

Ayrıntılı not ve rehber dosyalarla farklar: [`docs/degisiklikler/2026-10-24_harita_sapmasi.md`](degisiklikler/2026-10-24_harita_sapmasi.md).

| Konu | v1.2'deki karar | Kaynak | Bölüm |
| --- | --- | --- | --- |
| Harita çeşitliliği | Harita artık sabit değil: 3.12'deki konumlar merkez değer; her maç tohumdan küçük, sınırlı sapmayla üretilir ve doğrulanır. Ulu ağaç, merkez açıklığı, kayalar, doğma noktaları ve öğe sayıları sabit | O | 1.2, 3.12, FR-50, NFR-07, 8 |
| Sapma ayarları | Sapma payları ve doğrulama sınırları `balance.json` `map` bölümüne eklendi (13 anahtar) | O | 3.12, NFR-05 |
| Çalılıkların alan dışına düşmesi | Merkez düzendeki anlar korunur; sapmalı haritadaki zaman aralıkları eklendi | Y | 3.12 |
| Harita görseli | 3.12 görseli merkez düzeni gösterir | Y | 3.12 |

### 11.3 Değişiklik Kaydı (v1.2 → v1.4)

v1.1.1 (Okan, PR #14) ve v1.2 (PR #5) aynı gün iki ayrı dosya olarak yayımlandı; v1.1.1 v1.2'deki harita değişikliklerini, v1.2 de v1.1.1'deki alan kararını içermiyordu. v1.4 ikisini tek belgede birleştirir. **v1.1, v1.1.1 ve v1.2 eskidi; geçerli olan yalnızca bu belgedir.**

| Konu | v1.4'teki karar | Kaynak | Bölüm |
| --- | --- | --- | --- |
| Alan uyarısı | Daralmadan 10 sn önce yeni sınırın yerde gösterilmesi kalktı; alan doğrudan daralır. Alan sesi daralma başlayınca çalar. `GameEvents.ZoneWarning` sözleşmede kalır ama yayınlanmaz; `balance.json` → `zone.warningSec` kullanılmaz | O (v1.1.1) | 3.8, NFR-10 |
| Ulti hasarı | Ulti rakibe kalan canı kadar hasar verir (sonuç aynı: rakip ölür); ekrandaki hasar sayısı anlamlı kalır. Ağaçlar ultiyle devrilir | H (PR #9) | 3.4, FR-23, FR-28 |
| Kalkan | Yalnızca bir rakipten gelen vuruşu emer; alan hasarını emmez | H (PR #9) | 3.4, FR-27 |
| Bot tarifi | Bot tarifi yalnızca Topla ve Kaç durumlarında başlatır; Saldır'da hedefe yürürken durmaz | H (PR #12) | BOT-10 |
| Bot toplama hedefi | Topla hedefi alanın içinde olmalı ve 10 sn içinde dışarıda kalmamalı | H (PR #12) | BOT-07 |
| Envanter paneli | Oyuncu elendiğinde veya maç bittiğinde panel kendiliğinden kapanır; sonuç ekranının üstünde kalmaz | Y | FR-40, FR-07 |
| Tarif maliyeti (denge) | 3 → 4 ve 4 → 5 maliyetleri ~%30 düştü (15/8/8 → 10/6/6, 25/15/15 → 18/10/10): testlerde 5 dakikada kimse 5. seviyeye çıkamıyor, yetenekler görülmüyordu (11.4 madde 3) | Y | 3.7 |

### 11.4 Açık sorular (ekip kararı bekliyor)

Aşağıdakiler gereksinim değildir; Aşama 3 denge turunda konuşulup gerekirse sonraki sürüme işlenir.

1. **KT-13 ve ağaç kesme:** BOT-07'ye göre bot ancak görünen yaprak yokken ağaç keser. Haritada çoğu zaman yaprak olduğu için testlerde 6 maçta toplam 0–2 ağaç kesildi; KT-13'teki "her bot en az 1 ağaç keser" ölçütü bu hâliyle zor geçer.
2. **Aynı anda iki ulti:** İki karakter aynı karede ulti başlatırsa önce patlayan diğerini öldürür, ölenin ultisi patlamaz. Hangisinin önce patlayacağı belirli (her seferinde aynı) ama SRS bu durumu tanımlamıyor.
3. **5. seviye:** Testlerde botlar 5 dakikalık maçta en fazla 4. seviyeye çıktı. İlk adım olarak 3 → 4 ve 4 → 5 maliyetleri düşürüldü (11.3); yetmezse yaprak üretimi denge turunda değerlendirilecek.
