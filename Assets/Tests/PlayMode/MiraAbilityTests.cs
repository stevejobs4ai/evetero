// MiraAbilityTests.cs
// PlayMode tests confirming all 4 of Mira's Druid abilities execute without error.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class MiraAbilityTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private HeroController CreateHero(string name, int hp, int mana, int magicPower, int magicDefense)
        {
            var go = new GameObject(name);
            var hc = go.AddComponent<HeroController>();

            var heroData = ScriptableObject.CreateInstance<HeroData>();
            heroData.heroName = name;
            heroData.baseStats = new HeroBaseStats
            {
                maxHP        = hp,
                attack       = 10,
                magicPower   = magicPower,
                defense      = 5,
                magicDefense = magicDefense,
                speed        = 3,
                critChance   = 0
            };

            hc.heroData          = heroData;
            hc.currentHP         = hp;
            hc.currentMana       = mana;
            hc.abilityCooldowns  = new int[4];

            return hc;
        }

        private AbilityData MakeAbility(
            string name, AbilityType type, AbilityTarget target,
            string formula, int cooldown, int mana)
        {
            var a = ScriptableObject.CreateInstance<AbilityData>();
            a.abilityName    = name;
            a.abilityType    = type;
            a.targetType     = target;
            a.damageFormula  = formula;
            a.cooldownTurns  = cooldown;
            a.manaCost       = mana;
            return a;
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator LivingRoot_Attack_DealsPositiveDamage()
        {
            var mira     = CreateHero("Mira",  80, 100, 85, 0);
            var enemy    = CreateHero("Enemy", 60,   0,  0, 10);
            var sysGo    = new GameObject("AbilitySystem");
            var sys      = sysGo.AddComponent<AbilitySystem>();
            sys.owner    = mira;

            var ability  = MakeAbility("Living Root", AbilityType.Attack,
                               AbilityTarget.SingleEnemy, "{matk} * 1.2 - {mdef}", 0, 3);

            yield return null;

            var result = sys.Execute(ability, 0, enemy);

            Assert.IsTrue(result.success, result.failReason);
            Assert.Greater(result.value, 0, "Living Root should deal positive damage");
            Assert.AreEqual("Living Root", result.abilityName);

            var miraData  = mira.heroData;
            var enemyData = enemy.heroData;
            Object.Destroy(mira.gameObject);
            Object.Destroy(enemy.gameObject);
            Object.Destroy(sysGo);
            Object.DestroyImmediate(ability);
            Object.DestroyImmediate(miraData);
            Object.DestroyImmediate(enemyData);
        }

        [UnityTest]
        public IEnumerator Entangle_Debuff_SucceedsWithNoFormula()
        {
            var mira     = CreateHero("Mira",  80, 100, 85, 0);
            var enemy    = CreateHero("Enemy", 60,   0,  0,  0);
            var sysGo    = new GameObject("AbilitySystem");
            var sys      = sysGo.AddComponent<AbilitySystem>();
            sys.owner    = mira;

            var ability  = MakeAbility("Entangle", AbilityType.Debuff,
                               AbilityTarget.SingleEnemy, "", 2, 8);

            yield return null;

            var result = sys.Execute(ability, 1, enemy);

            Assert.IsTrue(result.success, result.failReason);
            Assert.AreEqual("Entangle", result.abilityName);
            // Debuff with no formula does zero damage — target should be alive
            Assert.IsTrue(enemy.IsAlive, "Enemy should survive a zero-damage debuff");

            var miraData  = mira.heroData;
            var enemyData = enemy.heroData;
            Object.Destroy(mira.gameObject);
            Object.Destroy(enemy.gameObject);
            Object.Destroy(sysGo);
            Object.DestroyImmediate(ability);
            Object.DestroyImmediate(miraData);
            Object.DestroyImmediate(enemyData);
        }

        [UnityTest]
        public IEnumerator MendingTouch_Heal_RestoresAllyHP()
        {
            var mira  = CreateHero("Mira", 80, 100, 85, 0);
            var ally  = CreateHero("Ally", 80,   0,  0,  0);
            ally.currentHP = 30; // wounded ally

            var sysGo    = new GameObject("AbilitySystem");
            var sys      = sysGo.AddComponent<AbilitySystem>();
            sys.owner    = mira;

            var ability  = MakeAbility("Mending Touch", AbilityType.Heal,
                               AbilityTarget.SingleAlly, "{matk} * 1.5", 3, 12);

            yield return null;

            int hpBefore = ally.currentHP;
            var result   = sys.Execute(ability, 2, ally);

            Assert.IsTrue(result.success, result.failReason);
            Assert.AreEqual("Mending Touch", result.abilityName);
            Assert.Greater(ally.currentHP, hpBefore, "Ally HP should increase after Mending Touch");

            var miraData = mira.heroData;
            var allyData = ally.heroData;
            Object.Destroy(mira.gameObject);
            Object.Destroy(ally.gameObject);
            Object.Destroy(sysGo);
            Object.DestroyImmediate(ability);
            Object.DestroyImmediate(miraData);
            Object.DestroyImmediate(allyData);
        }

        [UnityTest]
        public IEnumerator GuardiansStand_Buff_SucceedsOnAlly()
        {
            var mira  = CreateHero("Mira", 80, 100, 85, 0);
            var ally  = CreateHero("Ally", 60,   0,  0,  0);

            var sysGo    = new GameObject("AbilitySystem");
            var sys      = sysGo.AddComponent<AbilitySystem>();
            sys.owner    = mira;

            var ability  = MakeAbility("Guardian's Stand", AbilityType.Buff,
                               AbilityTarget.AllAllies, "", 5, 20);

            yield return null;

            var result = sys.Execute(ability, 3, ally);

            Assert.IsTrue(result.success, result.failReason);
            Assert.AreEqual("Guardian's Stand", result.abilityName);

            var miraData = mira.heroData;
            var allyData = ally.heroData;
            Object.Destroy(mira.gameObject);
            Object.Destroy(ally.gameObject);
            Object.Destroy(sysGo);
            Object.DestroyImmediate(ability);
            Object.DestroyImmediate(miraData);
            Object.DestroyImmediate(allyData);
        }

        [UnityTest]
        public IEnumerator AllFourAbilities_Cooldowns_TickCorrectly()
        {
            var mira   = CreateHero("Mira",  80, 200, 85, 0);
            var target = CreateHero("Enemy", 200,  0,  0,  0);

            var sysGo  = new GameObject("AbilitySystem");
            var sys    = sysGo.AddComponent<AbilitySystem>();
            sys.owner  = mira;

            var livingRoot    = MakeAbility("Living Root",     AbilityType.Attack, AbilityTarget.SingleEnemy, "{matk} * 1.2 - {mdef}", 0, 3);
            var entangle      = MakeAbility("Entangle",        AbilityType.Debuff, AbilityTarget.SingleEnemy, "",                      2, 8);
            var mendingTouch  = MakeAbility("Mending Touch",   AbilityType.Heal,   AbilityTarget.SingleAlly,  "{matk} * 1.5",          3, 12);
            var guardiansStand = MakeAbility("Guardian's Stand", AbilityType.Buff, AbilityTarget.AllAllies,   "",                      5, 20);

            yield return null;

            sys.Execute(livingRoot,     0, target);
            sys.Execute(entangle,       1, target);
            sys.Execute(mendingTouch,   2, target);
            sys.Execute(guardiansStand, 3, target);

            // After first use, cooldowns should be set
            Assert.AreEqual(0, mira.abilityCooldowns[0], "Living Root cooldown: 0");
            Assert.AreEqual(2, mira.abilityCooldowns[1], "Entangle cooldown: 2");
            Assert.AreEqual(3, mira.abilityCooldowns[2], "Mending Touch cooldown: 3");
            Assert.AreEqual(5, mira.abilityCooldowns[3], "Guardian's Stand cooldown: 5");

            // Tick once — all cooldowns decrement
            sys.TickCooldowns();

            Assert.AreEqual(0, mira.abilityCooldowns[0]);
            Assert.AreEqual(1, mira.abilityCooldowns[1]);
            Assert.AreEqual(2, mira.abilityCooldowns[2]);
            Assert.AreEqual(4, mira.abilityCooldowns[3]);

            var miraData   = mira.heroData;
            var targetData = target.heroData;
            Object.Destroy(mira.gameObject);
            Object.Destroy(target.gameObject);
            Object.Destroy(sysGo);
            Object.DestroyImmediate(livingRoot);
            Object.DestroyImmediate(entangle);
            Object.DestroyImmediate(mendingTouch);
            Object.DestroyImmediate(guardiansStand);
            Object.DestroyImmediate(miraData);
            Object.DestroyImmediate(targetData);
        }
    }
}
