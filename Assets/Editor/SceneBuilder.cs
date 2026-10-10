using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using StarterAssets;

namespace Chisanuphong
{
    public static class SceneBuilder
    {
        private const string SceneDir = "Assets/Scenes";
        private const string SpriteDir = "Assets/Sprites";
        private const string SODir = "Assets/ScriptableObjects/Items";
        private const string PrefabDir = "Assets/Prefabs/Items";
        private const string MaterialDir = "Assets/Materials/Items";

        private static UnityEngine.Sprite roundedBoxSprite;
        private static UnityEngine.Sprite circleSprite;

        [MenuItem("Tools/Build Stage 1 Only (Capsule & ProBuilder)")]
        public static void BuildStage1Only()
        {
            EnsureAssets();
            EnsureDirectories();
            EnsureItemDataAndPrefabs();

            BuildStage1Scene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#10B981><b>[SceneBuilder] Stage 1 (Capsule, Overlays & ProBuilder Level) built successfully!</b></color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(SceneDir)) Directory.CreateDirectory(SceneDir);
            if (!Directory.Exists(SpriteDir)) Directory.CreateDirectory(SpriteDir);
            if (!Directory.Exists(SODir)) Directory.CreateDirectory(SODir);
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);
            if (!Directory.Exists(MaterialDir)) Directory.CreateDirectory(MaterialDir);
        }

        private static void EnsureAssets()
        {
            string boxPath = $"{SpriteDir}/UIRoundedBox.png";
            string circlePath = $"{SpriteDir}/UICircle.png";

            if (File.Exists(boxPath))
            {
                roundedBoxSprite = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(boxPath);
            }

            if (File.Exists(circlePath))
            {
                circleSprite = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(circlePath);
            }
        }

        #region ScriptableObjects & Prefabs Setup (Q.4)

        public static void EnsureItemDataAndPrefabs()
        {
            EnsureDirectories();

            AudioClip starClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/Sounds/SFX/Collect star 1.wav");
            AudioClip debuffClip1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/Sounds/SFX/Debuff Downgrade 1.wav");
            AudioClip coinClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/Sounds/SFX/Coin Pickup 1.wav");
            AudioClip debuffClip2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/Sounds/SFX/Debuff Downgrade 2.wav");

            // 1. ItemData_TimePlus (+3s)
            CreateOrUpdateItem(
                "Item_TimePlus",
                "ItemData_TimePlus",
                ItemType.TimeBonus,
                0,
                3f,
                new Color(0f, 0.9f, 1f),
                starClip,
                PrimitiveType.Sphere,
                new Vector3(0.75f, 0.75f, 0.75f)
            );

            // 2. Item_TimeMinus (-5s)
            CreateOrUpdateItem(
                "Item_TimeMinus",
                "ItemData_TimeMinus",
                ItemType.TimePenalty,
                0,
                -5f,
                new Color(1f, 0.25f, 0.05f),
                debuffClip1,
                PrimitiveType.Cube,
                new Vector3(0.75f, 0.75f, 0.75f)
            );

            // 3. Item_ScorePlus (+500)
            CreateOrUpdateItem(
                "Item_ScorePlus",
                "ItemData_ScorePlus",
                ItemType.ScoreBonus,
                500,
                0f,
                new Color(1f, 0.85f, 0.1f),
                coinClip,
                PrimitiveType.Cylinder,
                new Vector3(0.75f, 0.2f, 0.75f)
            );

            // 4. Item_ScoreMinus (-500)
            CreateOrUpdateItem(
                "Item_ScoreMinus",
                "ItemData_ScoreMinus",
                ItemType.ScorePenalty,
                -500,
                0f,
                new Color(0.85f, 0.1f, 0.95f),
                debuffClip2,
                PrimitiveType.Cube,
                new Vector3(0.65f, 0.65f, 0.65f)
            );

            AssetDatabase.SaveAssets();
        }

