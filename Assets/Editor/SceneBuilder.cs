using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

namespace Chisanuphong
{
    public static class SceneBuilder
    {
        private const string SceneDir = "Assets/Scenes";
        private const string SpriteDir = "Assets/Sprites";

        private static Sprite roundedBoxSprite;
        private static Sprite circleSprite;

        [MenuItem("Tools/Build All Practical Exam Scenes")]
        public static void BuildAllScenes()
        {
            EnsureAssets();
            EnsureDirectories();

            BuildMainMenuScene();
            BuildStageSelectionScene();
            BuildOptionsScene();
            BuildCreditsScene();
            BuildStage1Scene();
            BuildStage2Scene();
            BuildWinScene();
            BuildGameOverScene();

            SetupBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Load MainMenu scene as active
            EditorSceneManager.OpenScene($"{SceneDir}/MainMenu.unity");
            Debug.Log("<color=#10B981><b>[SceneBuilder] All 8 scenes successfully created, styled, and linked with persistent events!</b></color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(SceneDir)) Directory.CreateDirectory(SceneDir);
            if (!Directory.Exists(SpriteDir)) Directory.CreateDirectory(SpriteDir);
        }

        private static void EnsureAssets()
        {
            string boxPath = $"{SpriteDir}/UIRoundedBox.png";
            string circlePath = $"{SpriteDir}/UICircle.png";

            if (!File.Exists(boxPath))
            {
                Texture2D tex = CreateRoundedRectTexture(128, 128, 24);
                File.WriteAllBytes(boxPath, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(boxPath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(boxPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteBorder = new Vector4(24, 24, 24, 24);
                    importer.SaveAndReimport();
                }
            }

            if (!File.Exists(circlePath))
            {
                Texture2D tex = CreateCircleTexture(64, 64);
                File.WriteAllBytes(circlePath, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(circlePath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(circlePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
            }

            roundedBoxSprite = AssetDatabase.LoadAssetAtPath<Sprite>(boxPath);
            circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(circlePath);
        }

        private static Texture2D CreateRoundedRectTexture(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] colors = new Color[width * height];
            float r2 = radius * radius;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dx = 0;
                    int dy = 0;

                    if (x < radius) dx = radius - x;
                    else if (x >= width - radius) dx = x - (width - radius - 1);

                    if (y < radius) dy = radius - y;
                    else if (y >= height - radius) dy = y - (height - radius - 1);

                    if (dx > 0 && dy > 0)
                    {
                        float distSq = dx * dx + dy * dy;
                        if (distSq > r2)
                        {
                            colors[y * width + x] = Color.clear;
                            continue;
                        }
                    }

                    colors[y * width + x] = Color.white;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCircleTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] colors = new Color[width * height];
            float cx = width / 2f;
            float cy = height / 2f;
            float r = width / 2f;
            float rSq = r * r;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - cx + 0.5f;
                    float dy = y - cy + 0.5f;
                    if (dx * dx + dy * dy <= rSq)
                    {
                        colors[y * width + x] = Color.white;
                    }
                    else
                    {
                        colors[y * width + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        #region Common Scene Factory Helpers

        private static (Canvas canvas, EventSystem es) SetupSceneBasics(string sceneName, Color cameraBg)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            GameObject camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = cameraBg;
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 1.5f, -10);

            // Light
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.color = Color.white;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Canvas
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Event System
            GameObject esGo = new GameObject("EventSystem");
            EventSystem es = esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();

            return (canvas, es);
        }

        private static GameObject CreateFullscreenOverlay(Transform parent, Color color)
        {
            GameObject bgGo = new GameObject("BackgroundOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(parent, false);
            RectTransform rt = bgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bgGo.GetComponent<Image>();
            img.color = color;
            return bgGo;
        }

        private static GameObject CreateCardPanel(Transform parent, string name, Vector2 size, Color bgColor)
        {
            GameObject cardGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardGo.transform.SetParent(parent, false);
            RectTransform rt = cardGo.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            Image img = cardGo.GetComponent<Image>();
            img.sprite = roundedBoxSprite;
            img.type = Image.Type.Sliced;
            img.color = bgColor;

            return cardGo;
        }

        private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, float fontSize, FontWeight weight, TextAlignmentOptions align, Color color, Vector2 sizeDelta)
        {
            GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);
            RectTransform rt = textGo.GetComponent<RectTransform>();
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontWeight = weight;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateStyledButton(Transform parent, string name, string label, Vector2 size, Color normalColor, Color hoverColor, Color textColor)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            Image img = btnGo.GetComponent<Image>();
            img.sprite = roundedBoxSprite;
            img.type = Image.Type.Sliced;
            img.color = normalColor;

            Button btn = btnGo.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = normalColor;
            cb.highlightedColor = hoverColor;
            cb.pressedColor = normalColor * 0.85f;
            cb.selectedColor = hoverColor;
            cb.disabledColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.4f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.1f;
            btn.colors = cb;

            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.fontWeight = FontWeight.SemiBold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.raycastTarget = false;

            return btn;
        }

        private static Slider CreateStyledSlider(Transform parent, string name, float initialVal, Color fillColor, out TextMeshProUGUI valText)
        {
            GameObject sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(parent, false);
            RectTransform rt = sliderGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(360, 36);

            Slider slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = initialVal;

            // Background
            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(sliderGo.transform, false);
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.25f);
            bgRt.anchorMax = new Vector2(1, 0.75f);
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = roundedBoxSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.25f);
            faRt.anchorMax = new Vector2(1, 0.75f);
            faRt.sizeDelta = new Vector2(-20, 0);

            // Fill
            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.GetComponent<Image>();
            fillImg.sprite = roundedBoxSprite;
            fillImg.type = Image.Type.Sliced;
            fillImg.color = fillColor;

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            RectTransform haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = new Vector2(-20, 0);

            // Handle
            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(28, 28);
            Image handleImg = handle.GetComponent<Image>();
            handleImg.sprite = circleSprite;
            handleImg.color = Color.white;

            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;

            // Value text
            valText = CreateTMPText(sliderGo.transform, "ValueText", $"{Mathf.RoundToInt(initialVal * 100)}%", 18, FontWeight.Bold, TextAlignmentOptions.Right, Color.white, new Vector2(60, 30));
            RectTransform vtRt = valText.GetComponent<RectTransform>();
            vtRt.anchorMin = new Vector2(1, 0.5f);
            vtRt.anchorMax = new Vector2(1, 0.5f);
            vtRt.anchoredPosition = new Vector2(50, 0);

            return slider;
        }

        #endregion

        #region Scene Builders

        // 1. MainMenu
        public static void BuildMainMenuScene()
        {
            var (canvas, es) = SetupSceneBasics("MainMenu", new Color(0.06f, 0.09f, 0.16f));
            CreateFullscreenOverlay(canvas.transform, new Color(0.06f, 0.09f, 0.16f, 0.95f));

            GameObject card = CreateCardPanel(canvas.transform, "MainMenuCard", new Vector2(650, 680), new Color(0.12f, 0.16f, 0.24f, 0.95f));
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 45, 45);
            vlg.spacing = 18;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Title Header per Fig 1
            CreateTMPText(card.transform, "CourseTitle", "958321 - PRACTICAL EXAMINATION", 26, FontWeight.Bold, TextAlignmentOptions.Center, new Color(0.22f, 0.74f, 0.98f), new Vector2(570, 36));

            // Student Info Card Header
            GameObject infoBox = CreateCardPanel(card.transform, "StudentInfoBox", new Vector2(570, 75), new Color(0.18f, 0.23f, 0.33f, 0.8f));
            VerticalLayoutGroup infoVlg = infoBox.AddComponent<VerticalLayoutGroup>();
            infoVlg.padding = new RectOffset(15, 15, 10, 10);
            infoVlg.spacing = 4;
            infoVlg.childAlignment = TextAnchor.MiddleCenter;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = false;

            var studentTmp = CreateTMPText(infoBox.transform, "StudentInfo", "Student Code: 672110088\nName: Chisanuphong Khiankrachang", 17, FontWeight.Medium, TextAlignmentOptions.Center, new Color(0.9f, 0.95f, 1f), new Vector2(540, 50));

            // Subtitle
            CreateTMPText(card.transform, "SubTitle", "MAIN MENU", 20, FontWeight.SemiBold, TextAlignmentOptions.Center, new Color(0.6f, 0.7f, 0.85f), new Vector2(570, 28));

            // Buttons
            Color btnPrimary = new Color(0.15f, 0.45f, 0.88f);
            Color btnPrimaryHover = new Color(0.25f, 0.58f, 1f);
            Color btnSecondary = new Color(0.2f, 0.26f, 0.36f);
            Color btnSecondaryHover = new Color(0.3f, 0.38f, 0.5f);
            Color btnDanger = new Color(0.85f, 0.25f, 0.25f);
            Color btnDangerHover = new Color(0.95f, 0.35f, 0.35f);

            Button startBtn = CreateStyledButton(card.transform, "StartButton", "START GAME", new Vector2(570, 56), btnPrimary, btnPrimaryHover, Color.white);
            Button optionsBtn = CreateStyledButton(card.transform, "OptionsButton", "OPTIONS", new Vector2(570, 56), btnSecondary, btnSecondaryHover, Color.white);
            Button creditsBtn = CreateStyledButton(card.transform, "CreditsButton", "CREDITS", new Vector2(570, 56), btnSecondary, btnSecondaryHover, Color.white);
            Button exitBtn = CreateStyledButton(card.transform, "ExitButton", "EXIT", new Vector2(570, 56), btnDanger, btnDangerHover, Color.white);

            // Hook UI Controller & SceneController
            GameObject uiMgr = new GameObject("MainMenuUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            MainMenuUI menuUI = uiMgr.AddComponent<MainMenuUI>();

            UnityEventTools.AddPersistentListener(startBtn.onClick, sc.LoadStageSelection);
            UnityEventTools.AddPersistentListener(optionsBtn.onClick, sc.LoadOptions);
            UnityEventTools.AddPersistentListener(creditsBtn.onClick, sc.LoadCredits);
            UnityEventTools.AddPersistentListener(exitBtn.onClick, sc.QuitGame);

            SerializedObject so = new SerializedObject(menuUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("startButton").objectReferenceValue = startBtn;
            so.FindProperty("optionsButton").objectReferenceValue = optionsBtn;
            so.FindProperty("creditsButton").objectReferenceValue = creditsBtn;
            so.FindProperty("exitButton").objectReferenceValue = exitBtn;
            so.FindProperty("studentInfoText").objectReferenceValue = studentTmp;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/MainMenu.unity");
        }

        // 2. StageSelection
        public static void BuildStageSelectionScene()
        {
            var (canvas, es) = SetupSceneBasics("StageSelection", new Color(0.06f, 0.09f, 0.16f));
            CreateFullscreenOverlay(canvas.transform, new Color(0.06f, 0.09f, 0.16f, 0.95f));

            GameObject card = CreateCardPanel(canvas.transform, "StageSelectCard", new Vector2(650, 540), new Color(0.12f, 0.16f, 0.24f, 0.95f));
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 45, 45);
            vlg.spacing = 22;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            CreateTMPText(card.transform, "Title", "STAGE SELECTION", 30, FontWeight.Bold, TextAlignmentOptions.Center, new Color(0.22f, 0.74f, 0.98f), new Vector2(570, 40));
            CreateTMPText(card.transform, "SubTitle", "Select a minigame stage to begin", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.6f, 0.7f, 0.85f), new Vector2(570, 24));

            Color btnPrimary = new Color(0.15f, 0.45f, 0.88f);
            Color btnPrimaryHover = new Color(0.25f, 0.58f, 1f);
            Color btnPurple = new Color(0.45f, 0.28f, 0.82f);
            Color btnPurpleHover = new Color(0.58f, 0.38f, 0.95f);
            Color btnBack = new Color(0.25f, 0.3f, 0.4f);
            Color btnBackHover = new Color(0.35f, 0.42f, 0.55f);

            Button stage1Btn = CreateStyledButton(card.transform, "Stage1Button", "Stage 1 : Capsule Player", new Vector2(570, 64), btnPrimary, btnPrimaryHover, Color.white);
            Button stage2Btn = CreateStyledButton(card.transform, "Stage2Button", "Stage 2 : Humanoid Player", new Vector2(570, 64), btnPurple, btnPurpleHover, Color.white);
            Button backBtn = CreateStyledButton(card.transform, "BackButton", "← BACK", new Vector2(570, 54), btnBack, btnBackHover, Color.white);

            GameObject uiMgr = new GameObject("StageSelectionUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            StageSelectionUI stageUI = uiMgr.AddComponent<StageSelectionUI>();

            UnityEventTools.AddPersistentListener(stage1Btn.onClick, sc.LoadStage1);
            UnityEventTools.AddPersistentListener(stage2Btn.onClick, sc.LoadStage2);
            UnityEventTools.AddPersistentListener(backBtn.onClick, sc.LoadMainMenu);

            SerializedObject so = new SerializedObject(stageUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("stage1Button").objectReferenceValue = stage1Btn;
            so.FindProperty("stage2Button").objectReferenceValue = stage2Btn;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/StageSelection.unity");
        }

        // 3. Options
        public static void BuildOptionsScene()
        {
            var (canvas, es) = SetupSceneBasics("Options", new Color(0.06f, 0.09f, 0.16f));
            CreateFullscreenOverlay(canvas.transform, new Color(0.06f, 0.09f, 0.16f, 0.95f));

            GameObject card = CreateCardPanel(canvas.transform, "OptionsCard", new Vector2(650, 560), new Color(0.12f, 0.16f, 0.24f, 0.95f));
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 45, 45);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            CreateTMPText(card.transform, "Title", "OPTIONS", 30, FontWeight.Bold, TextAlignmentOptions.Center, new Color(0.22f, 0.74f, 0.98f), new Vector2(570, 40));
            CreateTMPText(card.transform, "SubTitle", "Audio & Sound Effects Settings", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.6f, 0.7f, 0.85f), new Vector2(570, 24));

            // BGM Row
            GameObject bgmRow = new GameObject("BGMRow", typeof(RectTransform));
            bgmRow.transform.SetParent(card.transform, false);
            bgmRow.GetComponent<RectTransform>().sizeDelta = new Vector2(570, 60);
            CreateTMPText(bgmRow.transform, "BGMLabel", "BGM Volume", 18, FontWeight.SemiBold, TextAlignmentOptions.Left, Color.white, new Vector2(150, 36));
            Slider bgmSlider = CreateStyledSlider(bgmRow.transform, "BGMSlider", 0.75f, new Color(0.22f, 0.74f, 0.98f), out TextMeshProUGUI bgmValText);
            bgmSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, 0);

            // SFX Row
            GameObject sfxRow = new GameObject("SFXRow", typeof(RectTransform));
            sfxRow.transform.SetParent(card.transform, false);
            sfxRow.GetComponent<RectTransform>().sizeDelta = new Vector2(570, 60);
            CreateTMPText(sfxRow.transform, "SFXLabel", "SFX Volume", 18, FontWeight.SemiBold, TextAlignmentOptions.Left, Color.white, new Vector2(150, 36));
            Slider sfxSlider = CreateStyledSlider(sfxRow.transform, "SFXSlider", 0.75f, new Color(0.22f, 0.74f, 0.98f), out TextMeshProUGUI sfxValText);
            sfxSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, 0);

