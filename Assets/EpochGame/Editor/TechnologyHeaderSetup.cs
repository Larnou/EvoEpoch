using EpochGame.Core;
using EpochGame.Economy;
using EpochGame.Era;
using EpochGame.Gathering;
using EpochGame.Presentation;
using EpochGame.Database;
using EpochGame.Technology;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.EditorTools
{
    [InitializeOnLoad]
    public static class TechnologyHeaderSetup
    {
        private const string SetupKey = "EpochGame.TechnologyHeaderSetup.v2";
        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string PortraitPath = "Assets/EpochGame/Art/Placeholder/hero-portrait.jpg";

        static TechnologyHeaderSetup()
        {
            EditorApplication.delayCall += TryCreate;
        }

        [MenuItem("EpochGame/Create Technology And Header")]
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

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorPrefs.GetBool(SetupKey, false))
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
                Debug.LogError("Technology setup could not find the TextMeshPro font.");
                return;
            }

            var era = LoadEra();
            var tent = LoadTechnology(
                "Assets/EpochGame/Content/Technologies/tent.asset",
                "tent",
                "Палатка",
                new string[0],
                new[] { (0, 10d), (1, 5d) },
                new string[0]);
            var extraction = LoadTechnology(
                "Assets/EpochGame/Content/Technologies/mineral_extraction.asset",
                "mineral_extraction",
                "Добыча полезных ископаемых",
                new[] { "tent" },
                new[] { (0, 80d), (1, 60d) },
                new[] { "stone" });
            SetStringArray(new SerializedObject(era).FindProperty("_technologyIds"), "tent", "mineral_extraction");

            var stone = LoadNode();
            var database = LoadDatabase(era, tent, extraction, stone);
            var portrait = LoadPortrait();

            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Technology setup could not find a Canvas. Open SampleScene.");
                return;
            }

            MoveResourceBar();
            var header = CreateHeader(canvas.transform, font, portrait);
            var stoneSlot = CreateStoneSlot(canvas.transform, font);
            AddSlot(stoneSlot);
            var stoneButton = CreateStoneButton(canvas.transform, font, stone);
            var tentRow = CreateTechnologyRow(canvas.transform, font, tent, "TentRow");
            var extractionRow = CreateTechnologyRow(canvas.transform, font, extraction, "ExtractionRow");
            var panel = CreatePanel(canvas.transform, font, era, tentRow, extractionRow);
            WireBootstrap(database, header, panel, stoneButton);
            IconSetup.Apply();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorPrefs.SetBool(SetupKey, true);
            Debug.Log("Technology menu is ready.");
        }

        private static EraDefinition LoadEra()
        {
            const string path = "Assets/EpochGame/Content/Eras/ice_age.asset";
            var era = AssetDatabase.LoadAssetAtPath<EraDefinition>(path);
            if (era == null)
            {
                era = ScriptableObject.CreateInstance<EraDefinition>();
                AssetDatabase.CreateAsset(era, path);
            }

            var serialized = new SerializedObject(era);
            serialized.FindProperty("_id").stringValue = "ice_age";
            serialized.FindProperty("_title").stringValue = "Ледниковый период";
            serialized.FindProperty("_order").intValue = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return era;
        }

        private static TechnologyDefinition LoadTechnology(
            string path,
            string id,
            string title,
            string[] prerequisites,
            (int resource, double amount)[] costs,
            string[] unlockedNodes)
        {
            var technology = AssetDatabase.LoadAssetAtPath<TechnologyDefinition>(path);
            if (technology == null)
            {
                technology = ScriptableObject.CreateInstance<TechnologyDefinition>();
                AssetDatabase.CreateAsset(technology, path);
            }

            var serialized = new SerializedObject(technology);
            serialized.FindProperty("_id").stringValue = id;
            serialized.FindProperty("_title").stringValue = title;
            serialized.FindProperty("_eraId").stringValue = "ice_age";
            SetStringArray(serialized.FindProperty("_prerequisiteIds"), prerequisites);
            SetStringArray(serialized.FindProperty("_unlockBuildingIds"));
            SetStringArray(serialized.FindProperty("_unlockManualResourceNodeIds"), unlockedNodes);
            var costProperty = serialized.FindProperty("_costs");
            costProperty.arraySize = costs.Length;
            for (var i = 0; i < costs.Length; i++)
            {
                var cost = costProperty.GetArrayElementAtIndex(i);
                cost.FindPropertyRelative("_resource").enumValueIndex = costs[i].resource;
                cost.FindPropertyRelative("_amount").doubleValue = costs[i].amount;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return technology;
        }

        private static ManualResourceNodeDefinition LoadNode()
        {
            const string path = "Assets/EpochGame/Content/Gathering/stone.asset";
            var node = AssetDatabase.LoadAssetAtPath<ManualResourceNodeDefinition>(path);
            if (node == null)
            {
                node = ScriptableObject.CreateInstance<ManualResourceNodeDefinition>();
                AssetDatabase.CreateAsset(node, path);
            }

            var serialized = new SerializedObject(node);
            serialized.FindProperty("_id").stringValue = "stone";
            serialized.FindProperty("_resource").enumValueIndex = (int)ResourceId.Stone;
            serialized.FindProperty("_amountPerTap").doubleValue = 1d;
            serialized.FindProperty("_requiredTechnologyId").stringValue = "mineral_extraction";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return node;
        }

        private static GameDatabase LoadDatabase(
            EraDefinition era,
            TechnologyDefinition tent,
            TechnologyDefinition extraction,
            ManualResourceNodeDefinition stone)
        {
            const string path = "Assets/EpochGame/Content/GameDatabase.asset";
            var database = AssetDatabase.LoadAssetAtPath<GameDatabase>(path);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<GameDatabase>();
                AssetDatabase.CreateAsset(database, path);
            }

            var berries = AssetDatabase.LoadAssetAtPath<ManualResourceNodeDefinition>("Assets/EpochGame/Content/Gathering/berries.asset");
            var tree = AssetDatabase.LoadAssetAtPath<ManualResourceNodeDefinition>("Assets/EpochGame/Content/Gathering/tree.asset");
            var serialized = new SerializedObject(database);
            SetObjectArray(serialized.FindProperty("_eras"), era);
            SetObjectArray(serialized.FindProperty("_technologies"), tent, extraction);
            SetObjectArray(serialized.FindProperty("_manualNodes"), berries, tree, stone);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return database;
        }

        private static Sprite LoadPortrait()
        {
            var importer = AssetImporter.GetAtPath(PortraitPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(PortraitPath);
        }

        private static void MoveResourceBar()
        {
            var bar = GameObject.Find("ResourceBar");
            if (bar == null)
            {
                return;
            }

            var rect = bar.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(24f, -330f);
            rect.offsetMax = new Vector2(-24f, -210f);
        }

        private static ScreenHeaderView CreateHeader(Transform parent, TMP_FontAsset font, Sprite portrait)
        {
            var header = FindOrCreate(parent, "ScreenHeader");
            Stretch(header.GetComponent<RectTransform>(), 16f, 210f, 0f);
            var background = GetOrAdd<Image>(header);
            background.color = new Color(0.13f, 0.24f, 0.48f, 0.96f);
            background.raycastTarget = false;

            var portraitObject = FindOrCreate(header.transform, "Portrait");
            var portraitRect = portraitObject.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0f, 0.5f);
            portraitRect.anchorMax = new Vector2(0f, 0.5f);
            portraitRect.pivot = new Vector2(0f, 0.5f);
            portraitRect.anchoredPosition = new Vector2(18f, 0f);
            portraitRect.sizeDelta = new Vector2(150f, 150f);
            var image = GetOrAdd<Image>(portraitObject);
            image.sprite = portrait;
            image.preserveAspect = true;
            image.color = Color.white;

            var era = CreateText(header.transform, "EraLabel", font, 40f, TextAlignmentOptions.BottomLeft, "Ледниковый период");
            PlaceText(era.rectTransform, 186f, 12f, 0f, 78f);
            var population = CreateText(header.transform, "PopulationLabel", font, 30f, TextAlignmentOptions.MidlineLeft, "3 человека");
            var populationRect = population.rectTransform;
            populationRect.anchorMin = new Vector2(0f, 0f);
            populationRect.anchorMax = new Vector2(0.62f, 0.48f);
            populationRect.offsetMin = new Vector2(186f, 16f);
            populationRect.offsetMax = new Vector2(0f, 0f);
            var territory = CreateText(header.transform, "TerritoryLabel", font, 30f, TextAlignmentOptions.TopRight, "1 акр");
            var territoryRect = territory.rectTransform;
            territoryRect.anchorMin = new Vector2(0.55f, 0f);
            territoryRect.anchorMax = new Vector2(1f, 0.55f);
            territoryRect.offsetMin = new Vector2(0f, 8f);
            territoryRect.offsetMax = new Vector2(-18f, 0f);

            var view = GetOrAdd<ScreenHeaderView>(header);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_portrait").objectReferenceValue = image;
            serialized.FindProperty("_eraLabel").objectReferenceValue = era;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static ResourceSlotView CreateStoneSlot(Transform parent, TMP_FontAsset font)
        {
            var slotObject = FindOrCreate(parent, "StoneSlot");
            var rect = slotObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(24f, -400f);
            rect.offsetMax = new Vector2(-24f, -330f);
            var label = GetOrAdd<TextMeshProUGUI>(slotObject);
            label.font = font;
            label.fontSize = 36f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = "Камень 0";
            var slot = GetOrAdd<ResourceSlotView>(slotObject);
            var serialized = new SerializedObject(slot);
            serialized.FindProperty("_resource").enumValueIndex = (int)ResourceId.Stone;
            serialized.FindProperty("_title").stringValue = "Камень";
            serialized.FindProperty("_amountLabel").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            slotObject.SetActive(false);
            return slot;
        }

        private static void AddSlot(ResourceSlotView stoneSlot)
        {
            var bar = GameObject.Find("ResourceBar");
            if (bar == null)
            {
                return;
            }

            var view = bar.GetComponent<ResourceBarView>();
            if (view == null)
            {
                return;
            }

            var serialized = new SerializedObject(view);
            var slots = serialized.FindProperty("_slots");
            for (var i = 0; i < slots.arraySize; i++)
            {
                if (slots.GetArrayElementAtIndex(i).objectReferenceValue == stoneSlot)
                {
                    return;
                }
            }

            slots.arraySize += 1;
            slots.GetArrayElementAtIndex(slots.arraySize - 1).objectReferenceValue = stoneSlot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ManualResourceNodeView CreateStoneButton(Transform parent, TMP_FontAsset font, ManualResourceNodeDefinition node)
        {
            var buttonObject = FindOrCreate(parent, "StoneButton");
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -220f);
            rect.sizeDelta = new Vector2(420f, 150f);
            var image = GetOrAdd<Image>(buttonObject);
            image.color = new Color(0.62f, 0.64f, 0.68f);
            var button = GetOrAdd<Button>(buttonObject);
            button.targetGraphic = image;
            var label = CreateText(buttonObject.transform, "Label", font, 40f, TextAlignmentOptions.Center, "Камень");
            Fill(label.rectTransform, 12f);
            label.color = new Color(0.15f, 0.1f, 0.08f);
            var view = GetOrAdd<ManualResourceNodeView>(buttonObject);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_node").objectReferenceValue = node;
            serialized.FindProperty("_button").objectReferenceValue = button;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            buttonObject.SetActive(false);
            return view;
        }

        private static TechnologyRowView CreateTechnologyRow(
            Transform parent,
            TMP_FontAsset font,
            TechnologyDefinition technology,
            string name)
        {
            var rowObject = FindOrCreateNamed(parent, name);
            var image = GetOrAdd<Image>(rowObject);
            image.color = new Color(0.93f, 0.86f, 0.68f);
            var button = GetOrAdd<Button>(rowObject);
            button.targetGraphic = image;
            var label = CreateText(rowObject.transform, "Label", font, 32f, TextAlignmentOptions.Center, technology.Title);
            Fill(label.rectTransform, 12f);
            label.color = new Color(0.18f, 0.12f, 0.08f);
            var row = GetOrAdd<TechnologyRowView>(rowObject);
            var serialized = new SerializedObject(row);
            serialized.FindProperty("_technology").objectReferenceValue = technology;
            serialized.FindProperty("_button").objectReferenceValue = button;
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return row;
        }

        private static TechnologyPanelView CreatePanel(
            Transform parent,
            TMP_FontAsset font,
            EraDefinition era,
            TechnologyRowView tentRow,
            TechnologyRowView extractionRow)
        {
            var openButton = CreateOpenButton(parent, font);
            var panelObject = FindOrCreateNamed(parent, "TechnologyPanel");
            StretchFull(panelObject.GetComponent<RectTransform>());
            var background = GetOrAdd<Image>(panelObject);
            background.color = new Color(0.1f, 0.14f, 0.24f, 0.98f);
            background.raycastTarget = true;

            var title = CreateText(panelObject.transform, "MenuTitle", font, 52f, TextAlignmentOptions.MidlineLeft, "Технологии");
            Stretch(title.rectTransform, 40f, 100f, 36f);
            var titleRect = title.rectTransform;
            titleRect.offsetMax = new Vector2(-150f, titleRect.offsetMax.y);

            var closeButton = CreateCloseButton(panelObject.transform, font);
            var eraButton = CreateEraPlaque(panelObject.transform, font, out var eraTitle, out var eraCounter, out var eraChevron);
            var list = FindOrCreate(panelObject.transform, "TechnologyList");
            var listRect = list.GetComponent<RectTransform>();
            listRect.anchorMin = Vector2.zero;
            listRect.anchorMax = Vector2.one;
            listRect.offsetMin = new Vector2(36f, 208f);
            listRect.offsetMax = new Vector2(-36f, -120f);
            tentRow.transform.SetParent(list.transform, false);
            extractionRow.transform.SetParent(list.transform, false);
            PlaceListRow(tentRow.GetComponent<RectTransform>(), 8f, 140f);
            PlaceListRow(extractionRow.GetComponent<RectTransform>(), 156f, 140f);
            list.SetActive(false);

            var panel = GetOrAdd<TechnologyPanelView>(panelObject);
            var serialized = new SerializedObject(panel);
            serialized.FindProperty("_openButton").objectReferenceValue = openButton;
            serialized.FindProperty("_closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("_eraButton").objectReferenceValue = eraButton;
            serialized.FindProperty("_eraTitle").objectReferenceValue = eraTitle;
            serialized.FindProperty("_eraCounter").objectReferenceValue = eraCounter;
            serialized.FindProperty("_eraChevron").objectReferenceValue = eraChevron;
            serialized.FindProperty("_technologyList").objectReferenceValue = list;
            serialized.FindProperty("_era").objectReferenceValue = era;
            SetObjectArray(serialized.FindProperty("_rows"), tentRow, extractionRow);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            openButton.transform.SetAsLastSibling();
            panelObject.transform.SetAsLastSibling();
            panelObject.SetActive(false);
            return panel;
        }

        private static Button CreateOpenButton(Transform parent, TMP_FontAsset font)
        {
            var buttonObject = FindOrCreateNamed(parent, "TechnologyButton");
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 36f);
            rect.sizeDelta = new Vector2(220f, 250f);
            var frame = GetOrAdd<Image>(buttonObject);
            frame.color = new Color(0.95f, 0.84f, 0.55f);
            var button = GetOrAdd<Button>(buttonObject);
            button.targetGraphic = frame;

            var iconObject = FindOrCreate(buttonObject.transform, "Icon");
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -16f);
            iconRect.sizeDelta = new Vector2(140f, 140f);
            var icon = GetOrAdd<Image>(iconObject);
            icon.color = new Color(0.55f, 0.36f, 0.18f);
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var caption = CreateText(buttonObject.transform, "Caption", font, 32f, TextAlignmentOptions.Center, "Технологии");
            var captionRect = caption.rectTransform;
            captionRect.anchorMin = new Vector2(0f, 0f);
            captionRect.anchorMax = new Vector2(1f, 0f);
            captionRect.pivot = new Vector2(0.5f, 0f);
            captionRect.anchoredPosition = new Vector2(0f, 12f);
            captionRect.sizeDelta = new Vector2(-12f, 64f);
            caption.color = new Color(0.18f, 0.12f, 0.08f);
            buttonObject.SetActive(true);
            return button;
        }

        private static Button CreateCloseButton(Transform parent, TMP_FontAsset font)
        {
            var buttonObject = FindOrCreate(parent, "CloseButton");
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -28f);
            rect.sizeDelta = new Vector2(96f, 96f);
            var image = GetOrAdd<Image>(buttonObject);
            image.color = new Color(0.85f, 0.9f, 0.95f);
            var button = GetOrAdd<Button>(buttonObject);
            button.targetGraphic = image;
            var label = CreateText(buttonObject.transform, "Label", font, 64f, TextAlignmentOptions.Center, "×");
            Fill(label.rectTransform, 0f);
            label.color = new Color(0.15f, 0.18f, 0.24f);
            return button;
        }

        private static Button CreateEraPlaque(
            Transform parent,
            TMP_FontAsset font,
            out TMP_Text title,
            out TMP_Text counter,
            out TMP_Text chevron)
        {
            var plaque = FindOrCreate(parent, "EraPlaque");
            var rect = plaque.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(32f, 36f);
            rect.offsetMax = new Vector2(-32f, 196f);
            var image = GetOrAdd<Image>(plaque);
            image.color = new Color(0.73f, 0.86f, 0.94f);
            var button = GetOrAdd<Button>(plaque);
            button.targetGraphic = image;

            title = CreateText(plaque.transform, "EraTitle", font, 46f, TextAlignmentOptions.MidlineLeft, "Ледниковый период");
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.42f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(28f, 0f);
            titleRect.offsetMax = new Vector2(-90f, -16f);
            title.color = new Color(0.12f, 0.2f, 0.32f);

            chevron = CreateText(plaque.transform, "EraChevron", font, 42f, TextAlignmentOptions.Center, "▲");
            var chevronRect = chevron.rectTransform;
            chevronRect.anchorMin = new Vector2(1f, 0.55f);
            chevronRect.anchorMax = new Vector2(1f, 1f);
            chevronRect.offsetMin = new Vector2(-88f, 0f);
            chevronRect.offsetMax = new Vector2(-8f, -8f);
            chevron.color = new Color(0.12f, 0.2f, 0.32f);

            counter = CreateText(plaque.transform, "EraCounter", font, 42f, TextAlignmentOptions.BottomRight, "00/02");
            var counterRect = counter.rectTransform;
            counterRect.anchorMin = new Vector2(0.4f, 0f);
            counterRect.anchorMax = new Vector2(1f, 0.48f);
            counterRect.offsetMin = new Vector2(0f, 16f);
            counterRect.offsetMax = new Vector2(-28f, 0f);
            counter.color = new Color(0.12f, 0.2f, 0.32f);
            return button;
        }

        private static void PlaceListRow(RectTransform rect, float bottom, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(0f, bottom);
            rect.offsetMax = new Vector2(0f, bottom + height);
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private static GameObject FindOrCreateNamed(Transform parent, string name)
        {
            var found = FindSceneObject(name);
            if (found != null)
            {
                found.transform.SetParent(parent, false);
                found.transform.localScale = Vector3.one;
                found.transform.localRotation = Quaternion.identity;
                return found;
            }

            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static GameObject FindSceneObject(string name)
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (candidate.name == name && candidate.gameObject.scene.IsValid())
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }

        private static void WireBootstrap(
            GameDatabase database,
            ScreenHeaderView header,
            TechnologyPanelView panel,
            ManualResourceNodeView stoneButton)
        {
            var game = GameObject.Find("Game");
            if (game == null)
            {
                Debug.LogError("Technology setup could not find Game.");
                return;
            }

            var bootstrap = game.GetComponent<GameBootstrap>();
            if (bootstrap == null)
            {
                return;
            }

            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("_database").objectReferenceValue = database;
            serialized.FindProperty("_header").objectReferenceValue = header;
            serialized.FindProperty("_technologyPanel").objectReferenceValue = panel;
            var nodes = serialized.FindProperty("_nodes");
            var alreadyAdded = false;
            for (var i = 0; i < nodes.arraySize; i++)
            {
                if (nodes.GetArrayElementAtIndex(i).objectReferenceValue == stoneButton)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded)
            {
                nodes.arraySize += 1;
                nodes.GetArrayElementAtIndex(nodes.arraySize - 1).objectReferenceValue = stoneButton;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            TMP_FontAsset font,
            float size,
            TextAlignmentOptions alignment,
            string text)
        {
            var textObject = FindOrCreate(parent, name);
            var label = GetOrAdd<TextMeshProUGUI>(textObject);
            label.font = font;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        private static void PlaceText(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = new Vector2(0f, 0.45f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void Fill(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                existing.localScale = Vector3.one;
                existing.localRotation = Quaternion.identity;
                return existing.gameObject;
            }

            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(RectTransform rect, float side, float height, float top)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(side, -(top + height));
            rect.offsetMax = new Vector2(-side, -top);
        }

        private static void SetStringArray(SerializedProperty property, params string[] values)
        {
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = values[i];
            }

            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(SerializedProperty property, params Object[] values)
        {
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
