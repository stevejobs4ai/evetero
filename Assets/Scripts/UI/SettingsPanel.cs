using UnityEngine;
using UnityEngine.UI;

namespace Evetero
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle damageNumbersToggle;
        [SerializeField] private SettingsData settingsData;

        private void Awake()
        {
            LoadFromPrefs();
            InitializeSliders();
        }

        private void InitializeSliders()
        {
            if (masterSlider != null)
            {
                masterSlider.value = settingsData.masterVolume;
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicSlider != null)
            {
                musicSlider.value = settingsData.musicVolume;
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = settingsData.sfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (damageNumbersToggle != null)
            {
                damageNumbersToggle.isOn = settingsData.showDamageNumbers;
                damageNumbersToggle.onValueChanged.AddListener(OnDamageNumbersChanged);
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            settingsData.masterVolume = value;
            PlayerPrefs.SetFloat("masterVol", value);
            PlayerPrefs.Save();
        }

        private void OnMusicVolumeChanged(float value)
        {
            settingsData.musicVolume = value;
            PlayerPrefs.SetFloat("musicVol", value);
            PlayerPrefs.Save();
        }

        private void OnSfxVolumeChanged(float value)
        {
            settingsData.sfxVolume = value;
            PlayerPrefs.SetFloat("sfxVol", value);
            PlayerPrefs.Save();
        }

        private void OnDamageNumbersChanged(bool value)
        {
            settingsData.showDamageNumbers = value;
            PlayerPrefs.SetInt("showDmgNums", value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void LoadFromPrefs()
        {
            if (settingsData == null) return;

            settingsData.masterVolume = PlayerPrefs.GetFloat("masterVol", settingsData.masterVolume);
            settingsData.musicVolume = PlayerPrefs.GetFloat("musicVol", settingsData.musicVolume);
            settingsData.sfxVolume = PlayerPrefs.GetFloat("sfxVol", settingsData.sfxVolume);
            settingsData.showDamageNumbers = PlayerPrefs.GetInt("showDmgNums", settingsData.showDamageNumbers ? 1 : 0) == 1;
        }
    }
}
