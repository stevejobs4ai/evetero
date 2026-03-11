// HeroSystemTests.cs
// Play Mode tests for HeroData, AbilityData, and HeroController.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class HeroSystemTests
    {
        // ── HeroData ──────────────────────────────────────────────────────────

        [Test]
        public void HeroData_CanBeCreated_WithValidData()
        {
            var hero = ScriptableObject.CreateInstance<HeroData>();
            hero.heroName = "Mira";
            hero.combatType = CombatType.Mage;

            Assert.IsNotNull(hero);
            Assert.AreEqual("Mira", hero.heroName);

            Object.DestroyImmediate(hero);
        }

        [Test]
        public void HeroData_HeroName_IsNotNullOrEmpty()
        {
            var hero = ScriptableObject.CreateInstance<HeroData>();
            hero.heroName = "Gorath";

            Assert.IsFalse(string.IsNullOrEmpty(hero.heroName));

            Object.DestroyImmediate(hero);
        }

        [Test]
        public void HeroData_Abilities_HasExactlyFourSlots()
        {
            var hero = ScriptableObject.CreateInstance<HeroData>();

            Assert.AreEqual(4, hero.abilities.Length);

            Object.DestroyImmediate(hero);
        }

        // ── AbilityData ───────────────────────────────────────────────────────

        [Test]
        public void AbilityData_CanBeCreated_WithValidFields()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.abilityName   = "Frost Bolt";
            ability.abilityType   = AbilityType.Attack;
            ability.targetType    = AbilityTarget.SingleEnemy;
            ability.cooldownTurns = 0;
            ability.manaCost      = 5;

            Assert.IsNotNull(ability);
            Assert.AreEqual("Frost Bolt", ability.abilityName);
            Assert.IsTrue(ability.IsDamaging);
            Assert.IsFalse(ability.IsHealing);

            Object.DestroyImmediate(ability);
        }

        [Test]
        public void AbilityData_HealType_IsHealingTrue()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.abilityName = "Restore";
            ability.abilityType = AbilityType.Heal;

            Assert.IsTrue(ability.IsHealing);
            Assert.IsFalse(ability.IsDamaging);

            Object.DestroyImmediate(ability);
        }

        // ── HeroController ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator HeroController_StartsInIdleState()
        {
            var go = new GameObject("TestHero");
            var controller = go.AddComponent<HeroController>();

            yield return null; // let Start() run

            Assert.AreEqual(HeroState.Idle, controller.State);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator HeroController_AfterMoveToTarget_StateIsMoving()
        {
            var go = new GameObject("TestHero");
            var controller = go.AddComponent<HeroController>();

            yield return null;

            controller.MoveToTarget(new Vector3(100f, 0f, 0f)); // far away — won't arrive this frame

            Assert.AreEqual(HeroState.Moving, controller.State);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator HeroController_AfterStartGathering_StateIsGathering()
        {
            var go = new GameObject("TestHero");
            var controller = go.AddComponent<HeroController>();

            yield return null;

            var nodeData = ScriptableObject.CreateInstance<WorldNodeData>();
            nodeData.nodeName    = "Test Grove";
            nodeData.nodeType    = NodeType.ResourceNode;
            nodeData.resourceType = ResourceType.Wood;
            nodeData.yieldPerHour = 40f;

            controller.StartGathering(nodeData);

            Assert.AreEqual(HeroState.Gathering, controller.State);
            Assert.AreEqual(nodeData, controller.GatheringNode);

            Object.Destroy(go);
            Object.DestroyImmediate(nodeData);
        }

        [UnityTest]
        public IEnumerator HeroController_AfterStopGathering_StateIsIdle()
        {
            var go = new GameObject("TestHero");
            var controller = go.AddComponent<HeroController>();

            yield return null;

            var nodeData = ScriptableObject.CreateInstance<WorldNodeData>();
            nodeData.nodeType     = NodeType.ResourceNode;
            nodeData.yieldPerHour = 10f;

            controller.StartGathering(nodeData);
            controller.StopGathering();

            Assert.AreEqual(HeroState.Idle, controller.State);
            Assert.IsNull(controller.GatheringNode);

            Object.Destroy(go);
            Object.DestroyImmediate(nodeData);
        }
    }
}
