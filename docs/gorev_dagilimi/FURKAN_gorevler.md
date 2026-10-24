# Görev Dosyası – Furkan

**Rol:** Oyunun görünen ve duyulan her şeyi: pixel art, oyuncu karakteri ve animasyonlar, efektler, sesler, HUD, ana menü, kılavuz, kontroller, duraklatma ve sonuç ekranları
**Araçlar:** Pixel art için Aseprite, LibreSprite veya Piskel (ücretsiz). Ses düzenleme için Audacity (ücretsiz). Unity.
**Sahip olduğun yerler:** `Assets/_Game/Art/`, `Audio/`, `Fonts/`, `Scripts/Presentation/`, `Prefabs/Presentation/` (`CharacterVisual.prefab`, `AudioManager.prefab`), `Prefabs/UI/` (`HUD.prefab` ve ekran prefab'ları), `Scenes/MainMenu.unity`, `Scenes/Sandbox/Sandbox_Furkan.unity`, repo kökündeki `CREDITS.md`
**PR'larını kim inceler:** Yiğit

Senin işin oyun mantığından tamamen ayrı. Kuralı tek cümleyle söylemek gerekirse: **oyunu dinlersin ve gösterirsin, oyunu değiştirmezsin.** Görsel ve ses kodların yalnızca `GameEvents` olaylarını dinler ve `Character` ile `Services`'teki değerleri okur. Can, yaprak, seviye gibi hiçbir değeri yazmaz.

Aşağıda `Player`, `Services.Match.Player` (oyuncunun karakteri) demektir.

Oyun mantığına yaptığın çağrılar yalnızca şunlar olabilir:
- Düğmelerden `SceneFlow.StartMatch()`, `SceneFlow.Restart()`, `SceneFlow.LoadMenu()`, `SceneFlow.Quit()`.
- Duraklatma menüsünden `Services.Match.Resume()` ve `Services.Match.Restart()`.
- Envanter panelindeki karta tıklanınca `Player.Crafting.Request(tarif)` (1–4 tuşlarıyla aynı şey).

## Herkes için ortak kurallar

> Bu bölüm dört dosyada da **aynıdır**. Kendi görevlerine geçmeden önce bir kez oku. Bir kural değişirse dört dosyada birlikte değişir.

### Tek doğru kaynak

- Oyunun ne yapacağını **SRS v1.2** söyler: [`docs/Leaf_Brawl_SRS_Demo_v1.2.md`](../Leaf_Brawl_SRS_Demo_v1.2.md). Bu dosyalar yalnızca **işi kimin, nerede, hangi sırayla** yapacağını söyler.
- Her görevin yanında SRS madde numarası (FR-xx, BOT-xx, UI-xx, KT-xx) yazar. PR açıklamasına bu numaraları yaz.
- Oyundaki **hiçbir sayı koda yazılmaz.** Bütün değerler `Assets/StreamingAssets/balance.json` dosyasından `Services.Config` ile okunur (NFR-05). Uzunluklar dosyada **dünya px** olarak durur; kodda `Units.ToUnits(px)` ile çevrilir (40 px = 1 Unity birimi).

### Ekip ve sorumluluk özeti

| Kişi | Araçlar | Ana sorumluluk | SRS bölümleri |
| --- | --- | --- | --- |
| **Yiğit** | Claude Opus 5.5 + REA + Unity MCP | Proje iskeleti, ortak sözleşme, karakter, envanter ve tarif, maç akışı, kamera, entegrasyon, build | 3.1, 3.2, 3.6, 3.7, 3.9, 3.13; FR-01–FR-13 (mantık), FR-23–FR-25, FR-40–FR-46 (mantık), FR-60, FR-80, FR-82–FR-85; NFR-02–NFR-05, NFR-07, NFR-11 |
| **Hamza** | Claude Opus 5.5 + REA + Unity MCP | Balta, savaş, 5. seviye yetenekleri, botların tamamı | 3.3, 3.4; FR-20–FR-22, FR-26–FR-29; Bölüm 5 (BOT-01–BOT-13); NFR-06 |
| **Okan** | — | Harita, ağaçlar, yapraklar, ağaç kesme, çalılık ve görüş, güvenli alan, ölüm ganimeti, test koordinasyonu | 3.5, 3.8, 3.10, 3.11, 3.12; FR-30–FR-37, FR-50–FR-54, FR-70–FR-73, FR-81; Bölüm 9 takibi |
| **Furkan** | — | Bütün pixel art, karakter görseli ve animasyonlar, efektler, sesler, HUD ve bütün ekranlar (ana menü, kılavuz, kontroller, duraklatma, sonuç) | 3.13 (sanat), 6.2–6.4, FR-01–FR-03 (görsel), FR-23 (görsel), FR-61–FR-64, FR-71 (görsel); NFR-08–NFR-10 |

### Kurulum (herkes, bir kez)

1. **Unity 6000.6.7f1** kur (Unity Hub → Installs → tam olarak bu sürüm). Modüller: *Mac Build Support* ve *Windows Build Support (Mono)*. Başka sürümle proje açma, çünkü bütün dosyalar yeniden yazılır ve herkeste çakışma çıkar.
2. Repoyu klonla: `git clone https://github.com/okan-can-nazli/juniorgamejam.git`
3. Sahne ve prefab birleştiricisini bu klonda bir kez ayarla:
   - macOS: `git config merge.unityyamlmerge.driver '"/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/Helpers/UnityYAMLMerge" merge -p %O %B %A %A'`
   - Windows: `git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p %O %B %A %A'`
   - (`.gitattributes` dosyası M0'da Yiğit tarafından eklenir; bu sürücüyü `.unity`, `.prefab`, `.asset` dosyalarına bağlar.)
4. **M0 main'e birleşene kadar Unity projesi açma**: proje iskeleti M0'da gelir. O zamana kadar kendi dosyandaki "M0 beklerken" maddelerini yap.
5. M0 geldikten sonra: Unity Hub → Add → repo klasörü → aç. İlk açılış birkaç dakika sürer.

### Klasör sahipliği (çakışmayı önleyen asıl kural)

**Yalnızca kendi klasörlerindeki dosyaları değiştir.** Başkasının klasöründe bir şeye ihtiyacın varsa dosyayı değiştirme. Bunun yerine GitHub'da **issue** aç ve sahibini etiketle (`@...`), ya da ona yaz.

| Yol | Sahibi | Not |
| --- | --- | --- |
| `Assets/_Game/Scripts/Shared/` | Yiğit | **Ortak sözleşme.** Değişiklik yalnızca "sözleşme" etiketli PR ile ve 4 kişinin onayıyla yapılır |
| `Assets/_Game/Scripts/Core/`, `Scripts/Characters/` | Yiğit | Maç akışı, karakter, envanter, tarif |
| `Assets/_Game/Scripts/Combat/`, `Scripts/Bots/` | Hamza | Balta, yetenekler, botlar |
| `Assets/_Game/Scripts/World/` | Okan | Harita, ağaç, yaprak, çalılık, alan, ganimet |
| `Assets/_Game/Scripts/Presentation/` | Furkan | Yalnızca görsel ve ses: karakter görseli, HUD, ekranlar, efekt, ses |
| `Assets/_Game/Prefabs/Systems/`, `Prefabs/Characters/` | Yiğit | `Systems.prefab`, `Character.prefab`, `Player.prefab` |
| `Assets/_Game/Prefabs/Combat/`, `Prefabs/Bots/` | Hamza | `CharacterCombat.prefab`, `Bot.prefab` (Character'ın prefab varyantı) |
| `Assets/_Game/Prefabs/World/` | Okan | `Map.prefab`, ağaç, yaprak, çalılık, kaya, `SafeZone.prefab`, `LootPile.prefab` |
| `Assets/_Game/Prefabs/Presentation/`, `Prefabs/UI/` | Furkan | `CharacterVisual.prefab`, `HUD.prefab`, ekran prefab'ları, `AudioManager.prefab` |
| `Assets/_Game/Art/`, `Audio/`, `Fonts/` | Furkan | Bütün sprite, animasyon, ses, font |
| `Assets/_Game/Scenes/Match.unity` | Yiğit | Asıl oyun sahnesi. **Başka kimse açıp kaydetmez** |
| `Assets/_Game/Scenes/MainMenu.unity` | Furkan | Ana menü, kılavuz, kontroller |
| `Assets/_Game/Scenes/Sandbox/Sandbox_<İsim>.unity` | Her kişi kendi | Kendi deneme sahnen. Başkasınınkini açabilirsin ama **kaydetme** |
| `Assets/StreamingAssets/balance.json` | Yiğit (yapı) | Herkes yalnızca **kendi bölümünün değerlerini** değiştirebilir (aşağıdaki tablo) |
| `Assets/_Game/Input/`, `Assets/_Game/Settings/`, `ProjectSettings/`, `Packages/` | Yiğit | Katman, tag, input, URP ayarları. Başkası değiştirmez; ihtiyaç varsa issue açar |
| `docs/` | Herkes kendi dosyası | |

**Prefab'larda kural:** Bir karakter üç kişinin parçasından oluşur, ama her parça ayrı bir **iç içe prefab**tır. `Character.prefab` (Yiğit) kökü taşır, içinde `CharacterCombat.prefab` (Hamza) ve `CharacterVisual.prefab` (Furkan) çocuk olarak durur. Aynı şekilde `Match.unity` sahnesinde `Map.prefab`, `SafeZone.prefab` (Okan), `HUD.prefab` ve `AudioManager.prefab` (Furkan) baştan yerleştirilmiş durur. Bu boş prefab'ları M0'da Yiğit oluşturur ve yerine koyar. Sonra herkes yalnızca **kendi prefab dosyasının içini** açıp düzenler. Kimse başkasının prefab'ına bileşen eklemez.

### Git akışı

1. İşe başlamadan önce: `git switch main && git pull`
2. Her iş için yeni dal aç: `git switch -c <isim>/<konu>`. Örnekler: `yigit/m0-iskelet`, `hamza/balta`, `okan/agac-kesme`, `furkan/karakter-sprite`.
3. Çalışan her küçük adımı commit'le ve dalı push'la: `git push -u origin <dal>`.
4. İş bitince PR aç (GitHub'da "Compare & pull request" veya `gh pr create --fill`). PR açıklamasında şunlar olsun: hangi SRS maddeleri (FR-xx), nasıl test edilir (hangi sahne, ne yapılır), görsel işse ekran görüntüsü veya GIF.
5. **Main'e doğrudan push yok.** PR'ı bir kişi inceler ve onaylar:
   - Yiğit'in PR'larını Hamza inceler.
   - Hamza'nın, Okan'ın ve Furkan'ın PR'larını Yiğit inceler.
   - Yiğit yoksa Hamza inceler.
6. Onaylanan PR'ı **Yiğit birleştirir**: "Squash and merge", sonra dal silinir. Böylece main'e giren her şey entegrasyon sahibinden geçer.
7. PR'lar küçük olsun: bir PR bir özellik, en fazla bir günlük iş. Uzun yaşayan dal, en büyük çakışma kaynağıdır.
8. Kendi dalını her gün main ile güncelle: `git switch <dal> && git merge main`.
9. Commit'lere ve PR'lara yapay zekâ ortak yazar satırı (`Co-Authored-By: Claude ...`) eklenmez.
10. `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/` asla commit'lenmez (`.gitignore` bunları dışarıda tutar). `git add -A` yapmadan önce `git status` ile neyi eklediğine bak.

**Çakışma çıkarsa:** Kendi dosyanda çakışma varsa sen çözersin. Bir sahne veya prefab'da UnityYAMLMerge çözemezse kendi değişikliğini koru (`git checkout --ours <dosya>`), ardından küçük değişikliği elle yeniden yap. Başkasının dosyasında çakışma çıkıyorsa, o dosyaya hiç dokunmamış olman gerekirdi: değişikliğini geri al.

### Ortak sözleşme (Shared)

Herkes birbirini **beklemeden** kod yazabilsin diye bütün sınıfların ve olayların **adları ve imzaları M0'da baştan sabitlenir**. M0'da Yiğit aşağıdaki her şeyi derlenir hâlde main'e koyar. Gövdesi boş olan yerler "stub" olarak gelir; sahibi daha sonra içini doldurur.

**Sözleşme kuralları**

- M0'dan sonra sözleşmede **hiçbir şeyin adı değiştirilmez, hiçbir şey silinmez**; yalnızca ekleme yapılır.
- Ekleme gerekiyorsa issue aç. Yiğit "sözleşme" etiketli bir PR açar, 4 kişi onaylar, sonra birleştirilir.
- Başkasının sistemine **yalnızca sözleşmedeki üyeler üzerinden** erişilir. Örneğin `GetComponent<SomeoneElsesPrivateClass>()` kullanılmaz.

**İsim alanları:** `LeafBrawl.Shared`, `LeafBrawl.Core`, `LeafBrawl.Characters`, `LeafBrawl.Combat`, `LeafBrawl.Bots`, `LeafBrawl.World`, `LeafBrawl.Presentation`. Kodlar tek bir assembly'de durur (asmdef yok).

```csharp
// ---- Shared/Enums.cs ----
public enum LeafType { Red, Yellow, Green, Orange }          // K, S, Y, T
public enum StatType { Power, Speed, Health }                // Güç, Hız, Sağlık
public enum RecipeId { Power, Speed, Health, Potion }        // 1, 2, 3, 4 tuşları
public enum AbilityId { Dash, Shield, Ulti }                 // Hız 5, Sağlık 5, Güç 5
public enum AbilityState { Locked, Ready, Cooldown, Used }
public enum PileTier { None, Small, Medium, Large }
public enum MatchState { Countdown, Playing, Paused, Ended }
public enum CraftFail { NotEnoughLeaves, NotSafe, MaxLevel, AlreadyCrafting }
public enum BotProfile { Aggressive, Agile, Tough }          // Saldırgan, Çevik, Dayanıklı
public enum ScarfColor { Blue, Purple, White, Black }        // oyuncu her zaman Blue

// ---- Shared/Units.cs ----
public static class Units {
    public const float PxPerUnit = 40f;
    public static float ToUnits(float px) => px / PxPerUnit;
    public static float ToPx(float units) => units * PxPerUnit;
}

// ---- Shared/CharacterIntent.cs ---- oyuncu ve bot AYNI girdiyi üretir
public struct CharacterIntent {
    public Vector2 Move;        // birim vektör veya sıfır
    public Vector2 Aim;         // bakış yönü (birim vektör)
    public bool Swing;          // basılı tutuldukça true
    public bool Dash, Shield, Ulti;   // yalnızca basıldığı karede true
    public RecipeId? Craft;     // bu karede istenen tarif, yoksa null
}
public interface IIntentSource { CharacterIntent ReadIntent(Character self); }
// Uygulayanlar: PlayerIntentSource (Yiğit), BotBrain (Hamza)

// ---- Shared/Damage.cs ----
public struct DamageInfo { public int Amount; public Character Source; public bool IsUlti; public bool IsZone; }
public interface IDamageable { Vector2 Position { get; } bool IsAlive { get; } void ApplyDamage(DamageInfo info); }
public interface IDamageInterceptor { bool TryAbsorb(DamageInfo info); }   // true: hasar tamamen emildi (Kalkan)
public interface ITree : IDamageable { bool Choppable { get; } bool Felled { get; } }
public interface IPickup { Vector2 Position { get; } bool IsLoot { get; } LeafType Type { get; } }   // Type: ganimette (IsLoot) yok sayılır

// ---- Shared/Services.cs ---- servis bulucu; kayıtlı değilse Stubs/ altındaki sahte sürüm döner
public static class Services {
    public static Balance Config;      // Yiğit (ConfigLoader)
    public static Rng Rng;             // Yiğit (tek tohum, NFR-07)
    public static IMatch Match;        // Yiğit (MatchManager)
    public static ISafeZone Zone;      // Okan  (SafeZone)          stub: her yer alan içi
    public static IVisibility Visibility; // Okan (VisibilityService) stub: herkes herkesi görür
    public static IWorld World;        // Okan  (WorldService)       stub: boş dünya
}
public interface IMatch {
    MatchState State { get; }  float MatchTime { get; }          // geri sayım bitince 0'dan başlar
    IReadOnlyList<Character> Characters { get; }  Character Player { get; }
    MatchResult Result { get; }                                   // Ended olunca dolu
    void Pause(); void Resume(); void Restart();
}
public interface ISafeZone {
    Vector2 Center { get; }  float RadiusPx { get; }  float NextRadiusPx { get; }
    float SecondsToNextShrink { get; }  float DamagePerSecond { get; }
    bool IsInside(Vector2 pos);  bool WillBeOutsideWithin(Vector2 pos, float seconds);
}
public interface IVisibility {
    bool CanSee(Character viewer, Character target);              // çalılık + 1 sn görünme kuralları
    bool IsInBush(Character c);  Vector2? NearestBush(Vector2 from);
}
public interface IWorld {
    IReadOnlyList<IPickup> PickupsNear(Vector2 pos, float radiusPx);
    IReadOnlyList<ITree> TreesNear(Vector2 pos, float radiusPx);  // yalnızca kesilmemiş ağaçlar
    ITree GreatTree { get; }  IReadOnlyList<Vector2> SpawnPoints { get; }
    bool IsBlocked(Vector2 pos, float radiusPx);                  // gövde / kaya / harita dışı
}
```

**`Character` sınıfının herkese açık yüzü** (`Characters/Character.cs`, sahibi Yiğit). Diğer herkes karakterle **yalnızca bu üyeler üzerinden** konuşur:

| Üye | Ne işe yarar | Kim yazar / kim okur |
| --- | --- | --- |
| `Id`, `IsPlayer`, `Profile` (BotProfile?), `Scarf` | Kimlik | Yiğit yazar; herkes okur |
| `Position`, `Velocity`, `Facing`, `IsAlive` | Konum ve durum | Yiğit yazar; herkes okur |
| `CurrentIntent` | Bu karedeki girdi. Geri sayımda, duraklatmada ve ölüyken sıfırdır | Yiğit yazar (IIntentSource'tan okur); Hamza'nın savaş kodu Swing/Dash/Shield/Ulti için okur |
| `Motor.Dash(Vector2 yön, float mesafePx, float süre)`, `Motor.IsDashing` | Engellere çarparak kısa itme | Yiğit uygular; Hamza'nın atılması çağırır |
| `Stats.Level(StatType)`, `Stats.Damage`, `Stats.SpeedPx`, `Stats.MaxHp`, `Stats.SetLevel(StatType,int)` | Seviyeler (3.1) | Yiğit; `SetLevel` yalnızca debug ve test için |
| `Health.Current`, `Health.Max`, `Health.Heal(int)`, `Health.LastDamageTime`, `ApplyDamage(DamageInfo)` | Can | Yiğit. Hasar veren herkes **yalnızca** `ApplyDamage` çağırır |
| `Inventory.Count(LeafType)`, `Inventory.Cap(LeafType)`, `Inventory.TryAdd(LeafType,int) → int eklenen` | Envanter | Yiğit. Okan toplarken `TryAdd` çağırır |
| `Crafting.IsCrafting`, `Crafting.Progress01`, `Crafting.CanCraft(RecipeId, out CraftFail)`, `Crafting.Request(RecipeId)` | Tarif (3.7) | Yiğit. Furkan'ın envanter kartı tıklaması `Request` çağırır |
| `ThreatScore`, `PileTier`, `IsSafe`, `Kills` | Tehdit, "güvende", eleme sayısı | Yiğit yazar; herkes okur |
| `MarkSwing()`, `SpeedMultiplier` | Savurma anı (güvende kuralı için), savururken %50 hız | Hamza yazar |
| `Abilities.State(AbilityId)`, `Abilities.CooldownLeft(AbilityId)` | Yetenek durumu | Hamza yazar; Furkan HUD'da okur |
| `VisibleToPlayer`, `IsInBush`, `IsRevealed` | Görünürlük | Okan yazar; Furkan görselde, Hamza botta okur |

**Olaylar** (`Shared/GameEvents.cs`, static C# event'leri ve `Raise...` metotları). Ses, efekt ve HUD **yalnızca bu olayları dinleyerek** çalışır. Böylece Furkan'ın kodu oyun mantığına hiç dokunmaz.

| Olay | Parametreler | Kim yayınlar |
| --- | --- | --- |
| `MatchStateChanged` | `MatchState` | Yiğit |
| `CountdownTick` | `int kalanSn` | Yiğit |
| `InventoryToggled` | `bool açık` | Yiğit (Tab) |
| `LevelChanged` | `Character, StatType, int yeniSeviye` | Yiğit |
| `Damaged` | `Character kurban, DamageInfo` | Yiğit (Health) |
| `Healed` | `Character, int miktar` | Yiğit |
| `Died` | `Character kurban, Character katil` (alan hasarında null) | Yiğit |
| `LeafCollected` | `Character, LeafType, int adet` | Yiğit (Inventory.TryAdd içinde) |
| `CraftStarted` / `CraftCompleted` / `CraftCanceled` | `Character, RecipeId` (+ `int yeniSeviye`) | Yiğit |
| `CraftRejected` | `Character, RecipeId, CraftFail` | Yiğit |
| `ThreatChanged` | `Character, int puan, PileTier` | Yiğit |
| `AbilityUnlocked` / `AbilityUsed` | `Character, AbilityId` | Hamza |
| `ShieldBroken` | `Character` | Hamza |
| `Swung` | `Character` | Hamza |
| `HitLanded` | `Character saldıran, IDamageable hedef, int hasar` | Hamza |
| `TreeHit` / `TreeFelled` | `ITree, Character` | Okan |
| `GreatTreeHit` | `Character` | Okan |
| `LeafSpawned` | `Vector2 konum, LeafType` | Okan |
| `LootDropped` | `Vector2 konum, int toplam` | Okan |
| `ZonePhaseChanged` | `int aşama` | Okan |
| `ZoneWarning` | `float sonrakiYarıçapPx` (10 sn kala) | Okan |
| `RevealChanged` | `Character, bool` | Okan |

**Unity katmanları** (M0'da Yiğit ayarlar): `Character`, `Obstacle` (gövde, kaya, harita sınırı), `Bush`, `Pickup`, `Tree`.
**Sıralama katmanları** (alttan üste): `Ground`, `Pickups`, `Pile`, `Characters`, `Bushes`, `Canopy`, `Effects`, `ZoneOverlay`.
**Girdi:** `Assets/_Game/Input/LeafBrawl.inputactions` (yeni Input System, sahibi Yiğit). Tuşlar SRS 6.1'deki gibidir.

### `balance.json` bölümleri

Dosyanın tamamını M0'da Yiğit SRS Bölüm 3'ten doldurur. Sonradan her kişi **yalnızca kendi bölümündeki değerleri** PR ile değiştirir; anahtar ekleme işi sözleşme kuralına tabidir. Test ayarları ayrı `debug.json` dosyasındadır. Kişisel denemeler için gitignore'lu `debug.local.json` kullanılır ve commit'lenmez.

| Bölüm | Sahibi | İçerik |
| --- | --- | --- |
| `levels`, `regen`, `inventory`, `recipes`, `crafting`, `threat`, `character`, `camera` | Yiğit | 3.1, 3.2, 3.6, 3.7, 3.9, 3.13 |
| `axe`, `abilities`, `bots` | Hamza | 3.3, 3.4, Bölüm 5 |
| `trees`, `leaves`, `zone`, `loot`, `bushes`, `map` | Okan | 3.5, 3.8, 3.10, 3.11, 3.12 |

### Bağımlılığı beklemeden çalışmak

- Bir sistem henüz yoksa `Services` otomatik olarak **stub** (sahte) sürümü verir: alan her yerde güvenli, herkes herkesi görür, dünya boş. Kendi işini kendi sandbox sahnende bunlarla test et.
- Sandbox sahnende her zaman `Systems.prefab` bulunur (Yiğit). İçinde ayar yükleyici, Rng ve maç yöneticisi vardır.
- Görsel gelmeden önce Unity'nin hazır 2D şekillerini (Square, Circle) kullan. `Art/` klasörüne yer tutucu dosya **ekleme**; o klasör Furkan'ındır.

### Aşamalar ve entegrasyon

Plan SRS Bölüm 10'daki aşamaları izler. Her aşamanın sonunda **entegrasyon turu** yapılır:

1. Herkes o aşamanın PR'larını açar.
2. Yiğit hepsini birleştirir ve `Match.unity` sahnesinde dener.
3. Ekip 15 dakika birlikte oynar.
4. Yiğit bir etiket atar: `asama-0`, `asama-1`, `asama-2`, `asama-3`.

| Aşama | Ne zaman biter | Bitiş ölçütü |
| --- | --- | --- |
| **M0 – İskelet** | Herkesten önce, ilk iş | Unity projesi, klasörler, sözleşme, stub'lar, `balance.json`, sandbox sahneleri main'de; herkes projeyi açabiliyor |
| **Aşama 1 – Çekirdek** | M0'dan sonra | Oyuncu 1 botla, gerçek haritada, 5 dakikalık maç oynayabiliyor |
| **Aşama 2 – Asıl oyun** | Aşama 1'den sonra | 1 oyuncu + 3 botla tam maç; çalılık, yetenekler, ganimet, sonuç ekranı var |
| **Aşama 3 – Cila** | Teslimden önce | Menüler, kılavuz, sesler, efektler tamam; KT-01–KT-18 geçiyor; Windows ve macOS yapısı alınmış |

### "Bitti" ne demek

Bir iş, aşağıdakilerin hepsi doğruysa bitmiştir:

- Unity Console'da **kırmızı hata yok**.
- Kendi sandbox sahnende çalışıyor, ve PR açıklaması başkasının bunu nasıl deneyeceğini söylüyor.
- Sayılar `balance.json`'dan geliyor.
- Yalnızca kendi klasörlerindeki dosyalar değişmiş (PR'daki "Files changed" sekmesinden kontrol et).
- İlgili SRS maddesi yazıldığı gibi davranıyor.

### Claude kullananlar için (Yiğit, Hamza)

- Repo kökündeki `CLAUDE.md` (M0'da Yiğit ekler), Claude'a klasör sahipliğini, sözleşme kuralını ve çalışma döngüsünü söyler. Her oturumun başında Claude'a bu dosyayı ve kendi görev dosyanı okut.
- Unity MCP ile sahne düzenlerken Claude'a **yalnızca kendi sandbox sahneni ve kendi prefab'larını** düzenlet. `Match.unity` sahnesini yalnızca Yiğit'in oturumu düzenler.
- REA, hazır bir oyunda bir davranışın nasıl yapıldığını incelemek için kullanılır (örneğin bir pixel-perfect kamera ayarı veya bot yol bulma örneği). Bulunan şeyler kendi kodumuzla yeniden yazılır; başka oyunlardan kod, görsel veya ses kopyalanmaz (NFR-09).

### İletişim

- Soru, istek veya hata → GitHub **Issues**. Başlığın başına sahibinin adını yaz (ör. `[Okan] Ağaç kesilince TreeFelled iki kez geliyor`). Etiketler: `sözleşme`, `hata`, `istek`, `sanat`, `ses`.
- Engel olduysan (biri seni bekletiyor) hemen yaz; beklemek yerine stub ile devam et.

## Sanat kuralları (önce bunlar)

- **Ölçek:** 1 Unity birimi = **16 sanat pikseli** = 40 dünya px. Karakter 16×16 pikseldir. Kamera aynı anda 640×360 sanat pikseli gösterir; 1080p ekranda her piksel 3×3 görünür.
- **İçe aktarma ayarı** (her sprite için; M0'daki hazır preset'i kullan): Pixels Per Unit **16**, Filter Mode **Point**, Compression **None**, Sprite Mode birden çok kare varsa Multiple. Pivot: karakterlerde ayak ortası, diğerlerinde merkez.
- **Palet:** sonbahar tonları. Tek bir palet dosyası (`Art/palette.png`, en fazla 32 renk) hazırla ve her şeyi ondan çiz.
- **Yaprak renkleri ayrılmış renklerdir:** kırmızı (K), sarı (S), yeşil (Y), turuncu (T). Bu dört rengi karakter atkısında, arayüz düğmesinde veya zeminde kullanma; yaprağı başka bir şeyle karıştırmamak gerekir.
- **Atkı renkleri:** oyuncu mavi; botlar mor, beyaz, siyah (FR-63).
- **Font:** Türkçe karakterleri (ç ğ ı İ ö ş ü) eksiksiz içeren, ticari kullanıma açık bir pixel font (OFL veya CC0). TextMeshPro font asset'ine çevir. HUD'da yazı 1080p'de en az 18 px görünmeli ve arka planla kontrastı en az 4.5:1 olmalı (NFR-08).
- **Özgünlük:** her şeyi sen çizersin veya CC0 ya da açık lisanslı kaynaktan alırsın (ör. Kenney, OpenGameArt CC0). Her dış kaynağı ve lisansını `CREDITS.md`'ye yaz. Brawl Stars, Clash Royale ve Minecraft'tan hiçbir görsel, ses veya isim kullanılmaz (NFR-09).
- **Dosya adları:** küçük harf, Türkçe karakter ve boşluk yok. Biçim: `tur_ad_varyant`. Örnekler: `spr_karakter_yuru_asagi`, `spr_yaprak_k`, `sfx_balta_savur`.

## Sanat listesi

| Öğe | Boyut (sanat px) | Kareler / varyantlar | Kim kullanır | SRS |
| --- | --- | --- | --- | --- |
| Karakter gövdesi | 16×16 | 3 yön (aşağı, yukarı, yan; sol için yan aynalanır). Bekleme 2 kare, yürüme 4 kare | Sen (`CharacterVisual`) | 3.13 |
| Atkı (karakterin üstüne çizilen ayrı katman) | 16×16 | Aynı kareler; 4 renk (mavi, mor, beyaz, siyah) | Sen | FR-63 |
| Oyuncu ayak halkası | 16×6 | 1 | Sen | FR-63 |
| Balta | 16×16 | Normal + parlayan (ulti hazır) | Sen (savurma yayında döndürülür) | 3.3 |
| Yaprak – yerde | 8×8 | 4 tür; şekiller SRS 6.3'teki gibi (K üç uçlu, S ince uzun, Y yuvarlak, T yıldız damarlı) | Okan (`Leaf.prefab`) | 6.3 |
| Yaprak – arayüz ikonu | 16×16 | 4 tür, aynı şekiller | Sen (HUD, panel) | UI-04 |
| Yaprak yığını parçası | 6×6 | 4–6 karışık sonbahar yaprağı | Sen (`PileView`) | 3.9 |
| Ganimet yığını | 12×12 | 3 kare: 1, 2 ve 3 üst üste yaprak | Okan (`LootPile.prefab`) | 3.10 |
| Küme/koru ağacı gövdesi | 8×12 | 1 + sallanma için 2 kare | Okan | 3.5 |
| Küme/koru ağacı tacı | 40×40 | 2–3 varyant | Okan (Canopy katmanı) | FR-54 |
| Kütük | 12×8 | 1 | Okan | FR-37 |
| Ulu ağaç gövdesi / tacı | 32×32 / 128×128 | 1 | Okan | 3.5 |
| Çalılık (dış / iç) | 48×48 / 32×32 | Her biri 2 varyant | Okan | 3.11 |
| Kaya | 24×24 | 2 varyant | Okan | 3.12 |
| Zemin karoları | 16×16 | Çim (3 varyant), açıklık zemini (2), yere düşmüş yaprak süsleri | Okan (Tilemap) | 3.12 |
| Harita sınırı | 16×16 | Sık çalı veya çit karosu | Okan | 3.12 |
| Alan örtüsü ve sınır çizgisi | Karo veya malzeme | Dışarısı yarı saydam koyu/kırmızımsı; sınır düz çizgi, sonraki sınır kesikli | Okan | FR-71 |

**Efektler** (Unity Particle System veya sprite animasyonu; hepsi senin `VfxManager`'ında, olaylarla tetiklenir)

| Efekt | Olay | SRS |
| --- | --- | --- |
| Hasar alınca 0,1 sn beyaz parlama ve hasar sayısı | `Damaged` | FR-23 |
| Ölümde 1 sn yaprak dağılması | `Died` | FR-80 |
| Tarif üretim çubuğu (karakterin üstünde, 1,5 sn) | `CraftStarted` / `CraftCanceled` / `CraftCompleted`, `Crafting.Progress01` | UI-07 |
| Yaprak birleşme efekti + "Güç 2 → 3" / "Can +40" yazısı (3 sn) | `CraftCompleted` | FR-45 |
| Eksik yaprak veya "Güvende değilsin" için kırmızı işaret | `CraftRejected` | FR-46 |
| Atılma izi | `AbilityUsed` (Dash) | 3.4 |
| Kalkan halkası (açıkken) ve kırılma | `AbilityUsed` (Shield), `ShieldBroken` | FR-27 |
| Ulti hazırlanma parıltısı (0,4 sn) ve 120 px'lik patlama halkası (48 sanat px yarıçap) | `AbilityUsed` (Ulti) | FR-28 |
| "Ulti açıldı [E]" gibi bildirim | `AbilityUnlocked` | 3.4 |
| Ağaç sallanması ve devrilmesi | `TreeHit`, `TreeFelled` | FR-35, FR-36 |

## Sesler

Her ses kısa (0,1–1 sn), `.ogg` veya `.wav`, dosya başına 1 MB'tan küçük. `AudioManager` olayları dinler ve `SfxLibrary` (ScriptableObject: olay → ses dosyası) üzerinden çalar. Hiçbir başka kod `AudioSource` çalmaz.

| Ses | Olay | SRS |
| --- | --- | --- |
| Balta savurma | `Swung` | NFR-10 |
| İsabet (karaktere) | `HitLanded` | NFR-10 |
| Ağaca vurma / ulu ağaca "tok" ses / ağaç devrilmesi | `TreeHit` / `GreatTreeHit` / `TreeFelled` | FR-35 |
| Yaprak toplama (çok sık gelirse saniyede en fazla 6 kez çal) | `LeafCollected` | NFR-10 |
| Tarif başladı / tamamlandı / reddedildi | `CraftStarted` / `CraftCompleted` / `CraftRejected` | FR-45 |
| Yetenek açıldı, atılma, kalkan açılma ve kırılma, ulti hazırlanma ve patlama | `AbilityUnlocked`, `AbilityUsed`, `ShieldBroken` | NFR-10 |
| Ölüm | `Died` | NFR-10 |
| Alan uyarısı | `ZoneWarning` | NFR-10 |
| Geri sayım bip sesi (3, 2, 1) | `CountdownTick` | FR-05 |
| Düğme tıklama | Arayüz | — |
| Müzik (isteğe bağlı) | — | NFR-10 |

Duraklatma menüsünde ses aç/kapa düğmesi `AudioManager.Muted` değerini değiştirir (NFR-10).

## Ekranlar ve HUD

Hepsi uGUI + TextMeshPro. Canvas Scaler referansı 640×360, "Scale With Screen Size". Ekranlar tek sütunlu ve en fazla 4 düğmeli (6.4).

| Ekran / öğe | İçerik | Veri nereden | SRS |
| --- | --- | --- | --- |
| UI-01 sol üst | Can çubuğu, "85 / 115", "Güç 2 · Hız 1 · Sağlık 2" | `Player.Health`, `Player.Stats` | UI-01 |
| UI-02 üst orta | Maç süresi, "Alan daralıyor: 0:12" | `Services.Match.MatchTime`, `Services.Zone.SecondsToNextShrink` | UI-02 |
| UI-03 sağ üst | "Kalan: 3", eleme sayısı | `Services.Match.Characters`, `Died` olayı | UI-03 |
| UI-04 alt orta | 4 yaprak ikonu ve adetleri; sınırdaki tür farklı renkte | `Player.Inventory` | UI-04 |
| UI-05 sağ alt | Balta, atılma, kalkan, ulti ikonları: kilitli (gri + kilit + "Seviye 5"), bekleme (dairesel dolum + saniye), kullanılmış (üstü çizili) | `Player.Abilities` | UI-05 |
| UI-06 yaprak çubuğunun üstü | "Güç hazır [1]" bildirimleri; karakter güvende değilse soluk | `Player.Crafting.CanCraft`, `Player.IsSafe` | UI-06 |
| UI-07 karakterin üstü | Üretim çubuğu | `Crafting.Progress01` | UI-07 |
| Alan dışı kırmızı ekran kenarı | Oyuncu alan dışındayken | `Services.Zone.IsInside(Player.Position)` | FR-71 |
| Rakip can çubuğu | Her rakibin üstünde küçük çubuk; rakip görünmezken gizli | `Health`, `VisibleToPlayer` | FR-64 |
| Geri sayım | 3, 2, 1 | `CountdownTick` | FR-05 |
| Envanter paneli (Tab) | 4 yaprak yuvası, 4 tarif kartı (mevcut seviye, sonraki maliyet, eksikler), "En üst seviye", "Güvende değilsin" | `InventoryToggled`, `Player.Inventory`, `Player.Crafting`; karta tıklama → `Crafting.Request` | FR-40–FR-42 |
| Duraklatma | Devam, Yeniden başlat, Ana menü, ses aç/kapa | `MatchStateChanged`; düğmeler → `Resume`, `Restart`, `SceneFlow.LoadMenu` | FR-06 |
| Sonuç | Sıra (1.–4.), hayatta kalma süresi, eleme sayısı, tehdit puanı; Yeniden başlat, Ana menü | `Services.Match.Result` | FR-08 |
| Ana menü (`MainMenu.unity`) | "Leaf Brawl" logosu; Başla, Kılavuz, Kontroller, Çıkış | `SceneFlow` | FR-01 |
| Kontroller | SRS 6.1'deki bütün tuşlar, tek sayfa | — | FR-02 |
| Kılavuz | İleri/geri ile en fazla 6 sayfa. Her sayfada başlık, en fazla 3 cümle ve bir görsel. Sayfalar: (1) yaprak toplama ve ağaç kesme, (2) tarifler, seviyeler ve "güvende" kuralı, (3) can yenilenmesi ve can iksiri, (4) 5. seviye yetenekleri, (5) çalılıklar, (6) daralan alan ve elenme. Metni SRS 3. bölümden sade bir dille yaz | — | FR-03 |

Esc, Kontroller ve Kılavuz ekranlarında ana menüye döner (6.4).

## Görevlerin

### M0 beklerken (Unity gerekmez, hemen başla)

- [ ] Palet ve stil denemesi: karakterin 16×16 ilk hâli, 4 yaprak, bir küme ağacı, bir çalılık ve bir zemin karosunu yan yana çizip ekibe göster. Görünüş üzerinde erken anlaşmak, sonradan yeniden çizmekten ucuz.
- [ ] Türkçe karakterli pixel font seç; lisansını not et.
- [ ] CC0 ses kaynaklarını topla, listeyi ve lisansları `CREDITS.md` taslağına yaz.
- [ ] Ana menü ve HUD için kâğıt üzerinde veya çizim programında kaba bir yerleşim çiz (640×360 çerçevede).

### Aşama 1 – Çekirdek (başkalarının hemen ihtiyacı olanlar önce)

Bu aşamada sprite'ları **küçük ve sık PR'larla** gönder. Okan ve Yiğit, sprite geldikçe yer tutucuları değiştirecek.

| Görev | SRS |
| --- | --- |
| [ ] PR 1: palet, font, 4 yaprak (yerde 8×8 + ikon 16×16) | 6.3 |
| [ ] PR 2: küme ve koru ağacı (gövde, taç, kütük), ulu ağaç, kaya, zemin karoları, harita sınırı | 3.12 |
| [ ] PR 3: karakter gövdesi (bekleme ve yürüme, 3 yön), 4 atkı rengi, oyuncu halkası | 3.13, FR-63 |
| [ ] `CharacterVisual.prefab` + `CharacterVisual.cs`: karakterin `Velocity` ve `Facing` değerlerine göre yön ve yürüme animasyonu, atkı rengi (`Scarf`), oyuncu halkası (`IsPlayer`). Karakter kökünden `GetComponentInParent<Character>()` ile bağlan | FR-11, FR-63 |
| [ ] `AudioManager.prefab` + `SfxLibrary`: ilk 3 ses (savurma, isabet, yaprak toplama) | NFR-10 |

### Aşama 2 – Asıl oyun

| Görev | SRS |
| --- | --- |
| [ ] Balta görseli ve savurma yayı (`Swung` → 0,12 sn içinde yay boyunca döner); ulti hazırken parlayan balta | 3.3 |
| [ ] `HUD.prefab`: UI-01–UI-07, alan dışı kırmızı kenar, geri sayım | 6.2, FR-71 |
| [ ] `PileView` (`CharacterVisual` içinde): kademeye göre 6/12/18 yaprak, karakter yarıçapının 1,2/1,4/1,6 katı, hafif salınım, kademe artınca 0,3 sn'de büyüme; karakter yığının üstünde çizilir | 3.9, FR-61, FR-62 |
| [ ] Rakip can çubuğu | FR-64 |
| [ ] Görünürlük: `VisibleToPlayer` false iken karakterin bütün görseli (yığın ve can çubuğu dahil) gizli. Oyuncu çalılıktayken (`IsPlayer && IsInBush`) yarı saydam | FR-51, FR-53 |
| [ ] Efekt listesinin tamamı (`VfxManager`) | Efekt tablosu |
| [ ] Çalılık, ganimet ve alan örtüsü sprite'ları (Okan'a PR) | 3.10, 3.11 |
| [ ] Envanter paneli, duraklatma ve sonuç ekranı | FR-06, FR-08, FR-40–FR-42 |
| [ ] Ses listesinin çoğu | Ses tablosu |

### Aşama 3 – Cila

| Görev | SRS |
| --- | --- |
| [ ] `MainMenu.unity`: ana menü, Kontroller ve Kılavuz (6 sayfa, görsellerle) | FR-01–FR-03 |
| [ ] Kalan sesler, ses aç/kapa, isteğe bağlı müzik | NFR-10 |
| [ ] Okunabilirlik: HUD yazıları 1080p'de en az 18 px, kontrast 4.5:1; yapraklar renk körlüğü simülasyonunda (ör. Coblis) şekilleriyle ayırt ediliyor | NFR-08 |
| [ ] `CREDITS.md` eksiksiz | NFR-09 |
| [ ] Genel görsel cila: animasyon zamanlamaları, efekt yoğunluğu (performansı düşürmeyecek şekilde) | NFR-01 |

### Kabul testlerinde senin payın

KT-07 (görsel kısmı), KT-18; NFR-08, NFR-09, NFR-10.

## Kod yazmakta zorlanırsan

`Presentation/` altındaki scriptler basit tutulur: olay dinle, değer oku, göster. Bir script'i yazmakta takılırsan GitHub'da `yardım` etiketli bir issue aç ve ne görünmesi gerektiğini yaz. Yiğit veya Hamza, Claude ile **senin dalına** bir öneri commit'i atar; sen Unity'de deneyip onaylarsın. Dosyanın sahibi yine sensin. Senin asıl odağın görsel, ses ve ekranlar.

## Dikkat et

- Oyun değerlerine yazan bir satır (`Health.Current = ...`, `Inventory...`) senin kodunda **hiç** olmamalı. Gereken bir değer sözleşmede yoksa Yiğit'e issue aç.
- Sprite'ı başkasının prefab'ına sen bağlamazsın; Okan'ın ağaç ve yaprak prefab'larına sprite'ları Okan bağlar. Sen sprite'ı main'e koyarsın ve ona haber verirsin.
- Sprite'ın adını veya klasörünü main'e girdikten sonra **değiştirme**; değiştirirsen başkalarının prefab'larındaki bağlantı kopar. Yeni bir sürüm çizersen aynı dosyanın üzerine kaydet.
- `MainMenu.unity` senin, `Match.unity` Yiğit'in. HUD'u denemek için kendi sandbox sahnende `HUD.prefab`'ı kullan.
