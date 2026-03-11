// MinimapIcon.cs
// Attach to any world-space GameObject to register it as a tracked entity on
// the minimap. On Awake it creates a small icon sprite in the MinimapIcons
// layer which the MinimapCamera renders into its RenderTexture.
//
// Layer setup:
//   Add a layer named "MinimapIcons" (e.g. layer 6) in Project Settings → Tags & Layers.
//   Set the MinimapCamera culling mask to include this layer.
//   Exclude it from the Main Camera culling mask so icons are only visible on the minimap.
//
// Icon visuals:
//   If no custom sprite is assigned the script generates a simple Unity default
//   sprite (a white quad) and tints it with the configured color. For production,
//   assign per-type sprites (dot, diamond, square, triangle) in the Inspector or
//   via the static DefaultIconSprite field.

using UnityEngine;

namespace Evetero
{
    // ── Entity type enum ──────────────────────────────────────────────────────

    public enum MinimapEntityType
    {
        Hero,           // green circle dot
        ResourceNode,   // colored diamond — color driven by ResourceType
        Building,       // white square
        Enemy,          // red triangle
        NPC,            // yellow dot
        QuestTarget,    // pulsing gold star (see MinimapMarker)
        Custom          // fully configurable from Inspector
    }

    [DisallowMultipleComponent]
    public class MinimapIcon : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Entity")]
        [Tooltip("Category of this entity — determines default color/shape.")]
        [SerializeField] private MinimapEntityType entityType = MinimapEntityType.Custom;

        [Tooltip("Override color. Leave alpha=0 to use the type's default color.")]
        [SerializeField] private Color colorOverride = new Color(0, 0, 0, 0);

        [Tooltip("Optional custom sprite. If null, a solid quad is used.")]
        [SerializeField] private Sprite iconSprite;

        [Header("Icon Transform")]
        [Tooltip("World-space size of the icon quad.")]
        [SerializeField] private float iconSize = 2f;

        [Tooltip("Vertical offset above the parent so it renders above terrain.")]
        [SerializeField] private float heightOffset = 0.5f;

        [Tooltip("Unity layer index for MinimapIcons. Must match the layer set in Project Settings.")]
        [SerializeField] private int minimapLayer = 6;

        // ── Private ───────────────────────────────────────────────────────────

        private GameObject   _iconObject;
        private SpriteRenderer _renderer;

        // ── Default colors by type ────────────────────────────────────────────

        private static readonly Color ColorHero         = new Color(0.20f, 0.85f, 0.30f); // green
        private static readonly Color ColorEnemy        = new Color(0.90f, 0.15f, 0.15f); // red
        private static readonly Color ColorBuilding     = Color.white;
        private static readonly Color ColorNPC          = new Color(0.95f, 0.85f, 0.10f); // yellow
        private static readonly Color ColorResourceWood = new Color(0.40f, 0.75f, 0.20f); // olive green
        private static readonly Color ColorResourceGold = new Color(0.95f, 0.75f, 0.10f); // gold
        private static readonly Color ColorResourceStone= new Color(0.65f, 0.65f, 0.65f); // grey
        private static readonly Color ColorResourceMana = new Color(0.45f, 0.20f, 0.85f); // purple
        private static readonly Color ColorResourceFood = new Color(0.85f, 0.45f, 0.15f); // orange
        private static readonly Color ColorResourceDefault = new Color(0.60f, 0.90f, 0.90f); // teal

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            CreateIcon();
        }

        private void LateUpdate()
        {
            if (_iconObject == null) return;

            // Keep icon centered on parent, at fixed height above terrain.
            Vector3 pos = transform.position;
            pos.y = heightOffset;
            _iconObject.transform.position = pos;
        }

        private void OnDestroy()
        {
            if (_iconObject != null)
                Destroy(_iconObject);
        }

        private void OnDisable()
        {
            if (_iconObject != null)
                _iconObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_iconObject != null)
                _iconObject.SetActive(true);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Override the icon color at runtime (e.g. ally vs enemy team coloring).
        /// </summary>
        public void SetColor(Color color)
        {
            if (_renderer != null)
                _renderer.color = color;
        }

        /// <summary>
        /// Show or hide this minimap icon without destroying it.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_iconObject != null)
                _iconObject.SetActive(visible);
        }

        /// <summary>
        /// Convenience initializer for ResourceNode icons — sets the correct
        /// resource color. Call after Awake (e.g. from WorldNodeInteractable.Start).
        /// </summary>
        public void InitAsResourceNode(ResourceType resourceType)
        {
            entityType = MinimapEntityType.ResourceNode;
            Color col  = ResourceTypeToColor(resourceType);
            SetColor(col);
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void CreateIcon()
        {
            _iconObject = new GameObject($"[MinimapIcon] {gameObject.name}");
            _iconObject.layer = minimapLayer;

            // Orient flat on XZ plane so the top-down camera sees it correctly.
            _iconObject.transform.rotation   = Quaternion.Euler(90f, 0f, 0f);
            _iconObject.transform.localScale  = new Vector3(iconSize, iconSize, 1f);

            _renderer = _iconObject.AddComponent<SpriteRenderer>();
            _renderer.sprite        = iconSprite != null ? iconSprite : GetDefaultSprite();
            _renderer.color         = ResolveColor();
            _renderer.sortingOrder  = 10; // render above terrain icons
        }

        private Color ResolveColor()
        {
            // Explicit override with non-transparent alpha wins.
            if (colorOverride.a > 0.01f)
                return colorOverride;

            return entityType switch
            {
                MinimapEntityType.Hero         => ColorHero,
                MinimapEntityType.Enemy        => ColorEnemy,
                MinimapEntityType.Building     => ColorBuilding,
                MinimapEntityType.NPC          => ColorNPC,
                MinimapEntityType.ResourceNode => ColorResourceDefault,
                _                              => Color.white
            };
        }

        private static Color ResourceTypeToColor(ResourceType type) => type switch
        {
            ResourceType.Wood or ResourceType.OakLog or ResourceType.WillowLog or ResourceType.YewLog
                => ColorResourceWood,
            ResourceType.Gold
                => ColorResourceGold,
            ResourceType.Stone or ResourceType.Iron or ResourceType.CopperOre or ResourceType.TinOre
            or ResourceType.CoalOre or ResourceType.MithrilOre
                => ColorResourceStone,
            ResourceType.Mana
                => ColorResourceMana,
            ResourceType.Food or ResourceType.CookedFish
                => ColorResourceFood,
            _ => ColorResourceDefault
        };

        // Returns the built-in Unity white circle sprite (or white quad as fallback).
        private static Sprite GetDefaultSprite()
        {
            // Unity's built-in UI/Knob is a soft circle — use it if available.
            Sprite knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            if (knob != null) return knob;

            // Fallback: 4x4 white texture quad.
            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        }
    }
}
