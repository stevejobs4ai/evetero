// LocalNotificationBridge.cs
// Seam class for platform local notifications.
// Replace the stub bodies inside the compile guard with real
// Unity.Notifications calls once com.unity.mobile.notifications is installed.

using UnityEngine;

namespace Evetero
{
    public static class LocalNotificationBridge
    {
#if UNITY_IOS || UNITY_ANDROID
        /// <summary>Schedules a local notification on device.</summary>
        public static void Send(string title, string body, int delaySeconds)
        {
            // TODO: replace with Unity.Notifications calls.
            // iOS:     UnityEngine.iOS.LocalNotification / UNUserNotificationCenter
            // Android: AndroidNotificationCenter.SendNotification(...)
        }

        /// <summary>Cancels all pending local notifications on device.</summary>
        public static void CancelAll()
        {
            // TODO: replace with Unity.Notifications calls.
            // iOS:     UnityEngine.iOS.LocalNotification.CancelAllLocalNotifications()
            // Android: AndroidNotificationCenter.CancelAllNotifications()
        }
#else
        /// <summary>Editor no-op — logs intent without sending a real notification.</summary>
        public static void Send(string title, string body, int delaySeconds)
        {
            Debug.Log($"[Notifications] Editor stub: {title}");
        }

        /// <summary>Editor no-op.</summary>
        public static void CancelAll()
        {
            Debug.Log("[Notifications] Editor stub: CancelAll");
        }
#endif
    }
}
