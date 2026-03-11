// SaveManagerTests.cs
// EditMode tests for the save/load system.
// Run via Unity Test Runner → EditMode tab.

using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Evetero.Tests
{
    public class SaveManagerTests
    {
        private SaveManager _saveManager;
        private string      _savePath;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("SaveManager");
            _saveManager = go.AddComponent<SaveManager>();
            _savePath    = _saveManager.SavePath;

            // Start each test with no leftover save file.
            if (File.Exists(_savePath)) File.Delete(_savePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (_saveManager != null)
                Object.DestroyImmediate(_saveManager.gameObject);

            if (File.Exists(_savePath)) File.Delete(_savePath);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Test]
        public void SaveManager_WritesFile()
        {
            _saveManager.SaveGame();

            Assert.IsTrue(File.Exists(_savePath), "save.json should exist after SaveGame()");
        }

        [Test]
        public void SaveManager_LoadReturnsData()
        {
            _saveManager.SaveGame();
            var data = _saveManager.LoadGame();

            Assert.IsNotNull(data, "LoadGame() should return non-null after a successful save");
        }

        [Test]
        public void SaveManager_RoundTripXP()
        {
            // Create a hero with HeroSkills and give it some XP.
            var heroGo     = new GameObject("TestHero");
            var heroSkills = heroGo.AddComponent<HeroSkills>();
            heroSkills.GainXP(SkillType.Woodcutting, 100);

            // Ensure the manual save captured the state (GainXP may not level-up,
            // so we force a save here as well).
            _saveManager.SaveGame();

            // Remove the hero to simulate a fresh scene load.
            Object.DestroyImmediate(heroGo);

            var data = _saveManager.LoadGame();
            Assert.IsNotNull(data);

            var xpDict = data.GetHeroXP();
            Assert.IsTrue(xpDict.ContainsKey("TestHero:Woodcutting"),
                          "Saved data should contain the composite hero:skill key");
            Assert.AreEqual(100f, xpDict["TestHero:Woodcutting"], 0.01f,
                            "Loaded XP should match the saved value");
        }

        [Test]
        public void SaveManager_HandlesNoFile()
        {
            // Guarantee no file exists.
            if (File.Exists(_savePath)) File.Delete(_savePath);

            SaveData result = null;
            Assert.DoesNotThrow(() => result = _saveManager.LoadGame(),
                                "LoadGame() must not throw when no save file exists");
            Assert.IsNull(result, "LoadGame() should return null when no file is present");
        }
    }
}
