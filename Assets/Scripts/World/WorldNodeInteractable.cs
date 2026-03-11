// WorldNodeInteractable.cs
// MonoBehaviour attached to a world-map tile GameObject.
// Requires a Collider2D so OnMouseDown fires properly.
//
// Responsibilities:
//   - Broadcast click events with the node's data
//   - Track hero assignment
//   - Track gather uses and depletion state
//   - Drive visual changes (grayed renderer + optional depleted GameObject)
//   - Run the respawn countdown and re-enable when ready

using System;
using System.Collections;
using UnityEngine;

namespace Evetero
{
    [RequireComponent(typeof(Collider2D))]
    public class WorldNodeInteractable : MonoBehaviour
    {
        [Header("Data")]
        public WorldNodeData nodeData;

        [Header("Depletion Visuals")]
        [Tooltip("Optional GameObject shown only while the node is depleted (e.g. a 'dry stump' mesh).")]
        public GameObject depletedVisual;

        [Tooltip("SpriteRenderer whose color is grayed out while depleted. Auto-found on this GameObject if left empty.")]
        public SpriteRenderer nodeRenderer;

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired when the player clicks / taps this node.</summary>
        public static event Action<WorldNodeData> OnNodeClicked;

        /// <summary>Fired when the node becomes depleted (passes the node data).</summary>
        public static event Action<WorldNodeData> OnNodeDepleted;

        /// <summary>Fired when the node respawns and is gatherable again.</summary>
        public static event Action<WorldNodeData> OnNodeRespawned;

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

        // ── Depletion state ───────────────────────────────────────────────────

        private int _currentUses;
        private Coroutine _respawnCoroutine;

        /// <summary>How many times this node has been gathered since last respawn.</summary>
        public int CurrentUses => _currentUses;

        /// <summary>True when the node has reached its maxUses and cannot be gathered.</summary>
        public bool IsDepleted { get; private set; }

        /// <summary>
        /// Call once per successful gather tick. Increments use count and
        /// triggers depletion when maxUses is reached (0 = infinite).
        /// </summary>
        public void RegisterGather()
        {
            if (IsDepleted || nodeData == null) return;

            _currentUses++;

            bool infiniteUses = nodeData.maxUses == 0;
            if (!infiniteUses && _currentUses >= nodeData.maxUses)
                Deplete();
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
                if (IsDepleted)
                {
                    float remaining = nodeData.respawnSeconds > 0
                        ? nodeData.respawnSeconds   // approximation; exact countdown lives in coroutine
                        : 0f;
                    return $"{nodeData.nodeName} (Depleted)";
                }
                return nodeData.resourceType == ResourceType.None
                    ? nodeData.nodeName
                    : $"{nodeData.nodeName} ({nodeData.resourceType})";
            }
        }

        // ── Unity messages ────────────────────────────────────────────────────

        private void Awake()
        {
            if (nodeRenderer == null)
                nodeRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            ApplyVisualState();
        }

        private void OnMouseDown()
        {
            if (!IsDepleted)
                OnNodeClicked?.Invoke(nodeData);
        }

        // ── Internal depletion / respawn ──────────────────────────────────────

        private void Deplete()
        {
            IsDepleted = true;
            ApplyVisualState();
            OnNodeDepleted?.Invoke(nodeData);

            if (nodeData.respawnSeconds > 0f)
                _respawnCoroutine = StartCoroutine(RespawnCountdown(nodeData.respawnSeconds));
        }

        private IEnumerator RespawnCountdown(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Respawn();
        }

        private void Respawn()
        {
            _currentUses = 0;
            IsDepleted = false;
            _respawnCoroutine = null;
            ApplyVisualState();
            OnNodeRespawned?.Invoke(nodeData);
        }

        private void ApplyVisualState()
        {
            // Toggle optional depleted mesh/visual
            if (depletedVisual != null)
                depletedVisual.SetActive(IsDepleted);

            // Gray out / restore sprite color
            if (nodeRenderer != null)
                nodeRenderer.color = IsDepleted ? new Color(0.45f, 0.45f, 0.45f, 1f) : Color.white;
        }
    }
}
