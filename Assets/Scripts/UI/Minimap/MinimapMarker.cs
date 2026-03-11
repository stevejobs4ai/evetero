// MinimapMarker.cs
// Special minimap icon for quest objectives and points of interest.
// Plays a pulsing scale animation to draw the player's attention.
// Can be added and removed at runtime via the static factory methods.
//
// Usage — add to a world object at runtime:
//
//   MinimapMarker marker = MinimapMarker.AddTo(myQuestTarget,
//       MinimapMarkerType.QuestObjective, "Rescue the merchant");
//   // ... later:
//   MinimapMarker.RemoveFrom(myQuestTarget);
//
// Or place the component in the scene/prefab and configure via Inspector.

using System.Collections;
using UnityEngine;

namespace Evetero
{
    // ── Marker type enum ──────────────────────────────────────────────────────

    public enum MinimapMarkerType
    {
        QuestObjective,     // pulsing gold star
        BossLocation,       // pulsing red skull icon
        Waypoint,           // steady blue diamond
        AllyFlag,           // green banner
        DangerZone,         // flashing orange warning
        Custom              // manually configured via Inspector
    }

    [DisallowMultipleComponent]
    public class MinimapMarker : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Marker")]
        [Tooltip("Semantic type — drives default color and animation.")]
        [SerializeField] private MinimapMarkerType markerType = MinimapMarkerType.QuestObjective;

        [Tooltip("Optional label shown in an expanded map tooltip (future feature).")]
        [SerializeField] private string markerLabel = "";

        [Tooltip("Override color. Leave alpha=0 to use the type default.")]
        [SerializeField] private Color colorOverride = new Color(0, 0, 0, 0);

        [Tooltip("World-space size of the marker icon.")]
        [SerializeField] private float iconSize = 3f;

        [Tooltip("Vertical offset above terrain.")]
        [SerializeField] private float heightOffset = 0.6f;

        [Tooltip("Unity layer index for MinimapIcons.")]
        [SerializeField] private int minimapLayer = 6;

        [Header("Pulse Animation")]
        [Tooltip("Enable pulsing scale animation.")]
        [SerializeField] private bool enablePulse = true;

        [Tooltip("Minimum scale multiplier during pulse.")]
        [SerializeField] private float pulseMin = 0.75f;

        [Tooltip("Maximum scale multiplier during pulse.")]
        [SerializeField] private float pulseMax = 1.35f;

        [Tooltip("Pulse cycles per second.")]
        [SerializeField] private float pulseFrequency = 1.5f;

        // ── Default colors by type ────────────────────────────────────────────

        private static readonly Color ColorQuestObjective = new Color(1.00f, 0.82f, 0.10f); // gold
        private static readonly Color ColorBossLocation   = new Color(0.85f, 0.10f, 0.10f); // red
        private static readonly Color ColorWaypoint       = new Color(0.20f, 0.50f, 0.95f); // blue
        private static readonly Color ColorAllyFlag       = new Color(0.25f, 0.85f, 0.35f); // green
        private static readonly Color ColorDangerZone     = new Color(0.95f, 0.50f, 0.10f); // orange

        // ── Private ───────────────────────────────────────────────────────────

        private GameObject     _iconObject;
        private SpriteRenderer _renderer;
        private Coroutine      _pulseCoroutine;
        private Vector3        _baseIconScale;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            CreateIcon();
        }

        private void OnEnable()
        {
            if (_iconObject != null)
                _iconObject.SetActive(true);

            if (enablePulse && _renderer != null && _pulseCoroutine == null)
                _pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        private void OnDisable()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }

            if (_iconObject != null)
                _iconObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_iconObject == null) return;

            Vector3 pos = transform.position;
            pos.y = heightOffset;
            _iconObject.transform.position = pos;
        }

        private void OnDestroy()
        {
            if (_iconObject != null)
                Destroy(_iconObject);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Marker label — shown in map tooltips (reserved for future use).</summary>
        public string Label
        {
            get => markerLabel;
            set => markerLabel = value;
        }

        /// <summary>
        /// Start or restart the pulse animation.
        /// </summary>
        public void StartPulse()
        {
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);

            enablePulse     = true;
            _pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        /// <summary>
        /// Stop the pulse animation and restore the icon to its base scale.
        /// </summary>
        public void StopPulse()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }

            enablePulse = false;

            if (_iconObject != null)
                _iconObject.transform.localScale = _baseIconScale;
        }

        /// <summary>
        /// Override the marker color at runtime.
        /// </summary>
        public void SetColor(Color color)
        {
            if (_renderer != null)
                _renderer.color = color;
        }

        // ── Static factory helpers ────────────────────────────────────────────

        /// <summary>
        /// Adds a MinimapMarker component to an existing GameObject and configures it.
        /// Returns the created marker for further configuration or removal.
        /// </summary>
        public static MinimapMarker AddTo(
            GameObject target,
            MinimapMarkerType type        = MinimapMarkerType.QuestObjective,
            string label                  = "",
            bool pulse                    = true)
        {
            if (target == null)
            {
                Debug.LogWarning("[MinimapMarker] AddTo: target is null.");
                return null;
            }

            // Remove any existing marker first to avoid duplicates.
            MinimapMarker existing = target.GetComponent<MinimapMarker>();
            if (existing != null)
                Destroy(existing);

            MinimapMarker marker = target.AddComponent<MinimapMarker>();
            marker.markerType  = type;
            marker.markerLabel = label;
            marker.enablePulse = pulse;
            return marker;
        }

        /// <summary>
        /// Removes the MinimapMarker (and its icon) from a GameObject, if present.
        /// </summary>
        public static void RemoveFrom(GameObject target)
        {
            if (target == null) return;
            MinimapMarker marker = target.GetComponent<MinimapMarker>();
            if (marker != null)
                Destroy(marker);
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void CreateIcon()
        {
            _iconObject = new GameObject($"[MinimapMarker] {gameObject.name}");
            _iconObject.layer = minimapLayer;
            _iconObject.transform.rotation   = Quaternion.Euler(90f, 0f, 0f);

            _baseIconScale = new Vector3(iconSize, iconSize, 1f);
            _iconObject.transform.localScale = _baseIconScale;

            _renderer = _iconObject.AddComponent<SpriteRenderer>();
            _renderer.sprite       = GetDefaultSprite();
            _renderer.color        = ResolveColor();
            _renderer.sortingOrder = 20; // render above regular MinimapIcons

            if (enablePulse)
                _pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        private Color ResolveColor()
        {
            if (colorOverride.a > 0.01f) return colorOverride;

            return markerType switch
            {
                MinimapMarkerType.QuestObjective => ColorQuestObjective,
                MinimapMarkerType.BossLocation   => ColorBossLocation,
                MinimapMarkerType.Waypoint       => ColorWaypoint,
                MinimapMarkerType.AllyFlag       => ColorAllyFlag,
                MinimapMarkerType.DangerZone     => ColorDangerZone,
                _                               => Color.white
            };
        }

        /// <summary>
        /// Smooth sinusoidal scale pulse on the icon object.
        /// </summary>
        private IEnumerator PulseRoutine()
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
                float scale = Mathf.Lerp(pulseMin, pulseMax, t);

                if (_iconObject != null)
                    _iconObject.transform.localScale = _baseIconScale * scale;

                yield return null;
            }
        }

        private static Sprite GetDefaultSprite()
        {
            Sprite knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            if (knob != null) return knob;

            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        }
    }
}
