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
        private const string SetupKey = "EpochGame.IconSetup.v5";
        private const string IconFolder = "Assets/EpochGame/Art/Icons/";
        private const string BackgroundPath = "Assets/EpochGame/Art/Backgrounds/ice_age.jpg";
        private const string EraBannerPath = "Assets/EpochGame/Art/UI/era_ice_age.png";
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
            if (land == null || people == null || player == null || technologies == null || stone == null || food == null || wood == null || background == null || eraBanner == null)
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

            if (!LayoutHeader(header, portrait, era, population, territory, player, people, land, eraBanner))
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
            Debug.Log("Header, resource popup, and ice age technology menu are ready.");
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
            Sprite eraBanner)
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
            PlaceIconTop(peopleIcon.rectTransform, statusX, 158f, 48f);
            Adopt(left, population.transform);
            PlaceTopLeft(population.GetComponent<RectTransform>(), statusX + 58f, 156f, 330f, 50f);
            StyleHeaderText(population.GetComponent<TMP_Text>(), 32f, FontStyles.Normal);

            Adopt(left, "TerritoryIcon");
            var landIcon = PlaceIcon(left, "TerritoryIcon", land);
            PlaceIconTop(landIcon.rectTransform, statusX, 214f, 48f);
            Adopt(left, territory.transform);
            PlaceTopLeft(territory.GetComponent<RectTransform>(), statusX + 58f, 212f, 330f, 50f);
            StyleHeaderText(territory.GetComponent<TMP_Text>(), 32f, FontStyles.Normal);

            var font = eraLabel.font;
            Adopt(center, "GloryCaption");
            var gloryCaption = CreateLabel(center, "GloryCaption", font, "Слава", 26f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            PlaceTopLeft(gloryCaption.rectTransform, 12f, 18f, 118f, 52f);
            Adopt(center, "GloryValue");
            var gloryValue = CreateLabel(center, "GloryValue", font, "10", 34f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            PlaceTopLeft(gloryValue.rectTransform, 134f, 16f, 80f, 54f);

            Adopt(center, "CrownCaption");
            var crownCaption = CreateLabel(center, "CrownCaption", font, "Короны", 26f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            PlaceTopLeft(crownCaption.rectTransform, 12f, 78f, 118f, 52f);
            Adopt(center, "CrownValue");
            var crownValue = CreateLabel(center, "CrownValue", font, "500", 34f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            PlaceTopLeft(crownValue.rectTransform, 134f, 76f, 80f, 54f);

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
            serialized.FindProperty("_gloryValue").objectReferenceValue = gloryValue;
            serialized.FindProperty("_crownValue").objectReferenceValue = crownValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return true;
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
