// SkillLevelsPanel.cs
// Dedicated panel that displays all non-combat skill levels for the selected hero.
// Shows each skill's name, current level (1-99), and XP progress bar toward next level.
//
// Setup:
//   1. Add this component to a UI panel GameObject.
//   2. Set heroNameText to a TMP label at the top of the panel (shows selected hero's name).
//   3. Set skillsContainer to the Content RectTransform of a ScrollRect.
//   4. Assign the SkillProgressBar prefab to skillRowPrefab.
//   5. (Optional) Assign a HeroSelector reference; the panel auto-subscribes to selection events.
//      If no HeroSelector is assigned, call SelectHero() manually from other scripts.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Evetero
{
    public class SkillLevelsPanel : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text heroNameText;

        [Header("Skills List")]
        [Tooltip("Content RectTransform of the ScrollRect that holds skill rows.")]
        [SerializeField] private Transform skillsContainer;
        [Tooltip("Prefab with a SkillProgressBar component (name label, level label, XP bar).")]
        [SerializeField] private SkillProgressBar skillRowPrefab;

        [Header("Hero Selection")]
        [Tooltip("Optional. Panel subscribes to OnHeroSelected automatically if assigned.")]
        [SerializeField] private HeroSelector heroSelector;

        private HeroController             _hero;
        private HeroSkills                 _heroSkills;
        private List<SkillProgressBar>     _rows = new List<SkillProgressBar>();

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            if (heroSelector != null)
                heroSelector.OnHeroSelected += SelectHero;
        }

        private void OnDisable()
        {
            if (heroSelector != null)
                heroSelector.OnHeroSelected -= SelectHero;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Switch the panel to display <paramref name="newHero"/>'s skills.
        /// Rebuilds all skill rows immediately. Pass null to clear.
        /// </summary>
        public void SelectHero(HeroController newHero)
        {
            _hero      = newHero;
            _heroSkills = newHero != null ? newHero.GetComponent<HeroSkills>() : null;

            RefreshHeader();
            RebuildRows();
        }

        // ── Header ────────────────────────────────────────────────────────────

        private void RefreshHeader()
        {
            if (heroNameText == null) return;

            if (_hero == null)
            {
                heroNameText.text = "No Hero Selected";
                return;
            }

            heroNameText.text = _hero.heroData != null ? _hero.heroData.heroName : _hero.name;
        }

        // ── Skills list ───────────────────────────────────────────────────────

        private void RebuildRows()
        {
            if (skillRowPrefab == null || skillsContainer == null) return;

            foreach (var row in _rows)
                if (row != null) Destroy(row.gameObject);
            _rows.Clear();

            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                SkillProgressBar row = Instantiate(skillRowPrefab, skillsContainer);
                row.name = $"SkillRow_{skill}";
                row.Bind(skill, _heroSkills);
                _rows.Add(row);
            }
        }
    }
}
