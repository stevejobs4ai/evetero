using UnityEngine;

namespace Evetero
{
    [AddComponentMenu("Evetero/Audio/Ambient Zone")]
    public class AmbientZone : MonoBehaviour
    {
        [Header("Ambient Track")]
        [Tooltip("Sound ID from the active SoundBank. Takes priority over direct clip.")]
        [SerializeField] private string soundId;

        [Tooltip("Direct clip — used if soundId is empty.")]
        [SerializeField] private AudioClip ambientClip;

        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.6f;

        [SerializeField] private float crossfadeDuration = 2f;

        [Header("Trigger Filter")]
        [Tooltip("Only trigger for objects with this tag. Leave empty to trigger for anything.")]
        [SerializeField] private string triggerTag = "MainCamera";

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;
            Activate();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;
            Activate();
        }

        private void Activate()
        {
            if (AudioManager.Instance == null) return;

            if (!string.IsNullOrEmpty(soundId))
                AudioManager.Instance.PlayAmbient(soundId, crossfadeDuration);
            else if (ambientClip != null)
                AudioManager.Instance.PlayAmbientClip(ambientClip, volume, crossfadeDuration);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.25f);
            var col = GetComponent<Collider>();
            if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);

            var col2d = GetComponent<Collider2D>();
            if (col2d != null) Gizmos.DrawCube(col2d.bounds.center, col2d.bounds.size);
        }
#endif
    }
}
