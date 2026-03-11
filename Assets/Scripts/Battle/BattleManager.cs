// BattleManager.cs
// Singleton MonoBehaviour driving turn-based combat.
//
// Turn order: all combatants sorted by speed (descending).
//   Ties broken by team (Hero first), then name alphabetically.
// Each turn a unit may Attack, use an Ability, Defend, Flee, or Talk.
// Enemy turns are resolved automatically by a simple AI.
// Battle ends when all heroes or all enemies (including recruits) are defeated.
// Victory awards XP from all defeated enemies to surviving heroes (logged).

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Evetero
{
    // ── Enums ────────────────────────────────────────────────────────────────

    public enum BattlePhase   { NotStarted, InProgress, Victory, Defeat, Fled }
    public enum BattleActionType { Attack, Ability, Defend, Flee, Talk }
    public enum BattleResult  { Victory, Defeat, Fled }

    // ── Action descriptor ────────────────────────────────────────────────────

    public struct BattleAction
    {
        public BattleActionType Type;
        public int              AbilitySlot; // 1–3 for Ability type
        public BattleUnit       Target;
    }

    // ── BattleManager ────────────────────────────────────────────────────────

    public class BattleManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────

        private static BattleManager _instance;
        public  static BattleManager Instance => _instance;

        private void Awake()
        {
            if (_instance == null) _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired when it becomes <paramref name="unit"/>'s turn.</summary>
        public event Action<BattleUnit>       OnTurnStarted;

        /// <summary>Fired after a unit takes damage. Args: unit, amount.</summary>
        public event Action<BattleUnit, int>  OnUnitDamaged;

        /// <summary>Fired when a unit's HP hits 0.</summary>
        public event Action<BattleUnit>       OnUnitDefeated;

        /// <summary>Fired when a recruitable enemy joins the hero side.</summary>
        public event Action<BattleUnit>       OnUnitRecruited;

        /// <summary>Fired when the battle resolves.</summary>
        public event Action<BattleResult>     OnBattleEnded;

        /// <summary>Fired each time something noteworthy happens (for the battle log).</summary>
        public event Action<string>           OnBattleLog;

        // ── State ─────────────────────────────────────────────────────────────

        public BattlePhase Phase { get; private set; } = BattlePhase.NotStarted;

        private List<BattleUnit> _turnQueue = new List<BattleUnit>();
        private int              _turnIndex;

        /// <summary>The unit whose turn it currently is.</summary>
        public BattleUnit CurrentUnit =>
            (_turnQueue.Count > 0 && _turnIndex < _turnQueue.Count)
                ? _turnQueue[_turnIndex]
                : null;

        /// <summary>Ordered preview of the next <paramref name="count"/> turns.</summary>
        public List<BattleUnit> GetTurnPreview(int count = 5)
        {
            var preview = new List<BattleUnit>();
            int idx = _turnIndex;
            for (int i = 0; i < count; i++)
            {
                // Skip dead units in preview
                int attempts = 0;
                while (attempts < _turnQueue.Count &&
                       (_turnQueue[idx % _turnQueue.Count] == null ||
                        !_turnQueue[idx % _turnQueue.Count].IsAlive))
                {
                    idx++;
                    attempts++;
                }
                if (attempts >= _turnQueue.Count) break;
                preview.Add(_turnQueue[idx % _turnQueue.Count]);
                idx++;
            }
            return preview;
        }

        // ── Setup ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Kick off a battle with the provided hero and enemy unit lists.
        /// </summary>
        public void StartBattle(List<BattleUnit> heroes, List<BattleUnit> enemies)
        {
            if (Phase == BattlePhase.InProgress)
            {
                Debug.LogWarning("[BattleManager] StartBattle called while already in progress.");
                return;
            }

            _turnQueue = BuildTurnQueue(heroes, enemies);
            _turnIndex = 0;
            Phase      = BattlePhase.InProgress;

            Log($"Battle started — {heroes.Count} heroes vs {enemies.Count} enemies.");
            BeginTurn();
        }

        // ── Turn queue construction (public for tests) ────────────────────────

        /// <summary>
        /// Returns a combined, sorted turn queue for the given combatant lists.
        /// Sorted by speed descending; ties: Hero team first, then name A→Z.
        /// </summary>
        public static List<BattleUnit> BuildTurnQueue(
            List<BattleUnit> heroes, List<BattleUnit> enemies)
        {
            var all = new List<BattleUnit>();
            if (heroes  != null) all.AddRange(heroes);
            if (enemies != null) all.AddRange(enemies);

            all.Sort((a, b) =>
            {
                int cmp = b.Speed.CompareTo(a.Speed);
                if (cmp != 0) return cmp;
                // Tie: heroes before enemies
                cmp = a.Team.CompareTo(b.Team);
                if (cmp != 0) return cmp;
                // Tie: alphabetical by name
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            return all;
        }

        // ── Hero action submission ────────────────────────────────────────────

        /// <summary>
        /// Called by BattleUI when the player selects an action for the current hero.
        /// Ignored if it is not currently a hero's turn.
        /// </summary>
        public void SubmitAction(BattleAction action)
        {
            if (Phase != BattlePhase.InProgress) return;
            var unit = CurrentUnit;
            if (unit == null || unit.Team != BattleTeam.Hero) return;

            ResolveHeroAction(unit, action);
        }

        // ── Action resolution ─────────────────────────────────────────────────

        private void ResolveHeroAction(BattleUnit unit, BattleAction action)
        {
            switch (action.Type)
            {
                case BattleActionType.Attack:
                    if (action.Target != null)
                        ResolveAttack(unit, action.Target);
                    break;

                case BattleActionType.Ability:
                    if (action.Target != null)
                        ResolveAbility(unit, action.AbilitySlot, action.Target);
                    break;

                case BattleActionType.Defend:
                    ResolveDefend(unit);
                    break;

                case BattleActionType.Flee:
                    if (ResolveFlee())
                        return; // battle ended — don't advance turn
                    break;

                case BattleActionType.Talk:
                    if (action.Target != null)
                        ResolveTalk(unit, action.Target);
                    break;
            }

            EndCurrentTurn();
        }

        // ── Attack ────────────────────────────────────────────────────────────

        public void ResolveAttack(BattleUnit attacker, BattleUnit target)
        {
            if (attacker == null || !attacker.IsAlive) return;
            if (target == null || !target.IsAlive) return;

            int raw    = Mathf.Max(1, attacker.Attack - target.Defense);
            int damage = target.IsDefending ? Mathf.Max(1, raw / 2) : raw;

            target.TakeDamage(damage);
            Log($"{attacker.Name} attacks {target.Name} for {damage} damage.");
            OnUnitDamaged?.Invoke(target, damage);

            if (!target.IsAlive)
                HandleUnitDefeated(target);
        }

        // ── Ability ───────────────────────────────────────────────────────────

        public void ResolveAbility(BattleUnit user, int slot, BattleUnit target)
        {
            if (user?.HeroData == null) return;
            if (slot < 0 || slot >= user.HeroData.abilities.Length) return;

            var ability = user.HeroData.GetAbility(slot);
            if (ability == null)
            {
                Log($"{user.Name}: ability slot {slot} is empty.");
                return;
            }
            if (user.AbilityCooldowns[slot] > 0)
            {
                Log($"{user.Name}: {ability.abilityName} on cooldown ({user.AbilityCooldowns[slot]} turns).");
                return;
            }
            if (user.CurrentMana < ability.manaCost)
            {
                Log($"{user.Name}: not enough mana for {ability.abilityName}.");
                return;
            }

            user.SpendMana(ability.manaCost);
            user.AbilityCooldowns[slot] = ability.cooldownTurns;

            int value = 0;
            if (!string.IsNullOrEmpty(ability.damageFormula))
                value = EvaluateAbilityFormula(ability.damageFormula, user, target);

            switch (ability.abilityType)
            {
                case AbilityType.Attack:
                {
                    int damage = target.IsDefending ? Mathf.Max(1, value / 2) : value;
                    target.TakeDamage(damage);
                    Log($"{user.Name} uses {ability.abilityName} on {target.Name} for {damage} damage.");
                    OnUnitDamaged?.Invoke(target, damage);
                    if (!target.IsAlive) HandleUnitDefeated(target);
                    break;
                }
                case AbilityType.Heal:
                    target.RestoreHP(value);
                    Log($"{user.Name} uses {ability.abilityName} — restores {value} HP to {target.Name}.");
                    break;
                case AbilityType.Buff:
                    Log($"{user.Name} uses {ability.abilityName} (buff) on {target.Name}.");
                    break;
                case AbilityType.Debuff:
                    if (value > 0)
                    {
                        target.TakeDamage(value);
                        OnUnitDamaged?.Invoke(target, value);
                        if (!target.IsAlive) HandleUnitDefeated(target);
                    }
                    Log($"{user.Name} uses {ability.abilityName} (debuff) on {target.Name}.");
                    break;
            }
        }

        // ── Defend ────────────────────────────────────────────────────────────

        public void ResolveDefend(BattleUnit unit)
        {
            unit.IsDefending = true;
            Log($"{unit.Name} takes a defensive stance — incoming damage halved.");
        }

        // ── Flee ──────────────────────────────────────────────────────────────

        public bool ResolveFlee()
        {
            Phase = BattlePhase.Fled;
            Log("The heroes fled the battle!");
            OnBattleEnded?.Invoke(BattleResult.Fled);
            return true;
        }

        // ── Talk / Recruitment ────────────────────────────────────────────────

        public bool ResolveTalk(BattleUnit hero, BattleUnit target)
        {
            if (target.RecruitableData == null)
            {
                Log($"{target.Name} cannot be talked to.");
                return false;
            }

            var condition = target.RecruitableData.requiredHero;
            if (condition != null && hero.HeroData != condition)
            {
                Log($"{target.Name} will only listen to {condition.heroName}.");
                return false;
            }

            // Recruitment succeeds
            target.SetRecruited();
            Log($"{target.Name} has joined the heroes!");

            if (target.RecruitableData.recruitDialogue != null)
                Debug.Log($"[BattleManager] Trigger dialogue: {target.RecruitableData.recruitDialogue.sceneLabel}");

            if (target.RecruitableData.joinHeroData != null)
                BattleContext.RecruitedHeroes.Add(target.RecruitableData.joinHeroData);

            OnUnitRecruited?.Invoke(target);
            return true;
        }

        // ── Enemy AI ─────────────────────────────────────────────────────────

        [Tooltip("Seconds to wait before the enemy takes their turn (visual pause).")]
        public float enemyTurnDelay = 0.8f;

        private IEnumerator ProcessEnemyTurn(BattleUnit enemy)
        {
            yield return new WaitForSeconds(enemyTurnDelay);

            if (!enemy.IsAlive || Phase != BattlePhase.InProgress)
            {
                EndCurrentTurn();
                yield break;
            }

            // Simple AI: pick a random alive hero and attack
            var targets = _turnQueue
                .Where(u => u.Team == BattleTeam.Hero && u.IsAlive)
                .ToList();

            if (targets.Count > 0)
            {
                var target = targets[UnityEngine.Random.Range(0, targets.Count)];
                ResolveAttack(enemy, target);
            }

            EndCurrentTurn();
        }

        // ── Turn management ───────────────────────────────────────────────────

        private void BeginTurn()
        {
            if (Phase != BattlePhase.InProgress) return;

            // Advance past dead units
            int attempts = 0;
            while (attempts < _turnQueue.Count &&
                   (_turnQueue[_turnIndex] == null || !_turnQueue[_turnIndex].IsAlive))
            {
                _turnIndex = (_turnIndex + 1) % _turnQueue.Count;
                attempts++;
            }

            if (attempts >= _turnQueue.Count)
            {
                // All units dead — shouldn't happen, but guard anyway
                EndBattle();
                return;
            }

            var unit = CurrentUnit;
            unit.IsDefending = false; // clear defend from previous round

            Log($"--- {unit.Name}'s turn ({unit.Team}, {unit.CurrentHP}/{unit.MaxHP} HP) ---");
            OnTurnStarted?.Invoke(unit);

            if (unit.Team == BattleTeam.Enemy)
                StartCoroutine(ProcessEnemyTurn(unit));
            // Hero turns wait for SubmitAction() from BattleUI
        }

        public void EndCurrentTurn()
        {
            if (Phase != BattlePhase.InProgress) return;

            var unit = CurrentUnit;
            if (unit != null) unit.TickCooldowns();

            _turnIndex = (_turnIndex + 1) % _turnQueue.Count;

            if (CheckBattleOver())
                return;

            BeginTurn();
        }

        // ── Battle-end logic ──────────────────────────────────────────────────

        private bool CheckBattleOver()
        {
            bool heroesAlive  = _turnQueue.Any(u => u.Team == BattleTeam.Hero  && u.IsAlive);
            bool enemiesAlive = _turnQueue.Any(u => u.Team == BattleTeam.Enemy && u.IsAlive && !u.IsRecruited);

            if (!heroesAlive)
            {
                Phase = BattlePhase.Defeat;
                Log("Defeat — all heroes have fallen.");
                OnBattleEnded?.Invoke(BattleResult.Defeat);
                return true;
            }
            if (!enemiesAlive)
            {
                Phase = BattlePhase.Victory;
                AwardXP();
                Log("Victory!");
                OnBattleEnded?.Invoke(BattleResult.Victory);
                return true;
            }
            return false;
        }

        private void EndBattle()
        {
            Phase = BattlePhase.Defeat;
            OnBattleEnded?.Invoke(BattleResult.Defeat);
        }

        private void HandleUnitDefeated(BattleUnit unit)
        {
            Log($"{unit.Name} has been defeated!");
            OnUnitDefeated?.Invoke(unit);
        }

        // ── XP reward ─────────────────────────────────────────────────────────

        private void AwardXP()
        {
            int totalXP = _turnQueue
                .Where(u => u.Team == BattleTeam.Enemy && !u.IsAlive && u.EnemyData != null)
                .Sum(u => u.EnemyData.xpReward);

            if (totalXP <= 0) return;

            var survivors = _turnQueue.Where(u => u.Team == BattleTeam.Hero && u.IsAlive).ToList();
            foreach (var hero in survivors)
                Log($"{hero.Name} receives {totalXP} XP.");

            Debug.Log($"[BattleManager] +{totalXP} XP awarded to {survivors.Count} surviving heroes.");
        }

        // ── Formula evaluation ────────────────────────────────────────────────

        private static int EvaluateAbilityFormula(string formula, BattleUnit caster, BattleUnit target)
        {
            formula = formula.Replace("{atk}",   caster.Attack.ToString());
            formula = formula.Replace("{matk}",  caster.MagicPower.ToString());
            formula = formula.Replace("{def}",   target?.Defense.ToString() ?? "0");
            formula = formula.Replace("{mdef}",  target?.MagicDefense.ToString() ?? "0");
            formula = Regex.Replace(formula, @"\s+", "");

            try
            {
                var dt     = new System.Data.DataTable();
                var result = dt.Compute(formula, null);
                return Mathf.Max(0, Mathf.RoundToInt(Convert.ToSingle(result)));
            }
            catch (Exception e)
            {
                Debug.LogError($"[BattleManager] Formula error: {e.Message} | '{formula}'");
                return 0;
            }
        }

        // ── Logging ───────────────────────────────────────────────────────────

        private void Log(string message)
        {
            Debug.Log($"[BattleManager] {message}");
            OnBattleLog?.Invoke(message);
        }
    }
}
