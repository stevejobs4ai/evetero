// LevelExporter.cs
// Editor-only utility: snapshots the currently open scene into a LevelData JSON file.
//
// Usage: Tools → Evetero → Export Level to JSON
//
// What gets captured:
//   • Every WorldNodeInteractable in the scene → EntityType.WorldNode
//   • GameObjects tagged "HeroSpawn"           → EntityType.SpawnPoint
//   • GameObjects tagged "EnvironmentProp"      → EntityType.EnvironmentProp
//   • GameObjects tagged "Building"             → EntityType.Building
//   • Every BattleTriggerZone in the scene      → EntityType.BattleTrigger
//
// Prefab paths are automatically resolved to the Resources/-relative path so the
// exported JSON can be used directly by LevelLoader.

#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evetero
{
    public static class LevelExporter
    {
        private const string DefaultOutputDir = "Assets/Data/Levels";

        // ── Menu entry ────────────────────────────────────────────────────────

        [MenuItem("Tools/Evetero/Export Level to JSON")]
        public static void ExportCurrentScene()
        {
            string defaultFileName = ToSnakeCase(SceneManager.GetActiveScene().name) + ".json";
            string outputPath = EditorUtility.SaveFilePanel(
                title: "Export Level JSON",
                directory: DefaultOutputDir,
                defaultName: defaultFileName,
                extension: "json");

            if (string.IsNullOrEmpty(outputPath))
                return;

            // Ensure output directory exists
            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            LevelData levelData = SnapshotScene();
            string    json      = JsonUtility.ToJson(levelData, prettyPrint: true);

            File.WriteAllText(outputPath, json, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"[LevelExporter] Exported '{levelData.levelName}' " +
                      $"({levelData.entities.Count} entities) → {outputPath}");

            EditorUtility.DisplayDialog(
                "Level Exported",
                $"Saved {levelData.entities.Count} entities to:\n{outputPath}\n\n" +
                "Tip: move the file into a Resources/ subfolder so LevelLoader can read it at runtime.\n" +
                "e.g. Assets/Resources/Data/Levels/",
                "OK");
        }

        // ── Snapshot ──────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a LevelData snapshot of all recognised entities in the active scene.
        /// Call this from other editor scripts if you need the data without the file dialog.
        /// </summary>
        public static LevelData SnapshotScene()
        {
            var scene = SceneManager.GetActiveScene();
            var levelData = new LevelData
            {
                levelName = scene.name,
                levelId   = ToSnakeCase(scene.name),
                sceneName = scene.name,
                entities  = new List<EntityData>()
            };

            // ── WorldNodeInteractables ─────────────────────────────────────────
            var nodes = Object.FindObjectsByType<WorldNodeInteractable>(FindObjectsSortMode.None);
            foreach (var node in nodes)
                levelData.entities.Add(BuildWorldNodeEntity(node));

            // ── Hero spawn points (tagged "HeroSpawn") ─────────────────────────
            foreach (var go in GameObject.FindGameObjectsWithTag("HeroSpawn"))
                levelData.entities.Add(BuildSpawnPointEntity(go));

            // ── Environment props (tagged "EnvironmentProp") ───────────────────
            // Skip objects that are also WorldNodeInteractables (already captured above)
            foreach (var go in GameObject.FindGameObjectsWithTag("EnvironmentProp"))
            {
                if (go.GetComponent<WorldNodeInteractable>() == null)
                    levelData.entities.Add(BuildPropEntity(go, EntityType.EnvironmentProp));
            }

            // ── Buildings (tagged "Building") ──────────────────────────────────
            foreach (var go in GameObject.FindGameObjectsWithTag("Building"))
                levelData.entities.Add(BuildPropEntity(go, EntityType.Building));

            // ── Battle triggers ────────────────────────────────────────────────
            var triggers = Object.FindObjectsByType<BattleTriggerZone>(FindObjectsSortMode.None);
            foreach (var trigger in triggers)
                levelData.entities.Add(BuildBattleTriggerEntity(trigger));

            Debug.Log($"[LevelExporter] Snapshot: {levelData.entities.Count} entities from '{scene.name}'");
            return levelData;
        }

        // ── Entity builders ───────────────────────────────────────────────────

        private static EntityData BuildWorldNodeEntity(WorldNodeInteractable node)
        {
            var data = new EntityData
            {
                id         = MakeId(node.gameObject.name),
                prefabPath = GetResourcesPrefabPath(node.gameObject),
                entityType = EntityType.WorldNode.ToString(),
                position   = SerializableVector3.FromVector3(node.transform.position),
                rotation   = SerializableVector3.FromEuler(node.transform.rotation),
                scale      = SerializableVector3.FromVector3(node.transform.lossyScale),
                metadata   = new List<MetadataEntry>()
            };

            if (node.nodeData != null)
            {
                var nd = node.nodeData;
                data.metadata.Add(KV("nodeName",        nd.nodeName));
                data.metadata.Add(KV("description",     nd.description));
                data.metadata.Add(KV("nodeType",        nd.nodeType.ToString()));
                data.metadata.Add(KV("resourceType",    nd.resourceType.ToString()));
                data.metadata.Add(KV("yieldPerHour",    nd.yieldPerHour.ToString()));
                data.metadata.Add(KV("storageCapacity", nd.storageCapacity.ToString()));
                data.metadata.Add(KV("maxUses",         nd.maxUses.ToString()));
                data.metadata.Add(KV("respawnSeconds",  nd.respawnSeconds.ToString()));
                data.metadata.Add(KV("recommendedLevel",nd.recommendedLevel.ToString()));
                data.metadata.Add(KV("difficulty",      nd.difficulty.ToString()));
            }

            return data;
        }

        private static EntityData BuildSpawnPointEntity(GameObject go)
        {
            return new EntityData
            {
                id         = MakeId(go.name),
                prefabPath = string.Empty,
                entityType = EntityType.SpawnPoint.ToString(),
                position   = SerializableVector3.FromVector3(go.transform.position),
                rotation   = SerializableVector3.FromEuler(go.transform.rotation),
                scale      = new SerializableVector3(1, 1, 1),
                metadata   = new List<MetadataEntry>
                {
                    KV("heroId", go.name)
                }
            };
        }

        private static EntityData BuildPropEntity(GameObject go, EntityType type)
        {
            return new EntityData
            {
                id         = MakeId(go.name),
                prefabPath = GetResourcesPrefabPath(go),
                entityType = type.ToString(),
                position   = SerializableVector3.FromVector3(go.transform.position),
                rotation   = SerializableVector3.FromEuler(go.transform.rotation),
                scale      = SerializableVector3.FromVector3(go.transform.lossyScale),
                metadata   = new List<MetadataEntry>()
            };
        }

        private static EntityData BuildBattleTriggerEntity(BattleTriggerZone trigger)
        {
            var data = new EntityData
            {
                id         = MakeId(trigger.gameObject.name),
                prefabPath = GetResourcesPrefabPath(trigger.gameObject),
                entityType = EntityType.BattleTrigger.ToString(),
                position   = SerializableVector3.FromVector3(trigger.transform.position),
                rotation   = SerializableVector3.FromEuler(trigger.transform.rotation),
                scale      = SerializableVector3.FromVector3(trigger.transform.lossyScale),
                metadata   = new List<MetadataEntry>()
            };

            // Capture the target scene name if the field is accessible
            // (BattleTriggerZone serialises it as overworldSceneName or similar)
            return data;
        }

        // ── Utilities ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the Resources/-relative path for a scene object's source prefab.
        /// Returns empty string if the object is not a prefab instance or is not
        /// under a Resources/ folder.
        /// </summary>
        private static string GetResourcesPrefabPath(GameObject go)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (source == null) return string.Empty;

            string assetPath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(assetPath)) return string.Empty;

            // Strip everything up to and including the Resources/ folder
            const string resourcesFolder = "/Resources/";
            int idx = assetPath.IndexOf(resourcesFolder, System.StringComparison.Ordinal);
            if (idx < 0) return string.Empty;  // not in a Resources/ folder

            string relativePath = assetPath.Substring(idx + resourcesFolder.Length);

            // Remove .prefab extension
            if (relativePath.EndsWith(".prefab"))
                relativePath = relativePath.Substring(0, relativePath.Length - 7);

            return relativePath;
        }

        private static MetadataEntry KV(string key, string value)
            => new MetadataEntry { key = key, value = value };

        private static string MakeId(string name)
        {
            var clean = name
                .Replace("(", "").Replace(")", "")
                .Replace(" ", "_").Trim('_');
            return ToSnakeCase(clean);
        }

        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return "entity";
            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c) && i > 0 && name[i - 1] != '_')
                    sb.Append('_');
                sb.Append(char.ToLower(c));
            }
            return sb.ToString();
        }
    }
}

#endif
