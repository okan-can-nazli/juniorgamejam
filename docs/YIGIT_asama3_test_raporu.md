# Yiğit – Aşama 3 test ve yapı raporu (24 Ekim 2026)

Kaynak: SRS v1.4 Bölüm 7 (NFR) ve 9.1 (KT). Test edilen kod: main (`85824b0`, #27 dahil) + bu dal (geri sayım düzeltmesi, `DebugOverlay`, VSync).
Kabul testleri tablosu Okan'ın (`docs/kabul_testleri.md`); bu sonuçlar oraya işlensin diye Okan'a issue açıldı.

## Kabul testleri (Yiğit'in payı)

| No | Nasıl denendi | Sonuç |
| --- | --- | --- |
| KT-01 | Match.unity, Play. Geri sayım olayları ve karakter konumları kaydedildi | **Geçti** (düzeltmeden sonra): 3 → 2 → 1 → 0 arası tam 1,00 sn, toplam 3,0 sn; 4 karakter 4 farklı doğma noktasında; maç saati 0:00'dan başlıyor. Düzeltmeden önce geri sayım ~2,3–2,8 sn sürüyordu, aşağıda |
| KT-05 | Güvendeyken envanter 3 K + 1 S + 1 Y, Güç tarifi | **Geçti** (mantık): üretim 1,50 sn, Güç 1 → 2, vuruş hasarı 13, 5 yaprak düştü, tehdit puanı 1. Üretim çubuğu ve "Güç 1 → 2" bildirimi Furkan'ın HUD'unda (#32) |
| KT-06 | Yapraksız Hız tarifi; yeterli yaprakla hasar alır almaz Güç tarifi | **Geçti** (mantık): ilki `NotEnoughLeaves`, ikincisi `NotSafe` ile reddedildi, seviye değişmedi. Kırmızı işaret Furkan'da (#32, #33) |
| KT-08 | `SetLevel` ile seviyeler adım adım 5'e | **Geçti** (mantık): tehdit 2 → Küçük, 4 → Orta, 7 → Büyük; hepsi 5'te puan 12; atılma, kalkan, ulti açıldı; üç kart da `MaxLevel`. Yığın görseli ve "En üst seviye" yazısı Furkan'da (#31, #33) |
| KT-11 | KT-12 ile aynı maç | **Kısmen**: sonuç verisi doğru (sıra 2, süre 300,3 sn, eleme 0, tehdit 0; sıralama FR-83'e uygun). Sonuç ekranı Furkan'da (#34) |
| KT-12 | Oyuncu saldırmadan alanın merkezinde, saat 4:55'e alındı | **Geçti**: maç 5:00,33'te bitti (≤ 5:05). Oyuncu ve bir bot aynı karede öldü; ölümden önce canı fazla olan bot (25 > 5) 1., oyuncu 2. (FR-83) |
| KT-16 | 10 hasar, sonra bekleme; ikinci yenilenmeden sonra yeniden hasar | **Geçti**: ilk +1 can hasardan 15,00 sn sonra (5 sn bekleme + 10 sn), ikincisi 25,00 sn'de; yeniden hasarda sayaç sıfırlandı, yine 15,00 sn |
| KT-14 | macOS yapısı, 1920×1080, ~2 dk | **Kısmen**: çökme yok, ağ bağlantısı yok, FPS 60'ın çok üstünde (aşağıda). 10 maç üst üste yapıda henüz oynanmadı; editörde 10 maç (Hamza #20 ve Yiğit) hatasız |

## Fonksiyonel olmayan gereksinimler

| No | Sonuç |
| --- | --- |
| FR-13 / 3.13 (kamera) | **Geçti**: 1920×1080'de görünen dünya tam 1600×900 px. 16:10'da (1920×1200, 2560×1600) siyah bant yok, 1600×1000 px görünüyor. Pixel Perfect Camera kırpma kapalı (`CropFrame: 0`) |
| NFR-01 (performans) | **Geçti**: macOS yapısı, bu Mac, 1920×1080: VSync kapalıyken ortalama 1.420–1.580 FPS, en düşük 1.061, en uzun kare 5–36 ms. Yapı sınırsız kare çizdiği için **VSync açıldı** (PC kalite seviyesi); artık ekran yenileme hızıyla sınırlı. M1 / i5 dizüstünde ölçüm Aşama 3 tur 2'de |
| NFR-03 (açılış) | **Geçti**: macOS yapısında uygulama açılışından ana menüye 2,10 sn (≤ 10 sn); Match sahnesi soğuk açılışta 2,13 sn. "Başla" düğmesi henüz yok (#35); menüden Match'e geçiş editörde 0,02 sn |
| NFR-04 (çevrimdışı) | **Geçti**: yapı çalışırken `lsof` ile hiçbir ağ bağlantısı görülmedi |
| NFR-07 (tekrarlanabilirlik) | **Geçti**: tohum 12345 ile iki ayrı koşuda harita yerleşimi (65 öğe) ve ilk 60 sn'de düşen 93 yaprağın konum ve tür dizisi birebir aynı |
| NFR-11 (dağıtım) | **Geçti**: macOS (`LeafBrawl.app`, 77 MB) ve Windows (`LeafBrawl.exe` + `LeafBrawl_Data`, 103 MB) yapıları hatasız; tek klasör, kurulum yok. Scripting backend Mono. Windows yapısı Mac'te alındı, Windows'ta çalıştırılarak henüz denenmedi |

## Bulunan ve düzeltilen hata

**Geri sayım 3 sn yerine ~2,3–2,8 sn sürüyordu (FR-05).** Sahne yüklendikten sonraki iki karede Unity `Time.deltaTime`'ı 0,333 verir ama `Time.time` ilerlemez; `MatchManager` geri sayımı `deltaTime` toplayarak saydığı için gerçekte geçmemiş süreyi düşüyordu. Artık geri sayım, başladığı andan beri geçen `Time.time`'a göre hesaplanıyor. Duraklatmada `Time.time` durduğu için Esc ile duraklatma aynen çalışıyor.

## Yeni araç: debug katmanı (`Core/DebugOverlay.cs`)

`debug.json`'daki `showOverlay` M0'dan beri vardı ama kullanılmıyordu. Açılınca ekranın sol üstünde FPS görünür. Console'a ve yapıda `Player.log`'a 10 sn'de bir ortalama ve en düşük FPS ile en uzun kare, sahne açılışında da açılış süresi yazılır. Varsayılan kapalıdır.

- **Editörde:** `Assets/StreamingAssets/debug.local.json` → `{ "showOverlay": true }` (gitignore'lu; commit'leme).
- **Yapıda:** Aynı dosyayı yapının `StreamingAssets` klasörüne koy. macOS: `LeafBrawl.app/Contents/Resources/Data/StreamingAssets/`, Windows: `LeafBrawl_Data/StreamingAssets/`.
- **Dikkat:** Yapı almadan önce `debug.local.json`'ı sil ya da taşı; yoksa yapının içine kopyalanır.

## Kalanlar

- Windows yapısını bir Windows makinede açıp oynamak (KT-14'ün Windows kısmı).
- KT-14: yapıda 10 maç üst üste, ağ kapalı.
- Görsel kısımlar Furkan'ın işleri gelince yeniden denenecek: KT-05 bildirimi, KT-06 kırmızı işaret, KT-07, KT-08 yığın ve kart, KT-11 sonuç ekranı, KT-18.
- Yapılar GitHub Releases'a `asama-3`'te yüklenecek (SRS 10).
