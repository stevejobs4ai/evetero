// CameraController.cs
// Simple 2D camera controller.
//   - WASD / arrow keys to pan
//   - Mouse scroll wheel to zoom (orthographic size clamped 3–20)
//
// Attach to the Main Camera GameObject.

using UnityEngine;

namespace Evetero
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Pan")]
        [Tooltip("World units per second the camera moves when a key is held.")]
        public float panSpeed = 10f;

        [Header("Zoom")]
        [Tooltip("How fast the orthographic size changes per scroll tick.")]
        public float zoomSpeed = 3f;

        [Tooltip("Minimum orthographic size (most zoomed-in).")]
        public float minZoom = 3f;

        [Tooltip("Maximum orthographic size (most zoomed-out).")]
        public float maxZoom = 20f;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            HandlePan();
            HandleZoom();
        }

        private void HandlePan()
        {
            float h = Input.GetAxisRaw("Horizontal"); // A/D or ←/→
            float v = Input.GetAxisRaw("Vertical");   // W/S or ↑/↓

            if (h == 0f && v == 0f) return;

            Vector3 delta = new Vector3(h, v, 0f) * (panSpeed * Time.deltaTime);
            transform.position += delta;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0f) return;

            _cam.orthographicSize = Mathf.Clamp(
                _cam.orthographicSize - scroll * zoomSpeed,
                minZoom,
                maxZoom);
        }
    }
}