            Color btnBack = new Color(0.25f, 0.3f, 0.4f);
            Color btnBackHover = new Color(0.35f, 0.42f, 0.55f);
            Button backBtn = CreateStyledButton(card.transform, "BackButton", "← BACK", new Vector2(570, 54), btnBack, btnBackHover, Color.white);

            GameObject uiMgr = new GameObject("OptionsUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            OptionsUI optUI = uiMgr.AddComponent<OptionsUI>();

            UnityEventTools.AddPersistentListener(backBtn.onClick, sc.LoadMainMenu);

            SerializedObject so = new SerializedObject(optUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("bgmSlider").objectReferenceValue = bgmSlider;
            so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            so.FindProperty("bgmValueText").objectReferenceValue = bgmValText;
            so.FindProperty("sfxValueText").objectReferenceValue = sfxValText;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/Options.unity");
        }

        // 4. Credits
        public static void BuildCreditsScene()
        {
            var (canvas, es) = SetupSceneBasics("Credits", new Color(0.06f, 0.09f, 0.16f));
            CreateFullscreenOverlay(canvas.transform, new Color(0.06f, 0.09f, 0.16f, 0.95f));

            GameObject card = CreateCardPanel(canvas.transform, "CreditsCard", new Vector2(680, 580), new Color(0.12f, 0.16f, 0.24f, 0.95f));
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 45, 45);
            vlg.spacing = 16;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            CreateTMPText(card.transform, "Title", "CREDITS", 30, FontWeight.Bold, TextAlignmentOptions.Center, new Color(0.22f, 0.74f, 0.98f), new Vector2(600, 40));

            GameObject infoBox = CreateCardPanel(card.transform, "InfoCard", new Vector2(600, 240), new Color(0.18f, 0.23f, 0.33f, 0.85f));
            VerticalLayoutGroup infoVlg = infoBox.AddComponent<VerticalLayoutGroup>();
            infoVlg.padding = new RectOffset(20, 20, 20, 20);
            infoVlg.spacing = 10;
            infoVlg.childAlignment = TextAnchor.MiddleCenter;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = false;

            var courseTmp = CreateTMPText(infoBox.transform, "Course", "Course: 958321 Game Development 3", 19, FontWeight.SemiBold, TextAlignmentOptions.Center, new Color(0.22f, 0.74f, 0.98f), new Vector2(560, 30));
            var codeTmp = CreateTMPText(infoBox.transform, "StudentCode", "Student Code: 672110088", 22, FontWeight.Bold, TextAlignmentOptions.Center, Color.white, new Vector2(560, 32));
            var nameTmp = CreateTMPText(infoBox.transform, "StudentName", "Name: Chisanuphong Khiankrachang", 22, FontWeight.Bold, TextAlignmentOptions.Center, Color.white, new Vector2(560, 32));
            var deptTmp = CreateTMPText(infoBox.transform, "Dept", "Department of Digital Game\nCollege of Arts, Media and Technology\nChiang Mai University", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.75f, 0.82f, 0.92f), new Vector2(560, 48));

