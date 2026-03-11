// WorldNodeData.cs
// ScriptableObject defining a single node on the Evetero world map.
//
// Usage: Right-click in Project → Create → Evetero → World Node Data
// Create one asset per tile type (not per tile instance).
// The world map reads these assets to populate tile behavior and visuals.

using UnityEngine;

namespace Evetero
{
    // ── Enums ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Broad category driving what a player can do on this tile.
    /// </summary>
    public enum NodeType
    {
        ResourceNode,   // produces resources over time (wood, gold, stone, food…)
        Town,           // recruit heroes, buy items, rest
        Dungeon,        // combat encounter, boss fight, loot
        AllianceBase    // alliance-owned building, cooperative mechanics
    }

    /// <summary>
    /// The resource a ResourceNode produces.
    /// Extend freely — no code outside WorldNodeData needs to change.
    /// </summary>
    public enum ResourceType
    {
        None,       // non-resource nodes (towns, dungeons)
        Wood,
        Gold,
        Stone,
        Food,
        Mana,       // magical resource for advanced abilities
        Iron
    }

    // ── WorldNodeData ScriptableObject ───────────────────────────────────────

    /// <summary>
    /// All static data for one type of world map node.
    /// Tile instances on the map reference one of these assets to know
    /// what they are and what they produce.
    /// </summary>
    [CreateAssetMenu(menuName = "Evetero/World Node Data", fileName = "NewWorldNode")]
    public class WorldNodeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown when the player taps the tile.")]
        public string nodeName;

        [Tooltip("Short description shown in the tile info panel.")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Type of node — drives available player interactions.")]
        public NodeType nodeType;

        [Header("Visuals")]
        [Tooltip("Sprite shown on the world map for this tile.")]
        public Sprite tileSprite;

        [Tooltip("Optional animated VFX prefab overlaid on the tile (e.g. smoke for a dungeon).")]
        public GameObject tileVFXPrefab;

        [Header("Resource Production")]
        [Tooltip("Only relevant when nodeType == ResourceNode.")]
        public ResourceType resourceType;

        [Tooltip("Units of the resource generated per real-time hour. 0 = non-productive node.")]
        [Min(0)]
        public float yieldPerHour;

        [Tooltip("Maximum stored resources before production pauses. Player must collect.")]
        [Min(0)]
        public int storageCapacity;

        [Header("Depletion & Respawn")]
        [Tooltip("Number of gather actions before this node is depleted. 0 = infinite uses.")]
        [Min(0)]
        public int maxUses = 5;

        [Tooltip("Seconds before a depleted node respawns and becomes gatherable again. 0 = never respawns.")]
        [Min(0)]
        public float respawnSeconds = 60f;

        [Header("Encounter (Dungeons / Hostile Nodes)")]
        [Tooltip("Minimum player level recommended to attempt this node.")]
        [Min(1)]
        public int recommendedLevel = 1;

        [Tooltip("Relative difficulty 1–5. Used by UI to show skull icons.")]
        [Range(1, 5)]
        public int difficulty = 1;

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>True if this node produces resources passively.</summary>
        public bool IsProductive => nodeType == NodeType.ResourceNode && yieldPerHour > 0;

        /// <summary>True if this node has combat encounters.</summary>
        public bool IsCombatNode => nodeType == NodeType.Dungeon;

        /// <summary>
        /// How much resource has accumulated since lastCollectTime.
        /// Call this from the tile instance with its last-collect timestamp.
        /// </summary>
        public int GetAccumulatedYield(System.DateTime lastCollectTime)
        {
            if (!IsProductive) return 0;
            double hoursElapsed = (System.DateTime.UtcNow - lastCollectTime).TotalHours;
            int raw = Mathf.FloorToInt((float)(hoursElapsed * yieldPerHour));
            return Mathf.Min(raw, storageCapacity);
        }
    }
}

/*
 * ── SAMPLE TILE DEFINITIONS — Inspector Reference ────────────────────────────
 *
 * ┌──────────────────────────────────────────────────────────────────────────┐
 * │ Asset: "Node_TreeTile"                                                   │
 * │   nodeName        : "Ancient Ironwood Grove"                             │
 * │   description     : "Old-growth forest. Harvesters work slower here,    │
 * │                       but the timber fetches a premium."                 │
 * │   nodeType        : ResourceNode                                         │
 * │   resourceType    : Wood                                                 │
 * │   yieldPerHour    : 40                                                   │
 * │   storageCapacity : 400                                                  │
 * │   recommendedLevel: 1                                                    │
 * │   difficulty      : 1                                                    │
 * ├──────────────────────────────────────────────────────────────────────────┤
 * │ Asset: "Node_BankTile"                                                   │
 * │   nodeName        : "Royal Mint"                                         │
 * │   description     : "A heavily guarded vault. Controlling this tile      │
 * │                       earns passive gold income for your alliance."      │
 * │   nodeType        : ResourceNode                                         │
 * │   resourceType    : Gold                                                 │
 * │   yieldPerHour    : 100                                                  │
 * │   storageCapacity : 2000                                                 │
 * │   recommendedLevel: 5                                                    │
 * │   difficulty      : 3                                                    │
 * ├──────────────────────────────────────────────────────────────────────────┤
 * │ Asset: "Node_DungeonTile"                                                │
 * │   nodeName        : "The Sunken Crypt"                                   │
 * │   description     : "Ruins of a pre-Shattering necropolis. Dangerous,   │
 * │                       but rumoured to hide ancient relics."              │
 * │   nodeType        : Dungeon                                              │
 * │   resourceType    : None                                                 │
 * │   yieldPerHour    : 0                                                    │
 * │   storageCapacity : 0                                                    │
 * │   recommendedLevel: 8                                                    │
 * │   difficulty      : 4                                                    │
 * └──────────────────────────────────────────────────────────────────────────┘
 *
 * The world map instantiates tile GameObjects and assigns one WorldNodeData
 * asset per tile. All behavior flows from the data. New tile type?
 * New asset. No new class.
 * ─────────────────────────────────────────────────────────────────────────────
 */
