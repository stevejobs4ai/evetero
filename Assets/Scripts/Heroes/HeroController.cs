// HeroController.cs
// MonoBehaviour that drives a hero entity on the world map.
// Supports NavMesh pathfinding via NavMeshAgent; falls back to lerp if the
// agent is absent or the hero is not placed on a baked NavMesh surface.
//
// ── NavMesh setup (editor) ────────────────────────────────────────────────
//   1. Add the NavMesh Components package (Window > Package Manager > "AI
//      Navigation" — built-in since Unity 6).
//   2. Add a NavMeshSurface component to your world-map GameObject (or a
//      dedicated empty "NavMeshBaker" object at root level).
//   3. For a 2D game on the XY plane set the NavMeshSurface "Use Geometry"
//      to "Render Meshes" and tick "Override Voxel Size" to match tile size.
//      Set agent type to "Humanoid" (or a custom 2D agent with height ~0.1).
//   4. Click "Bake" in the NavMeshSurface inspector.
//   5. Add a NavMeshAgent component to the hero prefab. Set:
//        Speed        ← leave at 0 (overridden by HeroController.moveSpeed)
//        Base Offset  ← 0 (2D)
//        updateRotation / updateUpAxis ← both disabled automatically in code
// ─────────────────────────────────────────────────────────────────────────
//
// States: Idle -> Moving -> Gathering -> Idle

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Evetero
{
    public enum HeroState
    {
        Idle,
        Moving,
        Gathering
    }

    public class HeroController : MonoBehaviour
    {
        [Header("Data")]
        public HeroData heroData;

        // Alias for AbilitySystem compatibility
        public HeroData data => heroData;

        [Header("Movement")]
        [Tooltip("Units per second — used by both NavMeshAgent and the lerp fallback.")]
        public float moveSpeed = 5f;

        // ── Combat state ──────────────────────────────────────────────────────

        public int currentHP;
        public int currentMana;
        public int[] abilityCooldowns;

        public bool IsAlive => currentHP > 0;

        public void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(0, currentHP - amount);
            Debug.Log($"[HeroController] {heroData?.heroName} takes {amount} dmg → {currentHP} HP");
        }

        public void RestoreHP(int amount)
        {
            if (heroData == null) return;
            currentHP = Mathf.Min(heroData.baseStats.maxHP, currentHP + amount);
        }

        // ── State ─────────────────────────────────────────────────────────────

        private HeroState      _state = HeroState.Idle;
        private WorldNodeData  _gatheringNode;
        private Coroutine      _moveCoroutine;

        /// <summary>Current activity state of this hero.</summary>
        public HeroState State => _state;

        /// <summary>The node being gathered from (null when not gathering).</summary>
        public WorldNodeData GatheringNode => _gatheringNode;

        // ── NavMesh ───────────────────────────────────────────────────────────

        private NavMeshAgent _agent;

        // Distance at which the hero is considered to have arrived.
        private const float ArrivalThreshold = 0.15f;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (heroData != null)
            {
                currentHP        = heroData.baseStats.maxHP;
                currentMana      = 100;
                abilityCooldowns = new int[heroData.abilities != null ? heroData.abilities.Length : 4];
            }

            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed          = moveSpeed;
                _agent.stoppingDistance = ArrivalThreshold * 0.5f;
                // 2D: prevent the agent from rotating the sprite or adjusting Z.
                _agent.updateRotation = false;
                _agent.updateUpAxis   = false;
            }
        }

        private void Update()
        {
            // Poll NavMeshAgent for arrival when it is steering the hero.
            if (_state == HeroState.Moving && _agent != null && _agent.isOnNavMesh)
            {
                if (!_agent.pathPending &&
                    _agent.remainingDistance <= ArrivalThreshold)
                {
                    _agent.ResetPath();
                    _state = HeroState.Idle;
                }
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Begin moving toward a world position via NavMesh (or lerp fallback).
        /// State becomes Moving; transitions back to Idle on arrival.
        /// </summary>
        public void MoveToTarget(Vector3 position)
        {
            // Cancel any in-progress lerp coroutine.
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            _state = HeroState.Moving;

            if (_agent != null && _agent.isOnNavMesh)
            {
                // NavMesh path — arrival is detected in Update().
                _agent.isStopped = false;
                _agent.SetDestination(position);
            }
            else
            {
                // Lerp fallback: no baked NavMesh or agent not on surface.
                _moveCoroutine = StartCoroutine(MoveRoutine(position));
            }
        }

        /// <summary>
        /// Begin gathering from a world node. State becomes Gathering.
        /// </summary>
        public void StartGathering(WorldNodeData node)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            // Halt any NavMesh movement so the hero stays put while gathering.
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            _gatheringNode = node;
            _state         = HeroState.Gathering;
        }

        /// <summary>
        /// Stop gathering and return to Idle.
        /// </summary>
        public void StopGathering()
        {
            if (_agent != null && _agent.isOnNavMesh)
                _agent.isStopped = false;

            _gatheringNode = null;
            _state         = HeroState.Idle;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        /// <summary>
        /// Lerp-based movement used when NavMeshAgent is unavailable.
        /// Keeps Z fixed for 2D scenes.
        /// </summary>
        private IEnumerator MoveRoutine(Vector3 target)
        {
            target.z = transform.position.z; // keep Z for 2D

            while (Vector3.Distance(transform.position, target) > ArrivalThreshold)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
            _moveCoroutine     = null;
            _state             = HeroState.Idle;
        }
    }
}
