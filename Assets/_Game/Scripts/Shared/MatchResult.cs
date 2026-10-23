using System.Collections.Generic;
using LeafBrawl.Characters;

namespace LeafBrawl.Shared
{
    /// <summary>Bir karakterin maç sonundaki satırı (FR-08, FR-82–FR-84).</summary>
    public struct RankEntry
    {
        public Character Character;
        public int Rank;              // 1–4; FR-83'e göre eşitlikte aynı sıra
        public float SurvivalTime;    // sn, geri sayım bitince başlar
        public int Kills;
        public int ThreatScore;
    }

    /// <summary>IMatch.Result; maç Ended olunca dolar.</summary>
    public class MatchResult
    {
        public int PlayerRank;
        public float PlayerSurvivalTime;
        public int PlayerKills;
        public int PlayerThreatScore;
        public IReadOnlyList<RankEntry> Ranking;   // 1. sıradan 4. sıraya
    }
}
