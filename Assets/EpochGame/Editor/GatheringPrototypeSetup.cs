using EpochGame.Core;
using EpochGame.Economy;
using EpochGame.Gathering;
using EpochGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EpochGame.EditorTools
{
    [InitializeOnLoad]
    public static class GatheringPrototypeSetup
    {
        private const string SetupKey = "EpochGame.GatheringPrototypeSetup.v1";
        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        static GatheringPrototypeSetup()
        {
            EditorApplication.delayCall += TryCreate;
        }

        [MenuItem("EpochGame/Create Gathering Prototype UI")]
        public static void CreateFromMenu()
        {
            Create();
        }

        private static void TryCreate()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryCreate;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorPrefs.GetBool(SetupKey, false))
            {
                return;
            }

            Create();
        }

        private static void Create()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                Debug.LogError("Gathering prototype setup could not find the TextMeshPro font.");
                return;
            }

            var berries = LoadNode(
                new[]
                {
                    "Assets/EpochGame/Content/Gathering/berries.asset",
                    "Assets/EpochGame/Content/Gathering/ManualResourceNode.asset"
                },
                "berries",
                "berries",
                ResourceId.Food,
                2);

            var tree = LoadNode(
                new[]
                {
                    "Assets/EpochGame/Content/Gathering/tree.asset",
                    "Assets/EpochGame/Content/Gathering/ManualResourceNode 1.asset"
                },
                "tree",
                "tree",
                ResourceId.Wood,
                1);

            var scene = EditorSceneManager.GetActiveScene();
            var canvas = FindOrCreateCanvas();
            var bar = FindOrCreateChild(canvas.transform, "ResourceBar");
            StretchTop(bar.GetComponent<RectTransform>(), 40f, 120f);
            var background = GetOrAdd<Image>(bar);
            background.color = new Color(0.16f, 0.13f, 0.1f, 0.94f);
            background.raycastTarget = false;

            var foodSlot = CreateSlot(bar.transform, "FoodSlot", ResourceId.Food, "Еда", font, true);
            var woodSlot = CreateSlot(bar.transform, "WoodSlot", ResourceId.Wood, "Дерево", font, false);
            var barView = GetOrAdd<ResourceBarView>(bar);
            SetObjectArray(barView, "_slots", foodSlot, woodSlot);

            var berriesButton = CreateNodeButton(
                canvas.transform,
                "BerriesButton",
                "Ягоды",
                berries,
                font,
                new Vector2(0f, 180f),
                new Color(0.93f, 0.45f, 0.42f));
            var treeButton = CreateNodeButton(
                canvas.transform,
                "TreeButton",
                "Дерево",
                tree,
                font,
                new Vector2(0f, -20f),
                new Color(0.55f, 0.72f, 0.38f));

            EnsureEventSystem();

            var game = FindInScene("Game");
            if (game == null)
            {
                game = new GameObject("Game");
            }

            var bootstrap = GetOrAdd<GameBootstrap>(game);
            var bootstrapObject = new SerializedObject(bootstrap);
            bootstrapObject.FindProperty("_resourceBar").objectReferenceValue = barView;
            SetObjectArray(bootstrapObject.FindProperty("_nodes"), berriesButton, treeButton);
            bootstrapObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorPrefs.SetBool(SetupKey, true);
            Debug.Log("Gathering prototype UI is ready in the open scene.");
        }

        private static ManualResourceNodeDefinition LoadNode(
            string[] candidatePaths,
            string assetName,
            string id,
            ResourceId resource,
            double amountPerTap)
        {
            var path = string.Empty;
            for (var i = 0; i < candidatePaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<ManualResourceNodeDefinition>(candidatePaths[i]) != null)
                {
                    path = candidatePaths[i];
                    break;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                var created = ScriptableObject.CreateInstance<ManualResourceNodeDefinition>();
                path = "Assets/EpochGame/Content/Gathering/" + assetName + ".asset";
                AssetDatabase.CreateAsset(created, path);
            }
            else if (System.IO.Path.GetFileNameWithoutExtension(path) != assetName)
            {
                var error = AssetDatabase.RenameAsset(path, assetName);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                }

                path = "Assets/EpochGame/Content/Gathering/" + assetName + ".asset";
            }

            var node = AssetDatabase.LoadAssetAtPath<ManualResourceNodeDefinition>(path);
            var serializedNode = new SerializedObject(node);
            serializedNode.FindProperty("_id").stringValue = id;
            serializedNode.FindProperty("_resource").enumValueIndex = (int)resource;
            serializedNode.FindProperty("_amountPerTap").doubleValue = amountPerTap;
            serializedNode.FindProperty("_requiredTechnologyId").stringValue = string.Empty;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(node);
            return node;
        }

        private static GameObject FindOrCreateCanvas()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            GameObject canvasObject;
            if (canvas == null)
            {
                canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
            }
            else
            {
                canvasObject = canvas.gameObject;
                GetOrAdd<CanvasScaler>(canvasObject);
                GetOrAdd<GraphicRaycaster>(canvasObject);
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static GameObject FindOrCreateChild(Transform parent, string name)
        {
            var existing = FindInScene(name);
            if (existing == null)
            {
                existing = new GameObject(name, typeof(RectTransform));
            }

            existing.transform.SetParent(parent, false);
            existing.transform.localScale = Vector3.one;
            existing.transform.localRotation = Quaternion.identity;
            return existing;
        }

        private static ResourceSlotView CreateSlot(
            Transform parent,
            string name,
            ResourceId resource,
            string title,
            TMP_FontAsset font,
            bool alignLeft)
        {
            var slotObject = FindOrCreateChild(parent, name);
            var label = GetOrAdd<TextMeshProUGUI>(slotObject);
            label.font = font;
            label.fontSize = 48f;
            label.color = Color.white;
            label.alignment = alignLeft ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
            label.raycastTarget = false;
            label.text = title + " 0";

            var rect = slotObject.GetComponent<RectTransform>();
            rect.anchorMin = alignLeft ? new Vector2(0f, 0f) : new Vector2(0.5f, 0f);
            rect.anchorMax = alignLeft ? new Vector2(0.5f, 1f) : new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(24f, 0f);
            rect.offsetMax = new Vector2(-24f, 0f);

            var slot = GetOrAdd<ResourceSlotView>(slotObject);
            var serializedSlot = new SerializedObject(slot);
            serializedSlot.FindProperty("_resource").enumValueIndex = (int)resource;
            serializedSlot.FindProperty("_title").stringValue = title;
            serializedSlot.FindProperty("_amountLabel").objectReferenceValue = label;
            serializedSlot.ApplyModifiedPropertiesWithoutUndo();
            return slot;
        }

        private static ManualResourceNodeView CreateNodeButton(
            Transform parent,
            string name,
            string caption,
            ManualResourceNodeDefinition node,
            TMP_FontAsset font,
            Vector2 position,
            Color color)
        {
            var buttonObject = FindOrCreateChild(parent, name);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(420f, 160f);

            var image = GetOrAdd<Image>(buttonObject);
            image.color = color;
            var button = GetOrAdd<Button>(buttonObject);
            button.targetGraphic = image;

            var labelTransform = buttonObject.transform.Find("Label");
            GameObject labelObject;
            if (labelTransform == null)
            {
                labelObject = new GameObject("Label", typeof(RectTransform));
                labelObject.transform.SetParent(buttonObject.transform, false);
            }
            else
            {
                labelObject = labelTransform.gameObject;
            }

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = GetOrAdd<TextMeshProUGUI>(labelObject);
            label.font = font;
            label.fontSize = 42f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.15f, 0.1f, 0.08f);
            label.raycastTarget = false;
            label.text = caption;

            var view = GetOrAdd<ManualResourceNodeView>(buttonObject);
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_node").objectReferenceValue = node;
            serializedView.FindProperty("_button").objectReferenceValue = button;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static void StretchTop(RectTransform rect, float sideInset, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(sideInset, -height);
            rect.offsetMax = new Vector2(-sideInset, 0f);
        }

        private static void SetObjectArray(Object target, string propertyName, Object first, Object second)
        {
            var serializedTarget = new SerializedObject(target);
            SetObjectArray(serializedTarget.FindProperty(propertyName), first, second);
            serializedTarget.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(SerializedProperty property, Object first, Object second)
        {
            property.arraySize = 2;
            property.GetArrayElementAtIndex(0).objectReferenceValue = first;
            property.GetArrayElementAtIndex(1).objectReferenceValue = second;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        private static GameObject FindInScene(string objectName)
        {
            var scene = EditorSceneManager.GetActiveScene();
            var objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            for (var i = 0; i < objects.Length; i++)
            {
                if (objects[i].name == objectName && objects[i].scene == scene)
                {
                    return objects[i];
                }
            }

            return null;
        }
    }
}
