// BattleTriggerZone.cs
// Collider2D trigger that starts a turn-based battle when a hero enters.
// One-shot: disables itself after the encounter is triggered.
//
// Companion to the original CombatTriggerZone (which uses the [Obsolete]
// real-time CombatManager).  Use this for new content.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evetero
{
    [RequireComponent(typeof(Collider2D))]
    public class BattleTriggerZone : MonoBehaviour
    {
        [Header("Encounter")]
        [Tooltip("The battle encounter to start.")]
        public BattleData battleData;

        [Tooltip("Name of the Battle scene to load.")]
        public string battleSceneName = "BattleScene";

        private bool _triggered;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;

            var hero = other.GetComponent<HeroController>();
            if (hero == null) return;

            _triggered = true;
            GetComponent<Collider2D>().enabled = false;

            Debug.Log($"[BattleTriggerZone] Triggering battle: {battleData?.battleName}");

            BattleContext.PendingBattle = battleData;
            // Hero party is populated by the overworld roster system (not yet implemented);
            // BattleSceneSetup falls back to its inspector-wired fallback party.

            SceneManager.LoadScene(battleSceneName);
        }
    }
}
