// SaveManager.cs
// Singleton MonoBehaviour that serializes / deserializes game state to JSON.
// Survives scene loads via DontDestroyOnLoad.
// Auto-saves on ResourceBank.OnResourceDeposited and HeroSkills.OnLevelUp.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evetero
{
    public class SaveManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────

        private static SaveManager _instance;
        public static SaveManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            UnsubscribeAll();
            if (_instance == this) _instance = null;
        }

        // ── Path ──────────────────────────────────────────────────────────────

        public static string SaveFilePath =>
            Path.Combine(Application.persistentDataPath, "save.json");

        /// <summary>Instance accessor for the save path (useful in tests).</summary>
        public string SavePath => SaveFilePath;

        // ── Internal tracking ─────────────────────────────────────────────────

        private readonly HashSet<HeroSkills> _registeredHeroes = new HashSet<HeroSkills>();
        private bool _subscribedToBank;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            SubscribeToBank();
            FindAndRegisterHeroSkills();
        }

        // ── Auto-save subscriptions ───────────────────────────────────────────

        private void SubscribeToBank()
        {
            if (_subscribedToBank) return;
            if (ResourceBank.Instance == null) return;

            ResourceBank.Instance.OnResourceDeposited += OnResourceDeposited;
            _subscribedToBank = true;
        }

        private void OnResourceDeposited(ResourceType _, int __) => Save();

        /// <summary>Find all HeroSkills in the active scene and wire up OnLevelUp auto-save.</summary>
        public void FindAndRegisterHeroSkills()
        {
            // Also try subscribing to the bank in case it wasn't ready in Start.
            SubscribeToBank();

            var heroes = FindObjectsByType<HeroSkills>(FindObjectsSortMode.None);
            foreach (var hero in heroes)
            {
                if (_registeredHeroes.Contains(hero)) continue;
                hero.OnLevelUp += OnHeroLevelUp;
                _registeredHeroes.Add(hero);
            }
        }

        private void OnHeroLevelUp(SkillType _, int __) => Save();

        private void UnsubscribeAll()
        {
            if (_subscribedToBank && ResourceBank.Instance != null)
                ResourceBank.Instance.OnResourceDeposited -= OnResourceDeposited;
            _subscribedToBank = false;

            foreach (var hero in _registeredHeroes)
            {
                if (hero != null)
                    hero.OnLevelUp -= OnHeroLevelUp;
            }
            _registeredHeroes.Clear();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Collect current game state and write it to disk as JSON.</summary>
        public void Save()
        {
            var data = new SaveData
            {
                sceneName = SceneManager.GetActiveScene().name,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            // Resources
            if (ResourceBank.Instance != null)
            {
                var allRes = ResourceBank.Instance.GetAllResources();
                foreach (var kvp in allRes)
                    data.resources.Add(new SaveData.ResourceEntry
                    {
                        key   = kvp.Key.ToString(),
                        value = kvp.Value
                    });
            }

            // Hero XP
            var heroes = FindObjectsByType<HeroSkills>(FindObjectsSortMode.None);
            foreach (var hero in heroes)
            {
                data.heroXP.Add(new SaveData.HeroXPEntry
                {
                    heroName = hero.gameObject.name,
                    xp       = hero.GetAllXP()
                });
            }

            File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, prettyPrint: true));
        }

        /// <summary>Read save file from disk and restore ResourceBank and all HeroSkills in scene.</summary>
        public void Load()
        {
            if (!File.Exists(SaveFilePath)) return;

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SaveFilePath));
            }
            catch
            {
                return;
            }

            if (data == null) return;

            // Restore resources
            if (ResourceBank.Instance != null)
            {
                var resDict = new Dictionary<ResourceType, int>();
                foreach (var entry in data.resources)
                {
                    if (Enum.TryParse<ResourceType>(entry.key, out var rt))
                        resDict[rt] = entry.value;
                }
                ResourceBank.Instance.LoadResources(resDict);
            }

            // Restore hero XP
            var heroXPDict = data.GetHeroXPDict();
            var heroes     = FindObjectsByType<HeroSkills>(FindObjectsSortMode.None);
            foreach (var hero in heroes)
            {
                if (heroXPDict.TryGetValue(hero.gameObject.name, out var xp))
                    hero.LoadXP(xp);
            }
        }
    }
}
