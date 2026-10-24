# Görev Dosyası – Okan Can Nazlı

**Rol:** Dünya: harita, ağaçlar, yapraklar, ağaç kesme, çalılık ve görüş, güvenli alan, ölüm ganimeti. Ayrıca kabul testlerinin koordinatörü.
**Araçlar:** Unity (Claude ve REA yok; zorlandığın yerde aşağıdaki "Yardım" bölümüne bak)
**Sahip olduğun yerler:** `Scripts/World/` (alt klasör `World/Editor/` dahil), `Prefabs/World/` (`Map.prefab`, `SafeZone.prefab`, ağaç, yaprak, çalılık, kaya, `LootPile.prefab`), `Scenes/Sandbox/Sandbox_Okan.unity`, `balance.json` içindeki `trees`, `leaves`, `zone`, `loot`, `bushes`, `map` bölümlerinin değerleri, `docs/kabul_testleri.md`, `docs/demo_degerlendirme.md`
**PR'larını kim inceler:** Yiğit

Oyuncunun içinde dolaştığı her şey senin: hangi ağaçtan ne zaman yaprak düşer, ağaç nasıl kesilir, kim çalılıkta görünmez olur, alan ne zaman daralır. Yiğit'in karakterine yalnızca birkaç sözleşme üyesiyle dokunursun: `Inventory.TryAdd`, `ApplyDamage`, `Position`, `IsAlive`, `VisibleToPlayer`, `IsInBush`, `IsRevealed`. Görseller Furkan'dan gelir; o gelene kadar Unity'nin hazır daire ve kare şekilleriyle çalışırsın.

## Herkes için ortak kurallar

> Bu bölüm dört dosyada da **aynıdır**. Kendi görevlerine geçmeden önce bir kez oku. Bir kural değişirse dört dosyada birlikte değişir.

### Tek doğru kaynak

- Oyunun ne yapacağını **SRS v1.1** söyler: [`docs/Leaf_Brawl_SRS_Demo_v1.1.md`](../Leaf_Brawl_SRS_Demo_v1.1.md). Bu dosyalar yalnızca **işi kimin, nerede, hangi sırayla** yapacağını söyler.
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
public interface IPickup { Vector2 Position { get; } bool IsLoot { get; } }

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

## Görevlerin

### M0 beklerken

- [ ] SRS 3.5, 3.8, 3.10, 3.11 ve 3.12'yi oku. Harita tablosundaki ölçüleri ve `docs/img/harita_duzeni_v1.1.png` görselini karşılaştır.
- [ ] `docs/kabul_testleri.md` dosyasını hazırla: KT-01–KT-18 için tablo. Sütunlar: No, Sahibi (aşağıdaki listeden), Son deneme tarihi, Sonuç (Geçti / Kaldı / Denenmedi), Issue bağlantısı. Bu dosyayı ayrı bir PR olarak hemen açabilirsin, çünkü Unity projesi gerektirmez.
- [ ] Unity'de şu konulara kısaca bak (Unity Learn veya YouTube, 2D): Prefab ve Prefab Variant, Tilemap, `Physics2D.OverlapCircle`, Collider2D'de "Is Trigger". Editör menüsüne komut ekleme (`[MenuItem]`) de işine yarar.

### Aşama 1 – Çekirdek

