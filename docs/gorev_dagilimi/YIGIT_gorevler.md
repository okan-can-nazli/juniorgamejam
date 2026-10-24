# Görev Dosyası – Yiğit Ahmet Hosta

**Rol:** Proje iskeleti, ortak sözleşme, karakter çekirdeği, envanter ve tarif, maç akışı, entegrasyon sahibi
**Araçlar:** Claude Opus 5.5 + REA + Unity MCP
**Sahip olduğun yerler:** `Scripts/Shared/`, `Scripts/Core/`, `Scripts/Characters/`, `Prefabs/Systems/`, `Prefabs/Characters/`, `Scenes/Match.unity`, `Scenes/Sandbox/Sandbox_Yigit.unity`, `Input/`, `Settings/`, `ProjectSettings/`, `Packages/`, `Assets/StreamingAssets/` (yapı), repo kökündeki `.gitignore`, `.gitattributes`, `CLAUDE.md`, `README.md`
**İncelediğin PR'lar:** Hamza, Okan ve Furkan'ın PR'ları. Main'e bütün birleştirmeleri sen yaparsın. Senin PR'larını Hamza inceler.

Ekibin geri kalanı senin sözleşmene ve iskeletine bağlı. Bu yüzden **ilk işin M0'ı olabildiğince hızlı main'e koymak**. Sözleşmede bir isim ne kadar erken sabitlenirse o kadar az çakışma olur.

## Herkes için ortak kurallar

> Bu bölüm dört dosyada da **aynıdır**. Kendi görevlerine geçmeden önce bir kez oku. Bir kural değişirse dört dosyada birlikte değişir.

### Tek doğru kaynak

- Oyunun ne yapacağını **SRS v1.4** söyler: [`docs/Leaf_Brawl_SRS_Demo_v1.4.md`](../Leaf_Brawl_SRS_Demo_v1.4.md). Bu dosyalar yalnızca **işi kimin, nerede, hangi sırayla** yapacağını söyler.
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
| `ZoneWarning` | `float sonrakiYarıçapPx`. **v1.4'ten beri yayınlanmaz** (alan uyarısı kalktı, SRS 3.8); ad sözleşmede kalır | Okan |
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

### M0 – İskelet (ilk iş, herkes buna bağlı)