        private static void CreateOrUpdateItem(string prefabName, string soName, ItemType type, int score, float time, Color color, AudioClip sfx, PrimitiveType shape, Vector3 scale)
        {
            string soPath = $"{SODir}/{soName}.asset";
            ItemData data = AssetDatabase.LoadAssetAtPath<ItemData>(soPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(data, soPath);
            }

            data.itemName = prefabName;
            data.itemType = type;
            data.scoreModifier = score;
            data.timeModifier = time;
            data.itemColor = color;
            data.sfxClip = sfx;
            EditorUtility.SetDirty(data);

            string matPath = $"{MaterialDir}/Mat_{prefabName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(urpShader != null ? urpShader : Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            mat.color = color;
            mat.SetColor("_BaseColor", color);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1.4f);
            EditorUtility.SetDirty(mat);

            string prefabPath = $"{PrefabDir}/{prefabName}.prefab";
            GameObject tempGo = GameObject.CreatePrimitive(shape);
            tempGo.name = prefabName;
            tempGo.transform.localScale = scale;

            MeshRenderer mr = tempGo.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;

            Collider col = tempGo.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            ItemPickup pickup = tempGo.AddComponent<ItemPickup>();
            SerializedObject so = new SerializedObject(pickup);
            so.FindProperty("itemData").objectReferenceValue = data;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(tempGo, prefabPath);
            GameObject.DestroyImmediate(tempGo);
        }

        #endregion

        #region Common Scene Factory Helpers

        private static (Canvas canvas, EventSystem es) SetupSceneBasics(string sceneName, Color cameraBg)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

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

        private static GameObject CreateFullscreenOverlay(Transform parent, string name, Color color)
        {
            GameObject bgGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
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
            if (roundedBoxSprite != null)
            {
                img.sprite = roundedBoxSprite;
                img.type = Image.Type.Sliced;
            }
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
            if (roundedBoxSprite != null)
            {
                img.sprite = roundedBoxSprite;
                img.type = Image.Type.Sliced;
            }
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
            rt.sizeDelta = new Vector2(340, 36);

            Slider slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = initialVal;

            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(sliderGo.transform, false);
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.25f);
            bgRt.anchorMax = new Vector2(1, 0.75f);
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgGo.GetComponent<Image>();
            if (roundedBoxSprite != null) { bgImg.sprite = roundedBoxSprite; bgImg.type = Image.Type.Sliced; }
            bgImg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.25f);
            faRt.anchorMax = new Vector2(1, 0.75f);
            faRt.sizeDelta = new Vector2(-20, 0);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.GetComponent<Image>();
            if (roundedBoxSprite != null) { fillImg.sprite = roundedBoxSprite; fillImg.type = Image.Type.Sliced; }
            fillImg.color = fillColor;

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            RectTransform haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = new Vector2(-20, 0);

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(28, 28);
            Image handleImg = handle.GetComponent<Image>();
            if (circleSprite != null) handleImg.sprite = circleSprite;
            handleImg.color = Color.white;

            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;

            valText = CreateTMPText(sliderGo.transform, "ValueText", $"{Mathf.RoundToInt(initialVal * 100)}%", 18, FontWeight.Bold, TextAlignmentOptions.Right, Color.white, new Vector2(60, 30));
            RectTransform vtRt = valText.GetComponent<RectTransform>();
            vtRt.anchorMin = new Vector2(1, 0.5f);
            vtRt.anchorMax = new Vector2(1, 0.5f);
            vtRt.anchoredPosition = new Vector2(50, 0);

            return slider;
        }

        #endregion

        #region Stage 1 Construction (Exclusive)

        public static void BuildStage1Scene()
        {
            var (canvas, es) = SetupSceneBasics("Stage1_Capsule", new Color(0.08f, 0.1f, 0.15f));

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.4f, 0.5f);

            Material trackMat = CreateTrackMaterial("Mat_ProBuilder_Track", new Color(0.18f, 0.22f, 0.3f));
            Material bridgeMat = CreateTrackMaterial("Mat_ProBuilder_Bridge", new Color(0.12f, 0.35f, 0.48f));
            Material highMat = CreateTrackMaterial("Mat_ProBuilder_High", new Color(0.25f, 0.18f, 0.35f));

            // 1. Level Design with ProBuilder (Q.3: Wide & Narrow paths elevated in the void)
            GameObject levelRoot = new GameObject("ProBuilder_Level");
            CreateProBuilderBlock(levelRoot.transform, "Plaza_Spawn_Wide", new Vector3(0, 0, 0), new Vector3(10, 1, 10), trackMat);
            CreateProBuilderBlock(levelRoot.transform, "Path_Highway1_Wide", new Vector3(0, 0, 12), new Vector3(6, 1, 14), trackMat);
            CreateProBuilderBlock(levelRoot.transform, "Path_NarrowBridge1", new Vector3(0, 0, 28), new Vector3(2.2f, 1, 18), bridgeMat);
            CreateProBuilderBlock(levelRoot.transform, "Plaza_Central_Wide", new Vector3(0, 0, 44), new Vector3(12, 1, 12), trackMat);

            // West Wing
            CreateProBuilderBlock(levelRoot.transform, "Path_WestNarrow", new Vector3(-14, 0, 44), new Vector3(16, 1, 2f), bridgeMat);
            CreateProBuilderBlock(levelRoot.transform, "Plaza_West_Wide", new Vector3(-26, 0, 44), new Vector3(8, 1, 8), trackMat);

            // East Wing
            CreateProBuilderBlock(levelRoot.transform, "Path_EastNarrow", new Vector3(14, 0, 44), new Vector3(16, 1, 2f), bridgeMat);
            CreateProBuilderBlock(levelRoot.transform, "Plaza_East_Wide", new Vector3(26, 0, 44), new Vector3(8, 1, 8), trackMat);

            // North Ascent
            CreateProBuilderBlock(levelRoot.transform, "Ramp_North", new Vector3(0, 1.5f, 54), new Vector3(3, 3, 8), bridgeMat);
            CreateProBuilderBlock(levelRoot.transform, "Plaza_HighPeak_Wide", new Vector3(0, 3, 65), new Vector3(10, 1, 14), highMat);
            CreateProBuilderBlock(levelRoot.transform, "Path_Catwalk_Narrow", new Vector3(0, 3, 78), new Vector3(1.6f, 1, 12), bridgeMat);
            CreateProBuilderBlock(levelRoot.transform, "Plaza_FinalSanctuary", new Vector3(0, 3, 88), new Vector3(6, 1, 6), highMat);

            // 2. Setup Capsule Player with Rigidbody & ThirdPersonController (Q.2)
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/NestedParentCapsule_Unpack.prefab");
            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerInstance.name = "PlayerRig";
            playerInstance.transform.position = new Vector3(0, 1.2f, 0);
            PrefabUtility.UnpackPrefabInstance(playerInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            Transform playerCapsule = playerInstance.transform.Find("PlayerCapsule");
            playerCapsule.tag = "Player";

            Rigidbody rb = playerCapsule.GetComponent<Rigidbody>();
            if (rb == null) rb = playerCapsule.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            ItemCollector collector = playerCapsule.GetComponent<ItemCollector>();
            if (collector == null) collector = playerCapsule.gameObject.AddComponent<ItemCollector>();

            AudioClip coinClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/Sounds/SFX/Coin Pickup 1.wav");
            AudioClip debuffClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/PracticalExamResources/Sounds/SFX/Debuff Downgrade 1.wav");
            SerializedObject collectorSo = new SerializedObject(collector);
            collectorSo.FindProperty("defaultBonusSfx").objectReferenceValue = coinClip;
            collectorSo.FindProperty("defaultPenaltySfx").objectReferenceValue = debuffClip;
            collectorSo.ApplyModifiedProperties();

            Transform followCam = playerInstance.transform.Find("PlayerFollowCamera");
            if (followCam != null)
            {
                var cinemachineCam = followCam.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (cinemachineCam != null)
                {
                    Transform cameraRoot = playerCapsule.Find("PlayerCameraRoot");
                    cinemachineCam.Follow = cameraRoot;
                }
            }

            // 3. Place Item Prefabs (Q.3, Q.4)
            GameObject itemsRoot = new GameObject("Items_And_Obstacles");
            PlaceAllStage1Items(itemsRoot.transform);

            // 4. Build UX/UI HUD Canvas (Top Left) & Action Panel (Q.5)
            GameObject hudCard = CreateCardPanel(canvas.transform, "HUD_Panel", new Vector2(320, 140), new Color(0.1f, 0.14f, 0.22f, 0.92f));
            RectTransform hudRt = hudCard.GetComponent<RectTransform>();
            hudRt.anchorMin = new Vector2(0, 1);
            hudRt.anchorMax = new Vector2(0, 1);
            hudRt.pivot = new Vector2(0, 1);
            hudRt.anchoredPosition = new Vector2(30, -30);

            VerticalLayoutGroup hudVlg = hudCard.AddComponent<VerticalLayoutGroup>();
            hudVlg.padding = new RectOffset(20, 20, 16, 16);
            hudVlg.spacing = 6;
            hudVlg.childControlWidth = true;
            hudVlg.childControlHeight = false;

            var titleTmp = CreateTMPText(hudCard.transform, "Title", "STAGE 1 : CAPSULE", 15, FontWeight.Bold, TextAlignmentOptions.Left, new Color(0.22f, 0.74f, 0.98f), new Vector2(280, 22));
            var timerTmp = CreateTMPText(hudCard.transform, "TimerText", "Time : 30s", 22, FontWeight.Bold, TextAlignmentOptions.Left, new Color(1f, 0.84f, 0f), new Vector2(280, 28));
            var scoreTmp = CreateTMPText(hudCard.transform, "ScoreText", "Score : 00000 / 10000", 20, FontWeight.Bold, TextAlignmentOptions.Left, Color.white, new Vector2(280, 26));

            // Right Action Panel
            GameObject actionCard = CreateCardPanel(canvas.transform, "ActionPanel", new Vector2(200, 240), new Color(0.1f, 0.14f, 0.22f, 0.9f));
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

            Color btnOpt = new Color(0.2f, 0.35f, 0.55f);
            Color btnOptHover = new Color(0.3f, 0.45f, 0.7f);
            Color btnWin = new Color(0.08f, 0.6f, 0.35f);
            Color btnWinHover = new Color(0.12f, 0.75f, 0.45f);
            Color btnLose = new Color(0.85f, 0.25f, 0.25f);
            Color btnLoseHover = new Color(0.95f, 0.35f, 0.35f);
            Color btnBack = new Color(0.25f, 0.3f, 0.4f);
            Color btnBackHover = new Color(0.35f, 0.42f, 0.55f);

            Button optHUDButton = CreateStyledButton(actionCard.transform, "OptionsButton", "Options", new Vector2(170, 42), btnOpt, btnOptHover, Color.white);
            Button winBtn = CreateStyledButton(actionCard.transform, "WinButton", "Win (Test)", new Vector2(170, 42), btnWin, btnWinHover, Color.white);
            Button loseBtn = CreateStyledButton(actionCard.transform, "GameOverButton", "Game Over (Test)", new Vector2(170, 42), btnLose, btnLoseHover, Color.white);
            Button backBtn = CreateStyledButton(actionCard.transform, "BackButton", "Back", new Vector2(170, 42), btnBack, btnBackHover, Color.white);

            // --- BUILD IN-SCENE MODAL OVERLAYS (Win, Game Over, Options) ---
            
            // 5. Win Overlay Panel
            GameObject winOverlay = CreateFullscreenOverlay(canvas.transform, "Win_Overlay", new Color(0.02f, 0.1f, 0.06f, 0.9f));
            GameObject winCard = CreateCardPanel(winOverlay.transform, "WinCard", new Vector2(620, 540), new Color(0.08f, 0.32f, 0.2f, 0.95f));
            VerticalLayoutGroup winVlg = winCard.AddComponent<VerticalLayoutGroup>();
            winVlg.padding = new RectOffset(35, 35, 40, 40);
            winVlg.spacing = 16;
            winVlg.childAlignment = TextAnchor.UpperCenter;
            winVlg.childControlWidth = true;
            winVlg.childControlHeight = false;

            CreateTMPText(winCard.transform, "Title", "VICTORY!", 38, FontWeight.Bold, TextAlignmentOptions.Center, new Color(0.3f, 0.95f, 0.5f), new Vector2(550, 48));
            CreateTMPText(winCard.transform, "Subtitle", "STAGE 1 COMPLETED SUCCESSFULLY", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.7f, 0.95f, 0.8f), new Vector2(550, 24));

            GameObject winScoreBox = CreateCardPanel(winCard.transform, "ScoreBox", new Vector2(550, 75), new Color(0.05f, 0.22f, 0.14f, 0.85f));
            VerticalLayoutGroup wsbVlg = winScoreBox.AddComponent<VerticalLayoutGroup>();
            wsbVlg.padding = new RectOffset(10, 10, 10, 10);
            wsbVlg.childAlignment = TextAnchor.MiddleCenter;
            wsbVlg.childControlWidth = true; wsbVlg.childControlHeight = false;
            var winScoreTmp = CreateTMPText(winScoreBox.transform, "ScoreDisplay", "Score : 00000", 28, FontWeight.Bold, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.3f), new Vector2(500, 36));

            Button winTryAgainBtn = CreateStyledButton(winCard.transform, "WinTryAgainBtn", "TRY AGAIN", new Vector2(550, 54), new Color(0.12f, 0.55f, 0.32f), new Color(0.18f, 0.72f, 0.42f), Color.white);
            Button winStageSelectBtn = CreateStyledButton(winCard.transform, "WinStageSelectBtn", "STAGE SELECTION", new Vector2(550, 54), new Color(0.15f, 0.45f, 0.88f), new Color(0.25f, 0.58f, 1f), Color.white);
            Button winMainMenuBtn = CreateStyledButton(winCard.transform, "WinMainMenuBtn", "MAIN MENU", new Vector2(550, 54), new Color(0.2f, 0.26f, 0.36f), new Color(0.3f, 0.38f, 0.5f), Color.white);
            winOverlay.SetActive(false);

            // 6. Game Over Overlay Panel
            GameObject goOverlay = CreateFullscreenOverlay(canvas.transform, "GameOver_Overlay", new Color(0.12f, 0.02f, 0.02f, 0.9f));
            GameObject goCard = CreateCardPanel(goOverlay.transform, "GameOverCard", new Vector2(620, 540), new Color(0.35f, 0.1f, 0.1f, 0.95f));
            VerticalLayoutGroup goVlg = goCard.AddComponent<VerticalLayoutGroup>();
            goVlg.padding = new RectOffset(35, 35, 40, 40);
            goVlg.spacing = 16;
            goVlg.childAlignment = TextAnchor.UpperCenter;
            goVlg.childControlWidth = true;
            goVlg.childControlHeight = false;

            CreateTMPText(goCard.transform, "Title", "GAME OVER", 38, FontWeight.Bold, TextAlignmentOptions.Center, new Color(1f, 0.35f, 0.35f), new Vector2(550, 48));
            CreateTMPText(goCard.transform, "Subtitle", "Time Expired / Fell Off Platform", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.95f, 0.75f, 0.75f), new Vector2(550, 24));

            GameObject goScoreBox = CreateCardPanel(goCard.transform, "ScoreBox", new Vector2(550, 75), new Color(0.24f, 0.06f, 0.06f, 0.85f));
            VerticalLayoutGroup gsbVlg = goScoreBox.AddComponent<VerticalLayoutGroup>();
            gsbVlg.padding = new RectOffset(10, 10, 10, 10);
            gsbVlg.childAlignment = TextAnchor.MiddleCenter;
            gsbVlg.childControlWidth = true; gsbVlg.childControlHeight = false;
            var goScoreTmp = CreateTMPText(goScoreBox.transform, "ScoreDisplay", "Score : 00000", 28, FontWeight.Bold, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.85f), new Vector2(500, 36));

            Button goTryAgainBtn = CreateStyledButton(goCard.transform, "GameOverTryAgainBtn", "TRY AGAIN", new Vector2(550, 54), new Color(0.75f, 0.18f, 0.18f), new Color(0.9f, 0.28f, 0.28f), Color.white);
            Button goStageSelectBtn = CreateStyledButton(goCard.transform, "GameOverStageSelectBtn", "STAGE SELECTION", new Vector2(550, 54), new Color(0.15f, 0.45f, 0.88f), new Color(0.25f, 0.58f, 1f), Color.white);
            Button goMainMenuBtn = CreateStyledButton(goCard.transform, "GameOverMainMenuBtn", "MAIN MENU", new Vector2(550, 54), new Color(0.25f, 0.15f, 0.15f), new Color(0.35f, 0.22f, 0.22f), Color.white);
            goOverlay.SetActive(false);

            // 7. Options Overlay Panel
            GameObject optOverlay = CreateFullscreenOverlay(canvas.transform, "Options_Overlay", new Color(0.05f, 0.08f, 0.12f, 0.92f));
            GameObject optCard = CreateCardPanel(optOverlay.transform, "OptionsCard", new Vector2(650, 540), new Color(0.12f, 0.16f, 0.24f, 0.95f));
            VerticalLayoutGroup optVlg = optCard.AddComponent<VerticalLayoutGroup>();
            optVlg.padding = new RectOffset(40, 40, 40, 40);
            optVlg.spacing = 20;
            optVlg.childAlignment = TextAnchor.UpperCenter;
            optVlg.childControlWidth = true;
            optVlg.childControlHeight = false;

            CreateTMPText(optCard.transform, "Title", "OPTIONS", 32, FontWeight.Bold, TextAlignmentOptions.Center, new Color(0.22f, 0.74f, 0.98f), new Vector2(570, 42));
            CreateTMPText(optCard.transform, "Subtitle", "Audio & Sound Effects Settings", 16, FontWeight.Regular, TextAlignmentOptions.Center, new Color(0.6f, 0.7f, 0.85f), new Vector2(570, 24));

            // BGM Row
            GameObject bgmRow = new GameObject("BGMRow", typeof(RectTransform));
            bgmRow.transform.SetParent(optCard.transform, false);
            bgmRow.GetComponent<RectTransform>().sizeDelta = new Vector2(570, 60);
            CreateTMPText(bgmRow.transform, "BGMLabel", "BGM Volume", 18, FontWeight.SemiBold, TextAlignmentOptions.Left, Color.white, new Vector2(150, 36));
            Slider bgmSlider = CreateStyledSlider(bgmRow.transform, "BGMSlider", 0.75f, new Color(0.22f, 0.74f, 0.98f), out TextMeshProUGUI bgmValText);
            bgmSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, 0);

            // SFX Row
            GameObject sfxRow = new GameObject("SFXRow", typeof(RectTransform));
            sfxRow.transform.SetParent(optCard.transform, false);
            sfxRow.GetComponent<RectTransform>().sizeDelta = new Vector2(570, 60);
            CreateTMPText(sfxRow.transform, "SFXLabel", "SFX Volume", 18, FontWeight.SemiBold, TextAlignmentOptions.Left, Color.white, new Vector2(150, 36));
            Slider sfxSlider = CreateStyledSlider(sfxRow.transform, "SFXSlider", 0.75f, new Color(0.22f, 0.74f, 0.98f), out TextMeshProUGUI sfxValText);
            sfxSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, 0);

            Button optResumeBtn = CreateStyledButton(optCard.transform, "OptionsResumeBtn", "RESUME GAME", new Vector2(570, 54), new Color(0.15f, 0.45f, 0.88f), new Color(0.25f, 0.58f, 1f), Color.white);
            Button optMainMenuBtn = CreateStyledButton(optCard.transform, "OptionsMainMenuBtn", "MAIN MENU", new Vector2(570, 54), new Color(0.25f, 0.3f, 0.4f), new Color(0.35f, 0.42f, 0.55f), Color.white);
            optOverlay.SetActive(false);

            // 8. Hook Everything into StageUI
            GameObject uiMgr = new GameObject("StageUI");
            SceneController sc = uiMgr.AddComponent<SceneController>();
            StageUI sUI = uiMgr.AddComponent<StageUI>();

            SerializedObject sUiSo = new SerializedObject(sUI);
            sUiSo.FindProperty("sceneController").objectReferenceValue = sc;
            sUiSo.FindProperty("stageTitleText").objectReferenceValue = titleTmp;
            sUiSo.FindProperty("timerText").objectReferenceValue = timerTmp;
            sUiSo.FindProperty("scoreText").objectReferenceValue = scoreTmp;

            sUiSo.FindProperty("optionsHUDButton").objectReferenceValue = optHUDButton;
            sUiSo.FindProperty("winDebugButton").objectReferenceValue = winBtn;
            sUiSo.FindProperty("gameOverDebugButton").objectReferenceValue = loseBtn;
            sUiSo.FindProperty("backHUDButton").objectReferenceValue = backBtn;

            sUiSo.FindProperty("stageName").stringValue = "Stage 1 - Capsule Player Minigame";
            sUiSo.FindProperty("playerTransform").objectReferenceValue = playerCapsule;
            sUiSo.FindProperty("fallThresholdY").floatValue = -6f;
            sUiSo.FindProperty("targetWinningScore").intValue = 10000;
            sUiSo.FindProperty("runCountdown").boolValue = true;

            sUiSo.FindProperty("winPanel").objectReferenceValue = winOverlay;
            sUiSo.FindProperty("gameOverPanel").objectReferenceValue = goOverlay;
            sUiSo.FindProperty("optionsPanel").objectReferenceValue = optOverlay;

            sUiSo.FindProperty("winScoreText").objectReferenceValue = winScoreTmp;
            sUiSo.FindProperty("winTryAgainButton").objectReferenceValue = winTryAgainBtn;
            sUiSo.FindProperty("winStageSelectButton").objectReferenceValue = winStageSelectBtn;
            sUiSo.FindProperty("winMainMenuButton").objectReferenceValue = winMainMenuBtn;

            sUiSo.FindProperty("gameOverScoreText").objectReferenceValue = goScoreTmp;
            sUiSo.FindProperty("gameOverTryAgainButton").objectReferenceValue = goTryAgainBtn;
            sUiSo.FindProperty("gameOverStageSelectButton").objectReferenceValue = goStageSelectBtn;
            sUiSo.FindProperty("gameOverMainMenuButton").objectReferenceValue = goMainMenuBtn;

            sUiSo.FindProperty("bgmSlider").objectReferenceValue = bgmSlider;
            sUiSo.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            sUiSo.FindProperty("bgmValueText").objectReferenceValue = bgmValText;
            sUiSo.FindProperty("sfxValueText").objectReferenceValue = sfxValText;
            sUiSo.FindProperty("optionsResumeButton").objectReferenceValue = optResumeBtn;
            sUiSo.FindProperty("optionsMainMenuButton").objectReferenceValue = optMainMenuBtn;

            sUiSo.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), $"{SceneDir}/Stage1_Capsule.unity");
        }

        private static Material CreateTrackMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            if (!Directory.Exists("Assets/Materials")) Directory.CreateDirectory("Assets/Materials");
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(s != null ? s : Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject CreateProBuilderBlock(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var pbMesh = ShapeFactory.Instantiate(new Cube());
            pbMesh.name = name;
            pbMesh.transform.SetParent(parent, false);
            pbMesh.transform.position = pos;
            pbMesh.transform.localScale = size;

            var mr = pbMesh.GetComponent<MeshRenderer>();
            if (mr != null && mat != null) mr.sharedMaterial = mat;

            var mc = pbMesh.gameObject.AddComponent<MeshCollider>();
            var mf = pbMesh.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) mc.sharedMesh = mf.sharedMesh;

            return pbMesh.gameObject;
        }

        private static void PlaceAllStage1Items(Transform parent)
        {
            GameObject pScorePlus = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Item_ScorePlus.prefab");
            GameObject pScoreMinus = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Item_ScoreMinus.prefab");
            GameObject pTimePlus = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Item_TimePlus.prefab");
            GameObject pTimeMinus = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Item_TimeMinus.prefab");

            // 1. Plaza Spawn
            SpawnItem(pScorePlus, parent, new Vector3(-2, 1.2f, -2));
            SpawnItem(pScorePlus, parent, new Vector3(2, 1.2f, -2));
            SpawnItem(pScorePlus, parent, new Vector3(-2, 1.2f, 2));
            SpawnItem(pScorePlus, parent, new Vector3(2, 1.2f, 2));

            // 2. Highway 1
            SpawnItem(pScorePlus, parent, new Vector3(-1.5f, 1.2f, 8));
            SpawnItem(pScorePlus, parent, new Vector3(1.5f, 1.2f, 8));
            SpawnItem(pScorePlus, parent, new Vector3(0, 1.2f, 12));
            SpawnItem(pTimePlus, parent, new Vector3(0, 1.2f, 16));

            // 3. Narrow Bridge 1
            SpawnItem(pScorePlus, parent, new Vector3(0, 1.2f, 22));
            SpawnItem(pScoreMinus, parent, new Vector3(0.5f, 1.2f, 25));
            SpawnItem(pScorePlus, parent, new Vector3(0, 1.2f, 28));
            SpawnItem(pTimeMinus, parent, new Vector3(-0.4f, 1.2f, 31));
            SpawnItem(pScorePlus, parent, new Vector3(0, 1.2f, 34));

            // 4. Central Plaza
            SpawnItem(pScorePlus, parent, new Vector3(-3, 1.2f, 42));
            SpawnItem(pScorePlus, parent, new Vector3(3, 1.2f, 42));
            SpawnItem(pScorePlus, parent, new Vector3(0, 1.2f, 44));
            SpawnItem(pScoreMinus, parent, new Vector3(0, 1.2f, 46));
            SpawnItem(pScorePlus, parent, new Vector3(-3, 1.2f, 46));
            SpawnItem(pScorePlus, parent, new Vector3(3, 1.2f, 46));

            // 5. West Wing
            SpawnItem(pScorePlus, parent, new Vector3(-9, 1.2f, 44));
            SpawnItem(pScoreMinus, parent, new Vector3(-13, 1.2f, 44));
            SpawnItem(pScorePlus, parent, new Vector3(-17, 1.2f, 44));
            SpawnItem(pScorePlus, parent, new Vector3(-24, 1.2f, 42));
            SpawnItem(pScorePlus, parent, new Vector3(-28, 1.2f, 42));
            SpawnItem(pScorePlus, parent, new Vector3(-24, 1.2f, 46));
            SpawnItem(pScorePlus, parent, new Vector3(-28, 1.2f, 46));
            SpawnItem(pTimePlus, parent, new Vector3(-26, 1.2f, 44));
            SpawnItem(pTimeMinus, parent, new Vector3(-26, 1.2f, 46.5f));

            // 6. East Wing
            SpawnItem(pScorePlus, parent, new Vector3(9, 1.2f, 44));
            SpawnItem(pTimeMinus, parent, new Vector3(13, 1.2f, 44));
            SpawnItem(pScorePlus, parent, new Vector3(17, 1.2f, 44));
            SpawnItem(pScorePlus, parent, new Vector3(24, 1.2f, 42));
            SpawnItem(pScorePlus, parent, new Vector3(28, 1.2f, 42));
            SpawnItem(pScorePlus, parent, new Vector3(24, 1.2f, 46));
            SpawnItem(pScorePlus, parent, new Vector3(28, 1.2f, 46));
            SpawnItem(pTimePlus, parent, new Vector3(26, 1.2f, 44));

            // 7. North Ramp & Upper Peak
            SpawnItem(pTimePlus, parent, new Vector3(0, 2.2f, 54));
            SpawnItem(pScorePlus, parent, new Vector3(-2.5f, 4.2f, 62));
            SpawnItem(pScorePlus, parent, new Vector3(2.5f, 4.2f, 62));
            SpawnItem(pScorePlus, parent, new Vector3(0, 4.2f, 65));
            SpawnItem(pTimePlus, parent, new Vector3(-2.5f, 4.2f, 68));
            SpawnItem(pTimePlus, parent, new Vector3(2.5f, 4.2f, 68));
            SpawnItem(pScoreMinus, parent, new Vector3(0, 4.2f, 68));

            // 8. Catwalk & Final Sanctuary
            SpawnItem(pScorePlus, parent, new Vector3(0, 4.2f, 74));
            SpawnItem(pTimeMinus, parent, new Vector3(0.2f, 4.2f, 77));
            SpawnItem(pScorePlus, parent, new Vector3(0, 4.2f, 80));
            SpawnItem(pScorePlus, parent, new Vector3(-1.5f, 4.2f, 87));
            SpawnItem(pScorePlus, parent, new Vector3(1.5f, 4.2f, 87));
            SpawnItem(pScorePlus, parent, new Vector3(-1.5f, 4.2f, 89));
            SpawnItem(pScorePlus, parent, new Vector3(1.5f, 4.2f, 89));
            SpawnItem(pScorePlus, parent, new Vector3(0, 4.2f, 88));
        }

        private static void SpawnItem(GameObject prefab, Transform parent, Vector3 pos)
        {
            if (prefab == null) return;
            GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            item.transform.position = pos;
        }

        #endregion
    }
}
