// EmberVillageSetup.cs
// MonoBehaviour that programmatically builds the Ember Village scene layout.
// Attach to the "SceneManager" GameObject in EmberVillageScene.
// All world GameObjects are created at runtime in Start().

using UnityEngine;

namespace Evetero
{
    public class EmberVillageSetup : MonoBehaviour
    {
        [Header("Optional Data References")]
        [Tooltip("MiraIntro dialogue asset — assign in Inspector.")]
        public DialogueData miraIntroDialogue;

        [Tooltip("Goblin Scout enemy data asset — assign in Inspector.")]
        public EnemyData goblinScoutData;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            BuildScene();
        }

        // ── Scene construction ────────────────────────────────────────────────

        private void BuildScene()
        {
            // Village ruins (3 coloured quads)
            CreateRuin("Ruin_BurnedHall",  new Vector3(-4f,  2f, 0f), Color.red);
            CreateRuin("Ruin_StoneWall",   new Vector3( 0f,  2f, 0f), new Color(0.5f, 0.5f, 0.5f));
            CreateRuin("Ruin_CharredPost", new Vector3( 4f,  2f, 0f), new Color(0.55f, 0.27f, 0.07f));

            // Resource nodes
            GameObject timber1 = CreateTimberNode("Ironwood Timber 1", new Vector3(-3f, 0f, 0f));
                                  CreateTimberNode("Ironwood Timber 2", new Vector3( 3f, 0f, 0f));
                                  CreateStoneNode("Stone Outcrop",      new Vector3( 0f,-2f, 0f));

            // Buildings
            CreateBuilding("Workbench", new Vector3(-5f, -3f, 0f), Color.yellow);
            CreateBuilding("Guild Hall", new Vector3( 5f, -3f, 0f), Color.blue);

            // Heroes
            CreateHero("Mira", new Vector3(0f, 0f, 0f));

            // Enemy — GoblinScout blocks timber node 1
            GameObject goblinGO = CreateGoblin("GoblinScout", new Vector3(-3f, 1f, 0f));
            EnemyController goblin = goblinGO.GetComponent<EnemyController>();

            // Combat trigger zone around timber node 1
            CreateCombatTrigger("CombatZone_Timber1", timber1.transform.position, goblin);

            Debug.Log("[EmberVillageSetup] Ember Village scene built.");
        }

        // ── Factory helpers ───────────────────────────────────────────────────

        private GameObject CreateRuin(string ruinName, Vector3 pos, Color color)
        {
            var go = new GameObject(ruinName);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeSprite(48, 32, color);
            sr.sortingOrder = 0;

            return go;
        }

        private GameObject CreateTimberNode(string nodeName, Vector3 pos)
        {
            var go = new GameObject(nodeName);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeSprite(32, 48, new Color(0.13f, 0.55f, 0.13f));
            sr.sortingOrder = 1;

            go.AddComponent<BoxCollider2D>().size = Vector2.one;

            var wni  = go.AddComponent<WorldNodeInteractable>();
            var data = ScriptableObject.CreateInstance<WorldNodeData>();
            data.nodeName        = nodeName;
            data.nodeType        = NodeType.ResourceNode;
            data.resourceType    = ResourceType.Wood;
            data.yieldPerHour    = 40f;
            data.storageCapacity = 400;
            wni.nodeData = data;

            return go;
        }

        private GameObject CreateStoneNode(string nodeName, Vector3 pos)
        {
            var go = new GameObject(nodeName);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeSprite(32, 32, new Color(0.7f, 0.7f, 0.7f));
            sr.sortingOrder = 1;

            go.AddComponent<BoxCollider2D>().size = Vector2.one;

            var wni  = go.AddComponent<WorldNodeInteractable>();
            var data = ScriptableObject.CreateInstance<WorldNodeData>();
            data.nodeName        = nodeName;
            data.nodeType        = NodeType.ResourceNode;
            data.resourceType    = ResourceType.Stone;
            data.yieldPerHour    = 20f;
            data.storageCapacity = 200;
            wni.nodeData = data;

            return go;
        }

        private GameObject CreateBuilding(string buildingName, Vector3 pos, Color color)
        {
            var go = new GameObject(buildingName);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeSprite(40, 40, color);
            sr.sortingOrder = 0;

            return go;
        }

        private GameObject CreateHero(string heroName, Vector3 pos)
        {
            var go = new GameObject(heroName);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeSprite(24, 24, Color.cyan);
            sr.sortingOrder = 2;

            go.AddComponent<BoxCollider2D>();
            go.AddComponent<HeroController>();

            // miraIntroDialogue referenced here — wire in Inspector for full dialogue
            if (miraIntroDialogue != null)
                Debug.Log($"[EmberVillageSetup] Dialogue ready: {miraIntroDialogue.sceneLabel}");

            return go;
        }

        private GameObject CreateGoblin(string goblinName, Vector3 pos)
        {
            var go = new GameObject(goblinName);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeSprite(24, 24, new Color(0.3f, 0.7f, 0.3f));
            sr.sortingOrder = 2;

            go.AddComponent<BoxCollider2D>();

            var enemy = go.AddComponent<EnemyController>();
            if (goblinScoutData != null)
                enemy.enemyData = goblinScoutData;

            return go;
        }

        private void CreateCombatTrigger(string zoneName, Vector3 pos, EnemyController enemy)
        {
            var go = new GameObject(zoneName);
            go.transform.position = pos;

            var col       = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size      = new Vector2(3f, 3f);

            var trigger   = go.AddComponent<CombatTriggerZone>();
            if (enemy != null)
                trigger.enemies = new[] { enemy };
        }

        // ── Sprite utility ────────────────────────────────────────────────────

        /// <summary>Creates a solid-colour sprite at runtime for placeholder visuals.</summary>
        private static Sprite MakeSprite(int w, int h, Color color)
        {
            var tex    = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
