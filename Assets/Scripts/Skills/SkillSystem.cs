// SkillSystem.cs
// ScriptableObject that owns the OSRS-style XP table and shared skill math.
//
// OSRS XP formula:
//   total_xp(L) = floor( sum_{n=1}^{L-1} floor(n + 300 * 2^(n/7)) ) / 4
//
// Precomputed thresholds are stored in xpThresholds[] (index 0 = level 1).
// All math is also exposed as static methods so callers work without an asset.
//
// Usage: Create → Evetero → SkillSystem — one asset shared across the project.

using UnityEngine;

namespace Evetero
{
    [CreateAssetMenu(menuName = "Evetero/SkillSystem", fileName = "SkillSystem")]
    public class SkillSystem : ScriptableObject
    {
        // Precomputed XP required to reach each level (index i = level i+1).
        // xpThresholds[0] = XP for level 1 = 0
        // xpThresholds[1] = XP for level 2 = 83
        // xpThresholds[98] = XP for level 99 = 13,034,431
        [HideInInspector] public int[] xpThresholds;

        // ── Singleton (set when the asset is enabled in the project) ─────────

        private static SkillSystem _instance;
        public static SkillSystem Instance => _instance;

        private void OnEnable()
        {
            _instance = this;
            PrecomputeTable();
        }

        private void PrecomputeTable()
        {
            xpThresholds = new int[99];
            for (int level = 1; level <= 99; level++)
                xpThresholds[level - 1] = XPForLevel(level);
        }

        // ── Static math (usable without an asset instance) ────────────────────

        /// <summary>
        /// Total XP required to reach <paramref name="level"/> (1-indexed, clamped 1-99).
        /// Level 1 → 0. Level 2 → 83. Level 99 → 13,034,431.
        /// </summary>
        public static int XPForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, 99);
            if (level == 1) return 0;

            int points = 0;
            for (int n = 1; n < level; n++)
                points += Mathf.FloorToInt(n + 300f * Mathf.Pow(2f, n / 7f));

            return points / 4;
        }

        /// <summary>
        /// Returns the skill level (1-99) that corresponds to <paramref name="totalXP"/>.
        /// Uses binary search over the XP table.
        /// </summary>
        public static int LevelForXP(int totalXP)
        {
            if (totalXP <= 0) return 1;

            int lo = 1, hi = 99;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (XPForLevel(mid) <= totalXP)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            return lo;
        }

        /// <summary>
        /// Returns true if <paramref name="level"/> is a breakthrough milestone (50, 75, 99).
        /// </summary>
        public static bool IsBreakthrough(int level) =>
            level == 50 || level == 75 || level == 99;
    }
}
