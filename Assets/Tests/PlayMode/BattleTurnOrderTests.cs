// BattleTurnOrderTests.cs
// Verifies that BattleManager.BuildTurnQueue produces the correct order.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Evetero.Tests
{
    public class BattleTurnOrderTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static HeroData MakeHeroData(string name, int speed)
        {
            var d = ScriptableObject.CreateInstance<HeroData>();
            d.heroName           = name;
            d.baseStats          = new HeroBaseStats { maxHP = 100, attack = 10, speed = speed };
            d.abilities          = new AbilityData[4];
            return d;
        }

        private static EnemyData MakeEnemyData(string name, float speed)
        {
            var d = ScriptableObject.CreateInstance<EnemyData>();
            d.enemyName  = name;
            d.maxHP      = 30;
            d.attackPower = 5;
            d.moveSpeed  = speed;
            return d;
        }

        private static BattleUnit Hero(string name, int speed)
        {
            var d = MakeHeroData(name, speed);
            return BattleUnit.FromHero(d);
        }

        private static BattleUnit Enemy(string name, float speed)
        {
            var d = MakeEnemyData(name, speed);
            return BattleUnit.FromEnemy(d);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Test]
        public void BuildTurnQueue_OrdersBySpeedDescending()
        {
            var heroes  = new List<BattleUnit> { Hero("Slow", 1), Hero("Fast", 5) };
            var enemies = new List<BattleUnit> { Enemy("Mid", 3f) };

            var queue = BattleManager.BuildTurnQueue(heroes, enemies);

            Assert.AreEqual(3, queue.Count);
            Assert.AreEqual("Fast",  queue[0].Name, "Fastest unit should be first.");
            Assert.AreEqual("Mid",   queue[1].Name);
            Assert.AreEqual("Slow",  queue[2].Name, "Slowest unit should be last.");
        }

        [Test]
        public void BuildTurnQueue_TieInSpeed_HeroBeforeEnemy()
        {
            var heroes  = new List<BattleUnit> { Hero("HeroA", 3) };
            var enemies = new List<BattleUnit> { Enemy("EnemyA", 3f) };

            var queue = BattleManager.BuildTurnQueue(heroes, enemies);

            Assert.AreEqual(BattleTeam.Hero,  queue[0].Team, "Hero should precede enemy on speed tie.");
            Assert.AreEqual(BattleTeam.Enemy, queue[1].Team);
        }

        [Test]
        public void BuildTurnQueue_TieInSpeedAndTeam_OrdersAlphabetically()
        {
            var heroes = new List<BattleUnit>
            {
                Hero("Zara", 5),
                Hero("Aldric", 5)
            };

            var queue = BattleManager.BuildTurnQueue(heroes, new List<BattleUnit>());

            Assert.AreEqual("Aldric", queue[0].Name, "Alphabetically first hero should go first.");
            Assert.AreEqual("Zara",   queue[1].Name);
        }

        [Test]
        public void BuildTurnQueue_EmptyLists_ReturnsEmpty()
        {
            var queue = BattleManager.BuildTurnQueue(
                new List<BattleUnit>(), new List<BattleUnit>());

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void BuildTurnQueue_NullLists_ReturnsEmpty()
        {
            var queue = BattleManager.BuildTurnQueue(null, null);
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void BuildTurnQueue_AllEnemies_OrdersBySpeedDesc()
        {
            var enemies = new List<BattleUnit>
            {
                Enemy("Goblin1", 2f),
                Enemy("Goblin2", 4f),
                Enemy("Goblin3", 1f)
            };

            var queue = BattleManager.BuildTurnQueue(new List<BattleUnit>(), enemies);

            Assert.AreEqual("Goblin2", queue[0].Name);
            Assert.AreEqual("Goblin1", queue[1].Name);
            Assert.AreEqual("Goblin3", queue[2].Name);
        }
    }
}
