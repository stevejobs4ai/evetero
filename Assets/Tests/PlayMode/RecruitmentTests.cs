// RecruitmentTests.cs
// Tests for RecruitmentSystem and BattleManager's Talk action.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class RecruitmentTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static HeroData MakeHeroData(string name, int speed = 3)
        {
            var d = ScriptableObject.CreateInstance<HeroData>();
            d.heroName  = name;
            d.baseStats = new HeroBaseStats { maxHP = 100, attack = 10, speed = speed };
            d.abilities = new AbilityData[4];
            return d;
        }

        private static RecruitableEnemyData MakeRecruitable(
            string name, HeroData requiredHero = null, HeroData joinData = null)
        {
            var d = ScriptableObject.CreateInstance<RecruitableEnemyData>();
            d.enemyName    = name;
            d.maxHP        = 30;
            d.attackPower  = 5;
            d.moveSpeed    = 2f;
            d.requiredHero = requiredHero;
            d.joinHeroData = joinData;
            return d;
        }

        private static EnemyData MakeEnemyData(string name)
        {
            var d = ScriptableObject.CreateInstance<EnemyData>();
            d.enemyName   = name;
            d.maxHP       = 30;
            d.attackPower = 5;
            d.moveSpeed   = 2f;
            return d;
        }

        // ── RecruitmentSystem.CanRecruit ──────────────────────────────────────

        [Test]
        public void CanRecruit_ReturnsFalse_WhenTargetNotRecruitable()
        {
            var hero   = BattleUnit.FromHero(MakeHeroData("Mira"));
            var target = BattleUnit.FromEnemy(MakeEnemyData("Goblin"));

            Assert.IsFalse(RecruitmentSystem.CanRecruit(hero, target),
                "Normal enemy should not be recruitable.");
        }

        [Test]
        public void CanRecruit_ReturnsFalse_WhenWrongHeroTalks()
        {
            var kael     = MakeHeroData("Kael");
            var mira     = MakeHeroData("Mira");
            var bandit   = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(mira);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            Assert.IsFalse(RecruitmentSystem.CanRecruit(heroUnit, targetUnit),
                "Wrong hero should not be able to recruit.");
        }

        [Test]
        public void CanRecruit_ReturnsTrue_WhenCorrectHeroTalks()
        {
            var kael   = MakeHeroData("Kael");
            var bandit = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(kael);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            Assert.IsTrue(RecruitmentSystem.CanRecruit(heroUnit, targetUnit));
        }

        [Test]
        public void CanRecruit_ReturnsTrue_WhenNoRequiredHeroSet()
        {
            var mira   = MakeHeroData("Mira");
            var bandit = MakeRecruitable("Bandit", requiredHero: null);

            var heroUnit   = BattleUnit.FromHero(mira);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            Assert.IsTrue(RecruitmentSystem.CanRecruit(heroUnit, targetUnit),
                "Any hero should be able to recruit when no requiredHero is set.");
        }

        [Test]
        public void CanRecruit_ReturnsFalse_WhenTargetAlreadyRecruited()
        {
            var kael   = MakeHeroData("Kael");
            var bandit = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(kael);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            targetUnit.SetRecruited(); // already recruited
            Assert.IsFalse(RecruitmentSystem.CanRecruit(heroUnit, targetUnit));
        }

        [Test]
        public void CanRecruit_ReturnsFalse_WhenTargetIsDead()
        {
            var kael   = MakeHeroData("Kael");
            var bandit = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(kael);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            targetUnit.TakeDamage(targetUnit.MaxHP);
            Assert.IsFalse(RecruitmentSystem.CanRecruit(heroUnit, targetUnit),
                "Dead unit cannot be recruited.");
        }

        // ── RecruitmentSystem.Recruit ─────────────────────────────────────────

        [Test]
        public void Recruit_SwitchesTargetTeamToHero()
        {
            var kael   = MakeHeroData("Kael");
            var bandit = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(kael);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            bool success = RecruitmentSystem.Recruit(heroUnit, targetUnit);

            Assert.IsTrue(success);
            Assert.AreEqual(BattleTeam.Hero, targetUnit.Team,
                "Recruited unit should switch to Hero team.");
        }

        [Test]
        public void Recruit_SetsIsRecruitedFlag()
        {
            var kael   = MakeHeroData("Kael");
            var bandit = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(kael);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            RecruitmentSystem.Recruit(heroUnit, targetUnit);

            Assert.IsTrue(targetUnit.IsRecruited);
        }

        [Test]
        public void Recruit_AddsJoinHeroDataToBattleContext()
        {
            BattleContext.ClearRecruited();

            var kael      = MakeHeroData("Kael");
            var joinData  = MakeHeroData("BanditJoinForm");
            var bandit    = MakeRecruitable("Bandit", requiredHero: kael, joinData: joinData);

            var heroUnit   = BattleUnit.FromHero(kael);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            RecruitmentSystem.Recruit(heroUnit, targetUnit);

            Assert.Contains(joinData, BattleContext.RecruitedHeroes,
                "JoinHeroData should be queued in BattleContext after recruitment.");

            BattleContext.ClearRecruited();
        }

        // ── BattleManager.ResolveTalk integration ─────────────────────────────

        [UnityTest]
        public IEnumerator BattleManager_Talk_CorrectHero_RecruitsUnit()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            BattleUnit recruitedUnit = null;
            mgr.OnUnitRecruited += u => recruitedUnit = u;

            var kael     = MakeHeroData("Kael");
            var bandit   = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(kael);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            yield return null;

            bool success = mgr.ResolveTalk(heroUnit, targetUnit);

            Assert.IsTrue(success);
            Assert.AreEqual(BattleTeam.Hero, targetUnit.Team);
            Assert.AreEqual(targetUnit, recruitedUnit,
                "OnUnitRecruited should fire with the recruited unit.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator BattleManager_Talk_WrongHero_DoesNotRecruit()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var kael   = MakeHeroData("Kael");
            var mira   = MakeHeroData("Mira");
            var bandit = MakeRecruitable("Bandit", requiredHero: kael);

            var heroUnit   = BattleUnit.FromHero(mira);
            var targetUnit = BattleUnit.FromRecruitableEnemy(bandit);

            yield return null;

            bool success = mgr.ResolveTalk(heroUnit, targetUnit);

            Assert.IsFalse(success);
            Assert.AreEqual(BattleTeam.Enemy, targetUnit.Team,
                "Target should remain an enemy when wrong hero tries to talk.");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator BattleManager_Talk_NonRecruitableEnemy_ReturnsFalse()
        {
            var go  = new GameObject("BM");
            var mgr = go.AddComponent<BattleManager>();

            var heroUnit   = BattleUnit.FromHero(MakeHeroData("Mira"));
            var targetUnit = BattleUnit.FromEnemy(MakeEnemyData("Goblin"));

            yield return null;

            bool success = mgr.ResolveTalk(heroUnit, targetUnit);

            Assert.IsFalse(success);

            Object.Destroy(go);
        }
    }
}
