// SkillProgressBar.cs
// Displays a single skill row: name label, level number, XP progress bar, and XP text.
// Call Bind() to attach to a hero's HeroSkills component.
// Call Refresh() each frame or on a timer to keep XP text current.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evetero
{
    public class SkillProgressBar : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text xpText;

        [Header("Bar")]
        [SerializeField] private Slider slider;

        private HeroSkills _heroSkills;
        private SkillType  _watchedSkill;

        private void Awake()
        {
            if (slider == null)
                slider = GetComponent<Slider>();
        }

        private void OnDestroy()
        {
            if (_heroSkills != null)
                _heroSkills.OnLevelUp -= OnLevelUp;
        }

        /// <summary>
        /// Bind this row to the given skill on the given HeroSkills component.
        /// Unsubscribes from any previous binding automatically.
        /// </summary>
        public void Bind(SkillType skill, HeroSkills skills)
        {
            if (_heroSkills != null)
                _heroSkills.OnLevelUp -= OnLevelUp;

            _watchedSkill = skill;
            _heroSkills   = skills;

            if (_heroSkills != null)
                _heroSkills.OnLevelUp += OnLevelUp;

            if (skillNameText != null)
                skillNameText.text = skill.ToString();

            Refresh();
        }

        /// <summary>
        /// Legacy name kept for backward-compatibility. Prefer Bind().
        /// </summary>
        public void UpdateForSkill(SkillType skill, HeroSkills skills) => Bind(skill, skills);

        /// <summary>
        /// Refreshes the level text, XP text, and slider fill.
        /// Called automatically on level-up; also safe to call on a polling interval.
        /// </summary>
        public void Refresh()
        {
            if (_heroSkills == null) return;

            int level     = _heroSkills.GetLevel(_watchedSkill);
            int currentXP = _heroSkills.GetXP(_watchedSkill);

            if (levelText != null)
                levelText.text = level.ToString();

            if (level >= 99)
            {
                if (xpText != null)
                    xpText.text = "MAX";

                if (slider != null)
                    slider.value = 1f;

                return;
            }

            int xpForCurrent = SkillSystem.XPForLevel(level);
            int xpForNext    = SkillSystem.XPForLevel(level + 1);
            int range        = xpForNext - xpForCurrent;
            int xpIntoLevel  = currentXP - xpForCurrent;

            if (xpText != null)
                xpText.text = $"{xpIntoLevel} / {range}";

            if (slider != null)
                slider.value = range > 0 ? (float)xpIntoLevel / range : 0f;
        }

        private void OnLevelUp(SkillType skill, int newLevel)
        {
            if (skill == _watchedSkill)
                Refresh();
        }
    }
}
