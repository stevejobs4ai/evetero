// BattleSceneSetup.cs
// Bootstraps the Battle scene from BattleContext.PendingBattle.
//
// Reads the pending BattleData and hero party, builds BattleUnit lists,
// positions prefabs (heroes left, enemies right), and calls BattleManager.StartBattle.
//
// Place this component on a root "BattleSetup" GameObject in the Battle scene.
// Wire heroSlotPositions and enemySlotPositions in the Inspector to define
// where each unit stands on screen.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evetero
{
    public class BattleSceneSetup : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Manager / UI")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private BattleUI      battleUI;

        [Header("Spawn Positions")]
        [Tooltip("World positions where heroes stand (left side). " +
                 "Index matches hero party order.")]
        [SerializeField] private Transform[] heroSlotPositions;

        [Tooltip("World positions where enemies stand (right side). " +
                 "Index matches BattleData.enemies order.")]
        [SerializeField] private Transform[] enemySlotPositions;

        [Header("Fallback Party")]
        [Tooltip("Used when BattleContext.HeroParty is null (e.g., in Editor play tests).")]
        [SerializeField] private HeroData[] fallbackHeroParty;

        [Header("Background")]
        [SerializeField] private SpriteRenderer backgroundRenderer;

        [Header("Return Scene")]
        [Tooltip("Scene to load on Defeat or Fled.")]
        [SerializeField] private string overworldSceneName = "Overworld";

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            var battleData = BattleContext.PendingBattle;
            var partyData  = BattleContext.HeroParty ?? fallbackHeroParty;

            BattleContext.ConsumePending();

            if (battleData == null)
            {
                Debug.LogWarning("[BattleSceneSetup] No BattleData found in BattleContext. " +
                                 "Using empty encounter.");
                battleData = ScriptableObject.CreateInstance<BattleData>();
            }

            SetupBackground(battleData);

            var heroes  = BuildHeroUnits(partyData);
            var enemies = BuildEnemyUnits(battleData);

            PositionHeroes(heroes);
            PositionEnemies(enemies, battleData);

            if (battleManager != null)
            {
                battleManager.OnBattleEnded += OnBattleEnded;
                battleManager.StartBattle(heroes, enemies);
            }
            else
            {
                Debug.LogError("[BattleSceneSetup] BattleManager reference is missing.");
            }
        }

        // ── Unit construction ─────────────────────────────────────────────────

        private List<BattleUnit> BuildHeroUnits(HeroData[] party)
        {
            var units = new List<BattleUnit>();
            if (party == null) return units;
            foreach (var data in party)
                if (data != null)
                    units.Add(BattleUnit.FromHero(data));
            return units;
        }

        private List<BattleUnit> BuildEnemyUnits(BattleData data)
        {
            var units = new List<BattleUnit>();
            if (data.enemies != null)
                foreach (var p in data.enemies)
                    if (p.enemy != null)
                        units.Add(BattleUnit.FromEnemy(p.enemy));

            if (data.recruitableEnemies != null)
                foreach (var p in data.recruitableEnemies)
                    if (p.enemy != null)
                        units.Add(BattleUnit.FromRecruitableEnemy(p.enemy));

            return units;
        }

        // ── Positioning ───────────────────────────────────────────────────────

        private void PositionHeroes(List<BattleUnit> heroes)
        {
            // Visual placeholder: log positions for now.
            // In a full implementation spawn hero sprite prefabs here.
            for (int i = 0; i < heroes.Count; i++)
            {
                var pos = i < heroSlotPositions?.Length
                    ? heroSlotPositions[i].position
                    : new Vector3(-4f + i * 1.5f, 0f, 0f);

                Debug.Log($"[BattleSceneSetup] {heroes[i].Name} → hero slot {i} at {pos}");
            }
        }

        private void PositionEnemies(List<BattleUnit> enemies, BattleData data)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                var pos = i < enemySlotPositions?.Length
                    ? enemySlotPositions[i].position
                    : new Vector3(2f + i * 1.5f, 0f, 0f);

                Debug.Log($"[BattleSceneSetup] {enemies[i].Name} → enemy slot {i} at {pos}");
            }
        }

        // ── Background ────────────────────────────────────────────────────────

        private void SetupBackground(BattleData data)
        {
            if (backgroundRenderer != null && data.background != null)
                backgroundRenderer.sprite = data.background;
        }

        // ── Battle ended ──────────────────────────────────────────────────────

        private void OnBattleEnded(BattleResult result)
        {
            Debug.Log($"[BattleSceneSetup] Battle ended: {result}");

            switch (result)
            {
                case BattleResult.Victory:
                    // Stay in scene briefly; a dedicated transition could be wired here.
                    break;
                case BattleResult.Defeat:
                case BattleResult.Fled:
                    SceneManager.LoadScene(overworldSceneName);
                    break;
            }
        }
    }
}
