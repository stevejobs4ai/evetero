// HeroStatusPanel.cs
// Canvas overlay MonoBehaviour that shows hero name, HP, active skill info,
// and a scrollable list of all skill levels with XP progress bars.
// Refreshes every 0.5s via InvokeRepeating. Wire fields in the Inspector.
//
// Skills List setup:
//   1. Add a ScrollRect to this panel (or a child).
//   2. Set skillsContainer to the ScrollRect's Content RectTransform.
//   3. Assign a SkillProgressBar prefab to skillRowPrefab.
//      The prefab should contain: skillNameText (TMP), levelText (TMP),
//      xpText (TMP), and a Slider for the XP bar.
//
// Hero selection:
//   Call SelectHero(heroController) at runtime to switch which hero is displayed.
//   The panel re-populates skill rows and refreshes immediately.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Evetero
{
    public class HeroStatusPanel : MonoBehaviour
    {
        [Header("Hero")]
        [Tooltip("Initial hero to display. Leave blank to auto-find, or call SelectHero() at runtime.")]
        [SerializeField] private HeroController hero;

        [Header("Active-Skill Labels")]
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text skillXPText;
        [SerializeField] private TMP_Text skillLevelText;

        [Header("Skills List")]
        [Tooltip("Content RectTransform of the ScrollRect that holds skill rows.")]
        [SerializeField] private Transform skillsContainer;
        [Tooltip("Prefab containing a SkillProgressBar component (name, level, XP bar).")]
        [SerializeField] private SkillProgressBar skillRowPrefab;

        private HeroSkills             _heroSkills;
        private GatheringAction        _gatheringAction;
        private List<SkillProgressBar> _skillBars = new List<SkillProgressBar>();

        private void Start()
        {
            if (hero == null)
                hero = FindObjectOfType<HeroController>();

            BindHero(hero);
            InvokeRepeating(nameof(Refresh), 0f, 0.5f);
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Switch the panel to display <paramref name="newHero"/>.
        /// Rebuilds skill rows and refreshes all labels immediately.
        /// Pass null to clear the panel.
        /// </summary>
        public void SelectHero(HeroController newHero)
        {
            BindHero(newHero);
            Refresh();
        }

        // ── Hero binding ─────────────────────────────────────────────────────────

        private void BindHero(HeroController newHero)
        {
            hero             = newHero;
            _heroSkills      = newHero != null ? newHero.GetComponent<HeroSkills>()      : null;
            _gatheringAction = newHero != null ? newHero.GetComponent<GatheringAction>() : null;

            PopulateSkillRows();
        }

        // ── Skills list ──────────────────────────────────────────────────────────

        private void PopulateSkillRows()
        {
            if (skillRowPrefab == null || skillsContainer == null) return;

            // Clear any existing rows before (re-)building.
            foreach (var bar in _skillBars)
                if (bar != null) Destroy(bar.gameObject);
            _skillBars.Clear();

            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                SkillProgressBar row = Instantiate(skillRowPrefab, skillsContainer);
                row.name = $"SkillRow_{skill}";
                row.Bind(skill, _heroSkills);
                _skillBars.Add(row);
            }
        }

        // ── Periodic refresh ─────────────────────────────────────────────────────

        private void Refresh()
        {
            if (hero == null) return;

            RefreshHeader();
            RefreshActiveSkillLabels();
            RefreshSkillBars();
        }

        private void RefreshHeader()
        {
            if (heroNameText != null)
                heroNameText.text = hero.heroData != null ? hero.heroData.heroName : "Hero";

            if (hpText != null)
            {
                int maxHP = hero.heroData != null ? hero.heroData.baseStats.maxHP : 0;
                hpText.text = $"HP: {hero.currentHP} / {maxHP}";
            }
        }

        private void RefreshActiveSkillLabels()
        {
            SkillType activeSkill = GetActiveSkill();

            if (skillNameText != null)
                skillNameText.text = $"Skill: {activeSkill}";

            if (_heroSkills != null)
            {
                if (skillXPText != null)
                    skillXPText.text = $"XP: {_heroSkills.GetXP(activeSkill)}";

                if (skillLevelText != null)
                    skillLevelText.text = $"Lv: {_heroSkills.GetLevel(activeSkill)}";
            }
        }

        private void RefreshSkillBars()
        {
            foreach (var bar in _skillBars)
                bar.Refresh();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private SkillType GetActiveSkill()
        {
            if (_gatheringAction != null
                && _gatheringAction.IsGathering
                && _gatheringAction.targetNode != null)
            {
                return ResourceToSkill(_gatheringAction.targetNode.resourceType);
            }
            return SkillType.Woodcutting;
        }

        private static SkillType ResourceToSkill(ResourceType resource) =>
            resource switch
            {
                ResourceType.Wood  => SkillType.Woodcutting,
                ResourceType.Stone => SkillType.Mining,
                ResourceType.Iron  => SkillType.Mining,
                ResourceType.Food  => SkillType.Fishing,
                ResourceType.Gold  => SkillType.Crafting,
                ResourceType.Mana  => SkillType.Crafting,
                _                  => SkillType.Woodcutting,
            };
    }
}
