// LevelLoader.cs
// Reads a LevelData JSON file from a Resources/ folder and spawns all entities.
//
// Setup:
//   1. Place your JSON file inside any Resources/ subfolder
//      e.g. Assets/Resources/Data/Levels/ember_village.json
//   2. Attach this component to a persistent scene GameObject
//   3. Set 'Level Data Path' to the path relative to Resources/ (no extension)
//      e.g. "Data/Levels/ember_village"
//   4. Press Play — all entities are spawned automatically on Start.
//
// Alternatively call LoadLevel(path) at runtime to switch levels dynamically.

using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    public class LevelLoader : MonoBehaviour
    {
        [Header("Level")]
        [Tooltip("Path to the JSON file inside any Resources/ folder, without extension. " +
                 "Example: 'Data/Levels/ember_village'")]
        [SerializeField] private string levelDataPath = "Data/Levels/ember_village";

        [Tooltip("Parent transform for all spawned entities. Defaults to this transform if null.")]
        [SerializeField] private Transform entityRoot;

        // ── State ─────────────────────────────────────────────────────────────

        private LevelData _levelData;
        private readonly List<GameObject> _spawnedObjects  = new List<GameObject>();
        private readonly List<Transform>  _heroSpawnPoints = new List<Transform>();

        /// <summary>The LevelData that is currently loaded, or null.</summary>
        public LevelData LoadedLevel => _levelData;

        /// <summary>
        /// Hero spawn Transforms in the order they appear in the JSON.
        /// Assign heroes to these positions after loading.
        /// </summary>
        public IReadOnlyList<Transform> HeroSpawnPoints => _heroSpawnPoints;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Awake()
        {
            if (entityRoot == null)
                entityRoot = transform;
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(levelDataPath))
                LoadLevel(levelDataPath);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Load and spawn a level from a JSON file inside Resources/.
        /// Destroys any previously loaded level first.
        /// </summary>
        /// <param name="resourcePath">
        /// Path relative to any Resources/ folder, without extension.
        /// e.g. "Data/Levels/ember_village"
        /// </param>
        public void LoadLevel(string resourcePath)
        {
            ClearLevel();

            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"[LevelLoader] Cannot find level JSON at Resources/{resourcePath}. " +
                               "Ensure the file is inside a Resources/ folder.");
                return;
            }

            _levelData = JsonUtility.FromJson<LevelData>(textAsset.text);
            if (_levelData == null)
            {
                Debug.LogError($"[LevelLoader] Failed to parse JSON at Resources/{resourcePath}");
                return;
            }

            int count = _levelData.entities?.Count ?? 0;
            Debug.Log($"[LevelLoader] Loading level '{_levelData.levelName}' ({count} entities)");
            SpawnEntities();
        }

        /// <summary>Destroy all previously spawned entities and clear internal state.</summary>
        public void ClearLevel()
        {
            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            _spawnedObjects.Clear();
            _heroSpawnPoints.Clear();
            _levelData = null;
        }

        // ── Spawning ──────────────────────────────────────────────────────────

        private void SpawnEntities()
        {
            if (_levelData.entities == null) return;

            foreach (var entity in _levelData.entities)
            {
                switch (entity.GetEntityType())
                {
                    case EntityType.WorldNode:      SpawnWorldNode(entity);      break;
                    case EntityType.SpawnPoint:     SpawnHeroSpawnPoint(entity); break;
                    case EntityType.EnvironmentProp:
                    case EntityType.Building:       SpawnProp(entity);           break;
                    case EntityType.BattleTrigger:  SpawnBattleTrigger(entity);  break;
                }
            }
        }

        // ── Entity spawners ───────────────────────────────────────────────────

        private void SpawnWorldNode(EntityData data)
        {
            var go = InstantiatePrefabOrPlaceholder(data);
            if (go == null) return;

            // Ensure WorldNodeInteractable is present
            var interactable = go.GetComponent<WorldNodeInteractable>();
            if (interactable == null)
                interactable = go.AddComponent<WorldNodeInteractable>();

            // Collider2D is required by WorldNodeInteractable for click detection
            if (go.GetComponent<Collider2D>() == null)
                go.AddComponent<BoxCollider2D>();

            // Build and assign WorldNodeData from metadata
            interactable.nodeData = BuildWorldNodeData(data);

            string displayName = interactable.nodeData.nodeName;
            go.name = displayName.Length > 0 ? displayName : data.id;

            _spawnedObjects.Add(go);
        }

        /// <summary>Constructs a runtime WorldNodeData from entity metadata.</summary>
        private static WorldNodeData BuildWorldNodeData(EntityData data)
        {
            var nd = ScriptableObject.CreateInstance<WorldNodeData>();

            nd.nodeName    = data.GetMeta("nodeName",    data.id);
            nd.description = data.GetMeta("description", string.Empty);

            if (System.Enum.TryParse(data.GetMeta("nodeType", "ResourceNode"), true, out NodeType nodeType))
                nd.nodeType = nodeType;

            if (System.Enum.TryParse(data.GetMeta("resourceType", "None"), true, out ResourceType resourceType))
                nd.resourceType = resourceType;

            if (float.TryParse(data.GetMeta("yieldPerHour",    "30"),  out float yph))  nd.yieldPerHour    = yph;
            if (int.TryParse  (data.GetMeta("storageCapacity", "200"), out int   cap))  nd.storageCapacity = cap;
            if (int.TryParse  (data.GetMeta("maxUses",         "5"),   out int   mu))   nd.maxUses         = mu;
            if (float.TryParse(data.GetMeta("respawnSeconds",  "60"),  out float rs))   nd.respawnSeconds  = rs;
            if (int.TryParse  (data.GetMeta("recommendedLevel","1"),   out int   rl))   nd.recommendedLevel = rl;
            if (int.TryParse  (data.GetMeta("difficulty",      "1"),   out int   diff)) nd.difficulty      = diff;

            return nd;
        }

        private void SpawnHeroSpawnPoint(EntityData data)
        {
            // Spawn points are invisible transform markers — no prefab or renderer needed.
            var go = new GameObject($"HeroSpawn_{data.id}");
            go.transform.SetParent(entityRoot, worldPositionStays: false);
            ApplyTransform(go.transform, data);

            _heroSpawnPoints.Add(go.transform);
            _spawnedObjects.Add(go);
        }

        private void SpawnProp(EntityData data)
        {
            var go = InstantiatePrefabOrPlaceholder(data);
            if (go != null)
                _spawnedObjects.Add(go);
        }

        private void SpawnBattleTrigger(EntityData data)
        {
            var go = InstantiatePrefabOrPlaceholder(data);
            if (go == null) return;

            // Log metadata-specified scene override; BattleTriggerZone reads its
            // own serialised field — pass data through the prefab's inspector values.
            if (data.TryGetMeta("battleSceneName", out string sceneName))
                Debug.Log($"[LevelLoader] BattleTrigger '{data.id}' → scene '{sceneName}'");

            if (data.TryGetMeta("enemyGroupId", out string groupId))
                Debug.Log($"[LevelLoader] BattleTrigger '{data.id}' → enemyGroup '{groupId}'");

            _spawnedObjects.Add(go);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Loads a prefab via Resources.Load and instantiates it.
        /// Falls back to a coloured quad placeholder when the prefab path is
        /// empty or the asset cannot be found.
        /// </summary>
        private GameObject InstantiatePrefabOrPlaceholder(EntityData data)
        {
            GameObject go = null;

            if (!string.IsNullOrEmpty(data.prefabPath))
            {
                var prefab = Resources.Load<GameObject>(data.prefabPath);
                if (prefab != null)
                {
                    go = Instantiate(prefab);
                }
                else
                {
                    Debug.LogWarning($"[LevelLoader] Prefab not found at Resources/{data.prefabPath} " +
                                     $"— spawning placeholder for '{data.id}'");
                }
            }

            if (go == null)
                go = CreatePlaceholder(data.GetEntityType());

            go.name = data.id;
            go.transform.SetParent(entityRoot, worldPositionStays: false);
            ApplyTransform(go.transform, data);
            return go;
        }

        private static GameObject CreatePlaceholder(EntityType type)
        {
            var go       = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var rend     = go.GetComponent<Renderer>();
            if (rend != null)
            {
                // Use Sprites/Default so it renders correctly in 2D scenes
                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.color       = PlaceholderColor(type);
                rend.sharedMaterial = mat;
            }
            return go;
        }

        private static Color PlaceholderColor(EntityType type) => type switch
        {
            EntityType.WorldNode       => new Color(0.20f, 0.70f, 0.20f), // green
            EntityType.SpawnPoint      => new Color(0.20f, 0.50f, 0.90f), // blue
            EntityType.EnvironmentProp => new Color(0.40f, 0.28f, 0.14f), // brown
            EntityType.Building        => new Color(0.70f, 0.62f, 0.42f), // tan
            EntityType.BattleTrigger   => new Color(0.80f, 0.12f, 0.12f), // red
            _                          => Color.grey
        };

        private static void ApplyTransform(Transform t, EntityData data)
        {
            t.localPosition = data.position.ToVector3();
            t.localRotation = data.rotation.ToQuaternion();
            t.localScale    = data.scale.ToVector3();
        }
    }
}
