// ChapterOneDirector.cs
// Orchestrates the Chapter 1 Ember Village tutorial flow.
//
// State machine: Intro → Gathering → Combat → Done
// - Intro   : plays Mira's opening dialogue 1 second after scene load
// - Gathering: plays gather reaction on the first resource node click
// - Combat  : plays combat warning when CombatManager fires OnCombatStart
//
// Wire in Inspector:
//   narratorDialogue → Narrator_Panel's DialogueSystem
//   miraDialogue     → Mira_Dialogue's DialogueSystem
//   introData        → Dialogue_MiraIntro.asset
//   gatherData       → Dialogue_MiraGatherReaction.asset
//   combatData       → Dialogue_MiraCombatWarning.asset

using System.Collections;
using UnityEngine;

namespace Evetero
{
    public class ChapterOneDirector : MonoBehaviour
    {
        public enum DirectorState { Intro, Gathering, Combat, Done }

        [Header("Dialogue Systems")]
        public DialogueSystem narratorDialogue;
        public DialogueSystem miraDialogue;

        [Header("Dialogue Data")]
        public DialogueData introData;
        public DialogueData gatherData;
        public DialogueData combatData;

        private DirectorState _state = DirectorState.Intro;
        private bool _gatherReactionPlayed;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            WorldNodeInteractable.OnNodeClicked += HandleNodeClicked;
            if (CombatManager.Instance != null)
                CombatManager.Instance.OnCombatStart += HandleCombatStart;
        }

        private void OnDisable()
        {
            WorldNodeInteractable.OnNodeClicked -= HandleNodeClicked;
            if (CombatManager.Instance != null)
                CombatManager.Instance.OnCombatStart -= HandleCombatStart;
        }

        private void Start()
        {
            // CombatManager may not be initialised yet in OnEnable; wire it here too.
            if (CombatManager.Instance != null)
                CombatManager.Instance.OnCombatStart += HandleCombatStart;

            StartCoroutine(PlayIntroAfterDelay(1f));
        }

        // ── State transitions ─────────────────────────────────────────────────

        private IEnumerator PlayIntroAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            narratorDialogue?.Play(introData);
            _state = DirectorState.Gathering;
        }

        public void OnFirstNodeClicked()
        {
            if (_state != DirectorState.Gathering) return;
            if (_gatherReactionPlayed) return;
            _gatherReactionPlayed = true;
            miraDialogue?.Play(gatherData);
        }

        public void OnCombatTriggered()
        {
            if (_state == DirectorState.Combat || _state == DirectorState.Done) return;
            _state = DirectorState.Combat;
            miraDialogue?.Play(combatData);
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleNodeClicked(WorldNodeData nodeData)
        {
            OnFirstNodeClicked();
        }

        private void HandleCombatStart(HeroController hero, EnemyController enemy)
        {
            OnCombatTriggered();
        }
    }
}
