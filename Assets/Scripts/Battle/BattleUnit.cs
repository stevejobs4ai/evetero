// BattleUnit.cs
// Pure-C# runtime wrapper for a single combatant in turn-based battle.
// Created from HeroData or EnemyData via static factory methods.
// No MonoBehaviour dependency — fully unit-testable.

using System;
using UnityEngine;

namespace Evetero
{
    public enum BattleTeam { Hero, Enemy }

    /// <summary>
    /// Holds mutable combat state (HP, mana, cooldowns, defending flag) for
    /// one combatant.  Source data (HeroData / EnemyData) is kept read-only.
    /// </summary>
    public class BattleUnit
    {
        // ── Identity ──────────────────────────────────────────────────────────

        public string    Name  { get; }
        public BattleTeam Team { get; set; }
        public int       Speed { get; }

        // ── HP / Mana ─────────────────────────────────────────────────────────

        public int MaxHP       { get; }
        public int CurrentHP   { get; private set; }
        public int MaxMana     { get; }
        public int CurrentMana { get; private set; }

        // ── Combat stats ──────────────────────────────────────────────────────

        public int Attack      { get; }
        public int Defense     { get; }
        public int MagicPower  { get; }
        public int MagicDefense { get; }
        public int CritChance  { get; }

        // ── Turn state ────────────────────────────────────────────────────────

        public bool IsDefending       { get; set; }
        public int[] AbilityCooldowns { get; }

        // ── Source data ───────────────────────────────────────────────────────

        public HeroData             HeroData        { get; }   // null for enemies
        public EnemyData            EnemyData       { get; }   // null for heroes
        public RecruitableEnemyData RecruitableData { get; }   // null if not recruitable
        public bool                 IsRecruited     { get; private set; }

        public bool IsAlive => CurrentHP > 0;

        // ── Constructor ───────────────────────────────────────────────────────

        private BattleUnit(
            string name, BattleTeam team,
            int maxHP, int maxMana,
            int attack, int defense, int magicPower, int magicDefense,
            int speed, int critChance, int abilitySlots,
            HeroData heroData, EnemyData enemyData, RecruitableEnemyData recruitableData)
        {
            Name             = name;
            Team             = team;
            MaxHP            = maxHP;
            CurrentHP        = maxHP;
            MaxMana          = maxMana;
            CurrentMana      = maxMana;
            Attack           = attack;
            Defense          = defense;
            MagicPower       = magicPower;
            MagicDefense     = magicDefense;
            Speed            = speed;
            CritChance       = critChance;
            AbilityCooldowns = new int[Mathf.Max(1, abilitySlots)];
            HeroData         = heroData;
            EnemyData        = enemyData;
            RecruitableData  = recruitableData;
        }

        // ── Factories ─────────────────────────────────────────────────────────

        public static BattleUnit FromHero(HeroData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var s = data.baseStats;
            return new BattleUnit(
                data.heroName, BattleTeam.Hero,
                maxHP: s.maxHP, maxMana: 100,
                attack: s.attack, defense: s.defense,
                magicPower: s.magicPower, magicDefense: s.magicDefense,
                speed: s.speed, critChance: s.critChance,
                abilitySlots: data.abilities?.Length ?? 4,
                heroData: data, enemyData: null, recruitableData: null);
        }

        public static BattleUnit FromEnemy(EnemyData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return new BattleUnit(
                data.enemyName, BattleTeam.Enemy,
                maxHP: data.maxHP, maxMana: 0,
                attack: data.attackPower, defense: data.defense,
                magicPower: 0, magicDefense: 0,
                speed: Mathf.Max(1, Mathf.RoundToInt(data.moveSpeed)),
                critChance: 0, abilitySlots: 1,
                heroData: null, enemyData: data, recruitableData: null);
        }

        public static BattleUnit FromRecruitableEnemy(RecruitableEnemyData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return new BattleUnit(
                data.enemyName, BattleTeam.Enemy,
                maxHP: data.maxHP, maxMana: 0,
                attack: data.attackPower, defense: data.defense,
                magicPower: 0, magicDefense: 0,
                speed: Mathf.Max(1, Mathf.RoundToInt(data.moveSpeed)),
                critChance: 0, abilitySlots: 1,
                heroData: null, enemyData: data, recruitableData: data);
        }

        // ── Mutation API ──────────────────────────────────────────────────────

        /// <summary>
        /// Reduces HP by <paramref name="amount"/>. Clamps to 0.
        /// Caller is responsible for applying defense reductions before calling this.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(0, amount));
        }

        public void RestoreHP(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + Mathf.Max(0, amount));
        }

        public void SpendMana(int amount)
        {
            CurrentMana = Mathf.Max(0, CurrentMana - amount);
        }

        /// <summary>Decrements all ability cooldowns by 1 at end of this unit's turn.</summary>
        public void TickCooldowns()
        {
            for (int i = 0; i < AbilityCooldowns.Length; i++)
                if (AbilityCooldowns[i] > 0) AbilityCooldowns[i]--;
        }

        /// <summary>Switches this unit to the Hero team (recruitment).</summary>
        public void SetRecruited()
        {
            IsRecruited = true;
            Team        = BattleTeam.Hero;
        }
    }
}
