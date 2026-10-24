# Görev Dosyası – Muhammed Hamza Karamanlı

**Rol:** Balta ve savaş, 5. seviye yetenekleri, bütün bot yapay zekâsı
**Araçlar:** Claude Opus 5.5 + REA + Unity MCP
**Sahip olduğun yerler:** `Scripts/Combat/`, `Scripts/Bots/`, `Prefabs/Combat/` (`CharacterCombat.prefab`), `Prefabs/Bots/` (`Bot.prefab`), `Scenes/Sandbox/Sandbox_Hamza.unity`, `balance.json` içindeki `axe`, `abilities`, `bots` bölümlerinin değerleri
**İncelediğin PR'lar:** Yiğit'in PR'ları (M0 dahil). Yiğit yokken diğer PR'lar. Senin PR'larını Yiğit inceler.

Savaş ve botlar, oyunun "hissini" belirleyen kısım. Yiğit'in karakterine ve Okan'ın dünyasına **yalnızca sözleşme üzerinden** bağlanırsın. Onlar bitmeden de çalışabilirsin; stub'lar sana boş ama çalışan bir dünya verir.

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

- [ ] SRS 3.3, 3.4 ve Bölüm 5'i (BOT-01–BOT-13) baştan sona oku. Botun ve baltanın ihtiyaç duyduğu her bilginin ortak sözleşmede olup olmadığını kontrol et. Eksik varsa (örneğin "bir karakter şu an üretim yapıyor mu?" sorusunun cevabı) **M0 birleşmeden önce** Yiğit'e yaz. M0'dan sonra ekleme yapmak daha zahmetli.
- [ ] Yiğit'in M0 PR'ının incelemecisi sensin. Özellikle sözleşme dosyalarına, `Character.prefab` yapısına ve `.gitattributes` ile `CLAUDE.md` dosyasına dikkatle bak. Onaylayınca herkes başlar; bu yüzden hızlı ol.
- [ ] Bot durum makinesinin kod taslağını çıkar (Claude ile): durumlar, geçişler (SRS'teki diyagram), her durumun ürettiği `CharacterIntent`.

### Aşama 1 – Çekirdek

| Görev | SRS |
| --- | --- |
| [ ] `Axe` (`CharacterCombat.prefab` üzerinde): `CurrentIntent.Swing` true ve bekleme bitmişse savur. Basılı tutunca bekleme bittikçe yeniden savrulsun. Üretim sırasında (`Crafting.IsCrafting`) savurma olmaz. Tab paneli açıkken Yiğit'in `PlayerIntentSource`'u zaten `Swing = false` verir | FR-20, 3.3 |
| [ ] Savurma zamanlaması: savurma başlar → `Swung` yayınla, `character.MarkSwing()` çağır, `SpeedMultiplier = 0,5` yap. 0,12 sn sonra hasar uygula; savurma bitince hız çarpanı 1'e dönsün | FR-22 |
| [ ] Vuruş çözümü: karakter merkezinden 60 px ve bakış yönünün ±50°'si içindeki **bütün** karakterlere bir kez `ApplyDamage(Amount = Stats.Damage, Source = self)`. Merkezden hedefe çizilen çizgi `Obstacle` katmanına çarpıyorsa (Physics2D.Linecast) o hedefe vuruş işlemez. Her isabette `HitLanded` yayınla | FR-21, 3.3 |
| [ ] Ağaca vuruş: aynı yay içinde `Tree` katmanındaki en yakın **bir** `ITree`'ye de `ApplyDamage` uygula. Ağacın ne yapacağı Okan'ın işi; sen yalnızca hasarı gönderirsin | 3.3, FR-35 |
| [ ] Debug çizimi (Gizmos, yalnızca editörde): savurma yayı ve menzil | — |
| [ ] Basit bot (`BotBrain : IIntentSource`, `Bot.prefab` üzerinde): şimdilik iki durum. **Topla:** en yakın `IPickup`'a, yoksa en yakın ağaca git ve kes. **Saldır:** görünen rakip 500 px içindeyse ona yaklaş ve vur. Yol bulma yok, doğrudan yürüme yeterli | BOT-06, BOT-07 |
| [ ] Botun `Swing` intent'i, yalnızca hedef menzildeyken ve yay içindeyken true olsun. Yeni hedefi ilk gördükten 0,25 sn sonra başlasın | BOT-09 |
| [ ] Sandbox sahnende: oyuncu + 1 bot. Bot oyuncuya yaklaşıp vuruyor, oyuncu botu 10 vuruşta öldürüyor (KT-03) | KT-03 |

### Aşama 2 – Asıl oyun

**Yetenekler** (`AbilityController`, `CharacterCombat.prefab` üzerinde)

| Görev | SRS |
| --- | --- |
| [ ] Kilit açma: `GameEvents.LevelChanged` olayını dinle (yalnızca kendi karakterin için). Hız 5 → Atılma, Sağlık 5 → Kalkan, Güç 5 → Ulti `Ready` olur ve `AbilityUnlocked` yayınlanır. `State()` ve `CooldownLeft()` HUD için doğru değerleri döndürsün | 3.4 |
| [ ] Atılma: `CurrentIntent.Dash` → hareket yönünde (hareket yoksa bakış yönünde) `Motor.Dash(yön, 160, 0,15)`. Bekleme 3 sn, tekrar kullanılabilir. Kilitliyken hiçbir şey olmaz | FR-26 |
| [ ] Kalkan: `CurrentIntent.Shield` → maçta bir kez, 6 sn açık. `IDamageInterceptor` olarak gelen **ilk** balta veya ulti hasarını emer (`true` döner), sonra kırılır ve `ShieldBroken` yayınlar. Alan hasarını (`IsZone`) emmez. Süre dolarsa kırılmadan kapanır, durum `Used` olur | FR-27 |
| [ ] Ulti: `CurrentIntent.Ulti` → maçta bir kez. 0,4 sn hazırlanma (`SpeedMultiplier = 0`, `AbilityUsed` yayınla, görsel için). Ardından 120 px içindeki bütün rakiplere `ApplyDamage(Amount = int.MaxValue, IsUlti = true, Source = self)`, 120 px içindeki bütün kesilebilir ağaçlara da `IsUlti = true` hasar. Kalkan bunu emerse rakip yaşar | FR-28 |
| [ ] Kullanılmış tek seferlik yetenek tekrar çalışmaz. Üretim sırasında ve geri sayımda hiçbir yetenek çalışmaz | FR-29 |

**Botlar** (`Scripts/Bots/`)

| Görev | SRS |
| --- | --- |
| [ ] Algılama (`BotPerception`): her 0,2 sn'de bir, 650 px içindeki karakterler (yalnızca `Visibility.CanSee(bot, hedef)` true olanlar), 650 px içindeki `IPickup`'lar, `World.TreesNear` ve alan durumu. Bot yalnızca bunları bilir; ağaç konumları ve alan her zaman bilinir | BOT-02, BOT-03 |
| [ ] Durum makinesi: Alana dön > Kaç > Saldır > Topla, SRS'teki diyagram ve geçişlerle. Kaç'tan çıkış %50 can veya rakibin 500 px dışına çıkması. Kaçarken 60 px içindeki rakibe vur | BOT-03–BOT-07 |
| [ ] Kaç: `Visibility.NearestBush`'a git. Atılma açıksa rakip 200 px'ten yakınken kullan | BOT-05 |
| [ ] Alana dön: `Zone.WillBeOutsideWithin(pos, 10)` true ise merkeze dön. Alan dışındaysa ve atılma açıksa kullan | BOT-04 |
| [ ] Hedef seçimi: (yığın kademesi × 200) − mesafe değeri en yüksek görünen rakip. Oyuncu ile botu ayırt etme | BOT-08 |
| [ ] Topla: en yakın yaprak veya ganimet → yoksa alan içindeki 650 px içinde en yakın kesilebilir ağacı kes → yoksa ulu ağaca git | BOT-07 |
| [ ] Tarif: bot `IsSafe` iken önce can iksiri (can %50'nin altındaysa ve yaprak yetiyorsa), yoksa profil sırasındaki ilk üretilebilir geliştirme. Üretim sırasında `Move = 0` | BOT-10, 5.1 |
| [ ] Yetenekler: kalkan (can %50'nin altında + 100 px içinde rakip), ulti (120 px içinde en az 1 rakip), atılma (Saldır'da hedef 160–300 px uzaktayken) | BOT-13, BOT-06 |
| [ ] Yol bulma (`NavGrid` + A*): maç başında 20 px'lik hücrelerle `World.IsBlocked` kullanarak ızgara kur. `TreeFelled` olayında o hücreleri aç (kütük engel değil). Yol hesaplamasını karede en fazla 1 bot yapsın | BOT-11 |
| [ ] Takılma: 2 sn'de 20 px'ten az ilerleyen bot, 200 px içinde rastgele (`Services.Rng`) bir noktaya yönelsin | BOT-11 |
| [ ] 3 profil (Saldırgan, Çevik, Dayanıklı) `Character.Profile`'dan okunsun. Hangi bota hangi profilin verileceğine `MatchManager` (Yiğit) karar verir | 5.1 |
| [ ] Debug: editörde her botun üstünde durum adı ve hedef çizgisi; F4 ile aç/kapa | — |
| [ ] Botlar arası ittifak veya iletişim yok; her bot kendi algısıyla karar verir | BOT-12 |

### Aşama 3 – Ayar ve test

| Görev | SRS |
| --- | --- |
| [ ] 3 botu 5 maç izle: her bot en az 1 tarif üretiyor ve 1 ağaç kesiyor, alan dışında 3 sn'den uzun kalmıyor, 2 sn'den uzun takılmıyor | KT-13 |
| [ ] Kalkan ve ulti testi (`debug.json`'da başlangıç seviyeleri ile) | KT-17 |
| [ ] 10 maç üst üste: bot kaynaklı donma veya takılma yok | NFR-06 |
| [ ] Bot performansı: 3 bot birlikte karede 1 ms'nin altında (Profiler) | NFR-01 |
| [ ] Okan'la denge turu: botlar maç sonunda ortalama kaç seviyeye ulaşıyor? Çok zayıf veya çok güçlüyse kendi `bots` ve `axe` değerlerini ayarla; ekonomiyle ilgili öneriyi Okan'a issue olarak aç | 9.2 |

### Kabul testlerinde senin payın

KT-03, KT-08 (yetenek kısmı), KT-13, KT-17; NFR-06.

## Dikkat et

- Botlar **oyuncuyla aynı kuralları** kullanır (BOT-01). Botun yaptığı her şey `CharacterIntent` üzerinden geçer. Bot asla doğrudan pozisyon değiştirmez, can eklemez veya envantere yazmaz.
- Hasar yalnızca `ApplyDamage` ile verilir. Kalkanın emmesi, ölüm ve eleme sayısı Yiğit'in `Health` sınıfında çözülür.
- Bot `Visibility.CanSee` false olan karakteri **hiçbir şekilde** kullanmamalı (hedef seçimi, kaçış, mesafe), yoksa çalılık işe yaramaz.
- `CharacterCombat.prefab` ve `Bot.prefab` senin. `Character.prefab`'ı (Yiğit) ve `CharacterVisual.prefab`'ı (Furkan) açıp kaydetme. Görsel bir şey gerekiyorsa (savurma animasyonu, kalkan halkası) olay yayınla; Furkan o olayı dinler.
- Claude oturumunda Unity MCP ile yalnızca `Sandbox_Hamza.unity`'yi ve kendi prefab'larını düzenlet.
