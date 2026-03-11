// MinimapUI.cs
// The UI panel that displays the minimap RenderTexture and handles user interaction.
//
// Features:
//   • Compact minimap in the top-right corner (circular mask or square, configurable).
//   • Tap the minimap to expand to a full-screen overlay.
//   • Zoom +/- buttons on the expanded view.
//   • Tap anywhere on the expanded map to pan the main camera to that world position.
//   • "Close" button (or tap outside) to collapse back to compact mode.
//
// Setup:
//   1. Create a Canvas (Screen Space – Overlay) if you don't have one.
//   2. Add a Panel for the compact minimap (top-right corner, e.g. 200x200).
//      • Add a RawImage child — assign the MinimapCamera's RenderTexture here.
//      • Optionally add a Mask component + circular Image for the circular look.
//      • Add an EventTrigger (or Button) that calls ToggleExpanded().
//   3. Add a separate full-screen Panel for the expanded view.
//      • RawImage child for the same RenderTexture.
//      • ZoomIn / ZoomOut buttons calling ZoomIn() / ZoomOut().
//      • Close button calling CollapseMap().
//      • EventTrigger on the RawImage calling OnExpandedMapTapped(eventData).
//   4. Wire all references in the Inspector.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Evetero
{
    public class MinimapUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Panels")]
        [Tooltip("The small always-visible minimap panel (top-right corner).")]
        [SerializeField] private GameObject compactPanel;

        [Tooltip("Full-screen map overlay shown when the player expands the minimap.")]
        [SerializeField] private GameObject expandedPanel;

        [Header("Raw Images")]
        [Tooltip("RawImage in the compact panel — assign the RenderTexture here.")]
        [SerializeField] private RawImage compactImage;

        [Tooltip("RawImage in the expanded panel — assign the same RenderTexture.")]
        [SerializeField] private RawImage expandedImage;

        [Header("Zoom")]
        [Tooltip("Reference to the MinimapCamera whose orthographic size we drive.")]
        [SerializeField] private MinimapCamera minimapCamera;

        [Tooltip("How much orthographic size changes per zoom button press.")]
        [SerializeField] private float zoomStep = 5f;

        [Header("Camera Pan")]
        [Tooltip("The main scene camera to pan when the player taps the expanded map.")]
        [SerializeField] private Camera mainCamera;

        [Tooltip("How fast (lerp speed) the main camera pans to the tapped location.")]
        [SerializeField] private float panLerpSpeed = 8f;

        [Tooltip("Fixed Y height of the main camera after panning.")]
        [SerializeField] private float mainCameraHeight = 15f;

        // ── Private ───────────────────────────────────────────────────────────

        private bool    _expanded;
        private Vector3 _panTarget;
        private bool    _isPanning;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            _panTarget = mainCamera != null ? mainCamera.transform.position : Vector3.zero;

            SetExpanded(false);
        }

        private void Update()
        {
            if (!_isPanning || mainCamera == null) return;

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                _panTarget,
                Time.deltaTime * panLerpSpeed);

            if (Vector3.Distance(mainCamera.transform.position, _panTarget) < 0.05f)
            {
                mainCamera.transform.position = _panTarget;
                _isPanning = false;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Toggle between compact and expanded minimap. Wire to tap on compact panel.
        /// </summary>
        public void ToggleExpanded()
        {
            SetExpanded(!_expanded);
        }

        /// <summary>
        /// Collapse the expanded map back to compact mode. Wire to the Close button.
        /// </summary>
        public void CollapseMap()
        {
            SetExpanded(false);
        }

        /// <summary>
        /// Zoom the minimap camera in. Wire to the ZoomIn (+) button on the expanded panel.
        /// </summary>
        public void ZoomIn()
        {
            if (minimapCamera != null)
                minimapCamera.AdjustZoom(zoomStep);
        }

        /// <summary>
        /// Zoom the minimap camera out. Wire to the ZoomOut (-) button on the expanded panel.
        /// </summary>
        public void ZoomOut()
        {
            if (minimapCamera != null)
                minimapCamera.AdjustZoom(-zoomStep);
        }

        /// <summary>
        /// Reset the minimap zoom to its default level. Optional — wire to a Reset button.
        /// </summary>
        public void ResetZoom()
        {
            if (minimapCamera != null)
                minimapCamera.SetZoom(minimapCamera.DefaultZoom);
        }

        /// <summary>
        /// Called by an EventTrigger (PointerClick) on the expanded RawImage.
        /// Converts the tap position to a world location and begins panning the main camera.
        /// </summary>
        public void OnExpandedMapTapped(BaseEventData eventData)
        {
            if (minimapCamera == null || mainCamera == null) return;
            if (!(eventData is PointerEventData pointerData)) return;

            RectTransform rectTransform = expandedImage.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    pointerData.position,
                    pointerData.pressEventCamera,
                    out Vector2 localPoint)) return;

            // Convert local point to normalized UV (0–1).
            Rect rect = rectTransform.rect;
            Vector2 uv = new Vector2(
                (localPoint.x - rect.x) / rect.width,
                (localPoint.y - rect.y) / rect.height);

            // Clamp to valid range.
            uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

            // Convert UV → world position via MinimapCamera.
            Vector3 worldPos = minimapCamera.MinimapUVToWorldPosition(uv);
            worldPos.y = mainCameraHeight;

            _panTarget = worldPos;
            _isPanning  = true;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void SetExpanded(bool expanded)
        {
            _expanded = expanded;

            if (compactPanel  != null) compactPanel.SetActive(!expanded);
            if (expandedPanel != null) expandedPanel.SetActive(expanded);
        }
    }
}
