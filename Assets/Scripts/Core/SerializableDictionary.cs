// SerializableDictionary.cs
// Generic dictionary wrapper that survives Unity serialization.
// Unity cannot serialize Dictionary<K,V> natively; this class uses
// ISerializationCallbackReceiver + two parallel Lists to bridge the gap.
//
// Usage:
//   [SerializeField] SerializableDictionary<string, int> _myMap = new();
//
// Note: TKey must be a Unity-serializable type (string, int, enum, UnityEngine.Object…).
//       Non-serializable key types will silently produce an empty dictionary after reload.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>,
        ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey>   _keys   = new();
        [SerializeField] private List<TValue> _values = new();

        // Called by Unity before serializing this object to disk / inspector.
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in this)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        // Called by Unity after deserializing this object from disk / inspector.
        public void OnAfterDeserialize()
        {
            Clear();

            int count = Mathf.Min(_keys.Count, _values.Count);
            for (int i = 0; i < count; i++)
            {
                TKey key = _keys[i];
                if (key == null)
                {
                    Debug.LogWarning($"[SerializableDictionary<{typeof(TKey).Name},{typeof(TValue).Name}>] " +
                                     $"Null key at index {i} — skipping.");
                    continue;
                }

                if (ContainsKey(key))
                {
                    Debug.LogWarning($"[SerializableDictionary<{typeof(TKey).Name},{typeof(TValue).Name}>] " +
                                     $"Duplicate key '{key}' at index {i} — skipping.");
                    continue;
                }

                this[key] = _values[i];
            }
        }
    }
}
