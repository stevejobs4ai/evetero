// EnemyData.cs
// ScriptableObject defining a single enemy type in Evetero.
//
// Usage: Right-click in Project → Create → Evetero → Combat → EnemyData

using UnityEngine;

namespace Evetero
{
    [CreateAssetMenu(menuName = "Evetero/Combat/EnemyData", fileName = "NewEnemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName;

        [TextArea(2, 4)]
        public string description;

        [Header("Stats")]
        [Tooltip("Maximum hit points.")]
        public int maxHP;

        [Tooltip("Raw attack power before defense reduction.")]
        public int attackPower;

        [Tooltip("Reduces incoming physical damage.")]
        public int defense;

        [Tooltip("Movement speed in units per second.")]
        public float moveSpeed;

        [Header("Combat")]
        [Tooltip("Seconds between each attack.")]
        public float attackIntervalSeconds = 2.0f;

        [Header("Rewards")]
        [Tooltip("Flat XP dropped on defeat (logged for now).")]
        public int xpReward;

        [Tooltip("Resource type dropped on defeat.")]
        public ResourceType rewardResourceType;

        [Tooltip("Amount of reward resource dropped.")]
        public int rewardAmount;
    }
}
