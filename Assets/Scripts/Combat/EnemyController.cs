// EnemyController.cs
// MonoBehaviour driving a single enemy entity.
// States: Idle → Chasing → Attacking → Dead
//
// Aggro: detects HeroController within aggroRange, chases, then attacks on interval.

using System;
using System.Collections;
using UnityEngine;

namespace Evetero
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Data")]
        public EnemyData enemyData;

        [Header("Ranges")]
        [Tooltip("Distance at which the enemy spots the hero.")]
        public float aggroRange = 5f;

        [Tooltip("Distance at which the enemy stops and attacks.")]
        public float attackRange = 1.5f;

        // ── State ─────────────────────────────────────────────────────────────

        public EnemyState State { get; private set; } = EnemyState.Idle;

        public int currentHP { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired when the enemy dies.</summary>
        public event Action OnEnemyDefeated;

        /// <summary>Fired with the damage amount each time the enemy is hit.</summary>
        public event Action<int> OnDamageTaken;

        // ── Internal ──────────────────────────────────────────────────────────

        private HeroController _target;
        private Coroutine      _attackCoroutine;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (enemyData != null)
                currentHP = enemyData.maxHP;
        }

        private void Update()
        {
            if (State == EnemyState.Dead || enemyData == null) return;

            if (_target == null || !_target.IsAlive)
                _target = FindHeroInRange(aggroRange);

            if (_target == null)
            {
                TransitionTo(EnemyState.Idle);
                return;
            }

            float dist = Vector3.Distance(transform.position, _target.transform.position);

            if (dist <= attackRange)
            {
                if (State != EnemyState.Attacking)
                {
                    TransitionTo(EnemyState.Attacking);
                    _attackCoroutine = StartCoroutine(AttackRoutine());
                }
            }
            else if (dist <= aggroRange)
            {
                if (State == EnemyState.Attacking)
                    StopAttackCoroutine();

                TransitionTo(EnemyState.Chasing);
                Chase();
            }
            else
            {
                _target = null;
                StopAttackCoroutine();
                TransitionTo(EnemyState.Idle);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (State == EnemyState.Dead) return;

            currentHP = Mathf.Max(0, currentHP - amount);
            Debug.Log($"[EnemyController] {enemyData?.enemyName} takes {amount} dmg → {currentHP} HP");

            OnDamageTaken?.Invoke(amount);

            if (currentHP <= 0)
                Die();
        }

        public void Die()
        {
            if (State == EnemyState.Dead) return;

            TransitionTo(EnemyState.Dead);
            StopAttackCoroutine();

            Debug.Log($"[EnemyController] {enemyData?.enemyName} defeated!");
            if (enemyData != null)
                Debug.Log($"[EnemyController] +{enemyData.xpReward} XP rewarded.");

            OnEnemyDefeated?.Invoke();

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void Chase()
        {
            if (_target == null || enemyData == null) return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _target.transform.position,
                enemyData.moveSpeed * Time.deltaTime);
        }

        private IEnumerator AttackRoutine()
        {
            while (State == EnemyState.Attacking && _target != null && _target.IsAlive)
            {
                int heroDefense = _target.heroData != null ? _target.heroData.baseStats.defense : 0;
                int dmg = Mathf.Max(1, enemyData.attackPower - heroDefense);

                _target.TakeDamage(dmg);
                Debug.Log($"[EnemyController] {enemyData.enemyName} attacks for {dmg}");

                float interval = Mathf.Max(0.1f, enemyData.attackIntervalSeconds);
                yield return new WaitForSeconds(interval);
            }
            _attackCoroutine = null;
        }

        private void StopAttackCoroutine()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }

        private void TransitionTo(EnemyState next)
        {
            State = next;
        }

        private HeroController FindHeroInRange(float range)
        {
            var heroes = FindObjectsByType<HeroController>(FindObjectsSortMode.None);
            HeroController nearest = null;
            float nearestDist = range;

            foreach (var hero in heroes)
            {
                if (!hero.IsAlive) continue;
                float d = Vector3.Distance(transform.position, hero.transform.position);
                if (d <= nearestDist)
                {
                    nearestDist = d;
                    nearest = hero;
                }
            }
            return nearest;
        }
    }
}
