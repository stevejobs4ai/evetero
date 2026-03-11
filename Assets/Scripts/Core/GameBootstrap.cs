// GameBootstrap.cs
// Place on a root GameObject in every scene (before other systems Start).
// Ensures SaveManager exists, then restores persisted state on load.

using System;
using UnityEngine;

namespace Evetero
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Create SaveManager if it doesn't exist yet.
            if (SaveManager.Instance == null)
            {
                var go = new GameObject("SaveManager");
                go.AddComponent<SaveManager>();
            }
        }

        private void Start()
        {
            var data = SaveManager.Instance.LoadGame();
            if (data == null) return;

            RestoreResources(data);
            RestoreHeroXP(data);
        }

        // ── Restore helpers ───────────────────────────────────────────────────

        private void RestoreResources(SaveData data)
        {
            if (ResourceBank.Instance == null) return;

            foreach (var kv in data.GetResources())
            {
                if (Enum.TryParse<ResourceType>(kv.Key, out var rt))
                    ResourceBank.Instance.Deposit(rt, kv.Value);
            }
        }

        private void RestoreHeroXP(SaveData data)
        {
            var xpDict    = data.GetHeroXP();
            var allSkills = FindObjectsByType<HeroSkills>(FindObjectsSortMode.None);

            foreach (var hs in allSkills)
            {
                string heroName = hs.gameObject.name;
                foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
                {
                    string key = $"{heroName}:{skill}";
                    if (xpDict.TryGetValue(key, out float xp))
                        hs.RestoreXP(skill, (int)xp);
                }
            }
        }
    }
}
