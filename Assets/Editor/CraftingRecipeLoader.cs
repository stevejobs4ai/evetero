// CraftingRecipeLoader.cs
// Editor utility that imports crafting recipes from a JSON file and creates
// CraftingRecipeData ScriptableObject assets in the Project.
//
// Usage: Unity menu → Evetero → Import Crafting Recipes from JSON
//        Point the file picker at Assets/Data/Crafting/recipes.json (or any
//        compatible JSON).  The tool writes one .asset per recipe into
//        Assets/ScriptableObjects/Crafting/.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Evetero.Editor
{
    /// <summary>
    /// Parses <c>recipes.json</c> and writes one <see cref="CraftingRecipeData"/>
    /// ScriptableObject asset per entry.
    /// </summary>
    public static class CraftingRecipeLoader
    {
        private const string OutputFolder = "Assets/ScriptableObjects/Crafting";

        // ── Menu entry ────────────────────────────────────────────────────────

        /// <summary>
        /// Opens a file-picker to select a recipes JSON, then imports all
        /// recipes as <see cref="CraftingRecipeData"/> assets.
        /// </summary>
        [MenuItem("Evetero/Import Crafting Recipes from JSON")]
        public static void ImportRecipesFromMenu()
        {
            string path = EditorUtility.OpenFilePanel(
                "Select Crafting Recipes JSON",
                Application.dataPath,
                "json");

            if (string.IsNullOrEmpty(path)) return;

            ImportFromPath(path);
        }

        // ── Core import ───────────────────────────────────────────────────────

        /// <summary>
        /// Reads the JSON file at <paramref name="absolutePath"/>, creates one
        /// <see cref="CraftingRecipeData"/> asset per recipe entry, and saves
        /// them under <c>Assets/ScriptableObjects/Crafting/</c>.
        /// </summary>
        /// <param name="absolutePath">Absolute file-system path to the JSON file.</param>
        public static void ImportFromPath(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                Debug.LogError($"[CraftingRecipeLoader] File not found: {absolutePath}");
                return;
            }

            string json = File.ReadAllText(absolutePath);
            RecipeListJson list;

            try
            {
                list = JsonUtility.FromJson<RecipeListJson>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CraftingRecipeLoader] JSON parse error: {e.Message}");
                return;
            }

            if (list?.recipes == null || list.recipes.Length == 0)
            {
                Debug.LogWarning("[CraftingRecipeLoader] No recipes found in JSON.");
                return;
            }

            EnsureOutputFolder();

            int created = 0, updated = 0;
            foreach (var raw in list.recipes)
            {
                if (string.IsNullOrWhiteSpace(raw.recipeName)) continue;

                string assetPath = $"{OutputFolder}/{SanitiseName(raw.recipeName)}.asset";
                bool   isNew     = !File.Exists(
                    Path.Combine(Application.dataPath, "..", assetPath));

                var asset = isNew
                    ? ScriptableObject.CreateInstance<CraftingRecipeData>()
                    : AssetDatabase.LoadAssetAtPath<CraftingRecipeData>(assetPath);

                if (asset == null)
                {
                    Debug.LogError(
                        $"[CraftingRecipeLoader] Failed to load or create asset for '{raw.recipeName}'.");
                    continue;
                }

                ApplyJson(asset, raw);

                if (isNew)
                {
                    AssetDatabase.CreateAsset(asset, assetPath);
                    created++;
                }
                else
                {
                    EditorUtility.SetDirty(asset);
                    updated++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CraftingRecipeLoader] Done — {created} created, {updated} updated.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void ApplyJson(CraftingRecipeData asset, RecipeJson raw)
        {
            asset.recipeName   = raw.recipeName;
            asset.description  = raw.description;
            asset.requiredLevel    = Mathf.Max(1, raw.requiredLevel);
            asset.xpReward         = Mathf.Max(0, raw.xpReward);
            asset.craftTimeSeconds = Mathf.Max(0.1f, raw.craftTimeSeconds);

            if (Enum.TryParse(raw.requiredSkill, out SkillType skill))
                asset.requiredSkill = skill;
            else
                Debug.LogWarning(
                    $"[CraftingRecipeLoader] Unknown SkillType '{raw.requiredSkill}' for '{raw.recipeName}'.");

            if (Enum.TryParse(raw.stationType, out CraftingStationType stationType))
                asset.stationType = stationType;
            else
                Debug.LogWarning(
                    $"[CraftingRecipeLoader] Unknown CraftingStationType '{raw.stationType}' for '{raw.recipeName}'.");

            // Ingredients
            if (raw.ingredients != null)
            {
                asset.ingredients = new CraftingIngredient[raw.ingredients.Length];
                for (int i = 0; i < raw.ingredients.Length; i++)
                {
                    if (Enum.TryParse(raw.ingredients[i].type, out ResourceType rt))
                        asset.ingredients[i].type = rt;
                    else
                        Debug.LogWarning(
                            $"[CraftingRecipeLoader] Unknown ResourceType '{raw.ingredients[i].type}'.");
                    asset.ingredients[i].amount = Mathf.Max(1, raw.ingredients[i].amount);
                }
            }

            // Outputs
            if (raw.outputs != null)
            {
                asset.outputs = new CraftingOutput[raw.outputs.Length];
                for (int i = 0; i < raw.outputs.Length; i++)
                {
                    if (Enum.TryParse(raw.outputs[i].type, out ResourceType rt))
                        asset.outputs[i].type = rt;
                    else
                        Debug.LogWarning(
                            $"[CraftingRecipeLoader] Unknown ResourceType '{raw.outputs[i].type}'.");
                    asset.outputs[i].amount = Mathf.Max(1, raw.outputs[i].amount);
                }
            }
        }

        private static void EnsureOutputFolder()
        {
            if (AssetDatabase.IsValidFolder(OutputFolder)) return;

            // Create intermediate folders as needed
            string parent = "Assets/ScriptableObjects";
            if (!AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

            AssetDatabase.CreateFolder(parent, "Crafting");
        }

        private static string SanitiseName(string name)
        {
            // Strip characters that are illegal in file names
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");
            return name.Replace(" ", "_");
        }

        // ── Serialization DTOs ────────────────────────────────────────────────

        [Serializable]
        private class RecipeListJson
        {
            public RecipeJson[] recipes;
        }

        [Serializable]
        private class RecipeJson
        {
            public string            recipeName;
            public string            description;
            public string            requiredSkill;
            public int               requiredLevel;
            public int               xpReward;
            public string            stationType;
            public float             craftTimeSeconds;
            public IngredientJson[]  ingredients;
            public OutputJson[]      outputs;
        }

        [Serializable]
        private class IngredientJson
        {
            public string type;
            public int    amount;
        }

        [Serializable]
        private class OutputJson
        {
            public string type;
            public int    amount;
        }
    }
}
