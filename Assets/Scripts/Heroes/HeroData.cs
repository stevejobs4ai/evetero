// HeroData.cs
// ScriptableObject defining a single hero in Evetero.
//
// Usage: Right-click in Project → Create → Evetero → Hero Data
// Create one asset per hero (e.g. Mira.asset, Gorath.asset, etc.)
// Zero code changes needed to add new heroes — just new assets.

using UnityEngine;

namespace Evetero
{
    // ── Enums ────────────────────────────────────────────────────────────────

    /// <summary>
    /// The hero's primary combat role, used by UI and AI targeting logic.
    /// </summary>
    public enum CombatType
    {
        Warrior,    // frontline melee, high defense
        Ranger,     // ranged physical, hit-and-run
        Mage,       // area magical damage
        Healer,     // support / restoration
        Rogue,      // single-target burst, stealth
        Paladin,    // tank + limited healing
        Summoner,   // deploys minions
        Warlock     // debuff + drain mechanics
    }

    // ── Stats ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Base combat stats for a hero at level 1.
    /// Scale these up in your level-progression system.
    /// </summary>
    [System.Serializable]
    public struct HeroBaseStats
    {
        [Tooltip("Maximum hit points at level 1.")]
        public int maxHP;

        [Tooltip("Physical attack power.")]
        public int attack;

        [Tooltip("Magic attack power.")]
        public int magicPower;

        [Tooltip("Physical damage reduction.")]
        public int defense;

        [Tooltip("Magic damage reduction.")]
        public int magicDefense;

        [Tooltip("Determines turn order. Higher = acts sooner.")]
        public int speed;

        [Tooltip("Critical hit chance 0–100.")]
        [Range(0, 100)]
        public int critChance;
    }

    // ── HeroData ScriptableObject ─────────────────────────────────────────────

    /// <summary>
    /// All static data that defines a hero. Lives entirely in data —
    /// no subclassing or code changes required to support all 8 heroes.
    /// </summary>
    [CreateAssetMenu(menuName = "Evetero/Hero Data", fileName = "NewHero")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown in UI.")]
        public string heroName;

        [Tooltip("Short lore blurb shown on the roster screen.")]
        [TextArea(2, 4)]
        public string lore;

        [Tooltip("Portrait sprite used in dialogue, roster, and combat UI.")]
        public Sprite portrait;

        [Tooltip("Full-body art shown on the hero detail screen.")]
        public Sprite fullArt;

        [Header("Combat")]
        [Tooltip("Determines which abilities this hero can learn and how AI uses them.")]
        public CombatType combatType;

        [Tooltip("Exactly 4 abilities. Assign AbilityData assets here.")]
        [Min(0)]
        public AbilityData[] abilities = new AbilityData[4];

        [Header("Base Stats (Level 1)")]
        public HeroBaseStats baseStats;

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the ability in slot 0–3, or null if the slot is empty.
        /// </summary>
        public AbilityData GetAbility(int slot)
        {
            if (slot < 0 || slot >= abilities.Length) return null;
            return abilities[slot];
        }

        /// <summary>
        /// True if every ability slot has been filled.
        /// Useful for validation in editor tooling.
        /// </summary>
        public bool IsFullyConfigured()
        {
            if (string.IsNullOrEmpty(heroName)) return false;
            foreach (var a in abilities)
                if (a == null) return false;
            return true;
        }
    }
}

/*
 * ── HOW TO DEFINE ALL 8 HEROES ───────────────────────────────────────────────
 *
 * 1. Right-click in Project → Create → Evetero → Hero Data
 * 2. Name the asset after the hero (e.g. "Mira")
 * 3. Fill in heroName, lore, portrait, combatType
 * 4. Drag 4 AbilityData assets into the abilities[] array
 * 5. Set baseStats values
 * 6. Repeat for Gorath, Seyla, Drakken, Vel, Taryn, Ossian, Nyx
 *
 * That's it. The runtime reads everything from data.
 * No HeroMira.cs, no HeroGorath.cs — just 8 assets.
 * ─────────────────────────────────────────────────────────────────────────────
 */
