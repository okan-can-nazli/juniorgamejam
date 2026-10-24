# Leaf Brawl – Claude kuralları

Sonbahar temalı, üstten görünümlü 2D eleme arena oyunu. Unity **6000.6.7f1**, URP 2D, yeni Input System, masaüstü yapısı (Windows, macOS). WebGL yok.

## Her oturumun başında
1. Bu dosyayı ve kullanıcının görev dosyasını oku: `docs/gorev_dagilimi/<İSİM>_gorevler.md`.
2. Oyunun ne yapacağını yalnızca **SRS v1.4** söyler: `docs/Leaf_Brawl_SRS_Demo_v1.4.md`. SRS'in dışına çıkma; yazmayan şey demoda yoktur (SRS 8).
3. Ortak sözleşme görev dosyalarındaki "Ortak sözleşme (Shared)" bölümündedir ve `Assets/_Game/Scripts/Shared/` içinde kodlanmıştır.

## Klasör sahipliği (en önemli kural)
Yalnızca kullanıcının kendi klasörlerindeki dosyaları değiştir. Başkasının dosyasına ihtiyaç varsa değiştirme; GitHub'da issue aç (`[Sahip] başlık`).

| Yol | Sahibi |
| --- | --- |
| `Scripts/Shared/` (ortak sözleşme) | Yiğit; değişiklik yalnızca "sözleşme" etiketli PR + 4 onay |
| `Scripts/Core/`, `Scripts/Characters/`, `Prefabs/Systems/`, `Prefabs/Characters/` | Yiğit |
| `Scripts/Combat/`, `Scripts/Bots/`, `Prefabs/Combat/`, `Prefabs/Bots/` | Hamza |
| `Scripts/World/`, `Prefabs/World/` | Okan |
| `Scripts/Presentation/`, `Prefabs/Presentation/`, `Prefabs/UI/`, `Art/`, `Audio/`, `Fonts/` | Furkan |
| `Scenes/Match.unity` | Yiğit (başka kimse açıp kaydetmez) |
| `Scenes/MainMenu.unity` | Furkan |
| `Scenes/Sandbox/Sandbox_<İsim>.unity` | o kişi |
| `Input/`, `Settings/`, `ProjectSettings/`, `Packages/`, `.gitignore`, `.gitattributes`, `CLAUDE.md`, `README.md` | Yiğit |
| `Assets/StreamingAssets/balance.json` | yapı Yiğit; her kişi yalnızca kendi bölümünün değerlerini değiştirir |

Bütün yollar `Assets/_Game/` altındadır (StreamingAssets hariç).

## Sözleşme kuralları
- Sözleşmede ad değiştirme ve silme yok; yalnızca ekleme (issue → "sözleşme" PR → 4 onay).
- Başkasının sistemine yalnızca sözleşmedeki üyelerle eriş (`Character`'ın herkese açık yüzü, `Services`, `GameEvents`). Başkasının iç sınıfına `GetComponent` yapma.
- Hasar yalnızca `ApplyDamage`, yaprak yalnızca `Inventory.TryAdd`, rastgelelik yalnızca `Services.Rng` (UnityEngine.Random yasak, NFR-07).
- Görsel ve ses kodu (Presentation) yalnızca `GameEvents` dinler ve değer okur; oyun durumuna yazmaz.
- İsim alanları: `LeafBrawl.Shared`, `.Core`, `.Characters`, `.Combat`, `.Bots`, `.World`, `.Presentation`. Tek assembly, asmdef yok.

## Sayılar ve birimler
- Oyundaki hiçbir sayı koda yazılmaz: `Services.Config` (`balance.json`) ve `Services.DebugConfig` (`debug.json`, kişisel ayarlar gitignore'lu `debug.local.json`).
- `balance.json`'daki uzunluklar **dünya px**; kodda `Units.ToUnits(px)` ile çevir (40 px = 1 Unity birimi). `Vector2` konumlar Unity birimidir; arayüzlerdeki `...Px` parametreleri px'tir.
- Sprite'lar PPU 16, Filter Point, Compression None. Kamera Pixel Perfect 640×360.
- Katmanlar: `Character`, `Obstacle`, `Bush`, `Pickup`, `Tree`. `Character` yalnızca `Obstacle` ile çarpışır; `Bush` ve `Pickup` trigger'dır.
- Sıralama katmanları (alttan üste): `Ground`, `Pickups`, `Pile`, `Characters`, `Bushes`, `Canopy`, `Effects`, `ZoneOverlay`.

## Çalışma döngüsü
1. Script yaz (`create_script` / `apply_text_edits`). Bir MonoBehaviour = bir dosya, dosya adı = sınıf adı.
2. Her script değişikliğinden sonra `read_console` (error) ile derleme hatalarını düzelt.
3. Kendi sandbox sahnende Play mode ile dene (`manage_editor` play/stop). Unity MCP ile yalnızca kendi sandbox sahneni ve kendi prefab'larını düzenle; `Match.unity`'yi yalnızca Yiğit'in oturumu düzenler.
4. Çalışan her adımı commit'le ve dalı push'la.

## Git akışı
- `main`'e doğrudan push yok. Başlamadan: `git switch main && git pull`, sonra `git switch -c <isim>/<konu>`.
- Her çalışan adımda: `git status` ile neyi eklediğine bak, `git add <dosyalar>`, `git commit -m "<ne çalışıyor>"`, `git push -u origin <dal>`.
- İş bitince `gh pr create --fill`; PR açıklamasında SRS maddeleri (FR-xx), nasıl denenir, görselse ekran görüntüsü. Yiğit'in PR'larını Hamza, diğerlerini Yiğit inceler; birleştirmeyi Yiğit yapar (squash).
- Commit ve PR'lara yapay zekâ ortak yazar satırı (`Co-Authored-By: ...`) **eklenmez**.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/` asla commit'lenmez.
- Sahne/prefab çakışmasında UnityYAMLMerge kullanılır (README'deki `git config` satırı). Çözülemezse kendi değişikliğini koru ve küçük değişikliği elle yeniden yap.

## "Bitti" ne demek
Console'da kırmızı hata yok · kendi sandbox sahnende çalışıyor · sayılar balance.json'dan · yalnızca kendi klasörlerin değişti · SRS maddesi yazıldığı gibi davranıyor.

## Token kuralları
- `Library/`, `Temp/`, `obj/`, `Logs/`, `Builds/` okunmaz.
- Sahneyi dar sorgula (`find_gameobjects`, sayfalı `get_hierarchy`); sahne ve prefab dosyasının tamamını dökme.
- Yeni yazdığın script'i yeniden okuma; yalnızca değiştirdiğin kısmı oku.
- Kullanıcı istemedikçe test, belge veya "ileride lazım olur" soyutlaması yazma.

## Yapı
Windows ve macOS masaüstü yapısı, tek klasör, kurulum yok, ağ isteği yok (NFR-04, NFR-11). Build Settings: `MainMenu` (0), `Match` (1).
