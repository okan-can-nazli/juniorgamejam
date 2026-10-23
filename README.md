# Leaf Brawl – Junior Game Jam

Sonbahar temalı, üstten görünümlü 2D eleme arena oyunu (Unity 6000.6.7f1, URP 2D).

- **Gereksinimler (SRS v1.1):** [docs/Leaf_Brawl_SRS_Demo_v1.1.md](docs/Leaf_Brawl_SRS_Demo_v1.1.md) · [PDF](docs/Leaf_Brawl_SRS_Demo_v1.1.pdf)
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
