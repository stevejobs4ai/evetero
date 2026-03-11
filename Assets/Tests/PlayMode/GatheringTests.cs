// GatheringTests.cs
// Play Mode tests for GatheringAction coroutine state and ResourceBank operations.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class GatheringTests
    {
        // ── GatheringAction.IsGathering ────────────────────────────────────────

        [UnityTest]
        public IEnumerator GatheringAction_IsGathering_FalseBeforeStart()
        {
            var go     = new GameObject("Gatherer");
            var action = go.AddComponent<GatheringAction>();

            yield return null;

            Assert.IsFalse(action.IsGathering);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator GatheringAction_IsGathering_TrueAfterStartGathering()
        {
            var heroGo = new GameObject("Hero");
            var skills = heroGo.AddComponent<HeroSkills>();

            var go     = new GameObject("Gatherer");
            var action = go.AddComponent<GatheringAction>();

            var node = ScriptableObject.CreateInstance<WorldNodeData>();
            node.nodeType     = NodeType.ResourceNode;
            node.resourceType = ResourceType.Wood;
            node.yieldPerHour = 10f;

            yield return null;

            action.StartGathering(node, skills);

            Assert.IsTrue(action.IsGathering);

            Object.Destroy(heroGo);
            Object.Destroy(go);
            Object.DestroyImmediate(node);
        }

        [UnityTest]
        public IEnumerator GatheringAction_StopGathering_SetsIsGatheringFalse()
        {
            var heroGo = new GameObject("Hero");
            var skills = heroGo.AddComponent<HeroSkills>();

            var go     = new GameObject("Gatherer");
            var action = go.AddComponent<GatheringAction>();

            var node = ScriptableObject.CreateInstance<WorldNodeData>();
            node.nodeType     = NodeType.ResourceNode;
            node.resourceType = ResourceType.Iron;

            yield return null;

            action.StartGathering(node, skills);
            action.StopGathering();

            Assert.IsFalse(action.IsGathering);

            Object.Destroy(heroGo);
            Object.Destroy(go);
            Object.DestroyImmediate(node);
        }

        [UnityTest]
        public IEnumerator GatheringAction_StopGathering_BeforeStart_DoesNotThrow()
        {
            var go     = new GameObject("Gatherer2");
            var action = go.AddComponent<GatheringAction>();

            yield return null;

            Assert.DoesNotThrow(() => action.StopGathering());

            Object.Destroy(go);
        }

        // ── ResourceBank.Deposit ───────────────────────────────────────────────

        [Test]
        public void ResourceBank_Deposit_AddsToBalance()
        {
            var go   = new GameObject("Bank");
            var bank = go.AddComponent<ResourceBank>();

            bank.Deposit(ResourceType.Wood, 50);

            Assert.AreEqual(50, bank.GetAmount(ResourceType.Wood));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResourceBank_Deposit_AccumulatesMultipleCalls()
        {
            var go   = new GameObject("Bank");
            var bank = go.AddComponent<ResourceBank>();

            bank.Deposit(ResourceType.Stone, 30);
            bank.Deposit(ResourceType.Stone, 20);

            Assert.AreEqual(50, bank.GetAmount(ResourceType.Stone));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResourceBank_Deposit_CapsAtMaxCapacity()
        {
            var go   = new GameObject("Bank");
            var bank = go.AddComponent<ResourceBank>();

            bank.Deposit(ResourceType.Gold, ResourceBank.MaxCapacity + 1000);

            Assert.AreEqual(ResourceBank.MaxCapacity, bank.GetAmount(ResourceType.Gold));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResourceBank_GetAmount_ReturnsZero_ForUntouchedResource()
        {
            var go   = new GameObject("Bank");
            var bank = go.AddComponent<ResourceBank>();

            Assert.AreEqual(0, bank.GetAmount(ResourceType.Mana));

            Object.DestroyImmediate(go);
        }

        // ── ResourceBank.Spend ─────────────────────────────────────────────────

        [Test]
        public void ResourceBank_Spend_ReturnsFalse_WhenInsufficient()
        {
            var go   = new GameObject("Bank");
            var bank = go.AddComponent<ResourceBank>();

            bank.Deposit(ResourceType.Wood, 10);
            bool result = bank.Spend(ResourceType.Wood, 50);

            Assert.IsFalse(result);
            Assert.AreEqual(10, bank.GetAmount(ResourceType.Wood)); // unchanged

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResourceBank_Spend_ReturnsTrue_WhenSufficient()
        {
            var go   = new GameObject("Bank");
            var bank = go.AddComponent<ResourceBank>();

            bank.Deposit(ResourceType.Iron, 100);
            bool result = bank.Spend(ResourceType.Iron, 40);

            Assert.IsTrue(result);
            Assert.AreEqual(60, bank.GetAmount(ResourceType.Iron));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ResourceBank_Spend_ReturnsFalse_WhenBalanceIsZero()
        {
            var go   = new GameObject("Bank");
            var bank = go.AddComponent<ResourceBank>();

            bool result = bank.Spend(ResourceType.Food, 1);

            Assert.IsFalse(result);

            Object.DestroyImmediate(go);
        }
    }
}
