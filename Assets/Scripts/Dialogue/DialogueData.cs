// DialogueData.cs
// ScriptableObject holding a sequence of dialogue lines.
//
// Usage: Right-click in Project → Create → Evetero → Dialogue Data
// Create one asset per scene / conversation.
// Swap entire storylines by swapping which DialogueData asset you load.

using UnityEngine;

namespace Evetero
{
    // ── DialogueLine ─────────────────────────────────────────────────────────

    /// <summary>
    /// One line of spoken dialogue. Serializable so it appears in the inspector.
    /// </summary>
    [System.Serializable]
    public struct DialogueLine
    {
        [Tooltip("Name shown in the dialogue nameplate (e.g. 'Mira', 'Narrator').")]
        public string speakerName;

        [Tooltip("Portrait sprite shown next to the dialogue box.")]
        public Sprite speakerPortrait;

        [Tooltip("The line of dialogue text.")]
        [TextArea(2, 6)]
        public string lineText;

        [Tooltip("Optional: delay in seconds before auto-advancing (0 = wait for player input).")]
        [Min(0f)]
        public float autoAdvanceDelay;
    }

    // ── DialogueData ScriptableObject ────────────────────────────────────────

    /// <summary>
    /// A complete dialogue sequence — just an ordered array of DialogueLines.
    /// This is the "swappable story" foundation: different assets = different stories.
    /// </summary>
    [CreateAssetMenu(menuName = "Evetero/Dialogue Data", fileName = "NewDialogue")]
    public class DialogueData : ScriptableObject
    {
        [Tooltip("Human-readable label for this conversation (not shown in game).")]
        public string sceneLabel;

        [Tooltip("All lines in this dialogue, played top to bottom.")]
        public DialogueLine[] lines;

        /// <summary>Total number of lines in this dialogue.</summary>
        public int LineCount => lines?.Length ?? 0;

        /// <summary>Returns the line at the given index, or a blank line if out of range.</summary>
        public DialogueLine GetLine(int index)
        {
            if (lines == null || index < 0 || index >= lines.Length)
                return default;
            return lines[index];
        }
    }
}

/*
 * ── MIRA'S INTRO SCENE — Inspector Reference ─────────────────────────────────
 *
 * Asset name: "Dialogue_MiraIntro"
 * sceneLabel : "Mira — Tutorial Intro"
 *
 * Line 0:
 *   speakerName    : "Narrator"
 *   speakerPortrait: (none / null)
 *   lineText       : "From the frozen peaks of the Icereach Range, she
 *                     descended — alone, armed only with her magic and a
 *                     simmering fury."
 *   autoAdvanceDelay: 0
 *
 * Line 1:
 *   speakerName    : "Mira"
 *   speakerPortrait: [Mira portrait sprite]
 *   lineText       : "You called for a mage. I'm here. Try not to waste
 *                     my time with easy problems."
 *   autoAdvanceDelay: 0
 *
 * Line 2:
 *   speakerName    : "Commander"
 *   speakerPortrait: [Commander portrait sprite]
 *   lineText       : "Charming. Welcome to Evetero, Mira. The kingdom
 *                     needs every blade — and every spell — it can get."
 *   autoAdvanceDelay: 0
 *
 * To wire a different story: swap the DialogueData asset fed into DialogueSystem.
 * The code never changes; only the data does.
 * ─────────────────────────────────────────────────────────────────────────────
 */
