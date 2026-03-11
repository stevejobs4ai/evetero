// AbilityData.cs
// ScriptableObject defining a single ability in Evetero.
//
// Usage: Right-click in Project → Create → Evetero → Ability Data
// Adding a new ability = create an asset, no code changes.
//
// Mira's 4 abilities are documented at the bottom as a reference
// for how to fill in the inspector fields.

using UnityEngine;

namespace Evetero
{
    // ── Enums ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Broad category that tells AbilitySystem how to resolve the effect.
    /// </summary>
    public enum AbilityType
    {
        Attack,     // deals damage to a target
        Heal,       // restores HP to self or ally
        Buff,       // applies a positive status to self or ally
        Debuff      // applies a negative status to an enemy
    }

    /// <summary>
    /// Who the ability can legally target.
    /// </summary>
    public enum AbilityTarget
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    // ── AbilityData ScriptableObject ─────────────────────────────────────────

    /// <summary>
    /// All static data for one ability. Runtime logic lives in AbilitySystem;
    /// this asset is pure configuration.
    /// </summary>
    [CreateAssetMenu(menuName = "Evetero/Ability Data", fileName = "NewAbility")]
    public class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown on the ability button.")]
        public string abilityName;

        [Tooltip("Tooltip / description shown to the player.")]
        [TextArea(2, 5)]
        public string description;

        [Header("Mechanics")]
        public AbilityType abilityType;
        public AbilityTarget targetType;

        [Tooltip(
            "Formula evaluated at runtime by AbilitySystem.\n" +
            "Variables: {atk} = caster attack, {matk} = caster magic power,\n" +
            "{def} = target defense, {mdef} = target magic defense.\n" +
            "Examples:\n" +
            "  '{atk} * 1.5'          — physical swing\n" +
            "  '{matk} * 2.0 - {mdef}'— magic burst\n" +
            "  '{matk} * 0.8'         — heal amount (positive = restore HP)\n" +
            "AbilitySystem.EvaluateFormula() handles the math."
        )]
        public string damageFormula;

        [Tooltip("Turns before this ability can be used again. 0 = no cooldown.")]
        [Min(0)]
        public int cooldownTurns;

        [Tooltip("Mana / energy cost. 0 = free.")]
        [Min(0)]
        public int manaCost;

        [Header("Visuals")]
        [Tooltip("Icon displayed on the ability button.")]
        public Sprite icon;

        [Tooltip(
            "Prefab or string key for the VFX to spawn on hit. " +
            "Keep as a GameObject reference; swap to Addressables later."
        )]
        public GameObject vfxPrefab; // placeholder — wire to Addressables when ready

        [Tooltip("Optional sound effect played when ability fires.")]
        public AudioClip sfx;

        // ── Runtime helpers ───────────────────────────────────────────────────

        /// <summary>True if this ability deals damage (used by AbilitySystem).</summary>
        public bool IsDamaging => abilityType == AbilityType.Attack;

        /// <summary>True if this ability restores HP.</summary>
        public bool IsHealing => abilityType == AbilityType.Heal;
    }
}

/*
 * ── MIRA'S 4 ABILITIES — Inspector Reference ─────────────────────────────────
 *
 * Create one AbilityData asset per ability, fill in the fields below,
 * then drag them into Mira's HeroData.abilities[] slots 0–3.
 *
 * ┌──────────────────────────────────────────────────────────────────────────┐
 * │ Slot 0 — "Frost Bolt"  (basic attack)                                    │
 * │   abilityType   : Attack                                                 │
 * │   targetType    : SingleEnemy                                            │
 * │   damageFormula : "{matk} * 1.2 - {mdef}"                               │
 * │   cooldownTurns : 0                                                      │
 * │   manaCost      : 5                                                      │
 * │   description   : "Hurls a shard of ice at a single foe."               │
 * ├──────────────────────────────────────────────────────────────────────────┤
 * │ Slot 1 — "Blizzard"  (AoE attack)                                        │
 * │   abilityType   : Attack                                                 │
 * │   targetType    : AllEnemies                                             │
 * │   damageFormula : "{matk} * 0.9 - {mdef}"                               │
 * │   cooldownTurns : 3                                                      │
 * │   manaCost      : 20                                                     │
 * │   description   : "Calls a blizzard that damages all enemies."          │
 * ├──────────────────────────────────────────────────────────────────────────┤
 * │ Slot 2 — "Ice Shield"  (buff)                                            │
 * │   abilityType   : Buff                                                   │
 * │   targetType    : Self                                                   │
 * │   damageFormula : ""  (no damage; effect handled by status system)      │
 * │   cooldownTurns : 4                                                      │
 * │   manaCost      : 15                                                     │
 * │   description   : "Encases Mira in ice, raising defense for 2 turns."  │
 * ├──────────────────────────────────────────────────────────────────────────┤
 * │ Slot 3 — "Glacial Pulse"  (debuff)                                       │
 * │   abilityType   : Debuff                                                 │
 * │   targetType    : SingleEnemy                                            │
 * │   damageFormula : "{matk} * 0.5 - {mdef}"  (light damage + slow)       │
 * │   cooldownTurns : 2                                                      │
 * │   manaCost      : 12                                                     │
 * │   description   : "Slows an enemy's speed for 2 turns."                │
 * └──────────────────────────────────────────────────────────────────────────┘
 *
 * Adding a 5th, 6th, or 100th ability to the game? Create a new asset.
 * No code changes. Ever.
 * ─────────────────────────────────────────────────────────────────────────────
 */
