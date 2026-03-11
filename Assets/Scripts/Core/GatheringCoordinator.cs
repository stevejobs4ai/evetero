// GatheringCoordinator.cs
// Wires WorldNodeInteractable click events to the hero's move-then-gather loop.
//
// Setup in scene:
//   1. Add this component to any persistent GameObject (e.g. "Coordinator").
//   2. Assign hero, heroSkills, gatheringAction (all on Hero_Mira).
//   3. Assign resourceBank (on the "Bank" GameObject).
//
// Flow: NodeClicked → hero moves to tile → wait for arrival → StartGathering.
// Each gathered resource is automatically deposited into the ResourceBank.

using System.Collections;
using UnityEngine;

namespace Evetero
{
    public class GatheringCoordinator : MonoBehaviour
    {
        [Header("Hero References")]
        public HeroController  hero;
        public HeroSkills      heroSkills;
        public GatheringAction gatheringAction;

        [Header("Economy")]
        public ResourceBank resourceBank;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            WorldNodeInteractable.OnNodeClicked += HandleNodeClicked;
        }

        private void OnDisable()
        {
            WorldNodeInteractable.OnNodeClicked -= HandleNodeClicked;
        }

        private void Start()
        {
            // Wire gathered resources straight into the bank once at startup.
            if (gatheringAction != null && resourceBank != null)
                gatheringAction.OnResourceGathered += (type, amount) =>
                    resourceBank.Deposit(type, amount);
        }

        // ── Event handler ─────────────────────────────────────────────────────

        private void HandleNodeClicked(WorldNodeData nodeData)
        {
            if (nodeData == null || nodeData.nodeType != NodeType.ResourceNode) return;
            if (hero == null) return;

            Vector3 targetPos = FindNodePosition(nodeData);
            StartCoroutine(MoveAndGather(nodeData, targetPos));
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private IEnumerator MoveAndGather(WorldNodeData nodeData, Vector3 targetPos)
        {
            gatheringAction?.StopGathering();
            hero.MoveToTarget(targetPos);

            // Wait until the hero finishes moving.
            yield return new WaitUntil(() => hero.State != HeroState.Moving);

            gatheringAction?.StartGathering(nodeData, heroSkills);
        }

        /// <summary>
        /// Finds the scene position of the WorldNodeInteractable that owns
        /// <paramref name="nodeData"/>. Falls back to the hero's position.
        /// </summary>
        private Vector3 FindNodePosition(WorldNodeData nodeData)
        {
            var nodes = Object.FindObjectsByType<WorldNodeInteractable>(FindObjectsSortMode.None);
            foreach (var node in nodes)
            {
                if (node.nodeData == nodeData)
                    return node.transform.position;
            }

            return hero != null ? hero.transform.position : Vector3.zero;
        }
    }
}
