using System;
using LeafBrawl.Characters;
using UnityEngine;

namespace LeafBrawl.Shared
{
    /// <summary>
    /// Oyun olayları. Ses, efekt ve HUD yalnızca bunları dinleyerek çalışır.
    /// Dinle: <c>GameEvents.Died += OnDied;</c> (OnDisable'da <c>-=</c> ile bırak). Yayınla: <c>GameEvents.RaiseDied(kurban, katil);</c>
    /// Her olayın yanında yayınlayan kişi yazar; başkasının olayını yayınlama.
    /// </summary>
    public static class GameEvents
    {
        // Yiğit
        public static event Action<MatchState> MatchStateChanged;
        public static void RaiseMatchStateChanged(MatchState state) => MatchStateChanged?.Invoke(state);

        // Yiğit
        public static event Action<int> CountdownTick;
        public static void RaiseCountdownTick(int secondsLeft) => CountdownTick?.Invoke(secondsLeft);

        // Yiğit (Tab)
        public static event Action<bool> InventoryToggled;
        public static void RaiseInventoryToggled(bool open) => InventoryToggled?.Invoke(open);

        // Yiğit
        public static event Action<Character, StatType, int> LevelChanged;
        public static void RaiseLevelChanged(Character character, StatType stat, int newLevel) => LevelChanged?.Invoke(character, stat, newLevel);

        // Yiğit (Health)
        public static event Action<Character, DamageInfo> Damaged;
        public static void RaiseDamaged(Character victim, DamageInfo info) => Damaged?.Invoke(victim, info);

        // Yiğit
        public static event Action<Character, int> Healed;
        public static void RaiseHealed(Character character, int amount) => Healed?.Invoke(character, amount);

        // Yiğit; alan hasarında killer null
        public static event Action<Character, Character> Died;
        public static void RaiseDied(Character victim, Character killer) => Died?.Invoke(victim, killer);

        // Yiğit (Inventory.TryAdd)
        public static event Action<Character, LeafType, int> LeafCollected;
        public static void RaiseLeafCollected(Character character, LeafType type, int count) => LeafCollected?.Invoke(character, type, count);

        // Yiğit
        public static event Action<Character, RecipeId> CraftStarted;
        public static void RaiseCraftStarted(Character character, RecipeId recipe) => CraftStarted?.Invoke(character, recipe);

        // Yiğit; iksirde newLevel 0
        public static event Action<Character, RecipeId, int> CraftCompleted;
        public static void RaiseCraftCompleted(Character character, RecipeId recipe, int newLevel) => CraftCompleted?.Invoke(character, recipe, newLevel);

        // Yiğit
        public static event Action<Character, RecipeId> CraftCanceled;
        public static void RaiseCraftCanceled(Character character, RecipeId recipe) => CraftCanceled?.Invoke(character, recipe);

        // Yiğit
        public static event Action<Character, RecipeId, CraftFail> CraftRejected;
        public static void RaiseCraftRejected(Character character, RecipeId recipe, CraftFail reason) => CraftRejected?.Invoke(character, recipe, reason);

        // Yiğit
        public static event Action<Character, int, PileTier> ThreatChanged;
        public static void RaiseThreatChanged(Character character, int score, PileTier tier) => ThreatChanged?.Invoke(character, score, tier);

        // Hamza
        public static event Action<Character, AbilityId> AbilityUnlocked;
        public static void RaiseAbilityUnlocked(Character character, AbilityId ability) => AbilityUnlocked?.Invoke(character, ability);

        // Hamza
        public static event Action<Character, AbilityId> AbilityUsed;
        public static void RaiseAbilityUsed(Character character, AbilityId ability) => AbilityUsed?.Invoke(character, ability);

        // Hamza
        public static event Action<Character> ShieldBroken;
        public static void RaiseShieldBroken(Character character) => ShieldBroken?.Invoke(character);

        // Hamza
        public static event Action<Character> Swung;
        public static void RaiseSwung(Character character) => Swung?.Invoke(character);

        // Hamza
        public static event Action<Character, IDamageable, int> HitLanded;
        public static void RaiseHitLanded(Character attacker, IDamageable target, int damage) => HitLanded?.Invoke(attacker, target, damage);

        // Okan
        public static event Action<ITree, Character> TreeHit;
        public static void RaiseTreeHit(ITree tree, Character by) => TreeHit?.Invoke(tree, by);

        // Okan
        public static event Action<ITree, Character> TreeFelled;
        public static void RaiseTreeFelled(ITree tree, Character by) => TreeFelled?.Invoke(tree, by);

        // Okan
        public static event Action<Character> GreatTreeHit;
        public static void RaiseGreatTreeHit(Character by) => GreatTreeHit?.Invoke(by);

        // Okan
        public static event Action<Vector2, LeafType> LeafSpawned;
        public static void RaiseLeafSpawned(Vector2 position, LeafType type) => LeafSpawned?.Invoke(position, type);

        // Okan
        public static event Action<Vector2, int> LootDropped;
        public static void RaiseLootDropped(Vector2 position, int total) => LootDropped?.Invoke(position, total);

        // Okan
        public static event Action<int> ZonePhaseChanged;
        public static void RaiseZonePhaseChanged(int phase) => ZonePhaseChanged?.Invoke(phase);

        // Okan (10 sn kala)
        public static event Action<float> ZoneWarning;
        public static void RaiseZoneWarning(float nextRadiusPx) => ZoneWarning?.Invoke(nextRadiusPx);

        // Okan
        public static event Action<Character, bool> RevealChanged;
        public static void RaiseRevealChanged(Character character, bool revealed) => RevealChanged?.Invoke(character, revealed);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay()
        {
            MatchStateChanged = null;
            CountdownTick = null;
            InventoryToggled = null;
            LevelChanged = null;
            Damaged = null;
            Healed = null;
            Died = null;
            LeafCollected = null;
            CraftStarted = null;
            CraftCompleted = null;
            CraftCanceled = null;
            CraftRejected = null;
            ThreatChanged = null;
            AbilityUnlocked = null;
            AbilityUsed = null;
            ShieldBroken = null;
            Swung = null;
            HitLanded = null;
            TreeHit = null;
            TreeFelled = null;
            GreatTreeHit = null;
            LeafSpawned = null;
            LootDropped = null;
            ZonePhaseChanged = null;
            ZoneWarning = null;
            RevealChanged = null;
        }
    }
}
