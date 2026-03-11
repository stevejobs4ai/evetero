// MinimapCamera.cs
// Secondary orthographic camera that renders a top-down view of the world
// to a RenderTexture, which is then displayed in a RawImage UI element.
//
// Setup:
//   1. Create a child Camera on your scene camera (or a standalone Camera object).
//   2. Add this component to it.
//   3. Create a RenderTexture asset (e.g. 256x256, R8G8B8A8) and assign it to
//      renderTexture. Also assign it as the camera's Target Texture.
//   4. Set the camera's Culling Mask to include only layers: Ground, MinimapIcons.
//   5. Assign the RawImage in your MinimapUI Canvas to the same RenderTexture.

using UnityEngine;

namespace Evetero
{
    [RequireComponent(typeof(Camera))]
    public class MinimapCamera : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Tracking")]
        [Tooltip("Transform to follow (usually the main camera or a world center anchor). " +
                 "Leave null to stay fixed at the spawn position.")]
        [SerializeField] private Transform followTarget;

        [Tooltip("If true, only XZ position is synced (height is fixed).")]
        [SerializeField] private bool lockHeight = true;

        [Tooltip("Fixed Y height for the minimap camera when lockHeight is true.")]
        [SerializeField] private float cameraHeight = 50f;

        [Header("Zoom")]
        [Tooltip("Orthographic size at default zoom. Smaller = more zoomed in.")]
        [SerializeField] private float defaultOrthographicSize = 30f;

        [Tooltip("Minimum orthographic size (max zoom in).")]
        [SerializeField] private float minOrthographicSize = 5f;

        [Tooltip("Maximum orthographic size (max zoom out).")]
        [SerializeField] private float maxOrthographicSize = 80f;

        [Header("Render Texture")]
        [Tooltip("The RenderTexture this camera renders into. " +
                 "Also assign this as the camera's Target Texture in the Inspector.")]
        [SerializeField] private RenderTexture renderTexture;

        // ── Private ───────────────────────────────────────────────────────────

        private Camera _cam;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic  = true;
            _cam.orthographicSize = defaultOrthographicSize;

            if (renderTexture != null && _cam.targetTexture == null)
                _cam.targetTexture = renderTexture;
        }

        private void LateUpdate()
        {
            if (followTarget == null) return;

            Vector3 pos = followTarget.position;

            if (lockHeight)
                pos.y = cameraHeight;

            transform.position = pos;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Smoothly sets the orthographic size, clamped to the configured range.
        /// Call from MinimapUI zoom buttons.
        /// </summary>
        public void SetZoom(float orthoSize)
        {
            _cam.orthographicSize = Mathf.Clamp(orthoSize, minOrthographicSize, maxOrthographicSize);
        }

        /// <summary>
        /// Adjusts zoom by a delta (positive = zoom in, negative = zoom out).
        /// </summary>
        public void AdjustZoom(float delta)
        {
            SetZoom(_cam.orthographicSize - delta);
        }

        /// <summary>Current orthographic size.</summary>
        public float CurrentZoom => _cam.orthographicSize;

        /// <summary>Default orthographic size.</summary>
        public float DefaultZoom => defaultOrthographicSize;

        /// <summary>
        /// Converts a minimap UV coordinate (0–1 in both axes) to a world
        /// XZ position. Used by MinimapUI tap-to-pan.
        /// </summary>
        /// <param name="normalizedPos">UV position on the minimap image.</param>
        public Vector3 MinimapUVToWorldPosition(Vector2 normalizedPos)
        {
            float halfSize = _cam.orthographicSize;
            float aspect   = _cam.aspect;

            Vector3 origin = transform.position;
            float worldX = origin.x + (normalizedPos.x - 0.5f) * 2f * halfSize * aspect;
            float worldZ = origin.z + (normalizedPos.y - 0.5f) * 2f * halfSize;

            return new Vector3(worldX, 0f, worldZ);
        }
    }
}
