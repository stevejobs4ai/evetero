// CraftingRecipeData.cs
// ScriptableObject defining a single crafting recipe.
// Create via: Right-click in Project → Create → Evetero → Crafting Recipe

using System;
using UnityEngine;

namespace Evetero
{
    // ── Supporting enums / structs ────────────────────────────────────────────

    /// <summary>
    /// Identifies which physical station can execute a recipe.
    /// </summary>
    public enum CraftingStationType
    {
        None,           // can be crafted anywhere (hand-crafted)
        Furnace,        // smelts ores into bars
        Anvil,          // forges bars into weapons / armour
        FletchingBench, // fletch logs into bows / arrows
        CookingRange    // cook raw food
    }

    /// <summary>One ingredient slot required by a crafting recipe.</summary>
    [Serializable]
    public struct CraftingIngredient
    {
        [Tooltip("Resource type consumed.")]
        public ResourceType type;

        [Tooltip("Amount consumed per single craft.")]
        [Min(1)] public int amount;
    }

    /// <summary>One item produced by a crafting recipe.</summary>
    [Serializable]
    public struct CraftingOutput
    {
        [Tooltip("Resource type produced.")]
        public ResourceType type;

        [Tooltip("Amount produced per single craft.")]
        [Min(1)] public int amount;
    }

    // ── CraftingRecipeData ────────────────────────────────────────────────────

    /// <summary>
    /// All static data for one crafting recipe.
    /// Assign to <see cref="CraftingStation.recipes"/> in the Inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "Evetero/Crafting Recipe", fileName = "NewCraftingRecipe")]
    public class CraftingRecipeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown in the crafting UI.")]
        public string recipeName;

        [Tooltip("Short description shown in the recipe detail panel.")]
        [TextArea(2, 4)]
        public string description;

        [Header("Requirements")]
        [Tooltip("The skill that governs this recipe.")]
        public SkillType requiredSkill;

        [Tooltip("Minimum skill level needed to see and craft this recipe.")]
        [Min(1)] public int requiredLevel = 1;

        [Tooltip("XP awarded to the hero's requiredSkill on each successful craft.")]
        [Min(0)] public int xpReward;

        [Header("Station")]
        [Tooltip("Which type of crafting station is needed.")]
        public CraftingStationType stationType;

        [Header("Timing")]
        [Tooltip("Real seconds taken for one craft cycle.")]
        [Min(0.1f)] public float craftTimeSeconds = 2f;

        [Header("Ingredients & Outputs")]
        [Tooltip("Resources consumed per single craft.")]
        public CraftingIngredient[] ingredients;

        [Tooltip("Resources produced per single craft.")]
        public CraftingOutput[] outputs;
    }
}
