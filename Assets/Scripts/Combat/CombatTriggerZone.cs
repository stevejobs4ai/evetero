// CombatTriggerZone.cs
// Collider2D trigger that activates an enemy encounter when the hero enters.
// One-shot: disables itself after the encounter is activated.

using UnityEngine;

namespace Evetero
{
    [RequireComponent(typeof(Collider2D))]
    public class CombatTriggerZone : MonoBehaviour
    {
        [Header("Encounter")]
        [Tooltip("Enemy GameObjects that activate when the hero enters the zone.")]
        public EnemyController[] enemies;

        private bool _triggered;

        private void Start()
        {
            // Enemies start inactive; the trigger wakes them
            foreach (var enemy in enemies)
                if (enemy != null)
                    enemy.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;

            var hero = other.GetComponent<HeroController>();
            if (hero == null) return;

            _triggered = true;
            ActivateEncounter(hero);

            // Disable this trigger so it only fires once
            GetComponent<Collider2D>().enabled = false;
        }

        private void ActivateEncounter(HeroController hero)
        {
            Debug.Log("[CombatTriggerZone] Encounter triggered!");

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                enemy.gameObject.SetActive(true);

                if (CombatManager.Instance != null)
                    CombatManager.Instance.RegisterEnemy(enemy);
            }

            if (CombatManager.Instance != null)
                CombatManager.Instance.StartHeroAutoAttack(hero);
        }
    }
}
