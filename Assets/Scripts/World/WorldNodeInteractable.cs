// WorldNodeInteractable.cs
// MonoBehaviour attached to a world-map tile GameObject.
// Requires a Collider2D so OnMouseDown fires properly.
//
// Responsibilities:
//   - Broadcast click events with the node's data
//   - Track hero assignment
//   - Provide a tooltip string

using System;
using UnityEngine;

namespace Evetero
{
    [RequireComponent(typeof(Collider2D))]
    public class WorldNodeInteractable : MonoBehaviour
    {
        [Header("Data")]
        public WorldNodeData nodeData;

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired when the player clicks / taps this node.</summary>
        public static event Action<WorldNodeData> OnNodeClicked;

        // ── Hero assignment ───────────────────────────────────────────────────

        private HeroController _assignedHero;

        /// <summary>True if a hero has been assigned to this node.</summary>
        public bool HasHeroAssigned => _assignedHero != null;

        /// <summary>Assign a hero to gather from this node.</summary>
        public void AssignHero(HeroController hero)
        {
            _assignedHero = hero;
        }

        /// <summary>Remove the currently assigned hero.</summary>
        public void UnassignHero()
        {
            _assignedHero = null;
        }

        // ── Tooltip ───────────────────────────────────────────────────────────

        /// <summary>
        /// A human-readable tooltip combining the node name and resource type.
        /// Shown in the tile info panel when the player taps a node.
        /// </summary>
        public string Tooltip
        {
            get
            {
                if (nodeData == null) return string.Empty;
                return nodeData.resourceType == ResourceType.None
                    ? nodeData.nodeName
                    : $"{nodeData.nodeName} ({nodeData.resourceType})";
            }
        }

        // ── Unity messages ────────────────────────────────────────────────────

        private void OnMouseDown()
        {
            OnNodeClicked?.Invoke(nodeData);
        }
    }
}
