// BattleData.cs
// ScriptableObject defining a complete battle encounter.
//
// Usage: Right-click in Project → Create → Evetero → Battle → Battle Data
// Assign a BattleData to BattleTriggerZone to link an encounter to a world trigger.

using UnityEngine;

namespace Evetero
{
    // ── Placement structs ─────────────────────────────────────────────────────

    [System.Serializable]
    public struct EnemyPlacement
    {
        [Tooltip("Enemy type to spawn.")]
        public EnemyData enemy;

        [Tooltip("Position in the battle scene (enemy side).")]
        public Vector2 position;
    }

    [System.Serializable]
    public struct RecruitablePlacement
    {
        [Tooltip("Recruitable enemy to spawn.")]
        public RecruitableEnemyData enemy;

        [Tooltip("Position in the battle scene (enemy side).")]
        public Vector2 position;
    }

    // ── BattleData ScriptableObject ───────────────────────────────────────────

    [CreateAssetMenu(menuName = "Evetero/Battle/Battle Data", fileName = "NewBattle")]
    public class BattleData : ScriptableObject
    {
        [Header("Identity")]
        public string battleName;

        [TextArea(1, 3)]
        public string description;

        [Header("Enemies")]
        [Tooltip("Standard enemies placed on the enemy side.")]
        public EnemyPlacement[] enemies;

        [Tooltip("Recruitable enemies — can be talked to mid-battle.")]
        public RecruitablePlacement[] recruitableEnemies;

        [Header("Scene")]
        [Tooltip("Background sprite rendered behind combatants.")]
        public Sprite background;

        [Tooltip("AudioManager music tag played during this battle. e.g. 'battle_village'.")]
        public string musicTag;

        // ── Helpers ───────────────────────────────────────────────────────────

        public int TotalEnemyCount =>
            (enemies?.Length ?? 0) + (recruitableEnemies?.Length ?? 0);
    }
}
