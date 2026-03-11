// CombatManager.cs
// Singleton MonoBehaviour coordinating real-time combat.
//
// Tracks active enemies, drives hero auto-attack coroutines,
// and exposes HeroAttack / HeroUseAbility for external callers.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#pragma warning disable CS0618 // CombatManager is intentionally marked obsolete

namespace Evetero
{
    [Obsolete("CombatManager drives the old real-time system. Use BattleManager for the turn-based battle scene.")]
    public class CombatManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────

        private static CombatManager _instance;
        public static CombatManager Instance => _instance;

        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ── Events ────────────────────────────────────────────────────────────

        public event Action<HeroController, EnemyController> OnCombatStart;
        public event Action<HeroController, EnemyController> OnCombatEnd;

        // ── Tracking ──────────────────────────────────────────────────────────

        private readonly List<EnemyController>                 _activeEnemies      = new();
        private readonly Dictionary<HeroController, Coroutine> _autoAttackRoutines = new();

        /// <summary>Register an enemy so CombatManager can include it in auto-targeting.</summary>
        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy == null || _activeEnemies.Contains(enemy)) return;
            _activeEnemies.Add(enemy);
            enemy.OnEnemyDefeated += () => _activeEnemies.Remove(enemy);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Hero uses ability slot 0 (basic attack) against an enemy.
        /// </summary>
        public void HeroAttack(HeroController hero, EnemyController target)
        {
            HeroUseAbility(hero, 0, target);
        }

        /// <summary>
        /// Hero uses a specific ability slot against an enemy.
        /// Evaluates the ability formula if available; falls back to stat-based damage.
        /// </summary>
        public void HeroUseAbility(HeroController hero, int abilitySlot, EnemyController target)
        {
            if (hero == null || !hero.IsAlive) return;
            if (target == null || target.State == EnemyState.Dead) return;

            int damage = ComputeDamageVsEnemy(hero, abilitySlot, target);
            target.TakeDamage(damage);
        }

        /// <summary>
        /// Begin the hero auto-attack coroutine. Attacks on interval = heroData.baseStats.speed seconds.
        /// </summary>
        public void StartHeroAutoAttack(HeroController hero)
        {
            if (hero == null) return;
            if (_autoAttackRoutines.TryGetValue(hero, out var existing) && existing != null) return;

            var co = StartCoroutine(HeroAutoAttackRoutine(hero));
            _autoAttackRoutines[hero] = co;
        }

        /// <summary>Stop the hero auto-attack loop.</summary>
        public void StopHeroAutoAttack(HeroController hero)
        {
            if (hero == null) return;
            if (_autoAttackRoutines.TryGetValue(hero, out var co) && co != null)
                StopCoroutine(co);
            _autoAttackRoutines[hero] = null;
        }

        // ── Auto-Attack Coroutine ─────────────────────────────────────────────

        private IEnumerator HeroAutoAttackRoutine(HeroController hero)
        {
            while (hero != null && hero.IsAlive)
            {
                float interval = (hero.heroData != null && hero.heroData.baseStats.speed > 0)
                    ? hero.heroData.baseStats.speed
                    : 2f;

                yield return new WaitForSeconds(interval);

                var target = FindNearestActiveEnemy(hero);
                if (target != null)
                    HeroAttack(hero, target);
            }

            _autoAttackRoutines[hero] = null;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int ComputeDamageVsEnemy(HeroController hero, int abilitySlot, EnemyController target)
        {
            if (hero.heroData == null || target.enemyData == null)
                return 1;

            // Attempt ability formula evaluation
            var ability = hero.heroData.GetAbility(abilitySlot);
            if (ability != null && !string.IsNullOrEmpty(ability.damageFormula))
            {
                string formula = ability.damageFormula;
                formula = formula.Replace("{atk}",  hero.heroData.baseStats.attack.ToString());
                formula = formula.Replace("{matk}", hero.heroData.baseStats.magicPower.ToString());
                formula = formula.Replace("{def}",  target.enemyData.defense.ToString());
                formula = formula.Replace("{mdef}", "0");
                formula = Regex.Replace(formula, @"\s+", "");

                try
                {
                    var dt     = new System.Data.DataTable();
                    var result = dt.Compute(formula, null);
                    return Mathf.Max(1, Mathf.RoundToInt(Convert.ToSingle(result)));
                }
                catch { /* fall through to stat-based */ }
            }

            // Fallback: raw attack minus enemy defense
            return Mathf.Max(1, hero.heroData.baseStats.attack - target.enemyData.defense);
        }

        private EnemyController FindNearestActiveEnemy(HeroController hero)
        {
            EnemyController nearest  = null;
            float           minDist  = float.MaxValue;

            foreach (var e in _activeEnemies)
            {
                if (e == null || e.State == EnemyState.Dead) continue;
                float d = Vector3.Distance(hero.transform.position, e.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = e;
                }
            }
            return nearest;
        }
    }
}
