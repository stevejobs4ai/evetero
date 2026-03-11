// GameLoopIntegrationTest.cs
// Wave 7 — headless Play Mode integration tests validating the full Evetero gameplay loop.
//
// Builds each scenario entirely in code — no SampleScene dependency.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class GameLoopIntegrationTest
    {
        // ── Reflection helper ─────────────────────────────────────────────────
        // WorldNodeInteractable.OnNodeClicked is a public static event, so it can
        // only be invoked from within its declaring class. We use reflection to
        // fire it from test code without modifying production code.

        private static void FireNodeClicked(WorldNodeData data)
        {
            var field = typeof(WorldNodeInteractable).GetField(
                "OnNodeClicked",
                BindingFlags.Static | BindingFlags.NonPublic);
            var del = field?.GetValue(null) as System.Action<WorldNodeData>;
            del?.Invoke(data);
        }

        // ── Test 1: Full gathering flow via node click ────────────────────────

        [UnityTest]
        public IEnumerator HeroGathersResource_AfterNodeClick()
        {
            // ResourceBank
            var bankGo = new GameObject("Bank");
            var bank   = bankGo.AddComponent<ResourceBank>();

            // Hero — controller, skills, and gathering action on one GameObject
            var heroGo     = new GameObject("Hero");
            var hero       = heroGo.AddComponent<HeroController>();
            var heroSkills = heroGo.AddComponent<HeroSkills>();
            var heroAction = heroGo.AddComponent<GatheringAction>();
            heroAction.gatherIntervalSeconds = 1f; // fast ticks for the test

            // GatheringCoordinator wires click → move → gather
            var coordGo = new GameObject("Coordinator");
            var coord   = coordGo.AddComponent<GatheringCoordinator>();
            coord.hero            = hero;
            coord.heroSkills      = heroSkills;
            coord.gatheringAction = heroAction;
            coord.resourceBank    = bank;

            yield return null; // let Awake / OnEnable / Start run on all objects

            // Build a wood resource node
            var nodeData = ScriptableObject.CreateInstance<WorldNodeData>();
            nodeData.nodeType     = NodeType.ResourceNode;
            nodeData.resourceType = ResourceType.Wood;
            nodeData.yieldPerHour = 60f;

            // Simulate player clicking the node
            FireNodeClicked(nodeData);

            // Hero moves (same position → instant), then gather ticks every 1 s
            yield return new WaitForSeconds(3f);

            Assert.Greater(bank.GetAmount(ResourceType.Wood), 0,
                "ResourceBank should contain wood after a full gather cycle");
            Assert.Greater(heroSkills.GetXP(SkillType.Woodcutting), 0,
                "Hero Woodcutting XP should increase after gathering wood");

            Object.Destroy(bankGo);
            Object.Destroy(heroGo);
            Object.Destroy(coordGo);
            Object.DestroyImmediate(nodeData);
        }

        // ── Test 2: Direct XP gain crosses level threshold ────────────────────

        [UnityTest]
        public IEnumerator HeroGainsXP_OnGather()
        {
            var heroGo     = new GameObject("Hero");
            var heroSkills = heroGo.AddComponent<HeroSkills>();

            yield return null; // Awake initialises _skillXP array

            // Level 2 threshold = 83 XP (OSRS table). 100 XP must land at level 2+.
            heroSkills.GainXP(SkillType.Woodcutting, 100);

            Assert.Greater(heroSkills.GetLevel(SkillType.Woodcutting), 1,
                "100 XP must reach Woodcutting level 2 (OSRS threshold: 83 XP)");

            Object.Destroy(heroGo);
            yield return null;
        }

        // ── Test 3: Save → wipe → load round-trip ────────────────────────────

        [UnityTest]
        public IEnumerator SaveRoundTrip_ResourcesMatch()
        {
            var bankGo    = new GameObject("Bank");
            var bank      = bankGo.AddComponent<ResourceBank>();

            var saveMgrGo = new GameObject("SaveManager");
            var saveMgr   = saveMgrGo.AddComponent<SaveManager>();

            yield return null; // Awake + Start (SaveManager subscribes to bank)

            bank.Deposit(ResourceType.Wood, 50);
            saveMgr.Save();

            // Wipe the bank in-memory so we verify Load actually restores data
            bank.LoadResources(new Dictionary<ResourceType, int>());
            Assert.AreEqual(0, bank.GetAmount(ResourceType.Wood),
                "Bank must be empty after LoadResources with empty dict");

            saveMgr.Load();

            Assert.AreEqual(50, bank.GetAmount(ResourceType.Wood),
                "Load() must restore exactly 50 wood from the saved file");

            // Destroy SaveManager first so it can cleanly unsubscribe from bank
            Object.Destroy(saveMgrGo);
            Object.Destroy(bankGo);
            yield return null;
        }

        // ── Test 4: Direct damage reduces enemy HP ────────────────────────────

        [UnityTest]
        public IEnumerator CombatTrigger_ReducesEnemyHP()
        {
            var enemyGo   = new GameObject("Enemy");
            enemyGo.AddComponent<BoxCollider2D>(); // required by EnemyController
            var enemy     = enemyGo.AddComponent<EnemyController>();
            var enemyData = ScriptableObject.CreateInstance<EnemyData>();
            enemyData.enemyName = "TestEnemy";
            enemyData.maxHP     = 100;
            enemyData.defense   = 0;
            enemy.enemyData     = enemyData;

            yield return null; // Awake: currentHP = enemyData.maxHP = 100

            enemy.TakeDamage(10);

            Assert.AreEqual(90, enemy.currentHP,
                "Enemy HP must be 90 after taking 10 damage from a starting HP of 100");

            Object.Destroy(enemyGo);
            Object.DestroyImmediate(enemyData);
            yield return null;
        }
    }
}
