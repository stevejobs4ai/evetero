// NotificationScheduler.cs
// Singleton MonoBehaviour that tracks scheduled local notifications.
// Does not send notifications — acts as a schedule registry only.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evetero
{
    public class NotificationScheduler : MonoBehaviour
    {
        public struct ScheduledNotification
        {
            public NotificationTrigger Trigger;
            public float DelaySeconds;
            public string Title;
            public string Body;
            public DateTime ScheduledAt;
        }

        public static NotificationScheduler Instance { get; private set; }

        private readonly List<ScheduledNotification> _scheduled = new List<ScheduledNotification>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Adds a notification entry to the schedule.</summary>
        public void Register(NotificationTrigger trigger, float delaySeconds, string title, string body)
        {
            _scheduled.Add(new ScheduledNotification
            {
                Trigger     = trigger,
                DelaySeconds = delaySeconds,
                Title       = title,
                Body        = body,
                ScheduledAt = DateTime.UtcNow
            });
        }

        /// <summary>Removes all scheduled entries matching the given trigger.</summary>
        public void Cancel(NotificationTrigger trigger)
        {
            _scheduled.RemoveAll(n => n.Trigger == trigger);
        }

        /// <summary>Returns a read-only view of the current schedule.</summary>
        public IReadOnlyList<ScheduledNotification> GetScheduled() => _scheduled;
    }
}