| Görev | SRS |
| --- | --- |
| [ ] **Harita üretici** (`World/Editor/MapBuilder.cs`): Unity menüsüne "Leaf Brawl → Haritayı Kur" komutu ekle. Komut `Map.prefab`'ın içini `balance.json`'daki `map` bölümünden **otomatik** kursun; yerleşimi `MapLayoutGenerator` hesaplar (yazıldı, `okan/harita-uretici`), builder yalnızca öğeleri yerleştirir: ulu ağaç, 4 kaya, 8 küme × 3 ağaç, 4 koru × 5 ağaç, 4 doğma noktası, harita sınırı. Elle yerleştirme yapma; böylece bir ölçü değişince komutu yeniden çalıştırmak yeter | 3.12 |
| [ ] Koordinatlar: merkez (0,0) kabul edilir. Halkadaki bir öğenin konumu `(r·cos a, r·sin a)` ile hesaplanır. Kümeler 600 px halkada, 0°'den başlayarak 45° aralıkla. Korular 1270 px'te, 45°, 135°, 225° ve 315°'de. Kayalar 280 px'te, aynı çaprazlarda. Doğma noktaları 1000 px'te, 0°, 90°, 180° ve 270°'de. Küme içi ve koru içi dizilim `map` bölümünde yazar. **Her maçta küçük sapma:** bu konumlar merkez değerlerdir; `MapLayoutGenerator` maç başında `Services.Rng` ile sapma ekler ve doğrular (SRS 3.12). Sapma değerleri sözleşme PR'ı gelene kadar `MapGenSettings.cs`'te durur | 3.12 |
| [ ] `MapGenerator` (MonoBehaviour, `Map.prefab` üzerinde): maç başında `MapGenSettings.FromConfig(Services.Config)` ve `Services.Rng` ile `MapLayoutGenerator.Generate` çağırıp haritayı kurar; `MapBuilder` aynı sınıfı editörde önizleme için kullanır. Harita `WorldService` ve `LeafSpawner`'dan önce hazır olmalı | 3.12, NFR-07 |
| [ ] Zemin: 2400×2400 px = 60×60 birim. Tilemap ile çim; merkezde 350 px yarıçaplı açıklık farklı zeminle. Harita sınırı `Obstacle` katmanında 4 kutu collider | 3.12 |
| [ ] Engeller: ağaç gövdeleri `CircleCollider2D` (küme ve koru 18 px, ulu ağaç 40 px) ve `Obstacle` katmanında. Hasar alabilmesi için gövdede ayrıca `Tree` katmanında bir collider bulunsun. Kayalar 60×60 px `BoxCollider2D`, `Obstacle` katmanında | FR-12 |
| [ ] `WorldService : IWorld`: `Awake`'te `Services.World = this`. `PickupsNear`, `TreesNear` (yalnızca kesilmemiş ağaçlar), `GreatTree`, `SpawnPoints`, `IsBlocked` (`Physics2D.OverlapCircle` + harita sınırı) | — |
| [ ] `Leaf` (prefab, `IPickup`): türü, yere düşerken en fazla 0,6 sn süzülme (bu sırada toplanamaz), 45 sn ömür, son 5 sn yanıp sönme. Her karede canlı karakterlerden 32 px yakında olan varsa `character.Inventory.TryAdd(tür, 1)` çağır; 1 dönerse yaprak yok olur, 0 dönerse (envanter dolu) yerde kalır | FR-31–FR-33 |
| [ ] `LeafSpawner`: her ağacın kendi sayacı (ulu ağaç 5 sn, küme ağacı 45 sn, koru ağacı 60 sn), turuncu olasılığı ağaç türüne göre; değilse K, S, Y eşit. Yaprak gövdenin 120 px çevresine düşer; nokta `IsBlocked` ise yeniden seç (en fazla 20 deneme). Haritada en fazla 80 yaprak; sınır doluyken üretim olmaz. Maç başında her kümenin yanında 3, ulu ağacın yanında 2 yaprak (biri turuncu). Her yaprakta `LeafSpawned` yayınla. Bütün rastgelelik `Services.Rng` ile | 3.5, FR-30, FR-34 |
| [ ] `ChoppableTree : ITree`: can 400. `ApplyDamage` her vuruşta `TreeHit` yayınlar ve 2 sn can çubuğu gösterir. Can 0 olunca veya `IsUlti` ise `TreeFelled` yayınlanır; gövdenin 120 px çevresine 6–10 yaprak saçılır (her biri %10 turuncu, 80 sınırına sayılmaz). Ağaç **kütüğe** dönüşür: `Obstacle` ve `Tree` collider'ları kapanır, üretim durur | 3.5, FR-35–FR-37 |
| [ ] `GreatTree : ITree`: `Choppable = false`. Vuruşta yalnızca `GreatTreeHit` yayınlar; can düşmez | FR-35 |
| [ ] `SafeZone : ISafeZone`: `Awake`'te `Services.Zone = this`. Yarıçap `Services.Match.MatchTime`'a göre aşama tablosundan doğrusal hesaplanır (3.8). Her 0,5 sn'de alan dışındaki canlı karakterlere saniyelik hasarın yarısı kadar `ApplyDamage(IsZone = true)` uygulanır. 5:00'ten sonra ani ölüm, 50/sn. Aşama değişince `ZonePhaseChanged` yayınlanır | 3.8, FR-70, FR-72 |
| [ ] Alan görseli (ilk sürüm): sınır çizgisi ve dışarıda yarı saydam örtü (yer tutucu renkle). Son görseli Furkan'ın vereceği sprite veya malzemeyle değiştirirsin | FR-71 |
| [ ] Gövdesi alan dışında kalan ağaçlar üretmeyi bıraksın | FR-73 |
| [ ] Sandbox sahnende: Systems + Map + SafeZone + Player. Yaprak topla ve HUD olmadan envanteri Inspector'dan izle. Alanın 1:30'da daraldığını gör; beklemek uzun sürerse `debug.local.json`'da `timeScale` değerini 4 yap | KT-04, KT-10 |

