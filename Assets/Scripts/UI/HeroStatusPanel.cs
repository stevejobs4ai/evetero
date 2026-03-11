// HeroStatusPanel.cs
// Canvas overlay MonoBehaviour that shows hero name, HP, and active skill info.
// Refreshes every 0.5s via InvokeRepeating. Wire fields in the Inspector.

using TMPro;
using UnityEngine;

namespace Evetero
{
    public class HeroStatusPanel : MonoBehaviour
    {
        [SerializeField] private HeroController hero;

        [Header("Labels")]
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text skillXPText;
        [SerializeField] private TMP_Text skillLevelText;

        private HeroSkills      _heroSkills;
        private GatheringAction _gatheringAction;

        private void Start()
        {
            if (hero == null)
                hero = FindObjectOfType<HeroController>();

            if (hero != null)
            {
                _heroSkills      = hero.GetComponent<HeroSkills>();
                _gatheringAction = hero.GetComponent<GatheringAction>();
            }

            InvokeRepeating(nameof(Refresh), 0f, 0.5f);
        }

        private void Refresh()
        {
            if (hero == null) return;

            if (heroNameText != null)
                heroNameText.text = hero.heroData != null ? hero.heroData.heroName : "Hero";

            if (hpText != null)
            {
                int maxHP = hero.heroData != null ? hero.heroData.baseStats.maxHP : 0;
                hpText.text = $"HP: {hero.currentHP} / {maxHP}";
            }

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
