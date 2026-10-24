# Kabul Testleri Takibi (KT-01 – KT-18)

Kaynak: SRS Bölüm 9.1. Koordinatör: Okan. Sonuç değerleri: **Geçti / Kaldı / Denenmedi**.
Bir test kalırsa sahibine `[Sahip] KT-xx ...` başlıklı issue açılır ve bağlantısı son sütuna yazılır.

| No | Test | Sahibi | Son deneme tarihi | Sonuç | Issue |
| --- | --- | --- | --- | --- | --- |
| KT-01 | Başla, 3 sn geri sayım, 4 karakter farklı doğma noktasında, saat 0:00 | Yiğit | | Denenmedi | |
| KT-02 | 30 sn hareket: 8 yön, 220 px/sn, gövde ve kayadan geçmez, çalılık ve kütükten geçer | Okan | | Denenmedi | |
| KT-03 | Bota 10 vuruş: 10. isabette ölür, tek ganimet yığını | Hamza | | Denenmedi | |
| KT-04 | Ulu ağaçta 60 sn yaprak topla: otomatik toplama, HUD adetleri, sınırdaki tür toplanmaz | Okan | | Denenmedi | |
| KT-05 | 3 K + 1 S + 1 Y ile 1'e bas: Güç 1 → 2, hasar 13 | Yiğit | | Denenmedi | |
| KT-06 | Yaprak yetmezken ve hasar alır almaz üretim denemesi: seviye değişmez, kırmızı uyarı | Yiğit | | Denenmedi | |
| KT-07 | Tab paneli: 4 kart, vurgulu ve soluk durumlar, hareket var saldırı yok | Furkan | | Denenmedi | |
| KT-08 | Debug ile Güç, Hız, Sağlık 5: yığın kademeleri, yetenekler açılır | Yiğit | | Denenmedi | |
| KT-09 | Çalılıkta bot geçerken görünmezlik, savurunca 1 sn görünme | Okan | | Denenmedi | |
| KT-10 | 1:30'a kadar köşe korusunda bekle: daralma, 4 hasar/sn, koru ağaçları üretimi bırakır | Okan | | Denenmedi | |
| KT-11 | Ölünceye kadar oyna: sonuç ekranı sıra, süre, eleme, tehdit | Yiğit | | Denenmedi | |
| KT-12 | Saldırmadan hayatta kal: maç en geç 5:05'te biter | Okan (Yiğit'le) | | Denenmedi | |
| KT-13 | 3 botu 5 maç izle: tarif, ağaç kesme, alan dışı ve takılma sınırları | Hamza | | Denenmedi | |
| KT-14 | Ağ kapalı, 10 maç, FPS sayacı: çökme yok, FPS 60 altına düşmez | Yiğit | | Denenmedi | |
| KT-15 | Güç 1'de küme ağacını kes: yaklaşık 20 sn, 6–10 yaprak, kütük | Okan | | Denenmedi | |
| KT-16 | Hasar al, 5 sn bekle: yenilenme başlar, hasarla durur | Yiğit | | Denenmedi | |
| KT-17 | Kalkan ve bot ultisi: kalkan kırılır, can değişmez, Q ve E ikinci kez çalışmaz | Hamza | | Denenmedi | |
| KT-18 | Kılavuz sayfaları ve Esc ile ana menü | Furkan | | Denenmedi | |

## Turlar

- Tur 1: `asama-2` etiketinden sonra.
- Tur 2: teslimden önce.

## Ek kontrol (NFR-07)

Sabit tohumla tekrarlanabilirlik: `debug.local.json` içine `{ "seed": 12345 }`, iki kez Play, ilk 60 sn `LeafSpawned` (konum, tür) olayları ve harita yerleşimi aynı mı. Sonuç: Denenmedi.
