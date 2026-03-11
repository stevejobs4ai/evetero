// LevelDataSchema.cs
// Serializable data classes that map 1-to-1 to the level JSON files.
// Compatible with Unity's built-in JsonUtility (no Newtonsoft required).
//
// JSON entity types (stored as plain strings for readability):
//   "WorldNode"      – resource / dungeon / town node with WorldNodeInteractable
//   "SpawnPoint"     – hero starting position (invisible marker, no prefab needed)
//   "EnvironmentProp"– static scenery (trees, rocks, ruins …)
//   "Building"       – interactive or decorative structure
//   "BattleTrigger"  – area that loads the battle scene when entered

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    // ── Top-level level container ─────────────────────────────────────────────

    [Serializable]
    public class LevelData
    {
        [Tooltip("Human-readable name shown in UI.")]
        public string levelName;

        [Tooltip("Unique snake_case identifier used to reference this level in code.")]
        public string levelId;

        [Tooltip("Unity scene name to load alongside this level data (if any).")]
        public string sceneName;

        [Tooltip("All entities that make up the level layout.")]
        public List<EntityData> entities = new List<EntityData>();
    }

    // ── Entity type enum ──────────────────────────────────────────────────────

    public enum EntityType
    {
        WorldNode,       // 0
        SpawnPoint,      // 1
        EnvironmentProp, // 2
        Building,        // 3
        BattleTrigger    // 4
    }

    // ── Per-entity data ───────────────────────────────────────────────────────

    [Serializable]
    public class EntityData
    {
        [Tooltip("Unique identifier for this entity within the level.")]
        public string id;

        [Tooltip("Path to a prefab inside any Resources/ folder, without extension. " +
                 "Leave empty to use a runtime placeholder.")]
        public string prefabPath;

        [Tooltip("One of: WorldNode, SpawnPoint, EnvironmentProp, Building, BattleTrigger.")]
        public string entityType;

        public SerializableVector3 position = new SerializableVector3(0, 0, 0);
        public SerializableVector3 rotation = new SerializableVector3(0, 0, 0);
        public SerializableVector3 scale    = new SerializableVector3(1, 1, 1);

        [Tooltip("Arbitrary key-value pairs specific to this entity type. " +
                 "WorldNode keys: nodeName, description, nodeType, resourceType, " +
                 "yieldPerHour, storageCapacity, maxUses, respawnSeconds, " +
                 "recommendedLevel, difficulty. " +
                 "SpawnPoint keys: heroId, heroIndex. " +
                 "BattleTrigger keys: battleSceneName, enemyGroupId, difficulty.")]
        public List<MetadataEntry> metadata = new List<MetadataEntry>();

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Parses the entityType string to the EntityType enum.</summary>
        public EntityType GetEntityType()
        {
            if (Enum.TryParse(entityType, ignoreCase: true, out EntityType et))
                return et;

            Debug.LogWarning($"[LevelData] Unknown entityType '{entityType}' on entity '{id}'. Defaulting to EnvironmentProp.");
            return EntityType.EnvironmentProp;
        }

        /// <summary>Returns the metadata value for key, or defaultValue if not found.</summary>
        public string GetMeta(string key, string defaultValue = "")
        {
            foreach (var entry in metadata)
                if (entry.key == key) return entry.value;
            return defaultValue;
        }

        /// <summary>Returns true and sets value if the key exists.</summary>
        public bool TryGetMeta(string key, out string value)
        {
            foreach (var entry in metadata)
            {
                if (entry.key == key)
                {
                    value = entry.value;
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }
    }

    // ── Metadata key-value pair ───────────────────────────────────────────────

    [Serializable]
    public class MetadataEntry
    {
        public string key;
        public string value;
    }

    // ── Vector3 wrapper (JsonUtility-serializable) ────────────────────────────

    [Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3() { }

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3    ToVector3()    => new Vector3(x, y, z);
        public Quaternion ToQuaternion() => Quaternion.Euler(x, y, z);

        public static SerializableVector3 FromVector3(Vector3 v)
            => new SerializableVector3(v.x, v.y, v.z);

        public static SerializableVector3 FromEuler(Quaternion q)
        {
            var e = q.eulerAngles;
            return new SerializableVector3(e.x, e.y, e.z);
        }

        public override string ToString() => $"({x:F2}, {y:F2}, {z:F2})";
    }
}
