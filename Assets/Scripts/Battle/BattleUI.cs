// BattleUI.cs
// MonoBehaviour that drives the turn-based battle HUD.
//
// Inspector wiring:
//   turnIndicator      — TMP text showing whose turn it is
//   actionPanel        — root GameObject toggled off during enemy turns
//   attackButton       — fires Attack action
//   ability1Button..3  — fire Ability actions (slots 1-3)
//   defendButton       — fires Defend action
//   fleeButton         — fires Flee action
//   talkButton         — fires Talk action (shown only when recruitable targets exist)
//   battleLogText      — TMP text area for scrolling battle log
//   turnOrderPanel     — parent Transform for turn-preview unit cards
//   turnOrderCardPrefab — prefab with a TMP_Text child for the unit name
//   unitBars           — array of UnitHPBar components (wired manually or built at runtime)

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evetero
{
    public class BattleUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Manager")]
        [SerializeField] private BattleManager battleManager;

        [Header("Turn Indicator")]
        [SerializeField] private TMP_Text turnIndicatorText;

        [Header("Action Panel")]
        [SerializeField] private GameObject actionPanel;
        [SerializeField] private Button     attackButton;
        [SerializeField] private Button     ability1Button;
        [SerializeField] private Button     ability2Button;
        [SerializeField] private Button     ability3Button;
        [SerializeField] private Button     defendButton;
        [SerializeField] private Button     fleeButton;
        [SerializeField] private Button     talkButton;

        [Header("Battle Log")]
        [SerializeField] private TMP_Text   battleLogText;
        [SerializeField] private int        maxLogLines = 12;

        [Header("Turn Order Preview")]
        [SerializeField] private Transform  turnOrderPanel;
        [SerializeField] private GameObject turnOrderCardPrefab;

        [Header("Unit HP/MP Bars")]
        [SerializeField] private UnitStatusBar[] unitBars;

        // ── Internal ──────────────────────────────────────────────────────────

        private BattleUnit       _currentUnit;
        private BattleUnit       _selectedTarget;
        private List<string>     _logLines = new List<string>();
        private List<GameObject> _turnCards = new List<GameObject>();

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            if (battleManager == null)
                battleManager = BattleManager.Instance;

            if (battleManager != null)
                SubscribeToManager();

            WireButtons();
            SetActionPanelActive(false);
        }

        private void OnDestroy()
        {
            if (battleManager == null) return;
            battleManager.OnTurnStarted  -= HandleTurnStarted;
            battleManager.OnUnitDamaged  -= HandleUnitDamaged;
            battleManager.OnUnitDefeated -= HandleUnitDefeated;
            battleManager.OnUnitRecruited -= HandleUnitRecruited;
            battleManager.OnBattleEnded  -= HandleBattleEnded;
            battleManager.OnBattleLog    -= AppendLog;
        }

        // ── Event subscriptions ───────────────────────────────────────────────

        private void SubscribeToManager()
        {
            battleManager.OnTurnStarted   += HandleTurnStarted;
            battleManager.OnUnitDamaged   += HandleUnitDamaged;
            battleManager.OnUnitDefeated  += HandleUnitDefeated;
            battleManager.OnUnitRecruited += HandleUnitRecruited;
            battleManager.OnBattleEnded   += HandleBattleEnded;
            battleManager.OnBattleLog     += AppendLog;
        }

        // ── Handlers ──────────────────────────────────────────────────────────

        private void HandleTurnStarted(BattleUnit unit)
        {
            _currentUnit    = unit;
            _selectedTarget = null;

            if (turnIndicatorText != null)
                turnIndicatorText.text = $"{unit.Name}'s Turn";

            bool isHeroTurn = unit.Team == BattleTeam.Hero;
            SetActionPanelActive(isHeroTurn);

            if (isHeroTurn)
                ConfigureAbilityButtons(unit);

            RefreshUnitBars();
            RefreshTurnOrder();
        }

        private void HandleUnitDamaged(BattleUnit unit, int amount)
        {
            RefreshUnitBars();
        }

        private void HandleUnitDefeated(BattleUnit unit)
        {
            RefreshUnitBars();
        }

        private void HandleUnitRecruited(BattleUnit unit)
        {
            AppendLog($"★ {unit.Name} joins the heroes!");
        }

        private void HandleBattleEnded(BattleResult result)
        {
            SetActionPanelActive(false);
            if (turnIndicatorText != null)
                turnIndicatorText.text = result switch
                {
                    BattleResult.Victory => "Victory!",
                    BattleResult.Defeat  => "Defeat...",
                    BattleResult.Fled    => "Fled!",
                    _                   => string.Empty
                };
        }

        // ── Button wiring ─────────────────────────────────────────────────────

        private void WireButtons()
        {
            attackButton?.onClick .AddListener(OnAttackClicked);
            ability1Button?.onClick.AddListener(() => OnAbilityClicked(1));
            ability2Button?.onClick.AddListener(() => OnAbilityClicked(2));
            ability3Button?.onClick.AddListener(() => OnAbilityClicked(3));
            defendButton?.onClick .AddListener(OnDefendClicked);
            fleeButton?.onClick   .AddListener(OnFleeClicked);
            talkButton?.onClick   .AddListener(OnTalkClicked);
        }

        private void OnAttackClicked()
        {
            _selectedTarget = PickDefaultEnemyTarget();
            if (_selectedTarget == null) return;

            battleManager.SubmitAction(new BattleAction
            {
                Type   = BattleActionType.Attack,
                Target = _selectedTarget
            });
        }

        private void OnAbilityClicked(int slot)
        {
            _selectedTarget = PickDefaultEnemyTarget();
            if (_selectedTarget == null) return;

            battleManager.SubmitAction(new BattleAction
            {
                Type        = BattleActionType.Ability,
                AbilitySlot = slot,
                Target      = _selectedTarget
            });
        }

        private void OnDefendClicked()
        {
            battleManager.SubmitAction(new BattleAction
            {
                Type = BattleActionType.Defend
            });
        }

        private void OnFleeClicked()
        {
            battleManager.SubmitAction(new BattleAction
            {
                Type = BattleActionType.Flee
            });
        }

        private void OnTalkClicked()
        {
            _selectedTarget = PickRecruitableTarget();
            if (_selectedTarget == null) return;

            battleManager.SubmitAction(new BattleAction
            {
                Type   = BattleActionType.Talk,
                Target = _selectedTarget
            });
        }

        // ── Ability button config ─────────────────────────────────────────────

        private void ConfigureAbilityButtons(BattleUnit unit)
        {
            ConfigureAbilityBtn(ability1Button, unit, 1);
            ConfigureAbilityBtn(ability2Button, unit, 2);
            ConfigureAbilityBtn(ability3Button, unit, 3);

            // Show Talk button only if a recruitable enemy target exists
            bool hasTalkTarget = PickRecruitableTarget() != null;
            talkButton?.gameObject.SetActive(hasTalkTarget);
        }

        private static void ConfigureAbilityBtn(Button btn, BattleUnit unit, int slot)
        {
            if (btn == null) return;
            if (unit.HeroData == null || slot >= unit.HeroData.abilities.Length)
            {
                btn.gameObject.SetActive(false);
                return;
            }

            var ability = unit.HeroData.GetAbility(slot);
            btn.gameObject.SetActive(ability != null);
            if (ability == null) return;

            btn.interactable = unit.AbilityCooldowns[slot] == 0 &&
                               unit.CurrentMana >= ability.manaCost;

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = ability.abilityName;
        }

        // ── Turn order preview ────────────────────────────────────────────────

        private void RefreshTurnOrder()
        {
            if (turnOrderPanel == null || battleManager == null) return;

            // Clear old cards
            foreach (var card in _turnCards)
                if (card != null) Destroy(card);
            _turnCards.Clear();

            if (turnOrderCardPrefab == null) return;

            var preview = battleManager.GetTurnPreview(5);
            foreach (var unit in preview)
            {
                var card = Instantiate(turnOrderCardPrefab, turnOrderPanel);
                var text = card.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = $"{unit.Name} ({unit.CurrentHP}/{unit.MaxHP})";
                _turnCards.Add(card);
            }
        }

        // ── Unit status bars ──────────────────────────────────────────────────

        private void RefreshUnitBars()
        {
            if (unitBars == null || battleManager == null) return;
            // UnitStatusBar binding is handled by BattleSceneSetup; just tell each to refresh.
            foreach (var bar in unitBars)
                bar?.Refresh();
        }

        // ── Battle log ────────────────────────────────────────────────────────

        private void AppendLog(string message)
        {
            _logLines.Add(message);
            if (_logLines.Count > maxLogLines)
                _logLines.RemoveAt(0);

            if (battleLogText != null)
                battleLogText.text = string.Join("\n", _logLines);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetActionPanelActive(bool active)
        {
            actionPanel?.SetActive(active);
        }

        private BattleUnit PickDefaultEnemyTarget()
        {
            if (battleManager == null) return null;
            // Simple: pick first alive enemy unit in turn queue
            return battleManager.GetTurnPreview(20)
                .Find(u => u.Team == BattleTeam.Enemy && u.IsAlive && !u.IsRecruited);
        }

        private BattleUnit PickRecruitableTarget()
        {
            if (battleManager == null) return null;
            return battleManager.GetTurnPreview(20)
                .Find(u => u.Team == BattleTeam.Enemy && u.IsAlive &&
                           u.RecruitableData != null && !u.IsRecruited);
        }
    }

    // ── UnitStatusBar ─────────────────────────────────────────────────────────

    /// <summary>
    /// Small component attached to a prefab card showing one unit's HP/MP.
    /// BattleSceneSetup calls Bind(unit) to associate a BattleUnit, then
    /// Refresh() updates the displayed values.
    /// </summary>
    public class UnitStatusBar : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Slider   hpSlider;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider   mpSlider;
        [SerializeField] private TMP_Text mpText;

        private BattleUnit _unit;

        public void Bind(BattleUnit unit)
        {
            _unit = unit;
            if (nameText != null) nameText.text = unit.Name;
            Refresh();
        }

        public void Refresh()
        {
            if (_unit == null) return;

            if (hpSlider != null)
                hpSlider.value = _unit.MaxHP > 0 ? (float)_unit.CurrentHP / _unit.MaxHP : 0f;
            if (hpText != null)
                hpText.text = $"{_unit.CurrentHP}/{_unit.MaxHP}";

            if (mpSlider != null)
                mpSlider.value = _unit.MaxMana > 0 ? (float)_unit.CurrentMana / _unit.MaxMana : 0f;
            if (mpText != null)
                mpText.text = $"{_unit.CurrentMana}/{_unit.MaxMana}";
        }
    }
}
