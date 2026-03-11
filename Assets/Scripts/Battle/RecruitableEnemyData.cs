// RecruitableEnemyData.cs
// Extends EnemyData with Fire Emblem-style recruitment fields.
//
// Usage: Right-click in Project → Create → Evetero → Battle → Recruitable Enemy
// Assign to BattleData.recruitableEnemies[] alongside placement positions.

using UnityEngine;

namespace Evetero
{
    [CreateAssetMenu(menuName = "Evetero/Battle/Recruitable Enemy", fileName = "NewRecruitableEnemy")]
    public class RecruitableEnemyData : EnemyData
    {
        [Header("Recruitment")]
        [Tooltip("The hero who must use the Talk action to trigger recruitment. " +
                 "Leave null to allow any hero.")]
        public HeroData requiredHero;

        [Tooltip("Dialogue played when a hero successfully initiates recruitment.")]
        public DialogueData recruitDialogue;

        [Tooltip("If set, this unit joins the hero roster as a permanent hero after battle.")]
        public HeroData joinHeroData;
    }
}
