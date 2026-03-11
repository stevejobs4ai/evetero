// NotificationSchedulerTests.cs
// Play Mode tests for NotificationScheduler.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evetero.Tests
{
    public class NotificationSchedulerTests
    {
        private NotificationScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("NotificationScheduler");
            _scheduler = go.AddComponent<NotificationScheduler>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_scheduler != null)
                Object.DestroyImmediate(_scheduler.gameObject);
        }

        [UnityTest]
        public IEnumerator Register_AddsOneEntry()
        {
            _scheduler.Register(NotificationTrigger.DailyLogin, 60f, "Come back!", "Your village awaits.");

            yield return null;

            Assert.AreEqual(1, _scheduler.GetScheduled().Count);
        }

        [UnityTest]
        public IEnumerator Cancel_RemovesMatchingEntry()
        {
            _scheduler.Register(NotificationTrigger.ResourceCapFull, 30f, "Full!", "Resources are at cap.");
            _scheduler.Cancel(NotificationTrigger.ResourceCapFull);

            yield return null;

            Assert.AreEqual(0, _scheduler.GetScheduled().Count);
        }

        [UnityTest]
        public IEnumerator MultipleRegistrations_WithDifferentTriggers_AreIndependent()
        {
            _scheduler.Register(NotificationTrigger.DailyLogin,    60f,  "Login",   "Daily login bonus.");
            _scheduler.Register(NotificationTrigger.TimerComplete, 300f, "Timer",   "Timer finished.");
            _scheduler.Register(NotificationTrigger.GuildActivity, 120f, "Guild",   "Guild event started.");

            _scheduler.Cancel(NotificationTrigger.TimerComplete);

            yield return null;

            var scheduled = _scheduler.GetScheduled();
            Assert.AreEqual(2, scheduled.Count);
            Assert.AreEqual(NotificationTrigger.DailyLogin,    scheduled[0].Trigger);
            Assert.AreEqual(NotificationTrigger.GuildActivity, scheduled[1].Trigger);
        }
    }
}
