// AbilitySystem.cs
// MonoBehaviour that executes abilities read from HeroData.
//
// Attach to a HeroController (or a dedicated CombatManager object).
// The system reads AbilityData assets and resolves their effects at runtime.
//
// KEY DESIGN POINT:
//   Adding a new ability = create an AbilityData asset.
//   AbilitySystem never needs to change.

using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Evetero
{

    // ── AbilityResult ────────────────────────────────────────────────────────

    /// <summary>
    /// Returned by Execute() so callers (UI, AI, logging) know what happened.
    /// </summary>
    public struct AbilityResult
    {
        public bool success;
        public string failReason;   // "On cooldown", "Not enough mana", etc.
        public int value;           // damage dealt or HP restored
        public string abilityName;
    }

    // ── AbilitySystem ────────────────────────────────────────────────────────

    /// <summary>
    /// Reads abilities from a HeroController's HeroData and executes them.
    /// Handles: formula evaluation, cooldown tracking, mana cost, VFX spawning.
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        [Tooltip("The hero this system belongs to.")]
        public HeroController owner;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Execute an ability from this hero's loadout by slot index (0–3).
        /// </summary>
        public AbilityResult ExecuteBySlot(int slot, HeroController target)
        {
            if (owner == null || owner.data == null)
                return Fail("No owner or owner has no HeroData.");

            var ability = owner.data.GetAbility(slot);
            if (ability == null)
                return Fail($"Slot {slot} is empty.");

            return Execute(ability, slot, target);
        }

        /// <summary>
        /// Execute a specific AbilityData against a target.
        /// Slot index is needed to look up cooldown state.
        /// </summary>
        public AbilityResult Execute(AbilityData ability, int slot, HeroController target)
        {
            // ── Validation ────────────────────────────────────────────────────
            if (owner.abilityCooldowns[slot] > 0)
                return Fail($"{ability.abilityName} is on cooldown ({owner.abilityCooldowns[slot]} turns left).");

            if (owner.currentMana < ability.manaCost)
                return Fail($"Not enough mana for {ability.abilityName}.");

            if (target == null || !target.IsAlive)
                return Fail("Invalid or dead target.");

            // ── Pay cost & start cooldown ─────────────────────────────────────
            owner.currentMana -= ability.manaCost;
            owner.abilityCooldowns[slot] = ability.cooldownTurns;

            // ── Evaluate formula ──────────────────────────────────────────────
            int value = 0;
            if (!string.IsNullOrEmpty(ability.damageFormula))
                value = EvaluateFormula(ability.damageFormula, owner, target);

            // ── Apply effect ──────────────────────────────────────────────────
            ApplyEffect(ability, value, target);

            // ── Spawn VFX ─────────────────────────────────────────────────────
            if (ability.vfxPrefab != null)
                SpawnVFX(ability.vfxPrefab, target.transform.position);

            // ── Play SFX ─────────────────────────────────────────────────────
            if (ability.sfx != null)
                AudioSource.PlayClipAtPoint(ability.sfx, target.transform.position);

            Debug.Log($"[AbilitySystem] {owner.data.heroName} used {ability.abilityName} → value={value}");

            return new AbilityResult
            {
                success     = true,
                value       = value,
                abilityName = ability.abilityName
            };
        }

        /// <summary>
        /// Call at the end of each turn to decrement all cooldowns by 1.
        /// </summary>
        public void TickCooldowns()
        {
            for (int i = 0; i < owner.abilityCooldowns.Length; i++)
                if (owner.abilityCooldowns[i] > 0)
                    owner.abilityCooldowns[i]--;
        }

        // ── Formula Evaluation ────────────────────────────────────────────────

        /// <summary>
        /// Replaces stat tokens in a formula string, then evaluates the math.
        /// Supports: {atk}, {matk}, {def}, {mdef}
        /// Example: "{matk} * 1.2 - {mdef}" → 120 * 1.2 - 40 = 104
        /// </summary>
        public static int EvaluateFormula(string formula, HeroController caster, HeroController target)
        {
            // Substitute tokens
            formula = formula.Replace("{atk}",   caster.data.baseStats.attack.ToString());
            formula = formula.Replace("{matk}",  caster.data.baseStats.magicPower.ToString());
            formula = formula.Replace("{def}",   target.data.baseStats.defense.ToString());
            formula = formula.Replace("{mdef}",  target.data.baseStats.magicDefense.ToString());

            // Evaluate simple arithmetic (no external parser needed for MVP)
            float result = EvalSimpleMath(formula);
            return Mathf.Max(0, Mathf.RoundToInt(result));
        }

        // ── Effect Application ────────────────────────────────────────────────

        private void ApplyEffect(AbilityData ability, int value, HeroController target)
        {
            switch (ability.abilityType)
            {
                case AbilityType.Attack:
                    target.TakeDamage(value);
                    break;

                case AbilityType.Heal:
                    target.RestoreHP(value);
                    break;

                case AbilityType.Buff:
                    // TODO: route to a StatusEffectSystem
                    Debug.Log($"[AbilitySystem] Buff '{ability.abilityName}' applied to {target.data.heroName}");
                    break;

                case AbilityType.Debuff:
                    // Light damage component first, then debuff status
                    if (value > 0) target.TakeDamage(value);
                    Debug.Log($"[AbilitySystem] Debuff '{ability.abilityName}' applied to {target.data.heroName}");
                    break;
            }
        }

        // ── VFX ───────────────────────────────────────────────────────────────

        private void SpawnVFX(GameObject prefab, Vector3 position)
        {
            var fx = Instantiate(prefab, position, Quaternion.identity);
            // Auto-destroy after 3 seconds — replace with pooling later
            Destroy(fx, 3f);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static AbilityResult Fail(string reason)
        {
            Debug.LogWarning($"[AbilitySystem] {reason}");
            return new AbilityResult { success = false, failReason = reason };
        }

        /// <summary>
        /// Evaluates a simple left-to-right math string with +, -, *, /.
        /// Good enough for MVP damage formulas. Swap in a real parser if needed.
        /// </summary>
        private static float EvalSimpleMath(string expr)
        {
            // Strip whitespace
            expr = Regex.Replace(expr, @"\s+", "");

            // Attempt to use C# DataTable.Compute for full operator precedence
            try
            {
                var dt = new System.Data.DataTable();
                var result = dt.Compute(expr, null);
                return Convert.ToSingle(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AbilitySystem] Formula parse error: {e.Message} | expr='{expr}'");
                return 0f;
            }
        }
    }
}

/*
 * ── HOW TO ADD A NEW ABILITY (no code changes) ───────────────────────────────
 *
 * 1. Create → Evetero → Ability Data  →  fill in the inspector fields
 * 2. Open the hero's HeroData asset
 * 3. Drag your new AbilityData into the abilities[] slot
 * Done. AbilitySystem.Execute() handles it automatically.
 *
 * ── NEXT: connect to your combat loop ────────────────────────────────────────
 *
 * In your CombatManager turn loop:
 *
 *   AbilityResult result = abilitySystem.ExecuteBySlot(slotIndex, enemyController);
 *   if (result.success)
 *       combatUI.ShowDamageNumber(result.value, target.transform.position);
 *   else
 *       combatUI.ShowError(result.failReason);
 *
 *   abilitySystem.TickCooldowns();  // call at end of each turn
 * ─────────────────────────────────────────────────────────────────────────────
 */
