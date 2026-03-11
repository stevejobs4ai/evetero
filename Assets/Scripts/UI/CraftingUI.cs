// CraftingUI.cs
// Full-screen crafting panel that appears when the hero approaches a
// CraftingStation.  Programmatically builds its own Canvas so it needs no
// prefab setup.  Assign one instance to the scene (e.g. via EmberVillageSetup)
// and call Open() / Close() as needed.

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evetero
{
    /// <summary>
    /// Crafting panel UI.
    /// <list type="bullet">
    ///   <item>Recipe list with per-row ingredient status (red = insufficient).</item>
    ///   <item>Quantity selector: ×1, ×5, ×10, All.</item>
    ///   <item>Progress bar shown during an active craft.</item>
    ///   <item>Close button and tap-outside-to-dismiss blocker.</item>
    /// </list>
    /// </summary>
    public class CraftingUI : MonoBehaviour
    {
        // ── Private state ─────────────────────────────────────────────────────

        private CraftingStation _station;
        private HeroSkills      _heroSkills;
        private int             _selectedQty  = 1;
        private bool            _isCrafting;

        // ── UI root refs ──────────────────────────────────────────────────────

        private GameObject               _root;           // top-level panel
        private Transform                _recipeContainer;// scroll content
        private Slider                   _progressBar;
        private TextMeshProUGUI          _titleText;
        private TextMeshProUGUI          _progressLabel;
        private readonly List<RecipeRow> _rows = new();

        // ── Quantity button state ─────────────────────────────────────────────

        private readonly int[]         _qtyOptions  = { 1, 5, 10, 0 }; // 0 = All
        private readonly Button[]       _qtyButtons  = new Button[4];

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            BuildCanvas();
            _root.SetActive(false);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Opens the crafting panel for <paramref name="station"/>, filtered
        /// to the hero's current skill level.
        /// </summary>
        /// <param name="station">Station the hero is interacting with.</param>
        /// <param name="heroSkills">Hero's skill component (may be null).</param>
        public void Open(CraftingStation station, HeroSkills heroSkills)
        {
            if (_isCrafting) return;

            _station    = station;
            _heroSkills = heroSkills;
            _selectedQty = 1;

            string label = station.stationType switch
            {
                CraftingStationType.Furnace        => "Furnace",
                CraftingStationType.Anvil          => "Anvil",
                CraftingStationType.FletchingBench => "Fletching Bench",
                CraftingStationType.CookingRange   => "Cooking Range",
                _                                  => "Crafting"
            };
            _titleText.text = label;

            PopulateRecipes();
            RefreshQuantityButtons();
            _root.SetActive(true);
        }

        /// <summary>Hides the crafting panel.  Has no effect while a craft is running.</summary>
        public void Close()
        {
            if (_isCrafting) return;
            _root.SetActive(false);
            ClearRows();
        }

        // ── Canvas construction ───────────────────────────────────────────────

        private void BuildCanvas()
        {
            // ── Canvas ────────────────────────────────────────────────────────
            var canvasGO = new GameObject("CraftingUICanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            // ── Fullscreen blocker (tap outside = close) ──────────────────────
            var blockerGO = MakePanel(canvasGO.transform, "Blocker",
                new Color(0f, 0f, 0f, 0.01f));
            StretchFull(blockerGO.GetComponent<RectTransform>());
            var blockerBtn = blockerGO.AddComponent<Button>();
            blockerBtn.onClick.AddListener(Close);

            // ── Main panel ────────────────────────────────────────────────────
            _root = MakePanel(canvasGO.transform, "CraftingPanel",
                new Color(0.12f, 0.12f, 0.14f, 0.97f));
            var rootRT = _root.GetComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.1f, 0.05f);
            rootRT.anchorMax = new Vector2(0.9f, 0.95f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Header
            var header = MakePanel(_root.transform, "Header",
                new Color(0.08f, 0.08f, 0.1f, 1f));
            var headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 0.88f);
            headerRT.anchorMax = Vector2.one;
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            _titleText = MakeText(header.transform, "Title", "", 22,
                FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            var titleRT = _titleText.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.02f, 0f);
            titleRT.anchorMax = new Vector2(0.85f, 1f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            // Close button
            var closeBtn = MakeButton(header.transform, "CloseBtn",
                "✕", 20, new Color(0.7f, 0.2f, 0.2f));
            var closeBtnRT = closeBtn.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(0.87f, 0.1f);
            closeBtnRT.anchorMax = new Vector2(0.99f, 0.9f);
            closeBtnRT.offsetMin = Vector2.zero;
            closeBtnRT.offsetMax = Vector2.zero;
            closeBtn.GetComponent<Button>().onClick.AddListener(Close);

            // Quantity row
            var qtyRow = MakePanel(_root.transform, "QuantityRow",
                new Color(0.15f, 0.15f, 0.17f, 1f));
            var qtyRT = qtyRow.GetComponent<RectTransform>();
            qtyRT.anchorMin = new Vector2(0f, 0.80f);
            qtyRT.anchorMax = new Vector2(1f, 0.88f);
            qtyRT.offsetMin = Vector2.zero;
            qtyRT.offsetMax = Vector2.zero;

            string[] qtyLabels = { "×1", "×5", "×10", "All" };
            for (int i = 0; i < 4; i++)
            {
                int  idx   = i;
                float xMin = 0.01f + i * 0.245f;
                float xMax = xMin + 0.235f;
                var btn = MakeButton(qtyRow.transform, $"Qty_{qtyLabels[i]}",
                    qtyLabels[i], 16, new Color(0.25f, 0.45f, 0.65f));
                var btnRT = btn.GetComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(xMin, 0.05f);
                btnRT.anchorMax = new Vector2(xMax, 0.95f);
                btnRT.offsetMin = Vector2.zero;
                btnRT.offsetMax = Vector2.zero;
                _qtyButtons[i] = btn.GetComponent<Button>();
                _qtyButtons[i].onClick.AddListener(() => OnQuantitySelected(idx));
            }

            // Scroll view for recipe list
            var scrollGO = new GameObject("RecipeScroll");
            scrollGO.transform.SetParent(_root.transform, false);
            var scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0f, 0.1f);
            scrollRT.anchorMax = new Vector2(1f, 0.80f);
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            var viewportGO = MakePanel(scrollGO.transform, "Viewport",
                new Color(0f, 0f, 0f, 0f));
            var vpRT = viewportGO.GetComponent<RectTransform>();
            StretchFull(vpRT);
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;
            scrollRect.viewport = vpRT;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot     = new Vector2(0.5f, 1f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;
            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing           = 4f;
            vlg.padding           = new RectOffset(6, 6, 4, 4);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;
            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRT;
            _recipeContainer = contentGO.transform;

            // Progress bar row
            var progressRow = MakePanel(_root.transform, "ProgressRow",
                new Color(0.1f, 0.1f, 0.12f, 1f));
            var progRT = progressRow.GetComponent<RectTransform>();
            progRT.anchorMin = new Vector2(0f, 0f);
            progRT.anchorMax = new Vector2(1f, 0.1f);
            progRT.offsetMin = Vector2.zero;
            progRT.offsetMax = Vector2.zero;
            progressRow.SetActive(false);

            _progressBar = BuildProgressBar(progressRow.transform);

            _progressLabel = MakeText(progressRow.transform, "ProgressLabel",
                "Crafting…", 14, FontStyles.Normal, TextAlignmentOptions.Center);
            var plRT = _progressLabel.GetComponent<RectTransform>();
            plRT.anchorMin = new Vector2(0f, 0f);
            plRT.anchorMax = new Vector2(1f, 0.45f);
            plRT.offsetMin = Vector2.zero;
            plRT.offsetMax = Vector2.zero;
        }

        // ── Recipe population ─────────────────────────────────────────────────

        private void PopulateRecipes()
        {
            ClearRows();

            int skillLevel = _heroSkills != null
                ? _heroSkills.GetLevel(_station.PrimarySkill)
                : 1;

            var available = _station.GetAvailableRecipes(skillLevel);
            foreach (var recipe in available)
            {
                var row = CreateRecipeRow(recipe);
                _rows.Add(row);
            }

            if (_rows.Count == 0)
            {
                var empty = MakeText(_recipeContainer, "EmptyLabel",
                    "No recipes available yet.", 15,
                    FontStyles.Italic, TextAlignmentOptions.Center);
                var eRT = empty.GetComponent<RectTransform>();
                eRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f);
            }
        }

        private void ClearRows()
        {
            _rows.Clear();
            foreach (Transform child in _recipeContainer)
                Destroy(child.gameObject);
        }

        private RecipeRow CreateRecipeRow(CraftingRecipeData recipe)
        {
            var rowGO = MakePanel(_recipeContainer, $"Row_{recipe.recipeName}",
                new Color(0.18f, 0.18f, 0.22f, 1f));
            var rowLayout = rowGO.AddComponent<LayoutElement>();
            rowLayout.minHeight       = 70f;
            rowLayout.preferredHeight = 70f;

            // Recipe name
            var nameText = MakeText(rowGO.transform, "RecipeName",
                recipe.recipeName, 16, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            var ntRT = nameText.GetComponent<RectTransform>();
            ntRT.anchorMin = new Vector2(0.01f, 0.55f);
            ntRT.anchorMax = new Vector2(0.68f, 0.98f);
            ntRT.offsetMin = Vector2.zero;
            ntRT.offsetMax = Vector2.zero;

            // Ingredients summary
            var ingText = MakeText(rowGO.transform, "Ingredients",
                BuildIngredientString(recipe), 12,
                FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            var itRT = ingText.GetComponent<RectTransform>();
            itRT.anchorMin = new Vector2(0.01f, 0.05f);
            itRT.anchorMax = new Vector2(0.68f, 0.54f);
            itRT.offsetMin = Vector2.zero;
            itRT.offsetMax = Vector2.zero;
            ingText.enableWordWrapping = true;

            // XP label
            var xpText = MakeText(rowGO.transform, "XP",
                $"+{recipe.xpReward} xp", 11, FontStyles.Italic,
                TextAlignmentOptions.MidlineRight);
            xpText.color = new Color(0.6f, 0.9f, 0.6f);
            var xpRT = xpText.GetComponent<RectTransform>();
            xpRT.anchorMin = new Vector2(0.68f, 0.55f);
            xpRT.anchorMax = new Vector2(0.99f, 0.98f);
            xpRT.offsetMin = Vector2.zero;
            xpRT.offsetMax = Vector2.zero;

            // Craft button
            bool canCraft = _station.CanCraft(recipe);
            var craftBtn = MakeButton(rowGO.transform, "CraftBtn",
                "Craft", 14,
                canCraft ? new Color(0.2f, 0.55f, 0.2f) : new Color(0.35f, 0.35f, 0.35f));
            var cbRT = craftBtn.GetComponent<RectTransform>();
            cbRT.anchorMin = new Vector2(0.70f, 0.08f);
            cbRT.anchorMax = new Vector2(0.99f, 0.52f);
            cbRT.offsetMin = Vector2.zero;
            cbRT.offsetMax = Vector2.zero;

            var btn = craftBtn.GetComponent<Button>();
            btn.interactable = canCraft;
            btn.onClick.AddListener(() => OnCraftClicked(recipe));

            // Mark ingredients red/green
            RefreshIngredientColour(ingText, recipe);

            var row = new RecipeRow
            {
                root        = rowGO,
                ingText     = ingText,
                craftButton = btn,
                recipe      = recipe
            };
            return row;
        }

        private string BuildIngredientString(CraftingRecipeData recipe)
        {
            if (recipe.ingredients == null || recipe.ingredients.Length == 0)
                return "No ingredients";

            var parts = new System.Text.StringBuilder();
            foreach (var ing in recipe.ingredients)
            {
                if (parts.Length > 0) parts.Append("  ");
                parts.Append($"{ing.type} ×{ing.amount}");
            }
            return parts.ToString();
        }

        private void RefreshIngredientColour(TextMeshProUGUI text,
            CraftingRecipeData recipe)
        {
            if (recipe.ingredients == null) { text.color = Color.white; return; }

            bool sufficient = true;
            if (ResourceBank.Instance != null)
            {
                foreach (var ing in recipe.ingredients)
                {
                    if (ResourceBank.Instance.GetAmount(ing.type) < ing.amount)
                    { sufficient = false; break; }
                }
            }
            text.color = sufficient
                ? new Color(0.7f, 1f, 0.7f)
                : new Color(1f, 0.45f, 0.45f);
        }

        // ── Quantity selection ────────────────────────────────────────────────

        private void OnQuantitySelected(int index)
        {
            _selectedQty = _qtyOptions[index]; // 0 = All
            RefreshQuantityButtons();
        }

        private void RefreshQuantityButtons()
        {
            for (int i = 0; i < _qtyButtons.Length; i++)
            {
                bool active = (_selectedQty == _qtyOptions[i]);
                var img = _qtyButtons[i].GetComponent<Image>();
                img.color = active
                    ? new Color(0.2f, 0.55f, 0.8f)
                    : new Color(0.25f, 0.25f, 0.3f);
            }
        }

        // ── Crafting ──────────────────────────────────────────────────────────

        private void OnCraftClicked(CraftingRecipeData recipe)
        {
            if (_isCrafting) return;

            int qty = _selectedQty == 0
                ? GetMaxCraftable(recipe)
                : _selectedQty;

            if (qty <= 0) return;

            StartCoroutine(CraftMultiple(recipe, qty));
        }

        private IEnumerator CraftMultiple(CraftingRecipeData recipe, int quantity)
        {
            _isCrafting = true;

            var progressRow = _progressBar.transform.parent.gameObject;
            progressRow.SetActive(true);

            for (int i = 0; i < quantity; i++)
            {
                if (!_station.CanCraft(recipe)) break;

                float elapsed  = 0f;
                float duration = recipe.craftTimeSeconds;
                _progressLabel.text = $"Crafting {recipe.recipeName}… ({i + 1}/{quantity})";

                // Run the station coroutine in parallel, track progress here.
                bool done = false;
                StartCoroutine(_station.DoCraft(recipe, () => done = true));

                while (!done)
                {
                    elapsed           += Time.deltaTime;
                    _progressBar.value = Mathf.Clamp01(elapsed / duration);
                    yield return null;
                }
                _progressBar.value = 1f;
                yield return null;

                RefreshRows();
            }

            _progressBar.value  = 0f;
            progressRow.SetActive(false);
            _isCrafting = false;
        }

        private void RefreshRows()
        {
            foreach (var row in _rows)
            {
                bool can = _station.CanCraft(row.recipe);
                row.craftButton.interactable = can;
                var img = row.craftButton.GetComponent<Image>();
                img.color = can
                    ? new Color(0.2f, 0.55f, 0.2f)
                    : new Color(0.35f, 0.35f, 0.35f);
                RefreshIngredientColour(row.ingText, row.recipe);
            }
        }

        /// <summary>
        /// Returns how many times <paramref name="recipe"/> can be crafted
        /// given current <see cref="ResourceBank"/> stock.
        /// </summary>
        private int GetMaxCraftable(CraftingRecipeData recipe)
        {
            if (ResourceBank.Instance == null) return 0;
            if (recipe.ingredients == null || recipe.ingredients.Length == 0) return 1;

            int max = int.MaxValue;
            foreach (var ing in recipe.ingredients)
            {
                if (ing.amount <= 0) continue;
                int have = ResourceBank.Instance.GetAmount(ing.type);
                max = Mathf.Min(max, have / ing.amount);
            }
            return max == int.MaxValue ? 0 : max;
        }

        // ── UI factory helpers ────────────────────────────────────────────────

        private static GameObject MakePanel(Transform parent, string goName, Color color)
        {
            var go  = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static TextMeshProUGUI MakeText(Transform parent, string goName,
            string text, float fontSize, FontStyles style, TextAlignmentOptions align)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color     = Color.white;

            // Assign default TMP font if none was set (prevents "No Font Asset" error)
            if (tmp.font == null)
            {
                var defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (defaultFont == null)
                    defaultFont = TMP_Settings.defaultFontAsset;
                if (defaultFont != null)
                    tmp.font = defaultFont;
            }

            return tmp;
        }

        private static GameObject MakeButton(Transform parent, string goName,
            string label, float fontSize, Color bgColor)
        {
            var go  = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();

            // Label child
            var lbl = MakeText(go.transform, "Label", label, fontSize,
                FontStyles.Bold, TextAlignmentOptions.Center);
            var lblRT = lbl.GetComponent<RectTransform>();
            StretchFull(lblRT);

            return go;
        }

        private static Slider BuildProgressBar(Transform parent)
        {
            var bgGO  = MakePanel(parent, "ProgressBG", new Color(0.2f, 0.2f, 0.2f));
            var bgRT  = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.01f, 0.5f);
            bgRT.anchorMax = new Vector2(0.99f, 0.95f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var fillAreaGO = new GameObject("FillArea");
            fillAreaGO.transform.SetParent(bgGO.transform, false);
            var faRT = fillAreaGO.AddComponent<RectTransform>();
            StretchFull(faRT);

            var fillGO  = MakePanel(fillAreaGO.transform, "Fill",
                new Color(0.25f, 0.65f, 0.25f));
            var fillRT  = fillGO.GetComponent<RectTransform>();
            StretchFull(fillRT);

            var slider = bgGO.AddComponent<Slider>();
            slider.fillRect  = fillRT;
            slider.minValue  = 0f;
            slider.maxValue  = 1f;
            slider.value     = 0f;
            slider.interactable = false;

            // Hide the default handle
            var handleArea = new GameObject("HandleSlideArea");
            handleArea.transform.SetParent(bgGO.transform, false);
            handleArea.AddComponent<RectTransform>();
            slider.handleRect = handleArea.AddComponent<RectTransform>();

            return slider;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // ── Nested helper type ────────────────────────────────────────────────

        private class RecipeRow
        {
            public GameObject        root;
            public TextMeshProUGUI   ingText;
            public Button            craftButton;
            public CraftingRecipeData recipe;
        }
    }
}
