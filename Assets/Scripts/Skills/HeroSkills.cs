// HeroSkills.cs
// MonoBehaviour that tracks per-skill XP for one hero.
// Attach to the same GameObject as HeroController.
//
// All heroes train all skills regardless of combat type (Warrior / Ranger / Mage).
// Skill levels are derived from accumulated XP via SkillSystem.LevelForXP().

using System;
using UnityEngine;

namespace Evetero
{
    public class HeroSkills : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("Optional reference to the SkillSystem asset — used for inspector display.")]
        public SkillSystem skillSystem;

        // XP per skill, indexed by (int)SkillType.
        // Serialized so values survive scene reload in the editor.
        [SerializeField] private int[] _skillXP;

        private static readonly int SkillCount = Enum.GetValues(typeof(SkillType)).Length;

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired whenever a skill gains at least one level.</summary>
        public event Action<SkillType, int> OnLevelUp;

        /// <summary>Fired when a skill reaches a breakthrough level (50, 75, 99).</summary>
        public event Action<SkillType, int> OnBreakthrough;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            // Always re-initialize to avoid stale lengths when SkillType grows.
            _skillXP = new int[SkillCount];
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Total XP accumulated in the given skill.</summary>
        public int GetXP(SkillType skill) => _skillXP[(int)skill];

        /// <summary>Current level (1-99) for the given skill.</summary>
        public int GetLevel(SkillType skill) => SkillSystem.LevelForXP(_skillXP[(int)skill]);

        /// <summary>
        /// Add <paramref name="amount"/> XP to <paramref name="skill"/>.
        /// Fires OnLevelUp if the level increases, and OnBreakthrough at levels 50, 75, 99.
        /// </summary>
        public void GainXP(SkillType skill, int amount)
        {
            if (amount <= 0) return;

            int idx      = (int)skill;
            int oldLevel = SkillSystem.LevelForXP(_skillXP[idx]);

            _skillXP[idx] += amount;

            int newLevel = SkillSystem.LevelForXP(_skillXP[idx]);
            if (newLevel > oldLevel)
            {
                OnLevelUp?.Invoke(skill, newLevel);
                if (SkillSystem.IsBreakthrough(newLevel))
                    OnBreakthrough?.Invoke(skill, newLevel);
            }
        }
    }
}
