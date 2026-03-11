// CraftingStation.cs
// MonoBehaviour representing a physical crafting station in the world.
// Attach to any GameObject that should act as a Furnace, Anvil, etc.
// Proximity detection uses a CircleCollider2D trigger; the UI opens
// automatically when a hero walks within interactRange.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    /// <summary>
    /// A world-space crafting station that filters available recipes by type
    /// and skill level, validates resource requirements, and executes crafts
    /// as coroutines.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CraftingStation : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Station Config")]
        [Tooltip("Which station type this object represents.")]
        public CraftingStationType stationType;

        [Tooltip("World-unit radius within which the hero triggers the crafting UI.")]
        [Min(0f)] public float interactRange = 2f;

        [Header("Recipes")]
        [Tooltip("All recipes assignable to this station; filtered at runtime by type and level.")]
        public CraftingRecipeData[] recipes;

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired after each successful craft; carries the completed recipe.</summary>
        public event Action<CraftingRecipeData> OnCraftComplete;

        /// <summary>Fired when a hero enters interact range; carries this station.</summary>
        public event Action<CraftingStation> OnPlayerEntered;

        /// <summary>Fired when a hero leaves interact range.</summary>
        public event Action OnPlayerExited;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            // Add a trigger collider sized to interactRange for proximity detection.
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.radius    = interactRange;
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<HeroController>() == null) return;
            OnPlayerEntered?.Invoke(this);

            var ui = FindFirstObjectByType<CraftingUI>();
            if (ui != null)
            {
                var heroSkills = other.GetComponent<HeroSkills>();
                ui.Open(this, heroSkills);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<HeroController>() == null) return;
            OnPlayerExited?.Invoke();

            FindFirstObjectByType<CraftingUI>()?.Close();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all recipes whose <see cref="CraftingRecipeData.stationType"/> matches
        /// this station and whose <see cref="CraftingRecipeData.requiredLevel"/> does not
        /// exceed <paramref name="skillLevel"/>.
        /// </summary>
        /// <param name="skillLevel">The hero's current level in the relevant skill.</param>
        public List<CraftingRecipeData> GetAvailableRecipes(int skillLevel)
        {
            var result = new List<CraftingRecipeData>();
            if (recipes == null) return result;

            foreach (var recipe in recipes)
            {
                if (recipe == null) continue;
                if (recipe.stationType != stationType) continue;
                if (skillLevel >= recipe.requiredLevel)
                    result.Add(recipe);
            }
            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="ResourceBank"/> currently holds all
        /// ingredients required by <paramref name="recipe"/>.
        /// </summary>
        public bool CanCraft(CraftingRecipeData recipe)
        {
            if (recipe == null || recipe.ingredients == null) return false;
            if (ResourceBank.Instance == null) return false;

            foreach (var ing in recipe.ingredients)
            {
                if (ResourceBank.Instance.GetAmount(ing.type) < ing.amount)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Coroutine that executes one full craft cycle:
        /// <list type="number">
        ///   <item>Spends ingredients from <see cref="ResourceBank"/>.</item>
        ///   <item>Waits <see cref="CraftingRecipeData.craftTimeSeconds"/> seconds.</item>
        ///   <item>Deposits outputs into <see cref="ResourceBank"/>.</item>
        ///   <item>Awards XP to the first <see cref="HeroSkills"/> found in the scene.</item>
        ///   <item>Fires <see cref="OnCraftComplete"/> and invokes <paramref name="onComplete"/>.</item>
        /// </list>
        /// Does nothing if <paramref name="recipe"/> is null or <see cref="CanCraft"/> is false.
        /// </summary>
        /// <param name="recipe">The recipe to execute.</param>
        /// <param name="onComplete">Optional callback invoked after completion.</param>
        public IEnumerator DoCraft(CraftingRecipeData recipe, Action onComplete)
        {
            if (recipe == null) yield break;
            if (!CanCraft(recipe)) yield break;

            // 1. Spend ingredients
            foreach (var ing in recipe.ingredients)
                ResourceBank.Instance.Spend(ing.type, ing.amount);

            // 2. Wait craft time
            yield return new WaitForSeconds(recipe.craftTimeSeconds);

            // 3. Deposit outputs
            foreach (var output in recipe.outputs)
                ResourceBank.Instance.Deposit(output.type, output.amount);

            // 4. Award XP to active hero
            var heroSkills = FindFirstObjectByType<HeroSkills>();
            if (heroSkills != null)
                heroSkills.GainXP(recipe.requiredSkill, recipe.xpReward);

            // 5. Notify listeners
            OnCraftComplete?.Invoke(recipe);
            onComplete?.Invoke();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the primary <see cref="SkillType"/> associated with this station type.
        /// Used by the UI to look up the hero's relevant skill level.
        /// </summary>
        public SkillType PrimarySkill => stationType switch
        {
            CraftingStationType.Furnace        => SkillType.Smithing,
            CraftingStationType.Anvil          => SkillType.Smithing,
            CraftingStationType.FletchingBench => SkillType.Fletching,
            CraftingStationType.CookingRange   => SkillType.Cooking,
            _                                  => SkillType.Crafting
        };

        // ── Gizmos ────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
