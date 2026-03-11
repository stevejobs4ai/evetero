// SkillSystemTests.cs
// Play Mode tests for SkillSystem math and HeroSkills XP/level tracking.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class SkillSystemTests
    {
        // ── SkillSystem.XPForLevel ─────────────────────────────────────────────

        [Test]
        public void XPForLevel_Level1_ReturnsZero()
        {
            Assert.AreEqual(0, SkillSystem.XPForLevel(1));
        }

        [Test]
        public void XPForLevel_Level2_ReturnsPositiveNumber()
        {
            Assert.Greater(SkillSystem.XPForLevel(2), 0);
        }

        [Test]
        public void XPForLevel_Level99_IsLargePositiveNumber()
        {
            // OSRS level 99 = 13,034,431 XP
            Assert.Greater(SkillSystem.XPForLevel(99), 10_000_000);
        }

        // ── SkillSystem.LevelForXP ─────────────────────────────────────────────

        [Test]
        public void LevelForXP_ZeroXP_ReturnsLevel1()
        {
            Assert.AreEqual(1, SkillSystem.LevelForXP(0));
        }

        [Test]
        public void LevelForXP_NegativeXP_ReturnsLevel1()
        {
            Assert.AreEqual(1, SkillSystem.LevelForXP(-999));
        }

        [Test]
        public void LevelForXP_HugeXP_ReturnsCappedAt99()
        {
            Assert.AreEqual(99, SkillSystem.LevelForXP(int.MaxValue));
        }

        [Test]
        public void LevelForXP_ExactLevel2XP_ReturnsLevel2()
        {
            int xpForLevel2 = SkillSystem.XPForLevel(2);
            Assert.AreEqual(2, SkillSystem.LevelForXP(xpForLevel2));
        }

        // ── SkillSystem.IsBreakthrough ─────────────────────────────────────────

        [Test]
        public void IsBreakthrough_Level50_ReturnsTrue()
        {
            Assert.IsTrue(SkillSystem.IsBreakthrough(50));
        }

        [Test]
        public void IsBreakthrough_Level75_ReturnsTrue()
        {
            Assert.IsTrue(SkillSystem.IsBreakthrough(75));
        }

        [Test]
        public void IsBreakthrough_Level99_ReturnsTrue()
        {
            Assert.IsTrue(SkillSystem.IsBreakthrough(99));
        }

        [Test]
        public void IsBreakthrough_Level51_ReturnsFalse()
        {
            Assert.IsFalse(SkillSystem.IsBreakthrough(51));
        }

        [Test]
        public void IsBreakthrough_Level1_ReturnsFalse()
        {
            Assert.IsFalse(SkillSystem.IsBreakthrough(1));
        }

        // ── HeroSkills.GainXP — level-up event ────────────────────────────────

        [UnityTest]
        public IEnumerator GainXP_FiresOnLevelUp_WhenXPCrossesThreshold()
        {
            var go         = new GameObject("TestHero");
            var heroSkills = go.AddComponent<HeroSkills>();

            yield return null; // let Awake run

            bool  levelUpFired = false;
            int   reportedLevel = 0;
            heroSkills.OnLevelUp += (skill, level) =>
            {
                levelUpFired   = true;
                reportedLevel  = level;
            };

            int xpForLevel2 = SkillSystem.XPForLevel(2);
            heroSkills.GainXP(SkillType.Woodcutting, xpForLevel2);

            Assert.IsTrue(levelUpFired,  "OnLevelUp should have fired.");
            Assert.AreEqual(2, reportedLevel);
            Assert.AreEqual(2, heroSkills.GetLevel(SkillType.Woodcutting));

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator GainXP_DoesNotFireOnLevelUp_WhenLevelUnchanged()
        {
            var go         = new GameObject("TestHero2");
            var heroSkills = go.AddComponent<HeroSkills>();

            yield return null;

            bool levelUpFired = false;
            heroSkills.OnLevelUp += (_, __) => levelUpFired = true;

            // Add only 1 XP — not enough to reach level 2 (needs 83+)
            heroSkills.GainXP(SkillType.Mining, 1);

            Assert.IsFalse(levelUpFired);

            Object.Destroy(go);
        }

        // ── HeroSkills.GainXP — breakthrough event ────────────────────────────

        [UnityTest]
        public IEnumerator GainXP_FiresOnBreakthrough_AtLevel50()
        {
            var go         = new GameObject("TestHeroBreakthrough");
            var heroSkills = go.AddComponent<HeroSkills>();

            yield return null;

            bool breakthroughFired = false;
            int  breakthroughLevel = 0;
            heroSkills.OnBreakthrough += (skill, level) =>
            {
                breakthroughFired = true;
                breakthroughLevel = level;
            };

            int xpForLevel50 = SkillSystem.XPForLevel(50);
            heroSkills.GainXP(SkillType.Fishing, xpForLevel50);

            Assert.IsTrue(breakthroughFired, "OnBreakthrough should fire at level 50.");
            Assert.AreEqual(50, breakthroughLevel);

            Object.Destroy(go);
        }

        // ── HeroSkills property checks ─────────────────────────────────────────

        [UnityTest]
        public IEnumerator HeroSkills_GetXP_ReturnsAccumulatedXP()
        {
            var go         = new GameObject("TestHeroXP");
            var heroSkills = go.AddComponent<HeroSkills>();

            yield return null;

            heroSkills.GainXP(SkillType.Crafting, 100);
            heroSkills.GainXP(SkillType.Crafting, 50);

            Assert.AreEqual(150, heroSkills.GetXP(SkillType.Crafting));

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator HeroSkills_IndependentSkills_DoNotInterfere()
        {
            var go         = new GameObject("TestHeroIndependent");
            var heroSkills = go.AddComponent<HeroSkills>();

            yield return null;

            heroSkills.GainXP(SkillType.Woodcutting, 500);

            Assert.AreEqual(500, heroSkills.GetXP(SkillType.Woodcutting));
            Assert.AreEqual(0,   heroSkills.GetXP(SkillType.Mining));

            Object.Destroy(go);
        }
    }
}
