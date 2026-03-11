// SerializableDictionaryTests.cs
// PlayMode tests for SerializableDictionary<TKey, TValue>.
// Validates round-trip serialization via ISerializationCallbackReceiver.

using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using Evetero;

namespace Evetero.Tests
{
    public class SerializableDictionaryTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────────

        /// Simulate one Unity serialize/deserialize cycle by calling the callbacks manually.
        private static void RoundTrip<TKey, TValue>(SerializableDictionary<TKey, TValue> dict)
        {
            dict.OnBeforeSerialize();
            dict.OnAfterDeserialize();
        }

        // ── Tests ─────────────────────────────────────────────────────────────────

        [Test]
        public void RoundTrip_PreservesEntries()
        {
            var dict = new SerializableDictionary<string, int>
            {
                { "gold",   500 },
                { "wood",   200 },
                { "stone",   75 }
            };

            RoundTrip(dict);

            Assert.AreEqual(3,   dict.Count,         "Entry count should be preserved.");
            Assert.AreEqual(500, dict["gold"],        "'gold' value should be preserved.");
            Assert.AreEqual(200, dict["wood"],        "'wood' value should be preserved.");
            Assert.AreEqual(75,  dict["stone"],       "'stone' value should be preserved.");
        }

        [Test]
        public void RoundTrip_EmptyDictionary()
        {
            var dict = new SerializableDictionary<string, int>();

            RoundTrip(dict);

            Assert.AreEqual(0, dict.Count, "Empty dictionary should remain empty after round-trip.");
        }

        [Test]
        public void RoundTrip_DuplicateKeys_SkipsAndWarns()
        {
            // Manually inject a duplicate into the backing lists to simulate
            // an inspector-edited asset with a repeated key.
            var dict = new SerializableDictionary<string, float>();

            // Populate backing lists directly before calling OnAfterDeserialize.
            dict.OnBeforeSerialize(); // ensures lists exist and are clear

            // Access via reflection to inject the duplicate scenario.
            var keysField   = typeof(SerializableDictionary<string, float>)
                              .GetField("_keys",   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var valuesField = typeof(SerializableDictionary<string, float>)
                              .GetField("_values", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var keys   = (System.Collections.Generic.List<string>)keysField.GetValue(dict);
            var values = (System.Collections.Generic.List<float>)valuesField.GetValue(dict);

            keys.Add("mana");
            values.Add(100f);
            keys.Add("mana"); // duplicate
            values.Add(999f);
            keys.Add("stamina");
            values.Add(50f);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Duplicate key"));
            dict.OnAfterDeserialize();

            Assert.AreEqual(2,    dict.Count,          "Duplicate key should be skipped.");
            Assert.AreEqual(100f, dict["mana"],        "First occurrence of duplicate key wins.");
            Assert.AreEqual(50f,  dict["stamina"],     "Non-duplicate keys should still be present.");
        }

        [Test]
        public void RoundTrip_LargeDictionary()
        {
            const int Size = 150;
            var dict = new SerializableDictionary<int, string>();

            for (int i = 0; i < Size; i++)
                dict[i] = $"value_{i}";

            RoundTrip(dict);

            Assert.AreEqual(Size, dict.Count, "All entries should survive round-trip.");
            for (int i = 0; i < Size; i++)
                Assert.AreEqual($"value_{i}", dict[i], $"Entry {i} should be preserved.");
        }

        [Test]
        public void MultipleRoundTrips_StableResult()
        {
            var dict = new SerializableDictionary<string, int>
            {
                { "alpha", 1 },
                { "beta",  2 }
            };

            RoundTrip(dict);
            RoundTrip(dict);
            RoundTrip(dict);

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(1, dict["alpha"]);
            Assert.AreEqual(2, dict["beta"]);
        }

        [UnityTest]
        public IEnumerator RoundTrip_OnScriptableObject_EntriesPreserved()
        {
            // Verify the dictionary works when hosted on a ScriptableObject.
            var so = ScriptableObject.CreateInstance<SerializableDictTestAsset>();
            so.Map.Add("hero", 42);
            so.Map.OnBeforeSerialize();
            so.Map.OnAfterDeserialize();

            yield return null;

            Assert.AreEqual(1,  so.Map.Count,    "ScriptableObject-hosted dict should survive round-trip.");
            Assert.AreEqual(42, so.Map["hero"],  "Value should be preserved on ScriptableObject.");

            Object.Destroy(so);
        }
    }

    // Minimal ScriptableObject used only by the ScriptableObject round-trip test.
    internal class SerializableDictTestAsset : ScriptableObject
    {
        public SerializableDictionary<string, int> Map = new();
    }
}
