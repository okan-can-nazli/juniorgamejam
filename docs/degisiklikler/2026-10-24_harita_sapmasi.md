# Değişiklik Notu – Her maçta sapmalı harita (24 Ekim 2026)

**Kaynak:** Okan'ın PR #4'ü ("Harita üretici: yerleşim, sapma ayarları ve testler", main'e birleşti)
**Onay:** Yiğit, Okan'la konuşup onayladı. Hamza ve Furkan'ın itirazı yoksa geçerlidir (SRS Bölüm 11, madde 8). İtirazınız varsa bu dosyanın PR'ına yazın.
**Durum:** SRS v1.1 güncellendi (1.2, 3.12, FR-50, NFR-07, Bölüm 8 ve 11). Bu not, SRS dışındaki rehber dosyalarla kalan farkları ve herkesin yapması gerekenleri listeler.

## Ne değişti?

- **Eskiden:** Harita sabitti; SRS 3.12'deki konumlar kesin konumlardı.
- **Şimdi:** 3.12'deki konumlar **merkez değerlerdir**. Her maçta harita, maçın tohumundan (`Services.Rng`) küçük ve sınırlı sapmalarla yeniden üretilir ve kurallara göre doğrulanır. Aynı tohum aynı haritayı verir.
- **Sabit kalanlar:** ulu ağaç, merkez açıklığı, 4 kaya, 4 doğma noktası ve bütün öğe sayıları.
- **Sapanlar:** küme ve koru merkezleri (±20°, ±150 px), çalılıklar (±18°, ±120 px), küme ve koru içindeki her ağaç (her eksende ±15 px).
- **Kod:** `World/MapLayoutGenerator.cs`, `World/MapGenSettings.cs`; editör menüleri `World/Editor/MapLayoutTest.cs` ve `MapPreviewPng.cs`.
- **Ayarlar:** `balance.json` → `map` bölümüne 13 yeni anahtar geldi: `angleJitterDeg`, `radiusJitterPx`, `rockAngleJitterDeg`, `rockRadiusJitterPx`, `bushAngleJitterDeg`, `bushRadiusJitterPx`, `treeJitterPx`, `placeTries`, `maxRetries`, `boundsMarginPx`, `minGapPx`, `minPassagePx`, `spawnClearPx`.

## Rehber dosyalarla çelişen noktalar

| # | Nerede | Ne yazıyor | Bundan sonra geçerli olan |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` › Git akışı | Birleştirmeyi Yiğit yapar | PR #4'ü Okan kendisi birleştirdi; Yiğit bu kez sonradan onayladı. **Kural değişmedi:** bundan sonra PR'ları Yiğit birleştirir. |
| 2 | Görev dosyaları › Ortak sözleşme, `balance.json` bölümleri | `Scripts/Shared/` ve yeni `balance.json` anahtarı yalnızca "sözleşme" PR'ı ve 4 onayla eklenir | `Shared/Balance.cs` → `MapConfig`'e yukarıdaki 13 alan normal PR ile eklendi. Değişiklik yalnızca ekleme olduğu için kimsenin kodunu bozmaz; Yiğit sonradan onayladı. **Kural değişmedi:** sözleşmeye yapılacak sonraki eklemeler "sözleşme" PR'ıyla yapılır. |
| 3 | SRS 3.12, tablonun altındaki cümle | "Dış çalılıklar 2. daralmanın ilk saniyelerinde, iç çalılıklar 3. daralmanın başında alan dışına düşer." | Bu artık yalnızca merkez düzen için doğru. Sapmayla dış çalılıklar (730–970 px) 2. daralma içinde, maçın yaklaşık 3:03–3:24 arasında; iç çalılıklar (330–570 px) yaklaşık 3:39–4:07 arasında alan dışına düşer. Köşe koruları için doğrulama kuralı (6) korunur: ilk daralma bitince koru ağaçları her zaman alan dışındadır. |
| 4 | SRS 3.12 görseli `docs/img/harita_duzeni_v1.1.png` | Haritayı tek bir sabit düzen olarak gösterir | Görsel **merkez düzeni** gösterir. Belirli bir tohumun gerçek haritası Unity'de Okan'ın önizleme menüsüyle görülür (`MapPreview/` gitignore'ludur). |
| 5 | `OKAN_gorevler.md` › Çalılıklar maddesi | `MapBuilder` çalılıkları sabit açılarla (22,5° + 45°) kursun | Açılar merkez değerdir; konumu `MapLayoutGenerator` hesaplar. Okan kendi dosyasını günceller. |
| 6 | `YIGIT_gorevler.md` › Aşama 3, NFR-07 maddesi | Sabit tohumla "aynı başlangıç ve yaprak üretimi" | Karşılaştırmaya **harita yerleşimi** de eklenir (SRS NFR-07'deki gibi). |

## Kim ne yapmalı?

- **Hamza (botlar):** Ağaç, çalılık, yaprak ve doğma noktası konumlarını sabit sayı olarak yazmayın; her zaman `Services.World` ve `Services.Visibility` üzerinden okuyun. Mevcut `BotBrain` zaten böyle çalışıyor; değişiklik gerekmiyor.
- **Furkan (görseller):** Haritayı tek parça boyanmış bir resim olarak çizmeyin. Ağaç, taç, kütük, çalılık ve kaya ayrı sprite olsun ki üretici bunları her maçta farklı yerlere koyabilsin. Zemin, öğelerin konumundan bağımsız döşenebilen karolardan oluşsun.
- **Okan (dünya):** Harita maç başında kurulur (`MapGenerator`). `WorldService` ağaç ve doğma noktalarını üretilen haritadan kaydetmeli. `OKAN_gorevler.md`'deki çalılık maddesini güncelleyin (madde 5).
- **Yiğit (entegrasyon):**
  - `Match.unity`'de `Map.prefab` içi boş kalır; harita çalışma anında kurulur.
  - Tekrarlanabilirlik için `Services.Rng`'den çekiliş sırası her maçta aynı olmalı: önce harita üretilir, sonra `MatchManager` doğma noktalarını dağıtır. Entegrasyonda bu sıra sabitlenecek ve NFR-07 testine harita da eklenecek.

## Değişmeyenler

Klasör sahipliği, sözleşme kuralı, PR inceleme ve birleştirme akışı, SRS'teki öğe sayıları ve ölçüler, güvenli alan aşamaları aynen geçerlidir.