**Proje**
- [ ] Repo kökünde Unity 6000.6.7f1 ile **Universal 2D (URP)** şablonundan proje oluştur. Proje kökü repo kökü olsun: `Assets/`, `Packages/` ve `ProjectSettings/` repo kökünde dursun.
- [ ] Paketleri ekle: Input System, 2D Sprite, 2D Tilemap, 2D Pixel Perfect (URP içindeki Pixel Perfect Camera), TextMeshPro (uGUI), `com.coplaydev.unity-mcp` (v10.3.0, JamSandbox'taki gibi).
- [ ] Project Settings: Asset Serialization = **Force Text**, Version Control = **Visible Meta Files**, Active Input Handling = Input System Package, Company/Product = "Leaf Brawl". Varsayılan sprite içe aktarma ayarı için bir preset hazırla: PPU 16, Filter Point, Compression None.
- [ ] Unity `.gitignore` (JamSandbox'taki), ek olarak `graphify-out/`, `.DS_Store`, `*.csproj`, `*.sln*`, `Assets/StreamingAssets/debug.local.json*`.
- [ ] `.gitattributes`: `*.unity`, `*.prefab`, `*.asset`, `*.anim`, `*.controller`, `*.mat` → `merge=unityyamlmerge eol=lf`. Görsel ve ses dosyaları → `binary`. LFS kullanılmıyor; ses dosyaları `.ogg` veya kısa `.wav` olacak, her biri 1 MB'tan küçük.
- [ ] Klasör yapısı (ortak bölümdeki tablo). Her klasöre boş klasör kaybolmasın diye bir `.keep` dosyası koy.
- [ ] Katmanlar, sıralama katmanları ve fizik çarpışma matrisi: `Character` yalnızca `Obstacle` ile çarpışır. `Bush` ve `Pickup` tetikleyici (trigger) olarak çalışır.
- [ ] `Input/LeafBrawl.inputactions`. **Player** haritası: Move (WASD), Aim (fare konumu), Swing (sol tık, L), Dash (Boşluk), Shield (Q), Ulti (E), Craft1–Craft4 (1–4), Inventory (Tab), Pause (Esc). **UI** haritası: varsayılan.

**Sözleşme (Shared/)**
- [ ] Ortak bölümdeki bütün enum, struct, interface, `Services`, `GameEvents` dosyaları. İmzalar bu belgedeki gibi olsun; bir şeyi değiştirmen gerekiyorsa önce bu dosyaları güncelle.
- [ ] `Shared/Stubs/`: `StubZone` (her yer içeride, hasar 0), `StubVisibility` (herkes herkesi görür, çalılık yok), `StubWorld` (boş listeler, 4 doğma noktası SRS 3.12'deki gibi). `Services` alanları null kalırsa bunlar döner.
- [ ] `Shared/Balance.cs`: `balance.json`'ın birebir C# karşılığı ([Serializable] sınıflar, `JsonUtility` ile okunur). Ayrıca `DebugSettings.cs`: seed, botCount (0–3), startInventory, startLevels, timeScale (test için maçı hızlandırır), showOverlay.
- [ ] `Assets/StreamingAssets/balance.json`: SRS Bölüm 3'teki **bütün** değerler, ortak bölümdeki bölüm adlarıyla. Seviye tablosu, tarif maliyetleri ve alan aşamaları dizi olarak dursun. Uzunluklar px, süreler saniye. `map` bölümüne SRS'te olmayan iki dizilim değerini de ekle (Okan'ın haritası kullanır): küme içinde 3 ağaç, küme merkezinden 65 px uzakta ve 120° arayla; koruda 1 ağaç merkezde, 4 ağaç merkezden 110 px uzakta ve 90° arayla.
- [ ] `debug.json` (varsayılan değerler) + `debug.local.json` desteği. Local dosya varsa onun değerleri geçerli olur.

**Stub sınıflar (adlar sabit, gövdeler boş, sahibine geçer)**
- [ ] Hamza için: `Combat/Axe.cs`, `Combat/AbilityController.cs` (`State()`, `CooldownLeft()` → Locked, 0), `Bots/BotBrain.cs : IIntentSource` (sıfır girdi döner).
- [ ] Okan için: `World/WorldService.cs`, `World/SafeZone.cs`, `World/VisibilityService.cs` (yalnızca sınıf adları ve boş `Awake`, kayıt yapmaz; Okan doldurunca kaydeder).
- [ ] Furkan için: `Presentation/CharacterVisual.cs`, `Presentation/HudView.cs`, `Presentation/AudioManager.cs` (boş MonoBehaviour).
- [ ] Bu dosyaların sahibi M0 birleştikten sonra **klasör sahibidir**. Sen bir daha değiştirmezsin.

**Prefab ve sahneler**
- [ ] `Systems.prefab`: ConfigLoader, Rng, MatchManager (stub). Bu prefab her sahnede bulunur.
- [ ] **Boş prefab'lar (herkesin kendi parçası).** Önce bu boş prefab'ları oluştur ve sahiplerinin klasörüne koy: `Prefabs/Combat/CharacterCombat.prefab` (Hamza), `Prefabs/Presentation/CharacterVisual.prefab` (Furkan; içinde stub `CharacterVisual`), `Prefabs/World/Map.prefab` ve `Prefabs/World/SafeZone.prefab` (Okan), `Prefabs/UI/HUD.prefab` ve `Prefabs/Presentation/AudioManager.prefab` (Furkan). Sahibi daha sonra bunların **içini** doldurur. Bu yüzden kimsenin senin prefab'ına veya `Match.unity`'ye dokunması gerekmez.
- [ ] `Character.prefab`: kökte Rigidbody2D (Kinematic, Interpolate) ve CircleCollider2D (yarıçap 0,5 birim = 20 px), `Character`, `CharacterStats`, `Health`, `Inventory`, `Crafting`, `CharacterMotor`. Çocuk olarak `CharacterCombat.prefab` ve `CharacterVisual.prefab` iç içe yerleştirilmiş olsun.
- [ ] `Player.prefab`: Character'ın varyantı + `PlayerIntentSource`.
- [ ] `Bot.prefab`: Character'ın varyantı + stub `BotBrain`. **Oluştur ve hemen Hamza'ya devret**; dosya Hamza'nın klasöründe (`Prefabs/Bots/`) dursun.
- [ ] `Scenes/Match.unity`: Systems, kamera, `Map.prefab`, `SafeZone.prefab`, `HUD.prefab` (bir Canvas altında), `AudioManager.prefab`. Hepsi baştan sahnede olsun; sonradan sahneye bir şey eklemek gerekmesin. `Scenes/MainMenu.unity`: boş sahne, Furkan'a devret. `Scenes/Sandbox/Sandbox_Yigit|Hamza|Okan|Furkan.unity`: her birinde Systems + kamera + bir Player.
- [ ] Build Settings: MainMenu (0), Match (1).

**Repo belgeleri**
- [ ] `CLAUDE.md`: JamSandbox'taki dosyayı uyarla. Eklemen gerekenler: klasör sahipliği tablosu, "yalnızca kendi klasörünü değiştir", sözleşme kuralı, "sayılar balance.json'dan", çalışma döngüsü (script → `read_console` → play → commit), dal/PR akışı, ortak yazar satırı yok, Unity sürümü. Bu projede WebGL yok, masaüstü yapısı var.
- [ ] `README.md`: projeyi açma adımları ve görev dosyalarına bağlantı.
- [ ] M0 PR'ını aç, Hamza onaylasın, birleştir, `asama-0` etiketini at. Ekibe "projeyi açabilirsiniz" de.

### Aşama 1 – Çekirdek

| Görev | SRS |
| --- | --- |
| [ ] `ConfigLoader`: `balance.json` ve `debug.json`'ı oyunun başında okusun. Hatalı JSON'da hangi alanın hatalı olduğunu Console'a yazsın | NFR-05 |
| [ ] `Rng`: tek tohumdan üretilen rastgelelik (`Range`, `Chance`, `Pick`, `InsideCircle`). Herkes `Services.Rng` kullanır; `UnityEngine.Random` yasak | NFR-07 |
| [ ] `Character`: her karede `IIntentSource.ReadIntent` → `CurrentIntent`. Geri sayımda, duraklatmada ve ölüyken sıfır intent. `Facing`'i `Aim`'den güncelle | FR-05, FR-11 |
| [ ] `CharacterMotor`: intent'ten hareket. 8 yönde hareket, çapraz hareket aynı hızda, `Stats.SpeedPx × SpeedMultiplier` ile. Rigidbody2D ile `Obstacle` katmanına çarpar. `Dash(yön, mesafePx, süre)` engellere çarparak itsin (Hamza'nın atılması bunu çağırır) | FR-10, FR-12 |
| [ ] `PlayerIntentSource`: Input System'den intent üretsin. Aim = fare dünya konumu − karakter. Tab, `InventoryOpen` durumunu değiştirsin ve `InventoryToggled(bool)` olayını yayınlasın (Furkan'ın paneli dinler). Panel açıkken `Swing` false olsun | FR-11, FR-40 |
| [ ] Kamera: URP Pixel Perfect Camera, referans 640×360, PPU 16, oyuncuyu takip. Oyuncu ölünce dursun. 16:10 ekranda siyah bant olmasın | FR-13, 3.13 |
| [ ] `CharacterStats`: seviye tablosu (3.1), `SetLevel`, `Level`, `Damage`, `SpeedPx`, `MaxHp`. Sağlık seviyesi artınca mevcut can da +15 artsın | 3.1 |
| [ ] `Health`: `ApplyDamage` içinde önce çocuklardaki `IDamageInterceptor`'lara sor (kalkan). Can 0 olunca `Died(kurban, katil)` olayını yayınla. Son hasarı veren katildir; alan hasarında katil null | FR-24, FR-84 |
| [ ] Can yenilenmesi: 5 sn hasar yoksa Sağlık seviyesine göre yenilensin; hasarda sayaç sıfırlansın | 3.2, FR-25 |
| [ ] `Inventory`: sınırlar (K/S/Y 40, T 5), `TryAdd` eklenen adedi döner ve `LeafCollected` yayınlar. Ölümde envanter **silinmez** (Okan ganimeti hesaplarken okur) | 3.6, FR-32 |
| [ ] `Crafting`: artan maliyet tablosu, can iksiri, `IsSafe` kuralı (alan içinde + son 3 sn hasar yok + son 3 sn savurma yok), 1,5 sn üretim, hasar gelirse iptal, yapraklar tamamlanınca düşülür. Olaylar: `CraftStarted`, `CraftCompleted`, `CraftCanceled`, `CraftRejected` | 3.7, FR-43–FR-46 |
| [ ] Tehdit puanı ve yığın kademesi: (Güç−1)+(Hız−1)+(Sağlık−1) → kademe tablosu (3.9). Değişince `ThreatChanged` yayınla | 3.9, FR-60 |
| [ ] Seviye 5'e ulaşılınca Hamza'nın yeteneği açılabilsin: `CharacterStats` her seviye değişiminde `GameEvents.LevelChanged` yayınlasın (debug `SetLevel` dahil). Hamza'nın `AbilityController`'ı bunu dinler | 3.4 |
| [ ] `MatchManager` (ilk sürüm): oyuncu + `debug.botCount` kadar botu `World.SpawnPoints`'e rastgele yerleştir, botlara profil ve atkı rengi ver. 3 sn geri sayım (`CountdownTick`), geri sayımda intent'leri yok say. Maç saati geri sayım bitince başlasın. Son karakter kalınca veya oyuncu ölünce `Ended` | FR-04, FR-05, FR-07 |
| [ ] Debug kısayolları (yalnızca editörde): F2 envantere 10'ar yaprak ekler, F3 bütün seviyeleri +1 yapar. Test için | KT-08 |
| [ ] Entegrasyon: Hamza'nın balta ve basit bot PR'ı, Okan'ın harita, ağaç, yaprak ve alan PR'ları, Furkan'ın ilk sprite'ları → `Match.unity`. 1 oyuncu + 1 bot ile 5 dakikalık maç oynanıyor → `asama-1` | SRS 10 |

### Aşama 2 – Asıl oyun

| Görev | SRS |
| --- | --- |
| [ ] Sıralama: ölüm sırası, aynı karede ölenler için ölümden önceki cana göre sıra, eşitlikte ortak sıra. Eleme sayıları, hayatta kalma süresi, tehdit puanı → `MatchResult` | FR-82–FR-84, FR-08 |
| [ ] Oyuncu ölünce maç donsun, sonuç ekranı açılsın; botların maçı arka planda sürmesin | FR-07, FR-85 |
| [ ] Duraklatma: Esc → `Pause()` (timeScale 0, bütün sayaçlar durur). `Resume()`, `Restart()`. Duraklatma menüsünde Esc = Devam | FR-06 |
| [ ] `SceneFlow` (static): `StartMatch()`, `Restart()`, `LoadMenu()`, `Quit()`. Furkan'ın düğmeleri yalnızca bunları çağırır | FR-01, FR-08 |
| [ ] `CharacterVisual` için okunabilir alanların eksiksiz olduğunu kontrol et (Furkan'la birlikte, gerekirse sözleşme PR'ı) | FR-61–FR-64 |
| [ ] Aynı karede birden çok ölümde `Died` olaylarının sırası belirli olsun (sıralama mantığı için) | FR-83 |
| [ ] Entegrasyon: 1 oyuncu + 3 bot, çalılık, yetenekler, ganimet, HUD, sonuç ekranı → `asama-2` | SRS 10 |

### Aşama 3 – Cila ve teslim

| Görev | SRS |
| --- | --- |
| [ ] Tohumla tekrarlanabilirlik: `debug.json`'da sabit seed verildiğinde aynı başlangıç ve yaprak üretimi oluşsun. Okan ve Hamza'yla birlikte iki maçta karşılaştır | NFR-07 |
| [ ] Performans: M1 veya i5 dizüstünde 1080p'de 60 FPS. Profiler'la en pahalı 3 şeyi bul, sahibine issue aç | NFR-01, KT-14 |
| [ ] Açılış süreleri: menü 10 sn, maç 3 sn içinde | NFR-03 |
| [ ] Windows ve macOS yapıları: tek klasör, kurulum yok. Bir yapıyı ağ kapalıyken baştan sona oynat | NFR-04, NFR-11 |
| [ ] Okan'ın yürüttüğü kabul testlerinde sana düşen hataları kapat | Bölüm 9 |
| [ ] Son etiket: `asama-3`, ardından yapıları GitHub Releases'a yükle (ekip içi, herkese açık değil) | — |

### Kabul testlerinde senin payın

KT-01, KT-02 (hareket kısmı), KT-05, KT-06, KT-07 (mantık), KT-08, KT-11, KT-12 (Okan'la), KT-14, KT-16.

## Dikkat et

- Sözleşmeyi M0'da **eksiksiz** düşün. Sonradan ad değiştirmek, dört kişinin kodunu kırar. Emin olmadığın bir şey varsa eklemeyi seç, ad değiştirmeyi değil.
- `Match.unity` sahnesini yalnızca sen düzenlersin. Başkasının işini sahneye eklemek için onun **prefab'ını** sahneye koyarsın; onun içini düzenlemezsin.
- PR birleştirmeden önce dalı main ile güncelle ve projeyi bir kez açıp Console'da kırmızı hata olmadığına bak.
- Yetişmeyen iş olursa SRS 10'daki sıraya göre kes: önce Aşama 3 cilası, sonra envanter paneli (1–4 tuşları yeterli).
