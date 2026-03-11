// DialogueSystemTests.cs
// Play Mode tests for DialogueData and DialogueSystem.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class DialogueSystemTests
    {
        // ── DialogueData ──────────────────────────────────────────────────────

        [Test]
        public void DialogueData_WithThreeLines_ReportsLineCountOfThree()
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.sceneLabel = "Test Scene";
            data.lines = new[]
            {
                new DialogueLine { speakerName = "Narrator", lineText = "Line one." },
                new DialogueLine { speakerName = "Mira",     lineText = "Line two." },
                new DialogueLine { speakerName = "Commander",lineText = "Line three." }
            };

            Assert.AreEqual(3, data.LineCount);

            Object.DestroyImmediate(data);
        }

        [Test]
        public void DialogueData_GetLine_ReturnsCorrectLine()
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.lines = new[]
            {
                new DialogueLine { speakerName = "Mira", lineText = "Hello." },
                new DialogueLine { speakerName = "Mira", lineText = "Goodbye." }
            };

            Assert.AreEqual("Hello.",   data.GetLine(0).lineText);
            Assert.AreEqual("Goodbye.", data.GetLine(1).lineText);

            Object.DestroyImmediate(data);
        }

        [Test]
        public void DialogueData_GetLine_OutOfRange_ReturnsDefault()
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.lines = new[] { new DialogueLine { speakerName = "X", lineText = "Y" } };

            DialogueLine outOfRange = data.GetLine(99);

            Assert.IsNull(outOfRange.speakerName);

            Object.DestroyImmediate(data);
        }

        // ── DialogueSystem ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator DialogueSystem_IsPlaying_IsFalse_BeforePlayCalled()
        {
            var go     = new GameObject("DialogueCanvas");
            var system = go.AddComponent<DialogueSystem>();

            yield return null;

            Assert.IsFalse(system.IsPlaying);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator DialogueSystem_IsPlaying_IsTrue_AfterPlayWithValidData()
        {
            var go     = new GameObject("DialogueCanvas");
            var system = go.AddComponent<DialogueSystem>();

            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.sceneLabel = "Intro";
            data.lines = new[]
            {
                new DialogueLine { speakerName = "Mira", lineText = "Ready." },
                new DialogueLine { speakerName = "Mira", lineText = "Let's go." }
            };

            yield return null;

            system.Play(data);

            Assert.IsTrue(system.IsPlaying);

            Object.Destroy(go);
            Object.DestroyImmediate(data);
        }

        [UnityTest]
        public IEnumerator DialogueSystem_Skip_SetsIsPlayingFalse()
        {
            var go     = new GameObject("DialogueCanvas");
            var system = go.AddComponent<DialogueSystem>();

            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.sceneLabel = "Skippable";
            data.lines = new[]
            {
                new DialogueLine { speakerName = "Narrator", lineText = "A long tale…" }
            };

            yield return null;

            system.Play(data);
            system.Skip();

            Assert.IsFalse(system.IsPlaying);

            Object.Destroy(go);
            Object.DestroyImmediate(data);
        }

        [UnityTest]
        public IEnumerator DialogueSystem_Next_AdvancesToNextLine()
        {
            var go     = new GameObject("DialogueCanvas");
            var system = go.AddComponent<DialogueSystem>();

            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.sceneLabel = "Two liner";
            data.lines = new[]
            {
                new DialogueLine { speakerName = "A", lineText = "First."  },
                new DialogueLine { speakerName = "B", lineText = "Second." }
            };

            yield return null;

            system.Play(data);
            Assert.IsTrue(system.IsPlaying);

            system.Next(); // advance past line 0 -> line 1
            Assert.IsTrue(system.IsPlaying);

            system.Next(); // advance past line 1 -> end
            Assert.IsFalse(system.IsPlaying);

            Object.Destroy(go);
            Object.DestroyImmediate(data);
        }
    }
}
