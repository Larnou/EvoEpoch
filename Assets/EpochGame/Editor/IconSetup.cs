using EpochGame.Core;
using EpochGame.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EpochGame.EditorTools
{
    [InitializeOnLoad]
    public static class IconSetup
    {
        private const string SetupKey = "EpochGame.IconSetup.v7";
        private const string IconFolder = "Assets/EpochGame/Art/Icons/";
        private const string BackgroundPath = "Assets/EpochGame/Art/Backgrounds/ice_age.jpg";
        private const string EraBannerPath = "Assets/EpochGame/Art/UI/era_ice_age.png";
        private const string GloryBannerPath = "Assets/EpochGame/Art/UI/glory_banner.png";
        private static int _attempts;

        static IconSetup()
        {
            EditorApplication.delayCall += TryApply;
        }

        [MenuItem("EpochGame/Apply Icons")]
        public static void ApplyFromMenu()
        {
            Apply();
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
                return;
            }

            _attempts++;
            if (_attempts < 8)
            {
                EditorApplication.delayCall += TryApply;
            }
        }

        public static bool Apply()
        {
            var land = LoadSprite(IconFolder + "land_winter.png");
            var people = LoadSprite(IconFolder + "people.png");
            var player = LoadSprite(IconFolder + "player.png");
            var technologies = LoadSprite(IconFolder + "technologies.png");
            var stone = LoadSprite(IconFolder + "stone.png");
            var food = LoadSprite(IconFolder + "food.png");
            var wood = LoadSprite(IconFolder + "wood.png");
            var background = LoadSprite(BackgroundPath);
            var eraBanner = LoadSprite(EraBannerPath);
            var gloryBanner = LoadSprite(GloryBannerPath);
            if (land == null || people == null || player == null || technologies == null || stone == null || food == null || wood == null || background == null || eraBanner == null || gloryBanner == null)
            {
                Debug.LogError("Icon setup could not load one of the icon sprites.");
                return false;
            }

            var portrait = FindSceneObject("Portrait");
            var era = FindSceneObject("EraLabel");
            var population = FindSceneObject("PopulationLabel");
            var territory = FindSceneObject("TerritoryLabel");
            var header = FindSceneObject("ScreenHeader");
            if (portrait == null || era == null || population == null || territory == null || header == null)
            {
                Debug.LogError("Icon setup could not find the screen header. Open SampleScene.");
                return false;
            }

            if (!LayoutHeader(header, portrait, era, population, territory, player, people, land, eraBanner, gloryBanner))
            {
                return false;
            }

            CreateBackground(header.transform.parent, background);

            if (!PlaceResourceIcon("FoodSlot", food) || !PlaceResourceIcon("WoodSlot", wood) || !PlaceResourceIcon("StoneSlot", stone))
            {
                return false;
            }

            if (!LayoutResourcePopup())
            {
                return false;
            }

            if (!PlaceTechnologyIcon(technologies))
            {
                return false;
            }

            if (!LayoutTechnologyMenu())
            {
                return false;
            }

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Header with level progress, resource popup, and ice age technology menu are ready.");
            return true;
        }

        private static bool LayoutHeader(
            GameObject header,
            GameObject portrait,
            GameObject era,
            GameObject population,
            GameObject territory,
            Sprite player,
            Sprite people,
            Sprite land,
            Sprite eraBanner,
            Sprite gloryBanner)
        {
            var eraLabel = era.GetComponent<TMP_Text>();
            if (eraLabel == null || eraLabel.font == null)
            {
                Debug.LogError("Icon setup could not read the header font.");
                return false;
            }

            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.offsetMin = new Vector2(0f, -292f);
            headerRect.offsetMax = Vector2.zero;
            var headerImage = header.GetComponent<Image>();
            if (headerImage != null)
            {
                headerImage.color = new Color(0.07f, 0.11f, 0.18f, 0.94f);
                headerImage.raycastTarget = true;
            }

            var portraitImage = portrait.GetComponent<Image>();
            portraitImage.sprite = player;
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;
            portraitImage.type = Image.Type.Simple;
            var left = EnsureColumn(header.transform, "HeaderLeft", 0f, 0.60f);
            var center = EnsureColumn(header.transform, "HeaderCenter", 0.60f, 0.81f);
            var right = EnsureColumn(header.transform, "HeaderRight", 0.81f, 1f);

            const float edge = 16f;
            const float portraitSize = 172f;
            const float avatarGap = 16f;
            const float statusX = edge + portraitSize + avatarGap;

            Adopt(left, portrait.transform);
            PlaceTopLeft(portrait.GetComponent<RectTransform>(), edge, 20f, portraitSize, portraitSize);
            var levelLabel = CreateLabel(left, "LevelLabel", eraLabel.font, "Ур. 1", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
            PlaceTopLeft(levelLabel.rectTransform, edge, 198f, portraitSize, 42f);

            Adopt(left, "EraBanner");
            var banner = PlaceIcon(left, "EraBanner", eraBanner);
            PlaceTopLeft(banner.rectTransform, statusX, 12f, 400f, 134f);
            Adopt(left, era.transform);
            PlaceTopLeft(era.GetComponent<RectTransform>(), statusX + 52f, 42f, 296f, 58f);
            StyleEraText(eraLabel, 28f);
            if (banner.transform.GetSiblingIndex() > era.transform.GetSiblingIndex())
            {
                banner.transform.SetSiblingIndex(era.transform.GetSiblingIndex());
            }

            Adopt(left, "PopulationIcon");
            var peopleIcon = PlaceIcon(left, "PopulationIcon", people);
            PlaceIconTop(peopleIcon.rectTransform, statusX, 154f, 48f);
            Adopt(left, population.transform);
            PlaceTopLeft(population.GetComponent<RectTransform>(), statusX + 58f, 152f, 330f, 40f);
            if (!SplitStatLabel(population.transform, "3", "человека", out var populationValue, out var populationCaption))
            {
                return false;
            }
            var populationFill = CreateProgressBar(
                left,
                "PopulationBar",
                new Color(0.25f, 0.72f, 0.95f),
                statusX + 58f,
                194f,
                300f,
                18f,
                0.2f);

            Adopt(left, "TerritoryIcon");
            var landIcon = PlaceIcon(left, "TerritoryIcon", land);
            PlaceIconTop(landIcon.rectTransform, statusX, 218f, 48f);
            Adopt(left, territory.transform);
            PlaceTopLeft(territory.GetComponent<RectTransform>(), statusX + 58f, 216f, 330f, 40f);
            if (!SplitStatLabel(territory.transform, "1", "акр", out var territoryValue, out var territoryCaption))
            {
                return false;
            }
            var territoryFill = CreateProgressBar(
                left,
                "TerritoryBar",
                new Color(0.55f, 0.78f, 0.28f),
                statusX + 58f,
                258f,
                300f,
                18f,
                0.1f);

            var font = eraLabel.font;
            var oldGloryCaption = center.Find("GloryCaption");
            if (oldGloryCaption != null)
            {
                oldGloryCaption.gameObject.SetActive(false);
            }

            const float bannerWidth = 216f;
            var bannerHeight = gloryBanner.rect.height > 0f
                ? bannerWidth * gloryBanner.rect.height / gloryBanner.rect.width
                : bannerWidth * 341f / 1024f;
            var gloryImage = PlaceIcon(center, "GloryBanner", gloryBanner);
            PlaceTopLeft(gloryImage.rectTransform, 4f, 6f, bannerWidth, bannerHeight);

            Adopt(gloryImage.transform, "GloryValue");
            var gloryValue = CreateLabel(gloryImage.transform, "GloryValue", font, "10", 24f, FontStyles.Bold, TextAlignmentOptions.Center);
            var gloryRect = gloryValue.rectTransform;
            gloryRect.anchorMin = new Vector2(0.42f, 0.36f);
            gloryRect.anchorMax = new Vector2(0.76f, 0.70f);
            gloryRect.offsetMin = Vector2.zero;
            gloryRect.offsetMax = Vector2.zero;
            gloryRect.pivot = new Vector2(0.5f, 0.5f);
            gloryValue.color = new Color(1f, 0.96f, 0.88f);
            gloryValue.alignment = TextAlignmentOptions.Center;
            gloryValue.enableAutoSizing = true;
            gloryValue.fontSizeMin = 14f;
            gloryValue.fontSizeMax = 26f;
            gloryValue.outlineColor = new Color32(92, 18, 16, 255);
            gloryValue.outlineWidth = 0.16f;

            Adopt(center, "CrownCaption");
            var crownCaption = CreateLabel(center, "CrownCaption", font, "Короны", 26f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            PlaceTopLeft(crownCaption.rectTransform, 12f, 96f, 118f, 52f);
            Adopt(center, "CrownValue");
            var crownValue = CreateLabel(center, "CrownValue", font, "500", 34f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            PlaceTopLeft(crownValue.rectTransform, 134f, 94f, 80f, 54f);

            Adopt(right, "ResourcesButton");
            var resourcesButton = CreateResourcesButton(right, font);
            PlaceTopLeft(resourcesButton.GetComponent<RectTransform>(), 8f, 16f, 168f, 64f);

            var view = header.GetComponent<ScreenHeaderView>();
            if (view == null)
            {
                Debug.LogError("Icon setup could not find ScreenHeaderView.");
                return false;
            }

            var serialized = new SerializedObject(view);
            serialized.FindProperty("_levelLabel").objectReferenceValue = levelLabel;
            serialized.FindProperty("_populationValue").objectReferenceValue = populationValue;
            serialized.FindProperty("_populationCaption").objectReferenceValue = populationCaption;
            serialized.FindProperty("_populationFill").objectReferenceValue = populationFill;
            serialized.FindProperty("_territoryValue").objectReferenceValue = territoryValue;
            serialized.FindProperty("_territoryCaption").objectReferenceValue = territoryCaption;
            serialized.FindProperty("_territoryFill").objectReferenceValue = territoryFill;
            serialized.FindProperty("_gloryValue").objectReferenceValue = gloryValue;
            serialized.FindProperty("_crownValue").objectReferenceValue = crownValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return CreateProgressDebug(header.transform.parent, eraLabel.font);
        }

        private static void CreateBackground(Transform canvas, Sprite sprite)
        {
            var root = FindOrCreate(canvas, "Background");
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            var picture = FindOrCreate(root.transform, "Picture");
            var pictureRect = picture.GetComponent<RectTransform>();
            pictureRect.anchorMin = Vector2.zero;
            pictureRect.anchorMax = Vector2.one;
            pictureRect.offsetMin = Vector2.zero;
            pictureRect.offsetMax = Vector2.zero;
            var image = picture.GetComponent<Image>();
            if (image == null)
            {
                image = picture.AddComponent<Image>();
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            root.transform.SetAsFirstSibling();
        }

        private static bool LayoutResourcePopup()
        {
            var bar = FindSceneObject("ResourceBar");
            var button = FindSceneObject("ResourcesButton");
            if (bar == null || button == null)
            {
                Debug.LogError("Icon setup could not find the resource popup.");
                return false;
            }

            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-16f, -304f);
            rect.sizeDelta = new Vector2(460f, 248f);
            var image = bar.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.08f, 0.1f, 0.16f, 0.96f);
                image.raycastTarget = true;
            }

            if (!StackSlot(bar.transform, "FoodSlot", 12f) || !StackSlot(bar.transform, "WoodSlot", 88f) || !StackSlot(bar.transform, "StoneSlot", 164f))
            {
                return false;
            }

            var stone = FindSceneObject("StoneSlot");
            if (stone != null)
            {
                stone.SetActive(false);
            }

            var barView = bar.GetComponent<ResourceBarView>();
            if (barView == null)
            {
                Debug.LogError("Icon setup could not find ResourceBarView.");
                return false;
            }

            var serialized = new SerializedObject(barView);
            serialized.FindProperty("_toggle").objectReferenceValue = button.GetComponent<Button>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            bar.SetActive(false);
            return true;
        }

        private static bool StackSlot(Transform bar, string name, float top)
        {
            var slot = FindSceneObject(name);
            if (slot == null)
            {
                Debug.LogError("Icon setup could not find " + name + ".");
                return false;
            }

            slot.transform.SetParent(bar, false);
            var rect = slot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(12f, -(top + 72f));
            rect.offsetMax = new Vector2(-12f, -top);

            var icon = slot.transform.Find("Icon");
            if (icon != null)
            {
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(60f, 60f);
            }

            var label = slot.GetComponent<TMP_Text>();
            if (label != null)
            {
                label.fontSize = 34f;
                label.color = Color.white;
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.margin = new Vector4(80f, 0f, 12f, 0f);
                label.outlineWidth = 0f;
            }

            return true;
        }

        private static bool LayoutTechnologyMenu()
        {
            var plaque = FindSceneObject("EraPlaque");
            var list = FindSceneObject("TechnologyList");
            var tent = FindSceneObject("TentRow");
            var extraction = FindSceneObject("ExtractionRow");
            if (plaque == null || list == null || tent == null || extraction == null)
            {
                Debug.LogError("Icon setup could not find the technology menu.");
                return false;
            }

            var plaqueRect = plaque.GetComponent<RectTransform>();
            plaqueRect.anchorMin = new Vector2(0f, 0f);
            plaqueRect.anchorMax = new Vector2(1f, 0f);
            plaqueRect.pivot = new Vector2(0.5f, 0f);
            plaqueRect.offsetMin = new Vector2(32f, 36f);
            plaqueRect.offsetMax = new Vector2(-32f, 196f);

            var listRect = list.GetComponent<RectTransform>();
            listRect.anchorMin = Vector2.zero;
            listRect.anchorMax = Vector2.one;
            listRect.offsetMin = new Vector2(36f, 208f);
            listRect.offsetMax = new Vector2(-36f, -120f);
            PlaceRowFromBottom(tent.GetComponent<RectTransform>(), 8f, 140f);
            PlaceRowFromBottom(extraction.GetComponent<RectTransform>(), 156f, 140f);
            list.SetActive(false);

            var chevron = plaque.transform.Find("EraChevron");
            if (chevron != null)
            {
                var label = chevron.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.text = "▲";
                }
            }

            return true;
        }

        private static void PlaceRowFromBottom(RectTransform rect, float bottom, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(0f, bottom);
            rect.offsetMax = new Vector2(0f, bottom + height);
        }

        private static void PlaceTopLeft(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void PlaceIconTop(RectTransform rect, float left, float top, float size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(size, size);
        }

        private static void StyleHeaderText(TMP_Text label, float size, FontStyles style)
        {
            if (label == null)
            {
                return;
            }

            label.color = Color.white;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.margin = Vector4.zero;
            label.outlineWidth = 0f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static void StyleEraText(TMP_Text label, float size)
        {
            label.color = new Color(0.32f, 0.18f, 0.08f);
            label.fontSize = size;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.margin = Vector4.zero;
            label.outlineWidth = 0f;
            label.textWrappingMode = TextWrappingModes.Normal;
        }

        public static bool SplitStatLabel(
            Transform root,
            string fallbackValue,
            string fallbackCaption,
            out TMP_Text value,
            out TMP_Text caption)
        {
            value = null;
            caption = null;
            if (root == null)
            {
                return false;
            }

            var existingValue = root.Find("Value");
            var existingCaption = root.Find("Caption");
            if (existingValue != null && existingCaption != null)
            {
                value = existingValue.GetComponent<TMP_Text>();
                caption = existingCaption.GetComponent<TMP_Text>();
                EnsureStatRow(root);
                return value != null && caption != null;
            }

            var source = root.GetComponent<TMP_Text>();
            if (source == null)
            {
                var texts = root.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0)
                {
                    source = texts[0];
                }
            }

            if (source == null || source.font == null)
            {
                Debug.LogError("Stat label split could not read a font from " + root.name + ".");
                return false;
            }

            var font = source.font;
            var size = source.fontSize;
            var color = source.color;
            var valueText = fallbackValue;
            var captionText = fallbackCaption;
            SplitStatText(source.text, ref valueText, ref captionText);
            if (source.transform == root)
            {
                UnityEngine.Object.DestroyImmediate(source);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(source.gameObject);
            }

            value = CreateLabel(root, "Value", font, valueText, size, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            caption = CreateLabel(root, "Caption", font, captionText, size, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            value.color = color;
            caption.color = color;
            EnsureStatRow(root);
            return true;
        }

        private static void SplitStatText(string text, ref string value, ref string caption)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var trimmed = text.Trim();
            var space = trimmed.IndexOf(' ');
            if (space <= 0)
            {
                value = trimmed;
                return;
            }

            value = trimmed.Substring(0, space);
            caption = trimmed.Substring(space + 1).Trim();
        }

        private static void EnsureStatRow(Transform root)
        {
            var layout = root.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
            layout.padding = new RectOffset(0, 0, 0, 0);
        }

        private static TMP_Text CreateLabel(
            Transform parent,
            string name,
            TMP_FontAsset font,
            string text,
            float size,
            FontStyles style,
            TextAlignmentOptions alignment)
        {
            var existing = parent.Find(name);
            var labelObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                labelObject.transform.SetParent(parent, false);
            }

            labelObject.transform.localScale = Vector3.one;
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                label = labelObject.AddComponent<TextMeshProUGUI>();
            }

            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = Color.white;
            label.outlineWidth = 0f;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static Image CreateProgressBar(
            Transform parent,
            string name,
            Color fillColor,
            float left,
            float top,
            float width,
            float height,
            float fill)
        {
            var trackObject = FindOrCreate(parent, name);
            PlaceTopLeft(trackObject.GetComponent<RectTransform>(), left, top, width, height);
            var track = trackObject.GetComponent<Image>();
            if (track == null)
            {
                track = trackObject.AddComponent<Image>();
            }

            track.color = new Color(0.33f, 0.4f, 0.5f, 1f);
            track.raycastTarget = false;
            track.type = Image.Type.Simple;

            var fillObject = FindOrCreate(trackObject.transform, "Fill");
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.pivot = new Vector2(0f, 0.5f);
            var image = fillObject.GetComponent<Image>();
            if (image == null)
            {
                image = fillObject.AddComponent<Image>();
            }

            image.color = fillColor;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            var ratio = fill < 0f ? 0f : fill > 1f ? 1f : fill;
            fillRect.anchorMax = new Vector2(ratio, 1f);
            return image;
        }

        private static bool CreateProgressDebug(Transform canvas, TMP_FontAsset font)
        {
            var panel = FindOrCreate(canvas, "ProgressDebug");
            PlaceTopLeft(panel.GetComponent<RectTransform>(), 16f, 308f, 360f, 136f);
            var background = panel.GetComponent<Image>();
            if (background == null)
            {
                background = panel.AddComponent<Image>();
            }

            background.color = new Color(0.07f, 0.11f, 0.18f, 0.92f);
            background.raycastTarget = true;

            var populationColor = new Color(0.2f, 0.5f, 0.72f);
            var territoryColor = new Color(0.32f, 0.55f, 0.24f);
            var populationDecrease = CreateDebugButton(panel.transform, "PopulationDecrease", font, "- человек", populationColor);
            PlaceTopLeft(populationDecrease.GetComponent<RectTransform>(), 8f, 8f, 168f, 56f);
            var populationIncrease = CreateDebugButton(panel.transform, "PopulationIncrease", font, "+ человек", populationColor);
            PlaceTopLeft(populationIncrease.GetComponent<RectTransform>(), 184f, 8f, 168f, 56f);
            var territoryDecrease = CreateDebugButton(panel.transform, "TerritoryDecrease", font, "- акр", territoryColor);
            PlaceTopLeft(territoryDecrease.GetComponent<RectTransform>(), 8f, 72f, 168f, 56f);
            var territoryIncrease = CreateDebugButton(panel.transform, "TerritoryIncrease", font, "+ акр", territoryColor);
            PlaceTopLeft(territoryIncrease.GetComponent<RectTransform>(), 184f, 72f, 168f, 56f);

            var view = panel.GetComponent<ProgressDebugView>();
            if (view == null)
            {
                view = panel.AddComponent<ProgressDebugView>();
            }

            var serialized = new SerializedObject(view);
            serialized.FindProperty("_populationDecrease").objectReferenceValue = populationDecrease;
            serialized.FindProperty("_populationIncrease").objectReferenceValue = populationIncrease;
            serialized.FindProperty("_territoryDecrease").objectReferenceValue = territoryDecrease;
            serialized.FindProperty("_territoryIncrease").objectReferenceValue = territoryIncrease;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var game = FindSceneObject("Game");
            if (game == null)
            {
                Debug.LogError("Icon setup could not find Game.");
                return false;
            }

            var bootstrap = game.GetComponent<GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogError("Icon setup could not find GameBootstrap.");
                return false;
            }

            var bootstrapObject = new SerializedObject(bootstrap);
            bootstrapObject.FindProperty("_progressDebug").objectReferenceValue = view;
            bootstrapObject.ApplyModifiedPropertiesWithoutUndo();
            panel.transform.SetAsLastSibling();
            return true;
        }

        private static Button CreateDebugButton(Transform parent, string name, TMP_FontAsset font, string text, Color color)
        {
            var existing = parent.Find(name);
            var buttonObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                buttonObject.transform.SetParent(parent, false);
            }

            buttonObject.transform.localScale = Vector3.one;
            var image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
            }

            image.color = color;
            image.raycastTarget = true;
            var button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            button.targetGraphic = image;
            var label = CreateLabel(buttonObject.transform, "Label", font, text, 24f, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = Color.white;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return button;
        }

        private static Button CreateResourcesButton(Transform parent, TMP_FontAsset font)
        {
            var existing = parent.Find("ResourcesButton");
            var buttonObject = existing != null ? existing.gameObject : new GameObject("ResourcesButton", typeof(RectTransform));
            if (existing == null)
            {
                buttonObject.transform.SetParent(parent, false);
            }

            buttonObject.transform.localScale = Vector3.one;
            var image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
            }

            image.color = new Color(0.93f, 0.78f, 0.42f);
            image.raycastTarget = true;
            var button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            button.targetGraphic = image;
            var label = CreateLabel(buttonObject.transform, "Label", font, "Ресурсы", 30f, FontStyles.Bold, TextAlignmentOptions.Center);
            label.color = new Color(0.18f, 0.12f, 0.08f);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return button;
        }

        private static Transform EnsureColumn(Transform header, string name, float minX, float maxX)
        {
            var existing = header.Find(name);
            var column = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                column.transform.SetParent(header, false);
            }

            column.transform.localScale = Vector3.one;
            column.transform.localRotation = Quaternion.identity;
            var rect = column.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return column.transform;
        }

        private static void Adopt(Transform parent, string name)
        {
            var found = FindSceneObject(name);
            if (found != null)
            {
                Adopt(parent, found.transform);
            }
        }

        private static void Adopt(Transform parent, Transform child)
        {
            if (child.parent != parent)
            {
                child.SetParent(parent, false);
            }

            child.localScale = Vector3.one;
            child.localRotation = Quaternion.identity;
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

        private static bool PlaceResourceIcon(string slotName, Sprite sprite)
        {
            var slot = FindSceneObject(slotName);
            if (slot == null)
            {
                Debug.LogError("Icon setup could not find " + slotName + ".");
                return false;
            }

            var icon = PlaceIcon(slot.transform, "Icon", sprite);
            var rect = icon.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(8f, 0f);
            rect.sizeDelta = new Vector2(84f, 84f);

            var label = slot.GetComponent<TMP_Text>();
            if (label != null)
            {
                label.margin = new Vector4(100f, 0f, 12f, 0f);
                label.alignment = TextAlignmentOptions.MidlineLeft;
            }

            return true;
        }

        private static bool PlaceTechnologyIcon(Sprite sprite)
        {
            var buttonObject = FindSceneObject("TechnologyButton");
            if (buttonObject == null)
            {
                Debug.LogError("Icon setup could not find TechnologyButton.");
                return false;
            }

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 24f);
            rect.sizeDelta = new Vector2(320f, 258f);

            var frame = buttonObject.GetComponent<Image>();
            frame.sprite = sprite;
            frame.color = Color.white;
            frame.preserveAspect = true;
            frame.type = Image.Type.Simple;

            var icon = buttonObject.transform.Find("Icon");
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }

            var caption = buttonObject.transform.Find("Caption");
            if (caption != null)
            {
                caption.gameObject.SetActive(false);
            }

            return true;
        }

        private static Image PlaceIcon(Transform parent, string name, Sprite sprite)
        {
            var existing = parent.Find(name);
            var iconObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                iconObject.transform.SetParent(parent, false);
            }

            iconObject.transform.localScale = Vector3.one;
            var image = iconObject.GetComponent<Image>();
            if (image == null)
            {
                image = iconObject.AddComponent<Image>();
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            return image;
        }

        private static Sprite LoadSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return null;
            }

            var changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
    }
}
