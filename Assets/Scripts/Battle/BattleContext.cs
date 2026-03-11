// BattleContext.cs
// Static relay that carries battle setup data across scene loads.
//
// Usage:
//   1. Before loading the Battle scene, set BattleContext.PendingBattle.
//   2. BattleSceneSetup reads PendingBattle in Start() to build the encounter.
//   3. After BattleSceneSetup consumes the data, PendingBattle is cleared.

using System.Collections.Generic;

namespace Evetero
{
    public static class BattleContext
    {
        // ── In-data ───────────────────────────────────────────────────────────

        /// <summary>The battle encounter to load. Set before loading the Battle scene.</summary>
        public static BattleData PendingBattle { get; set; }

        /// <summary>Hero party taking part in the battle (from the overworld roster).</summary>
        public static HeroData[] HeroParty { get; set; }

        // ── Out-data ──────────────────────────────────────────────────────────

        /// <summary>Heroes recruited during the last battle, to be added to the roster.</summary>
        public static List<HeroData> RecruitedHeroes { get; } = new List<HeroData>();

        /// <summary>Clear pending-in data once consumed.</summary>
        public static void ConsumePending()
        {
            PendingBattle = null;
            HeroParty     = null;
        }

        /// <summary>Clear post-battle out data after the roster processes it.</summary>
        public static void ClearRecruited() => RecruitedHeroes.Clear();
    }
}
