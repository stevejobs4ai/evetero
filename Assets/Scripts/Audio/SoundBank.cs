using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    public enum SoundCategory { UI, SFX, Ambient, Music }

    [Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitchMin = 1f;
        [Range(0.5f, 2f)] public float pitchMax = 1f;
        public SoundCategory category;
    }

    [CreateAssetMenu(menuName = "Evetero/Audio/Sound Bank", fileName = "NewSoundBank")]
    public class SoundBank : ScriptableObject
    {
        [SerializeField] private List<SoundEntry> entries = new();

        private Dictionary<string, SoundEntry> _lookup;

        public SoundEntry Get(string id)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(id, out var entry);
            return entry;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, SoundEntry>(entries.Count);
            foreach (var e in entries)
                if (!string.IsNullOrEmpty(e.id))
                    _lookup[e.id] = e;
        }

        private void OnValidate() => _lookup = null;
    }
}
