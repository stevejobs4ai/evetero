using System.Collections;
using TMPro;
using UnityEngine;

namespace Evetero
{
    public class DamageNumberPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private SettingsData settingsData;

        public void Show(int damage, Vector3 worldPos)
        {
            if (settingsData != null && !settingsData.showDamageNumbers)
            {
                Destroy(gameObject);
                return;
            }

            if (damageText != null)
                damageText.text = damage.ToString();

            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                transform.position = screenPos;
            }

            StartCoroutine(AnimateAndDestroy());
        }

        private IEnumerator AnimateAndDestroy()
        {
            Vector3 startPos = transform.position;
            float elapsed = 0f;
            float duration = 1f;
            float riseAmount = 80f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.position = startPos + Vector3.up * (riseAmount * t);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
