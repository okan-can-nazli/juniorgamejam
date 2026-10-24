# Leaf Brawl – Junior Game Jam

Sonbahar temalı, üstten görünümlü 2D eleme arena oyunu (Unity 6000.6.7f1, URP 2D).

- **Gereksinimler (SRS v1.4):** [docs/Leaf_Brawl_SRS_Demo_v1.4.md](docs/Leaf_Brawl_SRS_Demo_v1.4.md). Eski sürümler (v1.0, v1.1, v1.1.1, v1.2) yalnızca geçmiş için durur.
- **Görev dağılımı:** herkes kendi dosyasını okuyup başlar.
  - [Yiğit](docs/gorev_dagilimi/YIGIT_gorevler.md) – iskelet, sözleşme, karakter, tarif, maç akışı, entegrasyon
  - [Hamza](docs/gorev_dagilimi/HAMZA_gorevler.md) – balta, yetenekler, botlar
  - [Okan](docs/gorev_dagilimi/OKAN_gorevler.md) – harita, ağaç, yaprak, çalılık, alan, ganimet, testler
  - [Furkan](docs/gorev_dagilimi/FURKAN_gorevler.md) – pixel art, karakter, efekt, ses, HUD ve ekranlar
- **Claude kullananlar:** [CLAUDE.md](CLAUDE.md)

## Projeyi açma

1. Unity Hub → Installs → **6000.6.7f1** (başka sürüm kullanma). Modüller: Mac Build Support, Windows Build Support (Mono).
2. `git clone https://github.com/okan-can-nazli/juniorgamejam.git`
3. Sahne/prefab birleştiricisini klonda bir kez ayarla:
   - macOS: `git config merge.unityyamlmerge.driver '"/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Helpers/UnityYAMLMerge" merge -p %O %B %A %A'`
   - Windows: `git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %A'`
4. Unity Hub → Add → repo klasörü → aç. İlk açılış birkaç dakika sürer (paketler iner).
5. `Assets/_Game/Scenes/Sandbox/Sandbox_<İsmin>.unity` sahnesini aç ve Play'e bas: W/A/S/D ile yürü, fareyle bak.

## Nerede ne var

| Yol | İçerik |
| --- | --- |
| `Assets/_Game/Scripts/Shared/` | Ortak sözleşme: enum'lar, `Services`, `GameEvents`, `Balance`, stub servisler |
| `Assets/_Game/Scripts/<Core, Characters, Combat, Bots, World, Presentation>/` | Herkesin kendi kodu (sahiplik görev dosyalarında) |
| `Assets/_Game/Prefabs/` | `Systems`, `Character`/`Player`, `Bot`, `CharacterCombat`, `CharacterVisual`, `Map`, `SafeZone`, `HUD`, `AudioManager` |
| `Assets/_Game/Scenes/` | `Match.unity` (asıl sahne), `MainMenu.unity`, `Sandbox/` |
| `Assets/StreamingAssets/balance.json` | Bütün oyun sayıları (SRS Bölüm 3). Derlenmiş yapıda da düzenlenebilir |
| `Assets/StreamingAssets/debug.json` | Test ayarları: tohum, bot sayısı, başlangıç envanteri ve seviyeleri, timeScale. Kişisel değişiklik için yanına `debug.local.json` koy (commit'lenmez) |

Main'e doğrudan push yok: her iş `isim/konu` dalında yapılır ve PR ile birleşir (ayrıntılar görev dosyalarındaki "Herkes için ortak kurallar" bölümünde).

## Oyunu çalıştırma

- **Tam maç:** `Assets/_Game/Scenes/Match.unity` → Play. 3 sn geri sayım, sonra oyuncu + 3 bot. Esc duraklatır.
- **Ana menüden:** `Assets/_Game/Scenes/MainMenu.unity` → Play → Başla.
- **Debug kısayolları (yalnızca editör):** F2 envantere 10'ar yaprak, F3 bütün seviyeler +1, F4 bot durumları (Scene görünümü).
- **Unity arka plandayken Play ilerlemez.** Tuşlar yalnızca Unity öndeyken ve Game görünümüne tıklanmışken çalışır.

## Test ayarları (`debug.local.json`)

`Assets/StreamingAssets/debug.local.json` dosyası kişiseldir, commit'lenmez. Yalnızca yazdığın alanlar `debug.json`'u ezer:

```json
{ "seed": 12345, "showOverlay": true, "botCount": 1, "timeScale": 2 }
```

- `seed`: sabit tohum; aynı harita ve aynı yaprak üretimi (NFR-07).
- `showOverlay`: FPS sayacı; FPS ve sahne açılış süreleri log'a da yazılır (KT-14, NFR-03).
- **Yapı almadan önce bu dosyayı sil**, yoksa yapının içine kopyalanır.

## Yapı alma (Windows ve macOS)

- File → Build Profiles → Windows veya macOS → Build. Çıktıyı `Builds/` altına al (git'e girmez).
- Sahne sırası: `MainMenu` (0), `Match` (1).
- Yapı sonrası Unity `Assets/_Game/Settings/PC_RPAsset.asset` ve `UniversalRenderPipelineGlobalSettings.asset`'i yeniden yazar (URP shader verisi). **Bunları commit'leme:** `git checkout -- Assets/_Game/Settings`.
- Yapıdaki `*_BackUpThisFolder_ButDontShipItWithYourGame` klasörünü dağıtma.

## Sık yapılan hatalar

- **`.meta` dosyası:** Unity'ye eklenen her dosyanın `.meta`'sı da commit'lenmeli; yoksa herkeste farklı GUID oluşur ve prefab bağlantıları kopar. Dosyaları GitHub web'den değil Unity'den ekle; taşırken Unity'nin Project penceresini kullan.
- **Görseller:** `Assets/_Game/Art/Sprites/<Klasör>/` altına. İçe aktarma: PPU 16, Point, Compression None; karakterde pivot ayak ortası. main'e girmiş bir dosyanın adını veya yerini değiştirme.
- **Script adı:** Dosya adı = sınıf adı (`MainMenuView.cs` → `class MainMenuView`); yoksa Unity bileşeni sahneye ekleyemez.
- **Satır sonları (Windows):** `git config core.autocrlf true`. Yoksa dosyalar herkeste sürekli "değişmiş" görünür.
- **`Match.unity`'yi yalnızca Yiğit kaydeder;** diğerleri Play'de dener, kaydetmez. Herkesin kendi `Sandbox_<İsim>.unity` sahnesi var.
- **Sayılar koda yazılmaz:** bütün oyun değerleri `balance.json`'da (uzunluklar dünya px, 40 px = 1 Unity birimi).
