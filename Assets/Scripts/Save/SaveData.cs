// SaveData.cs
// Plain serializable data container for the save file.
// Uses nested serializable pairs because JsonUtility cannot serialize Dictionary<K,V>.

using System;
using System.Collections.Generic;

namespace Evetero
{
    [Serializable]
    public class SaveData
    {
        // ── Nested serializable pairs ──────────────────────────────────────────

        [Serializable]
        public class ResourceEntry
        {
            public string key;
            public int    value;
        }

        [Serializable]
        public class HeroXPEntry
        {
            public string heroName;
            public int[]  xp;
        }

        // ── Fields ─────────────────────────────────────────────────────────────

        public List<ResourceEntry> resources      = new List<ResourceEntry>();
        public List<HeroXPEntry>   heroXP         = new List<HeroXPEntry>();
        public List<ResourceEntry> walletBalances = new List<ResourceEntry>();
        public string              sceneName;
        public string              timestamp;

        // ── Dictionary helpers ─────────────────────────────────────────────────

        public Dictionary<string, int> GetResourceDict()
        {
            var dict = new Dictionary<string, int>(resources.Count);
            foreach (var e in resources)
                dict[e.key] = e.value;
            return dict;
        }

        public Dictionary<string, int[]> GetHeroXPDict()
        {
            var dict = new Dictionary<string, int[]>(heroXP.Count);
            foreach (var e in heroXP)
                dict[e.heroName] = e.xp;
            return dict;
        }

        public Dictionary<string, int> GetWalletDict()
        {
            var dict = new Dictionary<string, int>(walletBalances.Count);
            foreach (var e in walletBalances)
                dict[e.key] = e.value;
            return dict;
        }
    }
}
