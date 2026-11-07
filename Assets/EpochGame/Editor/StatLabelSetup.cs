using EpochGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EpochGame.EditorTools
{
    [InitializeOnLoad]
    public static class StatLabelSetup
    {
        private const string SetupKey = "EpochGame.StatLabelSetup.v1";

        static StatLabelSetup()
        {
            EditorApplication.delayCall += TryApply;
        }

        private static void TryApply()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryApply;
                return;
            }

            if (EditorPrefs.GetBool(SetupKey, false))
            {
                return;
            }

            if (Apply())
            {
                EditorPrefs.SetBool(SetupKey, true);
            }
        }

        private static bool Apply()
        {
            var population = FindCombinedLabel(FindStatRoot("Population", "PopulationLabel"));
            var territory = FindCombinedLabel(FindStatRoot("Territory", "TerritoryLabel"));
            var header = Object.FindAnyObjectByType<ScreenHeaderView>();
            if (population == null || territory == null || header == null)
            {
                return false;
            }

            if (!IconSetup.SplitStatLabel(population, "3", "человека", out var populationValue, out var populationCaption))
            {
                return false;
            }

            if (!IconSetup.SplitStatLabel(territory, "1", "акр", out var territoryValue, out var territoryCaption))
            {
                return false;
            }

            var serialized = new SerializedObject(header);
            serialized.FindProperty("_populationValue").objectReferenceValue = populationValue;
            serialized.FindProperty("_populationCaption").objectReferenceValue = populationCaption;
            serialized.FindProperty("_territoryValue").objectReferenceValue = territoryValue;
            serialized.FindProperty("_territoryCaption").objectReferenceValue = territoryCaption;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var scene = header.gameObject.scene;
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("Population and territory labels are split into a value and a caption.");
            return true;
        }

        private static Transform FindStatRoot(string groupName, string legacyName)
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            Transform block = null;
            Transform legacy = null;
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (!candidate.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (candidate.name == "EraInfoBlock")
                {
                    block = candidate;
                }

                if (candidate.name == legacyName)
                {
                    legacy = candidate;
                }
            }

            if (block != null)
            {
                var nested = block.Find(groupName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return legacy;
        }

        private static Transform FindCombinedLabel(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            if (root.Find("Value") != null && root.Find("Caption") != null)
            {
                return root;
            }

            if (root.GetComponent<TMP_Text>() != null)
            {
                return root;
            }

            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            Transform named = null;
            Transform withNumber = null;
            for (var i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                var name = text.gameObject.name;
                if (name == "PopulationLabel" || name == "TerritoryLabel" || name == "Label")
                {
                    named = text.transform;
                }

                if (HasDigit(text.text))
                {
                    withNumber = text.transform;
                }
            }

            if (named != null)
            {
                return named;
            }

            if (texts.Length == 1)
            {
                return texts[0].transform;
            }

            return withNumber;
        }

        private static bool HasDigit(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            for (var i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
