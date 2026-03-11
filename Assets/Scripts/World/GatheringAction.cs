// GatheringAction.cs
// MonoBehaviour that runs the timed gathering coroutine for one hero.
// Attach to the same GameObject as HeroController.
//
// Each tick the hero:
//   1. Collects 1 unit of the node's resource (fires OnResourceGathered)
//   2. Gains skill XP proportional to the resource type
//
// XP per gather:  Woodcutting = 25, Mining = 35, Fishing = 20, default = 10

using System;
using System.Collections;
using UnityEngine;

namespace Evetero
{
    public class GatheringAction : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("Seconds between each gather tick.")]
        public float gatherIntervalSeconds = 3f;

        [Header("Runtime (set by StartGathering)")]
        public HeroSkills           heroSkills;
        public WorldNodeData        targetNode;
        public WorldNodeInteractable targetNodeInteractable;

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired each tick: the resource type collected and the amount (always 1).</summary>
        public event Action<ResourceType, int> OnResourceGathered;

        // ── State ─────────────────────────────────────────────────────────────

        private Coroutine _gatherCoroutine;

        /// <summary>True while a gather coroutine is running.</summary>
        public bool IsGathering => _gatherCoroutine != null;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Begin gathering from <paramref name="node"/> using <paramref name="skills"/> for XP.</summary>
        public void StartGathering(WorldNodeData node, HeroSkills skills,
                                   WorldNodeInteractable nodeInteractable = null)
        {
            StopGathering();
            targetNode             = node;
            heroSkills             = skills;
            targetNodeInteractable = nodeInteractable;
            _gatherCoroutine       = StartCoroutine(GatherLoop());
        }

        /// <summary>Stop the gather coroutine immediately.</summary>
        public void StopGathering()
        {
            if (_gatherCoroutine != null)
            {
                StopCoroutine(_gatherCoroutine);
                _gatherCoroutine = null;
            }
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private IEnumerator GatherLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(gatherIntervalSeconds);

                if (targetNode == null) break;

                // Stop if the node has become depleted (e.g. another hero exhausted it)
                if (targetNodeInteractable != null && targetNodeInteractable.IsDepleted) break;

                // Award 1 resource unit
                OnResourceGathered?.Invoke(targetNode.resourceType, 1);

                // Register the gather with the node (may trigger depletion)
                targetNodeInteractable?.RegisterGather();

                // Award skill XP
                SkillType skill = ResourceToSkill(targetNode.resourceType);
                int       xp    = XPForResource(targetNode.resourceType);
                heroSkills?.GainXP(skill, xp);

                // If the gather just depleted the node, stop looping
                if (targetNodeInteractable != null && targetNodeInteractable.IsDepleted) break;
            }

            _gatherCoroutine = null;
        }

        private static SkillType ResourceToSkill(ResourceType resource) =>
            resource switch
            {
                ResourceType.Wood  => SkillType.Woodcutting,
                ResourceType.Stone => SkillType.Mining,
                ResourceType.Iron  => SkillType.Mining,
                ResourceType.Food  => SkillType.Fishing,
                ResourceType.Gold  => SkillType.Crafting,
                ResourceType.Mana  => SkillType.Crafting,
                _                  => SkillType.Woodcutting,
            };

        private static int XPForResource(ResourceType resource) =>
            resource switch
            {
                ResourceType.Wood  => 25,
                ResourceType.Stone => 35,
                ResourceType.Iron  => 35,
                ResourceType.Food  => 20,
                _                  => 10,
            };
    }
}
