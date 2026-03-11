// RecruitmentSystem.cs
// Fire Emblem-style recruitment helper used by BattleManager.
//
// A unit is recruitable when its BattleUnit.RecruitableData != null.
// The Talk action passes through here for condition checks before the
// unit switches sides.

using UnityEngine;

namespace Evetero
{
    /// <summary>
    /// Validates and executes mid-battle recruitment.
    /// BattleManager calls CanRecruit / Recruit rather than handling
    /// the logic inline, keeping recruitment rules centralised here.
    /// </summary>
    public static class RecruitmentSystem
    {
        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true when <paramref name="hero"/> meets the conditions to
        /// recruit <paramref name="target"/>.
        /// </summary>
        public static bool CanRecruit(BattleUnit hero, BattleUnit target)
        {
            if (hero   == null || target == null)             return false;
            if (!hero.IsAlive || !target.IsAlive)             return false;
            if (target.RecruitableData == null)               return false;
            if (target.IsRecruited)                           return false;
            if (hero.Team != BattleTeam.Hero)                 return false;

            var required = target.RecruitableData.requiredHero;
            if (required != null && hero.HeroData != required) return false;

            return true;
        }

        /// <summary>
        /// Executes the recruitment: switches the target to the hero team,
        /// queues their HeroData in BattleContext, and fires the dialogue log.
        /// Returns true on success.
        /// </summary>
        public static bool Recruit(BattleUnit hero, BattleUnit target)
        {
            if (!CanRecruit(hero, target))
            {
                Debug.LogWarning($"[RecruitmentSystem] Recruit failed — conditions not met " +
                                 $"(hero={hero?.Name}, target={target?.Name}).");
                return false;
            }

            target.SetRecruited();
            Debug.Log($"[RecruitmentSystem] {target.Name} recruited by {hero.Name}!");

            if (target.RecruitableData.recruitDialogue != null)
            {
                Debug.Log($"[RecruitmentSystem] Play dialogue: " +
                          $"{target.RecruitableData.recruitDialogue.sceneLabel}");
            }

            if (target.RecruitableData.joinHeroData != null &&
                !BattleContext.RecruitedHeroes.Contains(target.RecruitableData.joinHeroData))
            {
                BattleContext.RecruitedHeroes.Add(target.RecruitableData.joinHeroData);
                Debug.Log($"[RecruitmentSystem] {target.RecruitableData.joinHeroData.heroName} " +
                          $"added to post-battle roster.");
            }

            return true;
        }
    }
}