            Color btnBack = new Color(0.25f, 0.3f, 0.4f);
            Color btnBackHover = new Color(0.35f, 0.42f, 0.55f);
            Button backBtn = CreateStyledButton(card.transform, "BackButton", "← BACK", new Vector2(600, 54), btnBack, btnBackHover, Color.white);

            GameObject uiMgr = new GameObject("CreditsUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            CreditsUI crUI = uiMgr.AddComponent<CreditsUI>();

            UnityEventTools.AddPersistentListener(backBtn.onClick, sc.LoadMainMenu);

            SerializedObject so = new SerializedObject(crUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("courseText").objectReferenceValue = courseTmp;
            so.FindProperty("studentCodeText").objectReferenceValue = codeTmp;
            so.FindProperty("studentNameText").objectReferenceValue = nameTmp;
            so.FindProperty("departmentText").objectReferenceValue = deptTmp;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/Credits.unity");
        }

        // 5. Stage1_Capsule
        public static void BuildStage1Scene()
        {
            var (canvas, es) = SetupSceneBasics("Stage1_Capsule", new Color(0.1f, 0.12f, 0.16f));

            // Create a 3D floor plane for Stage 1 environment
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor_Stage1";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3, 1, 3);

            // Capsule placeholder
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "CapsulePlayer_Placeholder";
            capsule.transform.position = new Vector3(0, 1f, 0);

            // Build HUD overlay (Top Left)
            GameObject hudCard = CreateCardPanel(canvas.transform, "HUD_Panel", new Vector2(280, 130), new Color(0.1f, 0.14f, 0.22f, 0.9f));
            RectTransform hudRt = hudCard.GetComponent<RectTransform>();
            hudRt.anchorMin = new Vector2(0, 1);
            hudRt.anchorMax = new Vector2(0, 1);
            hudRt.pivot = new Vector2(0, 1);
            hudRt.anchoredPosition = new Vector2(30, -30);

            VerticalLayoutGroup hudVlg = hudCard.AddComponent<VerticalLayoutGroup>();
            hudVlg.padding = new RectOffset(20, 20, 15, 15);
            hudVlg.spacing = 8;
            hudVlg.childControlWidth = true;
            hudVlg.childControlHeight = false;

            var titleTmp = CreateTMPText(hudCard.transform, "Title", "STAGE 1 : CAPSULE", 16, FontWeight.Bold, TextAlignmentOptions.Left, new Color(0.22f, 0.74f, 0.98f), new Vector2(240, 24));
            var timerTmp = CreateTMPText(hudCard.transform, "TimerText", "Time : 30s", 20, FontWeight.Bold, TextAlignmentOptions.Left, new Color(1f, 0.84f, 0f), new Vector2(240, 26));
            var scoreTmp = CreateTMPText(hudCard.transform, "ScoreText", "Score : 00000", 20, FontWeight.Bold, TextAlignmentOptions.Left, Color.white, new Vector2(240, 26));

            // Right Action / Test Buttons Panel per Fig 1
            GameObject actionCard = CreateCardPanel(canvas.transform, "ActionPanel", new Vector2(200, 190), new Color(0.1f, 0.14f, 0.22f, 0.9f));
            RectTransform actRt = actionCard.GetComponent<RectTransform>();
            actRt.anchorMin = new Vector2(1, 0.5f);
            actRt.anchorMax = new Vector2(1, 0.5f);
            actRt.pivot = new Vector2(1, 0.5f);
            actRt.anchoredPosition = new Vector2(-30, 0);

            VerticalLayoutGroup actVlg = actionCard.AddComponent<VerticalLayoutGroup>();
            actVlg.padding = new RectOffset(15, 15, 15, 15);
            actVlg.spacing = 10;
            actVlg.childControlWidth = true;
            actVlg.childControlHeight = false;

            Color btnWin = new Color(0.08f, 0.6f, 0.35f);
            Color btnWinHover = new Color(0.12f, 0.75f, 0.45f);
            Color btnLose = new Color(0.85f, 0.25f, 0.25f);
            Color btnLoseHover = new Color(0.95f, 0.35f, 0.35f);
            Color btnBack = new Color(0.25f, 0.3f, 0.4f);
            Color btnBackHover = new Color(0.35f, 0.42f, 0.55f);

            Button winBtn = CreateStyledButton(actionCard.transform, "WinButton", "Win", new Vector2(170, 42), btnWin, btnWinHover, Color.white);
            Button loseBtn = CreateStyledButton(actionCard.transform, "GameOverButton", "Game Over", new Vector2(170, 42), btnLose, btnLoseHover, Color.white);
            Button backBtn = CreateStyledButton(actionCard.transform, "BackButton", "Back", new Vector2(170, 42), btnBack, btnBackHover, Color.white);

            GameObject uiMgr = new GameObject("StageUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            StageUI sUI = uiMgr.AddComponent<StageUI>();

            UnityEventTools.AddPersistentListener(winBtn.onClick, sc.LoadWin);
            UnityEventTools.AddPersistentListener(loseBtn.onClick, sc.LoadGameOver);
            UnityEventTools.AddPersistentListener(backBtn.onClick, sc.LoadStageSelection);

            SerializedObject so = new SerializedObject(sUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("stageTitleText").objectReferenceValue = titleTmp;
            so.FindProperty("timerText").objectReferenceValue = timerTmp;
            so.FindProperty("scoreText").objectReferenceValue = scoreTmp;
            so.FindProperty("winButton").objectReferenceValue = winBtn;
            so.FindProperty("gameOverButton").objectReferenceValue = loseBtn;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("stageName").stringValue = "Stage 1 - Capsule Player Minigame";
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/Stage1_Capsule.unity");
        }

        // 6. Stage2_Humanoid
        public static void BuildStage2Scene()
        {
            var (canvas, es) = SetupSceneBasics("Stage2_Humanoid", new Color(0.12f, 0.1f, 0.16f));

            // Create a 3D floor plane for Stage 2 arena
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor_Stage2";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(5, 1, 5);

            // Humanoid placeholder
            GameObject humanoid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            humanoid.name = "HumanoidPlayer_Placeholder";
            humanoid.transform.position = new Vector3(0, 1f, 0);

            // Build HUD overlay (Top Left)
            GameObject hudCard = CreateCardPanel(canvas.transform, "HUD_Panel", new Vector2(280, 130), new Color(0.14f, 0.12f, 0.22f, 0.9f));
            RectTransform hudRt = hudCard.GetComponent<RectTransform>();
            hudRt.anchorMin = new Vector2(0, 1);
            hudRt.anchorMax = new Vector2(0, 1);
            hudRt.pivot = new Vector2(0, 1);
            hudRt.anchoredPosition = new Vector2(30, -30);

            VerticalLayoutGroup hudVlg = hudCard.AddComponent<VerticalLayoutGroup>();
            hudVlg.padding = new RectOffset(20, 20, 15, 15);
            hudVlg.spacing = 8;
            hudVlg.childControlWidth = true;
            hudVlg.childControlHeight = false;

            var titleTmp = CreateTMPText(hudCard.transform, "Title", "STAGE 2 : HUMANOID", 16, FontWeight.Bold, TextAlignmentOptions.Left, new Color(0.65f, 0.45f, 0.95f), new Vector2(240, 24));
            var timerTmp = CreateTMPText(hudCard.transform, "TimerText", "Time : 30s", 20, FontWeight.Bold, TextAlignmentOptions.Left, new Color(1f, 0.84f, 0f), new Vector2(240, 26));
            var scoreTmp = CreateTMPText(hudCard.transform, "ScoreText", "Score : 00000", 20, FontWeight.Bold, TextAlignmentOptions.Left, Color.white, new Vector2(240, 26));

            // Right Action / Test Buttons Panel per Fig 1
            GameObject actionCard = CreateCardPanel(canvas.transform, "ActionPanel", new Vector2(200, 190), new Color(0.14f, 0.12f, 0.22f, 0.9f));
            RectTransform actRt = actionCard.GetComponent<RectTransform>();
            actRt.anchorMin = new Vector2(1, 0.5f);
            actRt.anchorMax = new Vector2(1, 0.5f);
            actRt.pivot = new Vector2(1, 0.5f);
            actRt.anchoredPosition = new Vector2(-30, 0);

            VerticalLayoutGroup actVlg = actionCard.AddComponent<VerticalLayoutGroup>();
            actVlg.padding = new RectOffset(15, 15, 15, 15);
            actVlg.spacing = 10;
            actVlg.childControlWidth = true;
            actVlg.childControlHeight = false;

            Color btnWin = new Color(0.08f, 0.6f, 0.35f);
            Color btnWinHover = new Color(0.12f, 0.75f, 0.45f);
            Color btnLose = new Color(0.85f, 0.25f, 0.25f);
            Color btnLoseHover = new Color(0.95f, 0.35f, 0.35f);
            Color btnBack = new Color(0.25f, 0.3f, 0.4f);
            Color btnBackHover = new Color(0.35f, 0.42f, 0.55f);

            Button winBtn = CreateStyledButton(actionCard.transform, "WinButton", "Win", new Vector2(170, 42), btnWin, btnWinHover, Color.white);
            Button loseBtn = CreateStyledButton(actionCard.transform, "GameOverButton", "Game Over", new Vector2(170, 42), btnLose, btnLoseHover, Color.white);
            Button backBtn = CreateStyledButton(actionCard.transform, "BackButton", "Back", new Vector2(170, 42), btnBack, btnBackHover, Color.white);

            GameObject uiMgr = new GameObject("StageUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            StageUI sUI = uiMgr.AddComponent<StageUI>();

            UnityEventTools.AddPersistentListener(winBtn.onClick, sc.LoadWin);
            UnityEventTools.AddPersistentListener(loseBtn.onClick, sc.LoadGameOver);
            UnityEventTools.AddPersistentListener(backBtn.onClick, sc.LoadStageSelection);

            SerializedObject so = new SerializedObject(sUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("stageTitleText").objectReferenceValue = titleTmp;
            so.FindProperty("timerText").objectReferenceValue = timerTmp;
            so.FindProperty("scoreText").objectReferenceValue = scoreTmp;
            so.FindProperty("winButton").objectReferenceValue = winBtn;
            so.FindProperty("gameOverButton").objectReferenceValue = loseBtn;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("stageName").stringValue = "Stage 2 - Humanoid Player Minigame";
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/Stage2_Humanoid.unity");
        }

        // 7. WinScreen
        public static void BuildWinScene()
        {
            var (canvas, es) = SetupSceneBasics("WinScreen", new Color(0.04f, 0.18f, 0.12f));
            CreateFullscreenOverlay(canvas.transform, new Color(0.04f, 0.18f, 0.12f, 0.95f));

            // Green Card per Fig 1
            GameObject card = CreateCardPanel(canvas.transform, "WinCard", new Vector2(620, 520), new Color(0.08f, 0.32f, 0.2f, 0.95f));
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 45, 45);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            CreateTMPText(card.transform, "Title", "WIN", 36, FontWeight.Bold, TextAlignmentOptions.Center, new Color(0.3f, 0.95f, 0.5f), new Vector2(540, 45));
            CreateTMPText(card.transform, "SubTitle", "Congratulations! Condition Achieved", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.7f, 0.95f, 0.8f), new Vector2(540, 24));

            // Score Display Box
            GameObject scoreBox = CreateCardPanel(card.transform, "ScoreBox", new Vector2(540, 80), new Color(0.05f, 0.22f, 0.14f, 0.85f));
            VerticalLayoutGroup sVlg = scoreBox.AddComponent<VerticalLayoutGroup>();
            sVlg.padding = new RectOffset(15, 15, 12, 12);
            sVlg.childAlignment = TextAnchor.MiddleCenter;
            sVlg.childControlWidth = true;
            sVlg.childControlHeight = false;

            var scoreTmp = CreateTMPText(scoreBox.transform, "ScoreDisplay", "Score : 00000", 28, FontWeight.Bold, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.3f), new Vector2(500, 36));

            Color btnGreen = new Color(0.12f, 0.55f, 0.32f);
            Color btnGreenHover = new Color(0.18f, 0.72f, 0.42f);
            Color btnSecondary = new Color(0.15f, 0.25f, 0.2f);
            Color btnSecondaryHover = new Color(0.22f, 0.35f, 0.28f);

            Button tryAgainBtn = CreateStyledButton(card.transform, "TryAgainButton", "Try Again", new Vector2(540, 56), btnGreen, btnGreenHover, Color.white);
            Button menuBtn = CreateStyledButton(card.transform, "MainMenuButton", "To Main Menu", new Vector2(540, 56), btnSecondary, btnSecondaryHover, Color.white);

            GameObject uiMgr = new GameObject("WinUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            WinUI wUI = uiMgr.AddComponent<WinUI>();

            UnityEventTools.AddPersistentListener(tryAgainBtn.onClick, sc.TryAgain);
            UnityEventTools.AddPersistentListener(menuBtn.onClick, sc.LoadMainMenu);

            SerializedObject so = new SerializedObject(wUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("scoreText").objectReferenceValue = scoreTmp;
            so.FindProperty("tryAgainButton").objectReferenceValue = tryAgainBtn;
            so.FindProperty("mainMenuButton").objectReferenceValue = menuBtn;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/WinScreen.unity");
        }

        // 8. GameOverScreen
        public static void BuildGameOverScene()
        {
            var (canvas, es) = SetupSceneBasics("GameOverScreen", new Color(0.18f, 0.05f, 0.05f));
            CreateFullscreenOverlay(canvas.transform, new Color(0.18f, 0.05f, 0.05f, 0.95f));

            // Red Card per Fig 1
            GameObject card = CreateCardPanel(canvas.transform, "GameOverCard", new Vector2(620, 520), new Color(0.35f, 0.1f, 0.1f, 0.95f));
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 45, 45);
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            CreateTMPText(card.transform, "Title", "GAME OVER", 36, FontWeight.Bold, TextAlignmentOptions.Center, new Color(1f, 0.35f, 0.35f), new Vector2(540, 45));
            CreateTMPText(card.transform, "SubTitle", "Time Expired / Fell Off Platform", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.95f, 0.75f, 0.75f), new Vector2(540, 24));

            // Score Display Box
            GameObject scoreBox = CreateCardPanel(card.transform, "ScoreBox", new Vector2(540, 80), new Color(0.24f, 0.06f, 0.06f, 0.85f));
            VerticalLayoutGroup sVlg = scoreBox.AddComponent<VerticalLayoutGroup>();
            sVlg.padding = new RectOffset(15, 15, 12, 12);
            sVlg.childAlignment = TextAnchor.MiddleCenter;
            sVlg.childControlWidth = true;
            sVlg.childControlHeight = false;

            var scoreTmp = CreateTMPText(scoreBox.transform, "ScoreDisplay", "Score : 00000", 28, FontWeight.Bold, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.85f), new Vector2(500, 36));

            Color btnRed = new Color(0.75f, 0.18f, 0.18f);
            Color btnRedHover = new Color(0.9f, 0.28f, 0.28f);
            Color btnSecondary = new Color(0.25f, 0.15f, 0.15f);
            Color btnSecondaryHover = new Color(0.35f, 0.22f, 0.22f);

            Button tryAgainBtn = CreateStyledButton(card.transform, "TryAgainButton", "Try Again", new Vector2(540, 56), btnRed, btnRedHover, Color.white);
            Button menuBtn = CreateStyledButton(card.transform, "MainMenuButton", "To Main Menu", new Vector2(540, 56), btnSecondary, btnSecondaryHover, Color.white);

            GameObject uiMgr = new GameObject("GameOverUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            GameOverUI goUI = uiMgr.AddComponent<GameOverUI>();

            UnityEventTools.AddPersistentListener(tryAgainBtn.onClick, sc.TryAgain);
            UnityEventTools.AddPersistentListener(menuBtn.onClick, sc.LoadMainMenu);

            SerializedObject so = new SerializedObject(goUI);
            so.FindProperty("sceneController").objectReferenceValue = sc;
            so.FindProperty("scoreText").objectReferenceValue = scoreTmp;
            so.FindProperty("tryAgainButton").objectReferenceValue = tryAgainBtn;
            so.FindProperty("mainMenuButton").objectReferenceValue = menuBtn;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/GameOverScreen.unity");
        }

        #endregion

        #region Build Settings Setup

        public static void SetupBuildSettings()
        {
            string[] scenePaths = new string[]
            {
                $"{SceneDir}/MainMenu.unity",
                $"{SceneDir}/StageSelection.unity",
                $"{SceneDir}/Stage1_Capsule.unity",
                $"{SceneDir}/Stage2_Humanoid.unity",
                $"{SceneDir}/Options.unity",
                $"{SceneDir}/Credits.unity",
                $"{SceneDir}/WinScreen.unity",
                $"{SceneDir}/GameOverScreen.unity"
            };

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }

            EditorBuildSettings.scenes = buildScenes;
            Debug.Log($"<color=#10B981>[SceneBuilder] Added {buildScenes.Length} scenes to EditorBuildSettings.</color>");
        }

        #endregion
    }
}
