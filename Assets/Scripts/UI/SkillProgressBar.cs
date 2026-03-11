// SkillProgressBar.cs
// MonoBehaviour that drives a UnityEngine.UI.Slider (0-1) showing XP progress
// toward the next level for a watched skill.
// Call UpdateForSkill() to bind to a hero's HeroSkills component.

using UnityEngine;
using UnityEngine.UI;

namespace Evetero
{
    public class SkillProgressBar : MonoBehaviour
    {
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
        /// Bind this bar to the given skill on the given HeroSkills component.
        /// Unsubscribes from any previous binding automatically.
        /// </summary>
        public void UpdateForSkill(SkillType skill, HeroSkills skills)
        {
            if (_heroSkills != null)
                _heroSkills.OnLevelUp -= OnLevelUp;

            _watchedSkill = skill;
            _heroSkills   = skills;

            if (_heroSkills != null)
                _heroSkills.OnLevelUp += OnLevelUp;

            RefreshBar();
        }

        private void OnLevelUp(SkillType skill, int newLevel)
        {
            if (skill == _watchedSkill)
                RefreshBar();
        }

        private void RefreshBar()
        {
            if (_heroSkills == null || slider == null) return;

            int currentXP = _heroSkills.GetXP(_watchedSkill);
            int level     = _heroSkills.GetLevel(_watchedSkill);

            if (level >= 99)
            {
                slider.value = 1f;
                return;
            }

            int xpForCurrent = SkillSystem.XPForLevel(level);
            int xpForNext    = SkillSystem.XPForLevel(level + 1);
            int range        = xpForNext - xpForCurrent;

            slider.value = range > 0 ? (float)(currentXP - xpForCurrent) / range : 0f;
        }
    }
}
