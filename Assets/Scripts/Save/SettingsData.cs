using UnityEngine;

namespace Evetero
{
    [CreateAssetMenu(menuName = "Evetero/Settings Data", fileName = "SettingsData")]
    public class SettingsData : ScriptableObject
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public bool showDamageNumbers = true;
    }
}
