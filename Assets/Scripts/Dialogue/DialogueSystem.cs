// DialogueSystem.cs
// MonoBehaviour that drives a dialogue sequence from a DialogueData asset.
//
// Attach to a persistent UI GameObject (e.g. "DialogueCanvas").
// Call Play(dialogueData) to start any conversation.
// Supports: Next(), Skip(), auto-advance, events for UI hookup.
//
// DESIGN GOAL: "swappable story" — the entire game narrative lives in
// DialogueData assets. Swap the asset, change the story. No code edits needed.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Evetero
{
    public class DialogueSystem : MonoBehaviour
    {
        // ── Inspector wiring ──────────────────────────────────────────────────

        [Header("UI References")]
        [Tooltip("Root panel to show/hide when dialogue starts/ends.")]
        public GameObject dialoguePanel;

        [Tooltip("TextMeshPro component for the speaker's name.")]
        public TMP_Text speakerNameText;

        [Tooltip("TextMeshPro component for the dialogue line.")]
        public TMP_Text dialogueLineText;

        [Tooltip("Image component for the speaker's portrait.")]
        public Image speakerPortraitImage;

        [Tooltip("'Next' button — wire its onClick to Next().")]
        public Button nextButton;

        [Tooltip("'Skip all' button — wire its onClick to Skip().")]
        public Button skipButton;

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Fired when a new line is displayed. Useful for animations.</summary>
        public event Action<DialogueLine> OnLineDisplayed;

        /// <summary>Fired when the entire dialogue sequence finishes.</summary>
        public event Action OnDialogueComplete;

        // ── State ─────────────────────────────────────────────────────────────

        private DialogueData _currentDialogue;
        private int          _currentLineIndex;
        private bool         _isPlaying;
        private Coroutine    _autoAdvanceCoroutine;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Start playing a dialogue sequence.
        /// Can be called from a trigger, cutscene controller, or combat event.
        /// </summary>
        public void Play(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.LineCount == 0)
            {
                Debug.LogWarning("[DialogueSystem] Tried to play null or empty DialogueData.");
                return;
            }

            _currentDialogue   = dialogue;
            _currentLineIndex  = 0;
            _isPlaying         = true;

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (nextButton   != null) nextButton.gameObject.SetActive(true);
            if (skipButton   != null) skipButton.gameObject.SetActive(true);

            DisplayCurrentLine();
        }

        /// <summary>
        /// Advance to the next line. Wire to Next button or a player input action.
        /// </summary>
        public void Next()
        {
            if (!_isPlaying) return;

            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            _currentLineIndex++;

            if (_currentLineIndex >= _currentDialogue.LineCount)
                EndDialogue();
            else
                DisplayCurrentLine();
        }

        /// <summary>
        /// Skip the entire dialogue immediately.
        /// </summary>
        public void Skip()
        {
            if (!_isPlaying) return;

            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            EndDialogue();
        }

        /// <summary>True while a dialogue is actively playing.</summary>
        public bool IsPlaying => _isPlaying;

        // ── Internal ──────────────────────────────────────────────────────────

        private void DisplayCurrentLine()
        {
            DialogueLine line = _currentDialogue.GetLine(_currentLineIndex);

            if (speakerNameText  != null) speakerNameText.text  = line.speakerName;
            if (dialogueLineText != null) dialogueLineText.text  = line.lineText;

            if (speakerPortraitImage != null)
            {
                bool hasPortrait = line.speakerPortrait != null;
                speakerPortraitImage.gameObject.SetActive(hasPortrait);
                if (hasPortrait) speakerPortraitImage.sprite = line.speakerPortrait;
            }

            OnLineDisplayed?.Invoke(line);

            if (line.autoAdvanceDelay > 0f)
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvance(line.autoAdvanceDelay));
        }

        private IEnumerator AutoAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            Next();
        }

        private void EndDialogue()
        {
            _isPlaying        = false;
            _currentDialogue  = null;
            _currentLineIndex = 0;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            OnDialogueComplete?.Invoke();
            Debug.Log("[DialogueSystem] Dialogue complete.");
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (nextButton != null) nextButton.onClick.AddListener(Next);
            if (skipButton != null) skipButton.onClick.AddListener(Skip);
        }

        private void OnDestroy()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(Next);
            if (skipButton != null) skipButton.onClick.RemoveListener(Skip);
        }
    }
}
