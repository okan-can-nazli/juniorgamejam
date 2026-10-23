namespace LeafBrawl.Shared
{
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
}
