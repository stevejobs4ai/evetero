// CombatTests.cs
// Play Mode tests for the enemy system and combat loop.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class CombatTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private EnemyData MakeEnemyData(string name = "Goblin", int hp = 30, int atk = 5, int def = 1)
        {
            var d = ScriptableObject.CreateInstance<EnemyData>();
            d.enemyName            = name;
            d.maxHP                = hp;
            d.attackPower          = atk;
            d.defense              = def;
            d.attackIntervalSeconds = 2f;
            d.xpReward             = 50;
            return d;
        }

        private (GameObject go, EnemyController enemy, EnemyData data) MakeEnemy(
            string name = "Goblin", int hp = 30, int atk = 5, int def = 1)
        {
            var go   = new GameObject("Enemy_" + name);
            go.AddComponent<BoxCollider2D>();
            var enemy = go.AddComponent<EnemyController>();
            var data  = MakeEnemyData(name, hp, atk, def);
            enemy.enemyData = data;
            return (go, enemy, data);
        }

        // ── EnemyController ───────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator EnemyController_StartsInIdleState()
        {
            var (go, enemy, data) = MakeEnemy();

            yield return null; // Awake

            Assert.AreEqual(EnemyState.Idle, enemy.State);

            Object.Destroy(go);
            Object.DestroyImmediate(data);
        }

        [UnityTest]
        public IEnumerator EnemyController_TakeDamage_ReducesHP()
        {
            var (go, enemy, data) = MakeEnemy(hp: 30);

            yield return null;

            enemy.TakeDamage(10);

            Assert.AreEqual(20, enemy.currentHP);

            Object.Destroy(go);
            Object.DestroyImmediate(data);
        }

        [UnityTest]
        public IEnumerator EnemyController_TakeDamageToZero_SetsDead_AndFiresEvent()
        {
            var (go, enemy, data) = MakeEnemy(hp: 10);

            yield return null;

            bool eventFired = false;
            enemy.OnEnemyDefeated += () => eventFired = true;

            enemy.TakeDamage(10);

            Assert.AreEqual(EnemyState.Dead, enemy.State);
            Assert.IsTrue(eventFired);

            Object.Destroy(go);
            Object.DestroyImmediate(data);
        }

        [UnityTest]
        public IEnumerator EnemyController_HP_NeverGoesBelow0()
        {
            var (go, enemy, data) = MakeEnemy(hp: 10);

            yield return null;

            enemy.TakeDamage(9999);

            Assert.GreaterOrEqual(enemy.currentHP, 0);

            Object.Destroy(go);
            Object.DestroyImmediate(data);
        }

        // ── CombatManager ─────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator CombatManager_HeroAttack_ReducesEnemyHP()
        {
            // CombatManager
            var mgrGo = new GameObject("CombatManager");
            var mgr   = mgrGo.AddComponent<CombatManager>();

            // Hero
            var heroGo   = new GameObject("Hero");
            var hero     = heroGo.AddComponent<HeroController>();
            var heroData = ScriptableObject.CreateInstance<HeroData>();
            heroData.heroName   = "Tester";
            heroData.baseStats  = new HeroBaseStats { maxHP = 100, attack = 10, defense = 5, speed = 2 };
            hero.heroData       = heroData;

            // Enemy
            var (enemyGo, enemy, enemyData) = MakeEnemy(hp: 30, def: 2);

            yield return null; // Awake on all components

            int hpBefore = enemy.currentHP;
            mgr.HeroAttack(hero, enemy);

            Assert.Less(enemy.currentHP, hpBefore,
                "Enemy HP should decrease after HeroAttack");

            Object.Destroy(mgrGo);
            Object.Destroy(heroGo);
            Object.Destroy(enemyGo);
            Object.DestroyImmediate(heroData);
            Object.DestroyImmediate(enemyData);
        }
    }
}