### Aşama 2 – Asıl oyun

| Görev | SRS |
| --- | --- |
| [ ] Çalılıklar: `MapBuilder` 8 dış (850 px, 22,5°'den başlayıp 45° aralıkla, yarıçap 60 px) ve 4 iç (450 px; 22,5°, 112,5°, 202,5°, 292,5°; yarıçap 40 px) çalılığı da kursun. `Bush` katmanında trigger collider; engel değil | 3.11, 3.12, FR-50 |
| [ ] `VisibilityService : IVisibility`: `Awake`'te `Services.Visibility = this`. Her karakter için hangi çalılıkta olduğunu tut ve `IsInBush` alanını yaz. `CanSee(izleyen, hedef)`: hedef çalılıkta değilse true; çalılıktaysa yalnızca izleyen **aynı** çalılıktaysa veya hedef "görünür" süresindeyse true | FR-51 |
| [ ] Görünme: `Swung` (savuran) ve `Damaged` (kurban) olaylarını dinle. O karakter 1 sn boyunca `IsRevealed = true` olsun; başlarken ve biterken `RevealChanged` yayınla | FR-52 |
| [ ] Her karede her karakter için `VisibleToPlayer = CanSee(oyuncu, karakter)` yaz (oyuncunun kendisi için her zaman true). Karakteri gizleme ve oyuncuyu yarı saydam çizme işini Furkan'ın görseli bu alanlara bakarak yapar | FR-51, FR-53 |
| [ ] `NearestBush(konum)`: botların kaçışı için en yakın çalılığın merkezi; alan dışındaki çalılıkları sayma | BOT-05 |
| [ ] Taç saydamlığı: ağaç tacının altında bir karakter varsa taç yarı saydam olsun | FR-54 |
| [ ] Ölüm ganimeti (`LootSystem`): `Died` olayını dinle. Kurbanın envanterinden K, S, Y'nin %35'i (aşağı yuvarla) ve T'nin %60'ı (yukarı yuvarla). Toplam 0 değilse öldüğü noktada (geçersizse en yakın geçerli nokta) **tek** bir `LootPile` bırak ve `LootDropped` yayınla | 3.10, FR-81 |
| [ ] `LootPile : IPickup` (`IsLoot = true`): toplam 1–10 ise 1, 11–20 ise 2, 21 ve üzeri ise 3 üst üste yaprak görünür. Karakter 32 px'e gelince her tür için `TryAdd` çağır; sığmayan adet yığında kalır, gösterim güncellenir. 45 sn sonra kaybolur; 80 sınırına sayılmaz | 3.10 |
| [ ] Alan uyarısı: bir sonraki daralmaya 10 sn kala yeni sınır yerde kesikli çizgiyle görünsün ve `ZoneWarning` yayınlansın. `WillBeOutsideWithin(pos, sn)` botlar için doğru çalışsın | 3.8, BOT-04 |
| [ ] Furkan'ın sprite'larını prefab'larına bağla: yaprak (4 tür), ağaç, kütük, ulu ağaç, çalılık, kaya, zemin karoları, ganimet (1/2/3), alan örtüsü. Sprite dosyalarına dokunmazsın; yalnızca kendi prefab'larında onları seçersin | — |
| [ ] Debug: editörde (Gizmos) ağaç canları, yaprak düşme yarıçapı, çalılık sınırları, alan yarıçapı | — |

### Aşama 3 – Test koordinatörlüğü ve denge

| Görev | SRS |
| --- | --- |
| [ ] Kabul testlerini yürüt: `asama-2` etiketinden sonra ve teslimden önce olmak üzere iki tam tur. KT-01–KT-18'in hepsini dene, `docs/kabul_testleri.md`'yi güncelle, kalan her test için **testin sahibine** issue aç | Bölüm 9 |
| [ ] Ekonomi dengesi: 5 maç oyna. Maç sonunda oyuncu ve botların ulaştığı seviyeleri ve kesilen ağaç sayısını not et. Gerekirse kendi bölümündeki değerleri değiştir (yaprak üretim aralıkları, ağaç canı, ganimet oranları). Tarif maliyeti Yiğit'in bölümünde olduğu için ona öneriyi issue olarak aç | 2.5, 9.2 |
| [ ] Sabit tohumla iki maçta harita yerleşiminin ve yaprak üretiminin aynı olduğunu Yiğit'le doğrula | NFR-07 |
| [ ] Demo değerlendirmesi: en az 5 kişi × 3 maç. Her maçtan sonra SRS 9.2'deki 5 soru (1–5 puan) sorulur. Sonuçlar ve ortalama `docs/demo_degerlendirme.md` dosyasına yazılır | 9.2 |

### Kabul testlerinde senin payın

Koordinatörlüğün yanında bizzat sorumlu olduğun testler: KT-02 (engeller), KT-04, KT-09, KT-10, KT-12 (Yiğit'le), KT-15.

Diğer testlerin sahipleri: KT-01, KT-05, KT-06, KT-08, KT-11, KT-14, KT-16 → Yiğit · KT-03, KT-13, KT-17 → Hamza · KT-07 (görsel), KT-18 → Furkan.

## Yardım

- Takıldığın bir script için GitHub'da `yardım` etiketli bir issue aç: ne yapmaya çalıştığını, SRS maddesini ve hata mesajını yaz. Yiğit veya Hamza, Claude ile **senin dalına** bir öneri commit'i veya PR açar; sen deneyip onaylarsın. Dosyanın sahibi yine sensin.
- Ücretsiz bir yapay zekâ aracı kullanıyorsan ona bu dosyayı, ortak sözleşme bölümünü ve ilgili SRS maddelerini birlikte ver. Sözleşmedeki adları **değiştirmemesini** özellikle söyle.

## Dikkat et

- Yaprak toplarken envantere doğrudan yazma; her zaman `Inventory.TryAdd` kullan. Hasar verirken her zaman `ApplyDamage` kullan. Böylece olaylar (ses, HUD) kendiliğinden çalışır.
- `Map.prefab` senin, ama `Match.unity` Yiğit'in. Haritayı denemek için kendi sandbox sahnende `Map.prefab`'ı kullan; `Match.unity`'yi açıp kaydetme.
- `MapBuilder` her çalıştığında `Map.prefab`'ın içini baştan kurar. Elle eklediğin bir şey varsa kaybolur; o yüzden her şeyi builder'dan üret.
- Rastgelelik için yalnızca `Services.Rng` kullan; `UnityEngine.Random` kullanma (NFR-07).
- Görsel dosyalarına (`Art/`) dokunma. Bir sprite eksikse veya ölçüsü yanlışsa Furkan'a `sanat` etiketli issue aç.
