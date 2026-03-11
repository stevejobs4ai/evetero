// HeroSelector.cs
// Manages hero selection for the UI layer.
// Place one instance in the scene (e.g. on the HUD root object).
//
// Usage:
//   - Assign heroStatusPanel in the Inspector.
//   - Call Select(hero) from a tap/click handler on each hero portrait or world sprite.
//   - The panel rebuilds automatically; listen to OnHeroSelected for other UI elements.
//
// Editor workflow:
//   - Wire heroStatusPanel directly; no hard-coded hero references needed.
//   - Call Select() from UnityEvent (button OnClick) or from other scripts.

using System;
using UnityEngine;

namespace Evetero
{
    public class HeroSelector : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("The HeroStatusPanel to update when a hero is selected.")]
        [SerializeField] private HeroStatusPanel heroStatusPanel;

        /// <summary>Fired whenever a new hero is selected (or null to deselect).</summary>
        public event Action<HeroController> OnHeroSelected;

        private HeroController _selected;

        /// <summary>The currently selected hero (null if none).</summary>
        public HeroController Selected => _selected;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Select <paramref name="hero"/> and update the HeroStatusPanel.
        /// Calling with the same hero is a no-op.
        /// Pass null to deselect.
        /// </summary>
        public void Select(HeroController hero)
        {
            if (_selected == hero) return;

            _selected = hero;

            if (heroStatusPanel != null)
                heroStatusPanel.SelectHero(hero);

            OnHeroSelected?.Invoke(hero);
        }

        /// <summary>
        /// Deselect the current hero.
        /// </summary>
        public void Deselect() => Select(null);
    }
}
