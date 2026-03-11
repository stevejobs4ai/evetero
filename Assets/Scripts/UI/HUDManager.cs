// HUDManager.cs
// Lightweight coordinator that holds refs to all HUD panels.
// On Awake it wires the hero to any panels that were not set in the Inspector.
// Use ShowHUD() / HideHUD() for later show/hide control.

using UnityEngine;

namespace Evetero
{
    public class HUDManager : MonoBehaviour
    {
        [SerializeField] private ResourceHUD       resourceHUD;
        [SerializeField] private HeroStatusPanel   heroStatusPanel;
        [SerializeField] private SkillProgressBar  skillProgressBar;

        [Header("Auto-wiring (leave null to use FindObjectOfType)")]
        [SerializeField] private HeroController hero;

        private void Awake()
        {
            if (hero == null)
                hero = FindObjectOfType<HeroController>();

            if (hero != null && skillProgressBar != null)
            {
                HeroSkills heroSkills = hero.GetComponent<HeroSkills>();
                if (heroSkills != null)
                    skillProgressBar.UpdateForSkill(SkillType.Woodcutting, heroSkills);
            }
        }

        public void ShowHUD()
        {
            gameObject.SetActive(true);
        }

        public void HideHUD()
        {
            gameObject.SetActive(false);
        }
    }
}
