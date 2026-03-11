#if UNITY_EDITOR
using UnityEditor;

namespace Evetero.Editor
{
    [InitializeOnLoad]
    public static class EveteroPlayerSettings
    {
        static EveteroPlayerSettings()
        {
            ApplyIfChanged();
        }

        private static void ApplyIfChanged()
        {
            bool dirty = false;

            if (PlayerSettings.companyName != "Evetero")
            {
                PlayerSettings.companyName = "Evetero";
                dirty = true;
            }

            if (PlayerSettings.productName != "Evetero")
            {
                PlayerSettings.productName = "Evetero";
                dirty = true;
            }

            if (PlayerSettings.bundleVersion != "0.1.0")
            {
                PlayerSettings.bundleVersion = "0.1.0";
                dirty = true;
            }

            if (PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) != "com.evetero.game")
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.evetero.game");
                dirty = true;
            }

            if (PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) != "com.evetero.game")
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.evetero.game");
                dirty = true;
            }

            if (dirty)
                UnityEngine.Debug.Log("[Evetero] PlayerSettings updated to project defaults.");
        }
    }
}
#endif
