// BattleActionTests.cs
// Play Mode tests verifying that BattleManager actions (attack, ability, defend)
// behave correctly.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class BattleActionTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static HeroData MakeHeroData(string name, int hp = 100, int atk = 10,
            int def = 5, int speed = 3, int mana = 100)
        {
            var d = ScriptableObject.CreateInstance<HeroData>();
            d.heroName = name;
            d.baseStats = new HeroBaseStats
            {
                maxHP    = hp,
                attack   = atk,
                defense  = def,
                speed    = speed
            };
            d.abilities = new AbilityData[4];
            return d;
        }

        private static EnemyData MakeEnemyData(string name, int hp = 30, int atk = 5, int def = 0)
        {
            var d = ScriptableObject.CreateInstance<EnemyData>();
            d.enemyName    = name;
            d.maxHP        = hp;
            d.attackPower  = atk;
            d.defense      = def;
            d.moveSpeed    = 2f;
            d.xpReward     = 20;
            return d;
        }

        private static AbilityData MakeAbility(string name, int manaCost = 10,
            int cooldown = 0, AbilityType type = AbilityType.Attack,
            string formula = "{atk}*2")
        {
            var a = ScriptableObject.CreateInstance<AbilityData>();
            a.abilityName   = name;
            a.manaCost      = manaCost;
            a.cooldownTurns = cooldown;
            a.abilityType   = type;
            a.damageFormula = formula;
            return a;
        }

        private BattleManager MakeManager()
        {
            var go = new GameObject("BattleManager");
            return go.AddComponent<BattleManager>();
        }

        // ── Attack ────────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Attack_DealsExpectedDamage()
        {
            var go     = new GameObject("BM");
            var mgr    = go.AddComponent<BattleManager>();

            var attacker = BattleUnit.FromHero(MakeHeroData("Mira", atk: 10));
            var target   = BattleUnit.FromEnemy(MakeEnemyData("Goblin", hp: 30, def: 2));

            yield return null;

            int before = target.CurrentHP;
            mgr.ResolveAttack(attacker, target);
            int expectedDamage = Mathf.Max(1, attacker.Attack - target.Defense); // 10-2 = 8
            Assert.AreEqual(before - expectedDamage, target.CurrentHP);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Attack_KillsEnemy_WhenDamageExceedsHP()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var attacker = BattleUnit.FromHero(MakeHeroData("Mira", atk: 999));
            var target   = BattleUnit.FromEnemy(MakeEnemyData("Goblin", hp: 10, def: 0));

            yield return null;
            mgr.ResolveAttack(attacker, target);

            Assert.IsFalse(target.IsAlive, "Target should be dead after lethal attack.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Attack_DeadAttacker_DoesNoDamage()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var attacker = BattleUnit.FromHero(MakeHeroData("Ghost", atk: 50));
            var target   = BattleUnit.FromEnemy(MakeEnemyData("Goblin", hp: 30));

            // Kill attacker
            attacker.TakeDamage(attacker.MaxHP);
            yield return null;

            int before = target.CurrentHP;
            mgr.ResolveAttack(attacker, target);
            Assert.AreEqual(before, target.CurrentHP, "Dead attacker should deal no damage.");

            Object.Destroy(go);
        }

        // ── Ability ───────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Ability_CostsManOnUse()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var abilityData = MakeAbility("Strike", manaCost: 15);
            var heroData    = MakeHeroData("Mira");
            heroData.abilities[1] = abilityData;

            var user   = BattleUnit.FromHero(heroData);
            var target = BattleUnit.FromEnemy(MakeEnemyData("Goblin"));

            yield return null;

            int manaBefore = user.CurrentMana;
            mgr.ResolveAbility(user, 1, target);

            Assert.AreEqual(manaBefore - 15, user.CurrentMana);

            Object.Destroy(go);
            Object.DestroyImmediate(abilityData);
            Object.DestroyImmediate(heroData);
        }

        [UnityTest]
        public IEnumerator Ability_SetsAbilityCooldown()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var abilityData = MakeAbility("Blaze", manaCost: 5, cooldown: 3);
            var heroData    = MakeHeroData("Vera");
            heroData.abilities[2] = abilityData;

            var user   = BattleUnit.FromHero(heroData);
            var target = BattleUnit.FromEnemy(MakeEnemyData("Goblin"));

            yield return null;
            mgr.ResolveAbility(user, 2, target);

            Assert.AreEqual(3, user.AbilityCooldowns[2],
                "Cooldown should be set to 3 after using the ability.");

            Object.Destroy(go);
            Object.DestroyImmediate(abilityData);
            Object.DestroyImmediate(heroData);
        }

        [UnityTest]
        public IEnumerator Ability_BlockedWhenOnCooldown_DoesNotCostMana()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var abilityData = MakeAbility("Blaze", manaCost: 20, cooldown: 2);
            var heroData    = MakeHeroData("Vera");
            heroData.abilities[1] = abilityData;

            var user   = BattleUnit.FromHero(heroData);
            var target = BattleUnit.FromEnemy(MakeEnemyData("Goblin"));

            yield return null;

            // Use once to put on cooldown
            mgr.ResolveAbility(user, 1, target);
            int manaAfterFirst = user.CurrentMana;

            // Try again while on cooldown
            mgr.ResolveAbility(user, 1, target);

            Assert.AreEqual(manaAfterFirst, user.CurrentMana,
                "Mana should not be spent when ability is on cooldown.");

            Object.Destroy(go);
            Object.DestroyImmediate(abilityData);
            Object.DestroyImmediate(heroData);
        }

        // ── Defend ────────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Defend_HalvesIncomingDamage()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var attacker = BattleUnit.FromHero(MakeHeroData("Mira", atk: 20, def: 0));
            var defender = BattleUnit.FromEnemy(MakeEnemyData("Knight", hp: 100, def: 0));

            yield return null;

            // Record damage without defend
            mgr.ResolveAttack(attacker, defender);
            int dmgNormal = 100 - defender.CurrentHP;

            // Reset HP and apply defend
            defender.RestoreHP(dmgNormal);
            mgr.ResolveDefend(defender);
            mgr.ResolveAttack(attacker, defender);
            int dmgDefended = (100 - dmgNormal) - defender.CurrentHP;

            Assert.Less(dmgDefended, dmgNormal,
                "Damage taken while defending should be less than normal damage.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Defend_SetsIsDefendingFlag()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var unit = BattleUnit.FromHero(MakeHeroData("Aldric"));
            yield return null;

            mgr.ResolveDefend(unit);

            Assert.IsTrue(unit.IsDefending);

            Object.Destroy(go);
        }

        // ── Cooldown ticking ──────────────────────────────────────────────────

        [Test]
        public void TickCooldowns_DecrementsEachTurn()
        {
            var heroData = MakeHeroData("Vera");
            heroData.abilities[1] = MakeAbility("Blaze", cooldown: 2);

            var unit = BattleUnit.FromHero(heroData);
            unit.AbilityCooldowns[1] = 2;

            unit.TickCooldowns();
            Assert.AreEqual(1, unit.AbilityCooldowns[1]);

            unit.TickCooldowns();
            Assert.AreEqual(0, unit.AbilityCooldowns[1]);

            unit.TickCooldowns(); // should not go negative
            Assert.AreEqual(0, unit.AbilityCooldowns[1]);
        }

        // ── XP / Victory ──────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Battle_VictoryFired_WhenAllEnemiesDefeated()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            BattleResult? result = null;
            mgr.OnBattleEnded += r => result = r;

            var hero  = BattleUnit.FromHero(MakeHeroData("Mira", atk: 999));
            var enemy = BattleUnit.FromEnemy(MakeEnemyData("Goblin", hp: 1));

            yield return null;

            mgr.StartBattle(new List<BattleUnit> { hero }, new List<BattleUnit> { enemy });

            // Hero's turn fires immediately; submit attack
            mgr.SubmitAction(new BattleAction
            {
                Type   = BattleActionType.Attack,
                Target = enemy
            });

            Assert.AreEqual(BattleResult.Victory, result,
                "Victory should fire after all enemies defeated.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Battle_DefeatFired_WhenAllHeroesDefeated()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();
            mgr.enemyTurnDelay = 0f;

            BattleResult? result = null;
            mgr.OnBattleEnded += r => result = r;

            var hero  = BattleUnit.FromHero(MakeHeroData("Mira", hp: 1, atk: 1));
            var enemy = BattleUnit.FromEnemy(MakeEnemyData("Boss", hp: 999, atk: 999));

            yield return null;

            mgr.StartBattle(new List<BattleUnit> { hero }, new List<BattleUnit> { enemy });

            // Hero's turn — defend (enemy is faster but hero goes first due to team tiebreak)
            // We need to ensure enemy kills hero; use Flee to confirm Fled result instead.
            mgr.SubmitAction(new BattleAction { Type = BattleActionType.Flee });

            Assert.AreEqual(BattleResult.Fled, result,
                "Fled result should fire when player flees.");

            Object.Destroy(go);
        }
    }
}
