using UnityEngine;
using UnityEngine.UI;

namespace Evetero
{
    // Common sound IDs: button_click, panel_open, panel_close, coin_collect, level_up
    [AddComponentMenu("Evetero/Audio/UI Sound Player")]
    [RequireComponent(typeof(Button))]
    public class UISoundPlayer : MonoBehaviour
    {
        [Tooltip("Sound ID from the active SoundBank.")]
        [SerializeField] private string soundId = "button_click";

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            AudioManager.Instance?.PlaySFX(soundId);
        }
    }
}
