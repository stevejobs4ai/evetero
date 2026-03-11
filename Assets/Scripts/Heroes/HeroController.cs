// HeroController.cs
// MonoBehaviour that drives a hero entity on the world map.
// Uses a simple lerp for movement (no NavMesh dependency).
//
// States: Idle -> Moving -> Gathering -> Idle

using System.Collections;
using UnityEngine;

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
        [Tooltip("Units per second when lerp-moving to a target.")]
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

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (heroData == null) return;
            currentHP        = heroData.baseStats.maxHP;
            currentMana      = 100;
            abilityCooldowns = new int[heroData.abilities != null ? heroData.abilities.Length : 4];
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Begin moving toward a world position. State becomes Moving.
        /// When the hero arrives, state returns to Idle (unless overridden).
        /// </summary>
        public void MoveToTarget(Vector3 position)
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _state = HeroState.Moving;
            _moveCoroutine = StartCoroutine(MoveRoutine(position));
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
            _gatheringNode = node;
            _state         = HeroState.Gathering;
        }

        /// <summary>
        /// Stop gathering and return to Idle.
        /// </summary>
        public void StopGathering()
        {
            _gatheringNode = null;
            _state         = HeroState.Idle;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private IEnumerator MoveRoutine(Vector3 target)
        {
            target.z = transform.position.z; // keep Z for 2D

            while (Vector3.Distance(transform.position, target) > 0.05f)
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
