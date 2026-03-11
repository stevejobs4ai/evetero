// WorldNodeTests.cs
// Play Mode tests for WorldNodeData and WorldNodeInteractable.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class WorldNodeTests
    {
        // ── WorldNodeData ─────────────────────────────────────────────────────

        [Test]
        public void WorldNodeData_CanBeCreated()
        {
            var node = ScriptableObject.CreateInstance<WorldNodeData>();
            node.nodeName     = "Iron Vein";
            node.nodeType     = NodeType.ResourceNode;
            node.resourceType = ResourceType.Iron;

            Assert.IsNotNull(node);
            Assert.AreEqual("Iron Vein", node.nodeName);

            Object.DestroyImmediate(node);
        }

        [Test]
        public void WorldNodeData_GetAccumulatedYield_ReturnsZero_WhenNoTimePassed()
        {
            var node = ScriptableObject.CreateInstance<WorldNodeData>();
            node.nodeType      = NodeType.ResourceNode;
            node.resourceType  = ResourceType.Gold;
            node.yieldPerHour  = 100f;
            node.storageCapacity = 2000;

            // Pass UtcNow — effectively zero elapsed time
            int yield = node.GetAccumulatedYield(System.DateTime.UtcNow);

            Assert.AreEqual(0, yield);

            Object.DestroyImmediate(node);
        }

        [Test]
        public void WorldNodeData_NonProductiveNode_YieldsZero()
        {
            var node = ScriptableObject.CreateInstance<WorldNodeData>();
            node.nodeType     = NodeType.Dungeon;
            node.yieldPerHour = 0f;

            // Even an hour ago, a dungeon should yield nothing
            int yield = node.GetAccumulatedYield(System.DateTime.UtcNow.AddHours(-1));

            Assert.AreEqual(0, yield);

            Object.DestroyImmediate(node);
        }

        // ── WorldNodeInteractable ─────────────────────────────────────────────

        [UnityTest]
        public IEnumerator WorldNodeInteractable_GameObject_HasBoxCollider2D()
        {
            var go = new GameObject("TreeNode");
            go.AddComponent<BoxCollider2D>();
            var interactable = go.AddComponent<WorldNodeInteractable>();

            var nodeData = ScriptableObject.CreateInstance<WorldNodeData>();
            nodeData.nodeName     = "Ancient Grove";
            nodeData.nodeType     = NodeType.ResourceNode;
            nodeData.resourceType = ResourceType.Wood;
            interactable.nodeData = nodeData;

            yield return null;

            Assert.IsNotNull(go.GetComponent<Collider2D>());

            Object.Destroy(go);
            Object.DestroyImmediate(nodeData);
        }

        [UnityTest]
        public IEnumerator WorldNodeInteractable_Tooltip_IsNotEmpty()
        {
            var go = new GameObject("BankNode");
            go.AddComponent<BoxCollider2D>();
            var interactable = go.AddComponent<WorldNodeInteractable>();

            var nodeData = ScriptableObject.CreateInstance<WorldNodeData>();
            nodeData.nodeName     = "Royal Mint";
            nodeData.nodeType     = NodeType.ResourceNode;
            nodeData.resourceType = ResourceType.Gold;
            interactable.nodeData = nodeData;

            yield return null;

            Assert.IsFalse(string.IsNullOrEmpty(interactable.Tooltip));
            StringAssert.Contains("Royal Mint", interactable.Tooltip);

            Object.Destroy(go);
            Object.DestroyImmediate(nodeData);
        }

        [UnityTest]
        public IEnumerator WorldNodeInteractable_HasHeroAssigned_IsFalseByDefault()
        {
            var go = new GameObject("EmptyNode");
            go.AddComponent<BoxCollider2D>();
            var interactable = go.AddComponent<WorldNodeInteractable>();

            var nodeData = ScriptableObject.CreateInstance<WorldNodeData>();
            nodeData.nodeName = "Empty Grove";
            interactable.nodeData = nodeData;

            yield return null;

            Assert.IsFalse(interactable.HasHeroAssigned);

            Object.Destroy(go);
            Object.DestroyImmediate(nodeData);
        }

        [UnityTest]
        public IEnumerator WorldNodeInteractable_AssignHero_SetsHasHeroAssigned()
        {
            var nodeGo = new GameObject("NodeObj");
            nodeGo.AddComponent<BoxCollider2D>();
            var interactable = nodeGo.AddComponent<WorldNodeInteractable>();

            var nodeData = ScriptableObject.CreateInstance<WorldNodeData>();
            nodeData.nodeName     = "Test Node";
            nodeData.nodeType     = NodeType.ResourceNode;
            nodeData.yieldPerHour = 10f;
            interactable.nodeData = nodeData;

            var heroGo     = new GameObject("HeroObj");
            var controller = heroGo.AddComponent<HeroController>();

            yield return null;

            interactable.AssignHero(controller);

            Assert.IsTrue(interactable.HasHeroAssigned);

            Object.Destroy(nodeGo);
            Object.Destroy(heroGo);
            Object.DestroyImmediate(nodeData);
        }
    }
}
