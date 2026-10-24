# Hamza – Aşama 3 durumu: test ve ayar

24 Ekim 2026 · Muhammed Hamza Karamanlı · dal `hamza/bot-kacis-duzeltme`

Görev dosyamdaki ([HAMZA_gorevler.md](gorev_dagilimi/HAMZA_gorevler.md)) Aşama 3 › Ayar ve test bölümü. Testler Unity 6000.6.7f1'de, main üzerinde (Okan'ın harita, ağaç, yaprak ve güvenli alanı dahil), geçici test betikleriyle yapıldı; betikler commit'lenmedi, `Match.unity` kaydedilmedi.

## KT-17 – Kalkan ve ulti: geçti (7/7)

`Sandbox_Hamza`'da Sağlık 5 bir karakter kalkanını açtı; yanındaki Güç 5 bot kendi kararıyla (BOT-13) ultiyi attı.

| Adım | Sonuç |
| --- | --- |
| Oyuncu kalkanı açtı (can 160/160); bot ultiyi başlattı | Geçti |
| 0,40 sn: ulti kalkana çarptı, **kalkan kırıldı, can 160/160** | Geçti |
| Ulti kullanıldı (Used) | Geçti |
| Q ikinci kez çalışmadı; botun ultisi ikinci kez başlamadı | Geçti |

Ultiden sonra bot baltaya geçiyor (22 hasar, Güç 5); kalkan artık kırık olduğu için bu beklenen davranış.

## NFR-06 – 10 maç üst üste: geçti

`Match.unity`'de 1 oyuncu + 3 bot, 10 maç art arda (6× hız; oyuncu botlara görünmez ve ölümsüz, böylece maç botlar arasında geçer).

| Ölçüt | Sonuç |
| --- | --- |
| Hata / istisna | **0** |
| Tamamlanan maç | 10/10 (her biri ~4:55'te ani ölümle bitti) |
| Donma (> 250 ms kare) | 4 – hepsi Unity arka plandayken; aynı karelerde bot süresi en fazla 15,8 ms, yani bot kaynaklı değil |
| Takılma (2 sn'de < 20 px) | 14 → düzeltmeden sonra **0** (aşağıda) |
| Alan dışında en uzun kalma | **2,3 sn** (KT-13 sınırı 3 sn) |

## Bot performansı (NFR-01 bütçesi: 3 bot karede < 1 ms)

`BotBrain.ReadIntent` süresi, 3 bot toplamı, kare başına (geçici kronometre, commit'lenmedi):

| | 10 maç (düzeltme öncesi) | 4 maç (düzeltme sonrası) |
| --- | --- | --- |
| Ortalama | 0,004 ms | 0,005 ms |
| %99'luk dilim | 0,064 ms | 0,072 ms |
| En yüksek | 15,8 ms (ızgaranın ilk kurulduğu kare) | 5,1 ms |
| 1 ms'yi aşan kare | 14 / 536 664 | 5 / 197 641 |

## KT-13 – 3 bot × 5 maç

İlk 5 maçta: her bot en az 1 tarif üretti; her bot en az 1 ağaç kesti (Saldırgan 4, Dayanıklı 2, Çevik 2); alan dışında en uzun 2,3 sn; düzeltmeden sonra takılma yok. **Geçti.**

Not: Ağaç kesme maç başına seyrek (bazı maçlarda hiç). BOT-07'ye göre bot ancak görünen yaprak yokken ağaç keser; ortada sürekli yaprak var. Denge turunda konuşulabilir.

## Bu testte bulunan ve düzeltilen 2 şey

1. **Kaçış noktası alan dışına düşüyordu.** Alan küçülmüşken kaçan bot, rakipten 500 px uzaktaki noktaya (alan ya da harita dışı) gitmeye çalışıyor, Kaç ile Alana dön arasında gidip geliyordu (14 takılma uyarısının 13'ü). Artık nokta alan dışındaysa alanın içinde, rakibin ters yönündeki en uzak noktaya kaçıyor. Çalılık (Okan) alan içindeyse yine önce çalılığa gider.
2. **Izgara maç içinde kuruluyordu.** İlk yol isteğinde bir kerelik ~16 ms takılma oluyordu. Artık `BotBrain.Start`'ta, geri sayım sırasında kuruluyor (`Physics2D.SyncTransforms` ile, harita aynı karede kurulmuş olsa bile).

## Kalan

- [ ] Okan'la denge turu: 5 dakikada botlar en fazla 4. seviyeye çıkıyor, maçta 5. seviye yeteneklerini kullanamıyorlar; ağaç kesme seyrek.
- [ ] Okan'ın çalılıkları (VisibilityService) gelince kaçış ve görünmezlikle bir tur daha.
- [ ] PR #16 (IPickup türü) birleşince bottaki geçici yaprak çözümünü türe bakan kontrolle değiştirmek.
