# Kabul Testleri Takibi (KT-01 – KT-18)

Kaynak: SRS Bölüm 9.1. Koordinatör: Okan. Sonuç değerleri: **Geçti / Kaldı / Denenmedi**.
Bir test kalırsa sahibine `[Sahip] KT-xx ...` başlıklı issue açılır ve bağlantısı son sütuna yazılır.

| No | Test | Sahibi | Son deneme tarihi | Sonuç | Issue |
| --- | --- | --- | --- | --- | --- |
| KT-01 | Başla, 3 sn geri sayım, 4 karakter farklı doğma noktasında, saat 0:00 | Yiğit | 24.10.2026 | Geçti | 4 karakter 4 farklı doğma noktasında, geri sayım 3-2-1-0, maç saati 0'dan, botlar hareket ediyor |
| KT-02 | 30 sn hareket: 8 yön, 220 px/sn, gövde ve kayadan geçmez, çalılık ve kütükten geçer | Okan | 24.10.2026 | Geçti | 8 yönde 220 px/sn (çapraz dahil); kayada ve gövdede duruyor; çalılık ve kütükten geçiyor |
| KT-03 | Bota 10 vuruş: 10. isabette ölür, tek ganimet yığını | Hamza | 24.10.2026 | Denenmedi | 10 vuruşta ölüm Yiğit'in testinde geçti (PR #2); tek ganimet yığını henüz denenmedi |
| KT-04 | Ulu ağaçta 60 sn yaprak topla: otomatik toplama, HUD adetleri, sınırdaki tür toplanmaz | Okan | 24.10.2026 | Denenmedi | Mantık geçti: 60 sn'de 15–17 yaprak otomatik toplandı, sınırdaki K toplanmadı. HUD adetleri arayüz bekliyor (#32) |
| KT-05 | 3 K + 1 S + 1 Y ile 1'e bas: Güç 1 → 2, hasar 13 | Yiğit | 24.10.2026 | Denenmedi | Mantık geçti: Güç 1 → 2, hasar 13, 5 yaprak düştü. Bildirim ve üretim çubuğu arayüz bekliyor (#32) |
| KT-06 | Yaprak yetmezken ve hasar alır almaz üretim denemesi: seviye değişmez, kırmızı uyarı | Yiğit | 24.10.2026 | Denenmedi | Mantık geçti: NotEnoughLeaves ve NotSafe ile reddedildi, seviye değişmedi. Kırmızı uyarı arayüz bekliyor (#32, #33) |
| KT-07 | Tab paneli: 4 kart, vurgulu ve soluk durumlar, hareket var saldırı yok | Furkan | 24.10.2026 | Denenmedi | Envanter paneli yok (#33) |
| KT-08 | Debug ile Güç, Hız, Sağlık 5: yığın kademeleri, yetenekler açılır | Yiğit | 24.10.2026 | Denenmedi | Mantık geçti: tehdit 2 → Küçük, 4 → Orta, 7 → Büyük; 3 yetenek açıldı. Yığın görseli ve 'En üst seviye' arayüz bekliyor (#31, #33) |
| KT-09 | Çalılıkta bot geçerken görünmezlik, savurunca 1 sn görünme | Okan | 24.10.2026 | Geçti | Çalılıktaki oyuncuyu 100 px'teki bot 2 sn hedef almadı; savurunca 0,1–0,2 sn içinde hedef aldı |
| KT-10 | 1:30'a kadar köşe korusunda bekle: daralma, 4 hasar/sn, koru ağaçları üretimi bırakır | Okan | 24.10.2026 | Denenmedi | Mantık geçti: koru 2:05'te alan dışında, hasar 4,0/sn, 2:16 sonrası korudan yaprak yok. Kırmızı ekran kenarı arayüz bekliyor (#32) |
| KT-11 | Ölünceye kadar oyna: sonuç ekranı sıra, süre, eleme, tehdit | Yiğit | 24.10.2026 | Denenmedi | Sonuç ekranı yok (#34); MatchResult doluyor (KT-12'de görüldü) |
| KT-12 | Saldırmadan hayatta kal: maç en geç 5:05'te biter | Okan (Yiğit'le) | 24.10.2026 | Geçti | Saldırmadan bekleyen oyuncuyla maç 4:52'de bitti (≤ 5:05), sonuç dolu |
| KT-13 | 3 botu 5 maç izle: tarif, ağaç kesme, alan dışı ve takılma sınırları | Hamza | 24.10.2026 | Geçti | Hamza: 10 maçta her bot tarif üretti ve ağaç kesti; alan dışı en fazla 2,3 sn; takılma düzeltmeden sonra 0 (PR #20, issue #23) |
| KT-14 | Ağ kapalı, 10 maç, FPS sayacı: çökme yok, FPS 60 altına düşmez | Yiğit | 24.10.2026 | Denenmedi | Windows/macOS yapısı henüz alınmadı. Editörde 10 maç 0 hata (NFR-06, PR #20) |
| KT-15 | Güç 1'de küme ağacını kes: yaklaşık 20 sn, 6–10 yaprak, kütük | Okan | 24.10.2026 | Geçti | Güç 1: 40 vuruş, 19,7 sn; 9–10 yaprak saçıldı; kütükten geçiliyor |
| KT-16 | Hasar al, 5 sn bekle: yenilenme başlar, hasarla durur | Yiğit | 24.10.2026 | Geçti | Sağlık 1: 5 sn bekleme + 10 sn'de 1 HP; yeniden hasar yenilenmeyi durdurdu |
| KT-17 | Kalkan ve bot ultisi: kalkan kırılır, can değişmez, Q ve E ikinci kez çalışmaz | Hamza | 24.10.2026 | Geçti | Hamza: ulti kalkanı kırdı, can 160/160 kaldı; Q ve E ikinci kez çalışmadı (PR #20) |
| KT-18 | Kılavuz sayfaları ve Esc ile ana menü | Furkan | 24.10.2026 | Denenmedi | Ana menü ve kılavuz yok (#35) |

Son sütun bu turda issue yerine kısa not içeriyor; "Denenmedi" olanların çoğunda oyun mantığı geçti, yalnızca Furkan'ın arayüz parçası (parantezdeki issue) bekleniyor.

## Turlar

- Ön tur (24.10.2026, Hamza, Okan'ın yerine): main üzerinde, Unity editöründe, geçici test betiğiyle (oyuncu ve test karakterleri betikle yönetildi). 8 test geçti; 10 test "Denenmedi" (KT-04, 05, 06, 08 ve 10 dahil 5 tanesinin oyun mantığı geçti, arayüz bekliyor; KT-03 ganimet kısmı, KT-07, 11, 18 arayüz, KT-14 yapı). Kalan test yok.
- Tur 1: `asama-2` etiketinden sonra.
- Tur 2: teslimden önce.

## Ek kontrol (NFR-07)

Sabit tohumla tekrarlanabilirlik: `debug.local.json` içine `{ "seed": 12345 }`, iki kez Play, ilk 60 sn `LeafSpawned` (konum, tür) olayları ve harita yerleşimi aynı mı. Sonuç: **Geçti** (24.10.2026). Tohum 12345 ile iki maç: 45 ağaç ve ilk 60 sn'deki 66 yaprağın konum ve türü birebir aynı. Kontrol: tohum 777 farklı harita ve yaprak dizisi verdi. Not: botların kendi Rng akışı (#22) ve LeafSpawner/LootSystem akışları (#21, #26) main'de.
