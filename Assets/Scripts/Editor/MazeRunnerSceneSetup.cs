using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using MazeRunner.Core;
using MazeRunner.Player;
using MazeRunner.Items;
using MazeRunner.Traps;
using MazeRunner.Maze;
using MazeRunner.UI;

/// <summary>
/// One-click test scene setup for Maze Runner.
/// Menu: Maze Runner > Setup Test Scene
/// Visual style: colorful toy/cartoon robot astronauts in a playful maze.
/// </summary>
public class MazeRunnerSceneSetup : EditorWindow
{
    // Concept art color palette
    static readonly Color PlayerOrange = new Color(1f, 0.55f, 0.1f);
    static readonly Color PlayerCyan = new Color(0.2f, 0.85f, 0.85f);
    static readonly Color PlayerPurple = new Color(0.65f, 0.25f, 0.85f);
    static readonly Color PlayerGreen = new Color(0.45f, 0.9f, 0.15f);

    static readonly Color WallBlue = new Color(0.3f, 0.55f, 0.9f);
    static readonly Color WallYellow = new Color(1f, 0.82f, 0.2f);
    static readonly Color WallRed = new Color(0.9f, 0.25f, 0.2f);
    static readonly Color WallOrange = new Color(1f, 0.6f, 0.15f);

    static readonly Color FloorBeige = new Color(0.92f, 0.85f, 0.72f);
    static readonly Color ItemPurpleGlow = new Color(0.75f, 0.3f, 1f);
    static readonly Color ItemCyan = new Color(0.2f, 0.9f, 0.85f);
    static readonly Color DeliveryYellow = new Color(1f, 0.9f, 0.4f);
    static readonly Color DeliveryBlue = new Color(0.35f, 0.55f, 0.85f);
    static readonly Color ScreenGreen = new Color(0.3f, 1f, 0.4f);

    [MenuItem("Maze Runner/Setup Test Scene (Full) %#t")]
    public static void SetupFullTestScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        SetupTagsAndLayers();
        CreateLighting();
        CreateGround();
        GameObject player = CreatePlayer();
        CreateMazeWalls();
        CreateItems();
        CreateTraps();
        CreateDeliveryMachine();
        CreateGatherZone();
        CreateGameManagers();

        Selection.activeGameObject = player;
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/TestScene.unity");

        Debug.Log("=== Maze Runner Test Scene Created! Press Play to test. ===\n" +
                  "Controls: WASD = Move, Mouse = Look, Space = Jump, E = Interact/Drop");
    }

    [MenuItem("Maze Runner/Setup Mini Maze (Procedural) %#m")]
    public static void SetupProceduralMaze()
    {
        SetupTagsAndLayers();
        CreateLighting();

        GameObject mazeObj = new GameObject("MazeSystem");
        var builder = mazeObj.AddComponent<MazeBuilder>();

        var so = new SerializedObject(builder);
        so.FindProperty("cellSize").floatValue = 4f;
        so.FindProperty("mazeWidth").intValue = 8;
        so.FindProperty("mazeHeight").intValue = 8;
        so.FindProperty("seed").intValue = 42;
        so.FindProperty("loopChance").floatValue = 0.15f;
        so.ApplyModifiedProperties();

        GameObject player = CreatePlayer();
        CreateGameManagers();
        mazeObj.AddComponent<AutoStartMaze>();

        Selection.activeGameObject = player;
        Debug.Log("=== Procedural Maze Scene Created! Press Play to generate maze. ===");
    }

    // ==================== MAIN MENU SCENE ====================

    [MenuItem("Maze Runner/Setup Main Menu Scene")]
    public static void SetupMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Camera ---
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.75f, 0.95f); // Soft sky blue
        cam.fieldOfView = 50f;
        camObj.transform.position = new Vector3(0f, 3f, -10f);
        camObj.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        camObj.AddComponent<AudioListener>();

        // --- Lighting ---
        GameObject sunObj = new GameObject("Directional Light");
        var sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.96f, 0.88f);
        sun.intensity = 1.5f;
        sunObj.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        sun.shadows = LightShadows.Soft;
        RenderSettings.ambientIntensity = 1.2f;

        GameObject fillObj = new GameObject("Fill Light");
        var fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.color = new Color(0.7f, 0.85f, 1f);
        fill.intensity = 0.5f;
        fillObj.transform.rotation = Quaternion.Euler(30f, 150f, 0f);
        fill.shadows = LightShadows.None;

        // --- Ground ---
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(40f, 1f, 30f);
        ground.isStatic = true;
        ground.GetComponent<Renderer>().material = CreateMat(FloorBeige, 0.3f);

        // --- Background walls (colorful toy walls behind the menu) ---
        GameObject bgParent = new GameObject("Background Decorations");

        CreateColorWall(bgParent.transform, new Vector3(-6f, 2f, 5f), new Vector3(2f, 4f, 0.8f), "Wall Blue", WallBlue);
        CreateColorWall(bgParent.transform, new Vector3(-3f, 1.5f, 6f), new Vector3(1.5f, 3f, 0.8f), "Wall Yellow", WallYellow);
        CreateColorWall(bgParent.transform, new Vector3(0f, 2.5f, 7f), new Vector3(2.5f, 5f, 0.8f), "Wall Red", WallRed);
        CreateColorWall(bgParent.transform, new Vector3(4f, 1.8f, 5.5f), new Vector3(1.8f, 3.6f, 0.8f), "Wall Orange", WallOrange);
        CreateColorWall(bgParent.transform, new Vector3(7f, 2.2f, 6.5f), new Vector3(2f, 4.4f, 0.8f), "Wall Blue 2", WallBlue);

        // Wall caps (LEGO studs)
        CreateWallCap(bgParent.transform, new Vector3(-6f, 4.1f, 5f), WallBlue);
        CreateWallCap(bgParent.transform, new Vector3(0f, 5.1f, 7f), WallRed);
        CreateWallCap(bgParent.transform, new Vector3(7f, 4.5f, 6.5f), WallBlue);

        // --- Floating decorations (items that bob and rotate) ---
        GameObject floatParent = new GameObject("Floating Decorations");

        CreateFloatingDecor(floatParent.transform, new Vector3(-5f, 4f, 3f), ItemPurpleGlow, PrimitiveType.Cube, 0.6f);
        CreateFloatingDecor(floatParent.transform, new Vector3(5.5f, 5f, 4f), ItemCyan, PrimitiveType.Sphere, 0.5f);
        CreateFloatingDecor(floatParent.transform, new Vector3(-2f, 5.5f, 5f), new Color(1f, 0.75f, 0.15f), PrimitiveType.Cube, 0.45f);
        CreateFloatingDecor(floatParent.transform, new Vector3(3f, 3.5f, 2f), ItemPurpleGlow, PrimitiveType.Sphere, 0.4f);
        CreateFloatingDecor(floatParent.transform, new Vector3(8f, 4.5f, 5f), WallYellow, PrimitiveType.Cube, 0.5f);
        CreateFloatingDecor(floatParent.transform, new Vector3(-8f, 3.5f, 4f), PlayerGreen, PrimitiveType.Sphere, 0.35f);

        // --- Mini robot character on the ground (static decoration) ---
        CreateMenuRobot(bgParent.transform, new Vector3(-4f, 0f, 2f), PlayerOrange, 0.7f);
        CreateMenuRobot(bgParent.transform, new Vector3(5f, 0f, 3f), PlayerCyan, 0.7f);
        CreateMenuRobot(bgParent.transform, new Vector3(1f, 0f, 4f), PlayerPurple, 0.6f);

        // --- AudioManager ---
        GameObject amObj = new GameObject("AudioManager");
        amObj.AddComponent<AudioManager>();
        var audioSource = amObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        var soAudio = new SerializedObject(amObj.GetComponent<AudioManager>());
        soAudio.FindProperty("musicSource").objectReferenceValue = audioSource;
        soAudio.ApplyModifiedProperties();

        // --- Canvas + UI ---
        CreateMainMenuCanvas();

        // --- Save scene ---
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenuScene.unity");

        // --- Register in Build Settings ---
        RegisterBuildScenes();

        Debug.Log("=== Main Menu Scene Created! ===\n" +
                  "Assets/Scenes/MainMenuScene.unity saved.\n" +
                  "Build Settings updated: MainMenuScene=0, TestScene=1.");
    }

    static void CreateFloatingDecor(Transform parent, Vector3 position, Color color, PrimitiveType shape, float size)
    {
        GameObject obj = GameObject.CreatePrimitive(shape);
        obj.name = "Floating Decor";
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * size;
        // Slight random rotation for variety
        obj.transform.rotation = Quaternion.Euler(15f, position.x * 30f, 10f);
        Object.DestroyImmediate(obj.GetComponent<Collider>());

        var mat = CreateMat(color, 0.85f);
        mat.SetColor("_EmissionColor", color * 1.2f);
        mat.EnableKeyword("_EMISSION");
        obj.GetComponent<Renderer>().material = mat;

        // Glow light
        GameObject lightObj = new GameObject("Glow");
        lightObj.transform.SetParent(obj.transform);
        lightObj.transform.localPosition = Vector3.zero;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 1.5f;
        light.range = 3f;

        obj.AddComponent<MainMenuBackground>();
    }

    static void CreateMenuRobot(Transform parent, Vector3 position, Color bodyColor, float scale)
    {
        GameObject robot = new GameObject("Menu Robot");
        robot.transform.SetParent(parent);
        robot.transform.position = position;
        robot.transform.localScale = Vector3.one * scale;

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(robot.transform);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        body.transform.localScale = Vector3.one;
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().material = CreateMat(bodyColor, 0.5f);

        // Helmet
        GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        helmet.name = "Helmet";
        helmet.transform.SetParent(body.transform);
        helmet.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        helmet.transform.localScale = Vector3.one * 0.7f;
        Object.DestroyImmediate(helmet.GetComponent<Collider>());
        helmet.GetComponent<Renderer>().material = CreateMat(bodyColor * 0.85f, 0.6f);

        // Visor
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "Visor";
        visor.transform.SetParent(helmet.transform);
        visor.transform.localPosition = new Vector3(0f, 0f, 0.4f);
        visor.transform.localScale = new Vector3(0.65f, 0.45f, 0.15f);
        Object.DestroyImmediate(visor.GetComponent<Collider>());
        visor.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.1f, 0.15f), 0.9f);

        // Eyes
        CreateLEDDot(visor.transform, new Vector3(-0.25f, 0.15f, 0.5f), ScreenGreen);
        CreateLEDDot(visor.transform, new Vector3(0.25f, 0.15f, 0.5f), ScreenGreen);
    }

    static void CreateMainMenuCanvas()
    {
        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // --- Main Menu Panel ---
        GameObject menuPanel = CreatePanel(canvasObj.transform, "MenuPanel");

        // Title
        GameObject titleObj = CreateUIText(menuPanel.transform, "TitleText", "MAZE RUNNER",
            48, new Color(1f, 0.82f, 0.2f), new Vector2(0f, 200f), new Vector2(600f, 80f));
        var titleText = titleObj.GetComponent<Text>();
        titleText.fontStyle = FontStyle.Bold;

        // Subtitle
        CreateUIText(menuPanel.transform, "SubtitleText", "Grab. Run. Score!",
            22, Color.white, new Vector2(0f, 150f), new Vector2(400f, 40f));

        // Buttons
        float buttonY = 50f;
        float buttonSpacing = 65f;

        GameObject singleBtn = CreateMenuButton(menuPanel.transform, "SinglePlayerButton", "Single Player",
            new Vector2(0f, buttonY), true);

        buttonY -= buttonSpacing;
        GameObject multiBtn = CreateMenuButton(menuPanel.transform, "MultiplayerButton", "Multiplayer  (Coming Soon)",
            new Vector2(0f, buttonY), false);

        buttonY -= buttonSpacing;
        GameObject settingsBtn = CreateMenuButton(menuPanel.transform, "SettingsButton", "Settings",
            new Vector2(0f, buttonY), true);

        buttonY -= buttonSpacing;
        GameObject quitBtn = CreateMenuButton(menuPanel.transform, "QuitButton", "Quit",
            new Vector2(0f, buttonY), true);

        // --- Settings Panel (hidden by default) ---
        GameObject settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel");
        var settingsPanelImg = settingsPanel.GetComponent<Image>();
        settingsPanelImg.color = new Color(0f, 0f, 0f, 0.85f);

        // Settings title
        CreateUIText(settingsPanel.transform, "SettingsTitle", "Settings",
            36, new Color(1f, 0.82f, 0.2f), new Vector2(0f, 120f), new Vector2(400f, 60f));

        // Music Volume label
        CreateUIText(settingsPanel.transform, "MusicLabel", "Music Volume",
            22, Color.white, new Vector2(0f, 50f), new Vector2(300f, 40f));

        // Music Volume slider
        GameObject sliderObj = CreateSlider(settingsPanel.transform, "MusicSlider",
            new Vector2(0f, 0f), new Vector2(300f, 30f));
        var slider = sliderObj.GetComponent<Slider>();

        // Close settings button
        GameObject closeBtn = CreateMenuButton(settingsPanel.transform, "CloseSettingsButton", "Back",
            new Vector2(0f, -80f), true);

        settingsPanel.SetActive(false);

        // --- MainMenuUI component ---
        var menuUI = canvasObj.AddComponent<MainMenuUI>();
        var soUI = new SerializedObject(menuUI);
        soUI.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        soUI.FindProperty("musicVolumeSlider").objectReferenceValue = slider;
        soUI.FindProperty("gameSceneName").stringValue = "TestScene";
        soUI.ApplyModifiedProperties();

        // --- Wire up button events ---
        WireButton(singleBtn, menuUI, "OnSinglePlayer");
        WireButton(multiBtn, menuUI, "OnMultiplayer");
        WireButton(settingsBtn, menuUI, "OnSettings");
        WireButton(quitBtn, menuUI, "OnQuit");
        WireButton(closeBtn, menuUI, "OnCloseSettings");
    }

    static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.0f); // Transparent
        return panel;
    }

    static GameObject CreateUIText(Transform parent, string name, string text,
        int fontSize, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        var txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        return obj;
    }

    static GameObject CreateMenuButton(Transform parent, string name, string label,
        Vector2 anchoredPos, bool interactable)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(320f, 50f);

        var img = btnObj.AddComponent<Image>();
        img.color = interactable ? new Color(0.3f, 0.55f, 0.9f) : new Color(0.4f, 0.4f, 0.4f);

        var btn = btnObj.AddComponent<Button>();
        btn.interactable = interactable;
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.normalColor = interactable ? new Color(0.3f, 0.55f, 0.9f) : new Color(0.4f, 0.4f, 0.4f);
        colors.highlightedColor = interactable ? new Color(0.4f, 0.65f, 1f) : new Color(0.4f, 0.4f, 0.4f);
        colors.pressedColor = interactable ? new Color(0.2f, 0.4f, 0.7f) : new Color(0.4f, 0.4f, 0.4f);
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f);
        btn.colors = colors;

        // Button label
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.color = interactable ? Color.white : new Color(0.65f, 0.65f, 0.65f);
        txt.alignment = TextAnchor.MiddleCenter;

        return btnObj;
    }

    static GameObject CreateSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        var rect = sliderObj.AddComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        var bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = background.AddComponent<Image>();
        bgImg.color = new Color(0.25f, 0.25f, 0.3f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5f, 5f);
        fillAreaRect.offsetMax = new Vector2(-5f, -5f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.55f, 0.9f);

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        var handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 0f);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        var slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;

        return sliderObj;
    }

    static void WireButton(GameObject buttonObj, MonoBehaviour target, string methodName)
    {
        var btn = buttonObj.GetComponent<Button>();
        if (btn == null) return;
        var action = new UnityEngine.Events.UnityAction(
            () => target.SendMessage(methodName));
        // Use persistent call via SerializedObject for editor wiring
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.InsertArrayElementAtIndex(calls.arraySize);
        var call = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = methodName;
        call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // Void
        call.FindPropertyRelative("m_CallState").enumValueIndex = 2; // RuntimeOnly -> EditorAndRuntime
        so.ApplyModifiedProperties();
    }

    static void RegisterBuildScenes()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        string mainMenuPath = "Assets/Scenes/MainMenuScene.unity";
        string testScenePath = "Assets/Scenes/TestScene.unity";

        scenes.Add(new EditorBuildSettingsScene(mainMenuPath, true));
        if (System.IO.File.Exists(testScenePath))
            scenes.Add(new EditorBuildSettingsScene(testScenePath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ==================== TAGS & LAYERS ====================

    static void SetupTagsAndLayers()
    {
        string[] tags = { "Player", "Ground", "Item", "DeliveryZone", "GatherZone", "Trap", "SpawnPoint", "Wall" };
        foreach (var tag in tags) AddTag(tag);
        AddLayer(6, "Ground");
        AddLayer(7, "Interactable");
    }

    static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    static void AddLayer(int index, string layerName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");
        if (layers.GetArrayElementAtIndex(index).stringValue == "")
        {
            layers.GetArrayElementAtIndex(index).stringValue = layerName;
            tagManager.ApplyModifiedProperties();
        }
    }

    // ==================== LIGHTING ====================

    static void CreateLighting()
    {
        // Warm cartoon lighting
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.96f, 0.88f);
                light.intensity = 1.5f;
                light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
                light.shadows = LightShadows.Soft;
            }
        }
        RenderSettings.ambientIntensity = 1.0f;

        // Fill light for cartoon feel (no harsh shadows)
        GameObject fillLight = new GameObject("Fill Light");
        var fill = fillLight.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.color = new Color(0.7f, 0.85f, 1f);
        fill.intensity = 0.4f;
        fillLight.transform.rotation = Quaternion.Euler(30f, 150f, 0f);
        fill.shadows = LightShadows.None;
    }

    // ==================== GROUND ====================

    static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(20f, -0.5f, 20f);
        ground.transform.localScale = new Vector3(60f, 1f, 60f);
        ground.tag = "Ground";
        ground.layer = 6;
        ground.isStatic = true;

        var mat = CreateMat(FloorBeige, 0.3f);
        ground.GetComponent<Renderer>().material = mat;
    }

    // ==================== PLAYER (Robot Astronaut) ====================

    static GameObject CreatePlayer()
    {
        // --- Body (capsule) ---
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(2f, 1.5f, 2f);
        player.tag = "Player";
        player.layer = 0;
        player.GetComponent<Renderer>().material = CreateMat(PlayerOrange, 0.5f);

        // Rigidbody
        var rb = player.GetComponent<Rigidbody>();
        if (rb == null) rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // --- Helmet (sphere on top) ---
        GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        helmet.name = "Helmet";
        helmet.transform.SetParent(player.transform);
        helmet.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        helmet.transform.localScale = Vector3.one * 0.7f;
        Object.DestroyImmediate(helmet.GetComponent<Collider>());
        helmet.GetComponent<Renderer>().material = CreateMat(PlayerOrange * 0.85f, 0.6f);

        // --- Visor (dark face screen with LED smile) ---
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "Visor";
        visor.transform.SetParent(helmet.transform);
        visor.transform.localPosition = new Vector3(0f, 0f, 0.4f);
        visor.transform.localScale = new Vector3(0.65f, 0.45f, 0.15f);
        Object.DestroyImmediate(visor.GetComponent<Collider>());
        visor.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.1f, 0.15f), 0.9f);

        // --- LED Eyes on visor ---
        CreateLEDDot(visor.transform, new Vector3(-0.25f, 0.15f, 0.5f), ScreenGreen);
        CreateLEDDot(visor.transform, new Vector3(0.25f, 0.15f, 0.5f), ScreenGreen);

        // --- Helmet light (headlamp) ---
        GameObject headlamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        headlamp.name = "Headlamp";
        headlamp.transform.SetParent(helmet.transform);
        headlamp.transform.localPosition = new Vector3(0f, 0.45f, 0.15f);
        headlamp.transform.localScale = Vector3.one * 0.2f;
        Object.DestroyImmediate(headlamp.GetComponent<Collider>());
        headlamp.GetComponent<Renderer>().material = CreateMat(new Color(1f, 1f, 0.7f), 0.9f);

        // Actual light from headlamp
        var lampLight = headlamp.AddComponent<Light>();
        lampLight.type = LightType.Spot;
        lampLight.color = new Color(1f, 1f, 0.85f);
        lampLight.intensity = 3f;
        lampLight.range = 10f;
        lampLight.spotAngle = 60f;

        // --- Backpack (cube on back) ---
        GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backpack.name = "Backpack";
        backpack.transform.SetParent(player.transform);
        backpack.transform.localPosition = new Vector3(0f, 0.1f, -0.35f);
        backpack.transform.localScale = new Vector3(0.5f, 0.5f, 0.25f);
        Object.DestroyImmediate(backpack.GetComponent<Collider>());
        backpack.GetComponent<Renderer>().material = CreateMat(PlayerOrange * 0.7f, 0.4f);

        // --- Belt (thin cube around waist) ---
        GameObject belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        belt.name = "Belt";
        belt.transform.SetParent(player.transform);
        belt.transform.localPosition = new Vector3(0f, -0.15f, 0f);
        belt.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);
        Object.DestroyImmediate(belt.GetComponent<Collider>());
        belt.GetComponent<Renderer>().material = CreateMat(Color.gray, 0.7f);

        // --- Belt buckle ---
        GameObject buckle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buckle.name = "Buckle";
        buckle.transform.SetParent(belt.transform);
        buckle.transform.localPosition = new Vector3(0f, 0f, 0.55f);
        buckle.transform.localScale = new Vector3(0.35f, 1.2f, 0.15f);
        Object.DestroyImmediate(buckle.GetComponent<Collider>());
        buckle.GetComponent<Renderer>().material = CreateMat(new Color(0.8f, 0.8f, 0.85f), 0.8f);

        // --- Number "1" indicator (small cube with number) ---
        // (In real game this would be a text mesh, for now it's a colored dot)
        GameObject numberBadge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        numberBadge.name = "Player Number";
        numberBadge.transform.SetParent(player.transform);
        numberBadge.transform.localPosition = new Vector3(0.25f, 0.2f, 0.35f);
        numberBadge.transform.localScale = Vector3.one * 0.12f;
        Object.DestroyImmediate(numberBadge.GetComponent<Collider>());
        numberBadge.GetComponent<Renderer>().material = CreateMat(Color.white, 0.5f);

        // === SCRIPTS ===
        var controller = player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInteraction>();
        player.AddComponent<PlayerInventory>();
        player.AddComponent<PlayerEffectHandler>();
        player.AddComponent<PlayerAnimationController>();
        player.AddComponent<PlayerEffects>();

        var so = new SerializedObject(controller);
        so.FindProperty("groundLayer").intValue = 1 << 6;
        so.ApplyModifiedProperties();

        var interaction = player.GetComponent<PlayerInteraction>();
        var soInteract = new SerializedObject(interaction);
        soInteract.FindProperty("interactLayer").intValue = 1 << 7;
        soInteract.ApplyModifiedProperties();

        // Input System
        var playerInput = player.AddComponent<PlayerInput>();
        string[] guids = AssetDatabase.FindAssets("PlayerInputActions t:InputActionAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (inputActions != null)
            {
                playerInput.actions = inputActions;
                playerInput.defaultActionMap = "Player";
                playerInput.notificationBehavior = PlayerNotifications.SendMessages;
            }
        }

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            var camScript = cam.gameObject.AddComponent<ThirdPersonCamera>();
            cam.gameObject.AddComponent<CameraShake>();
            var soCam = new SerializedObject(camScript);
            soCam.FindProperty("target").objectReferenceValue = player.transform;
            soCam.FindProperty("collisionLayers").intValue = ~0;
            soCam.ApplyModifiedProperties();
            cam.transform.position = player.transform.position + new Vector3(0f, 4f, -8f);
        }

        return player;
    }

    static void CreateLEDDot(Transform parent, Vector3 localPos, Color color)
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "LED";
        dot.transform.SetParent(parent);
        dot.transform.localPosition = localPos;
        dot.transform.localScale = Vector3.one * 0.18f;
        Object.DestroyImmediate(dot.GetComponent<Collider>());

        var mat = CreateMat(color, 0.9f);
        mat.SetColor("_EmissionColor", color * 2f);
        mat.EnableKeyword("_EMISSION");
        dot.GetComponent<Renderer>().material = mat;
    }

    // ==================== MAZE WALLS (Colorful toy blocks) ====================

    static void CreateMazeWalls()
    {
        GameObject wallParent = new GameObject("Walls");
        Color[] wallColors = { WallBlue, WallYellow, WallRed, WallOrange };
        int colorIdx = 0;

        // Outer walls
        CreateColorWall(wallParent.transform, new Vector3(10f, 1.5f, 0f), new Vector3(20f, 3f, 0.6f),
            "South Wall", wallColors[colorIdx++ % 4]);
        CreateColorWall(wallParent.transform, new Vector3(0f, 1.5f, 10f), new Vector3(0.6f, 3f, 20f),
            "West Wall", wallColors[colorIdx++ % 4]);
        CreateColorWall(wallParent.transform, new Vector3(10f, 1.5f, 20f), new Vector3(20f, 3f, 0.6f),
            "North Wall", wallColors[colorIdx++ % 4]);
        CreateColorWall(wallParent.transform, new Vector3(20f, 1.5f, 10f), new Vector3(0.6f, 3f, 20f),
            "East Wall", wallColors[colorIdx++ % 4]);

        // Inner walls (alternating colors like concept art)
        CreateColorWall(wallParent.transform, new Vector3(8f, 1.5f, 5f), new Vector3(0.6f, 3f, 10f),
            "Inner Wall 1", WallBlue);
        CreateColorWall(wallParent.transform, new Vector3(12f, 1.5f, 12f), new Vector3(8f, 3f, 0.6f),
            "Inner Wall 2", WallYellow);
        CreateColorWall(wallParent.transform, new Vector3(16f, 1.5f, 8f), new Vector3(0.6f, 3f, 6f),
            "Inner Wall 3", WallRed);
        CreateColorWall(wallParent.transform, new Vector3(5f, 1.5f, 14f), new Vector3(6f, 3f, 0.6f),
            "Inner Wall 4", WallOrange);
        CreateColorWall(wallParent.transform, new Vector3(3f, 1.5f, 8f), new Vector3(4f, 3f, 0.6f),
            "Inner Wall 5", WallBlue);
        CreateColorWall(wallParent.transform, new Vector3(17f, 1.5f, 15f), new Vector3(0.6f, 3f, 5f),
            "Inner Wall 6", WallYellow);

        // Decorative top trim on walls (rounded top like toy blocks)
        // Add cylinder caps on some walls for the toy look
        CreateWallCap(wallParent.transform, new Vector3(8f, 3.1f, 2f), WallBlue);
        CreateWallCap(wallParent.transform, new Vector3(8f, 3.1f, 5f), WallBlue);
        CreateWallCap(wallParent.transform, new Vector3(8f, 3.1f, 8f), WallBlue);
        CreateWallCap(wallParent.transform, new Vector3(12f, 3.1f, 12f), WallYellow);
        CreateWallCap(wallParent.transform, new Vector3(15f, 3.1f, 12f), WallYellow);
    }

    static void CreateColorWall(Transform parent, Vector3 position, Vector3 scale, string name, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.tag = "Wall";
        wall.layer = 6;
        wall.isStatic = true;
        wall.GetComponent<Renderer>().material = CreateMat(color, 0.4f);
    }

    static void CreateWallCap(Transform parent, Vector3 position, Color color)
    {
        // Rounded bumps on top like building blocks / LEGO studs
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "WallCap";
        cap.transform.SetParent(parent);
        cap.transform.position = position;
        cap.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f);
        Object.DestroyImmediate(cap.GetComponent<Collider>());
        cap.isStatic = true;
        cap.GetComponent<Renderer>().material = CreateMat(color * 1.1f, 0.5f);
    }

    // ==================== ITEMS (Purple glowing cubes + cyan spheres) ====================

    static void CreateItems()
    {
        GameObject itemParent = new GameObject("Items");

        // Purple glowing cubes (main collectible from concept art)
        CreateCollectibleItem(itemParent.transform, new Vector3(5f, 1f, 5f), "Energy Cube",
            ItemType.Gem, ItemPurpleGlow, 100, PrimitiveType.Cube, 0.55f);
        CreateCollectibleItem(itemParent.transform, new Vector3(15f, 1f, 5f), "Energy Cube",
            ItemType.Key, ItemPurpleGlow, 150, PrimitiveType.Cube, 0.55f);
        CreateCollectibleItem(itemParent.transform, new Vector3(18f, 1f, 15f), "Energy Cube",
            ItemType.Battery, ItemPurpleGlow, 75, PrimitiveType.Cube, 0.55f);

        // Cyan spheres (from concept art image 4)
        CreateCollectibleItem(itemParent.transform, new Vector3(3f, 1f, 18f), "Data Orb",
            ItemType.Banana, ItemCyan, 50, PrimitiveType.Sphere, 0.4f);
        CreateCollectibleItem(itemParent.transform, new Vector3(10f, 1f, 15f), "Data Orb",
            ItemType.MysteryBox, ItemCyan, 80, PrimitiveType.Sphere, 0.4f);

        // Golden idol (special rare item)
        CreateCollectibleItem(itemParent.transform, new Vector3(17f, 1f, 3f), "Golden Core",
            ItemType.GoldenIdol, new Color(1f, 0.75f, 0.15f), 300, PrimitiveType.Cube, 0.45f);
    }

    static void CreateCollectibleItem(Transform parent, Vector3 position, string name, ItemType type,
        Color color, int points, PrimitiveType shape, float size)
    {
        GameObject item = GameObject.CreatePrimitive(shape);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = Vector3.one * size;
        item.tag = "Item";
        item.layer = 7;

        // Glowing material
        var mat = CreateMat(color, 0.85f);
        mat.SetColor("_EmissionColor", color * 1.5f);
        mat.EnableKeyword("_EMISSION");
        item.GetComponent<Renderer>().material = mat;

        var rb2 = item.AddComponent<Rigidbody>();
        rb2.isKinematic = true;
        rb2.useGravity = false;

        // Glow light
        GameObject lightObj = new GameObject("Glow");
        lightObj.transform.SetParent(item.transform);
        lightObj.transform.localPosition = Vector3.zero;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 3f;
        light.range = 4f;

        var collectible = item.AddComponent<CollectibleItem>();
        var so = new SerializedObject(collectible);
        so.FindProperty("itemType").enumValueIndex = (int)type;
        so.FindProperty("pointValue").intValue = points;
        so.FindProperty("glowLight").objectReferenceValue = light;
        so.ApplyModifiedProperties();
    }

    // ==================== TRAPS (Colorful, toy-like) ====================

    static void CreateTraps()
    {
        GameObject trapParent = new GameObject("Traps");

        // --- Spring Launcher (from concept art - silver coil) ---
        {
            // Base platform
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trap.name = "Spring Launcher";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(14f, 0.15f, 10f);
            trap.transform.localScale = new Vector3(2f, 0.15f, 2f);
            trap.tag = "Trap";
            trap.GetComponent<Renderer>().material = CreateMat(WallOrange, 0.5f);

            var col = trap.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var triggerCol = trap.AddComponent<CapsuleCollider>();
            triggerCol.isTrigger = true;

            // Spring coil visual (stacked rings)
            for (int i = 0; i < 4; i++)
            {
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"Spring Ring {i}";
                ring.transform.SetParent(trap.transform);
                ring.transform.localPosition = new Vector3(0f, (i + 1) * 1.5f, 0f);
                ring.transform.localScale = new Vector3(2f, 0.15f, 2f);
                Object.DestroyImmediate(ring.GetComponent<Collider>());
                ring.GetComponent<Renderer>().material = CreateMat(
                    Color.Lerp(new Color(0.75f, 0.75f, 0.8f), new Color(0.9f, 0.9f, 0.95f), i / 3f), 0.8f);
            }

            trap.AddComponent<LauncherTrap>();
        }

        // --- Push Wall (red danger wall) ---
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trap.name = "Push Wall";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(12f, 1.5f, 5f);
            trap.transform.localScale = new Vector3(3f, 3f, 0.5f);
            trap.tag = "Trap";
            trap.GetComponent<Renderer>().material = CreateMat(WallRed, 0.4f);

            // Warning stripes (small yellow bars)
            for (int i = 0; i < 3; i++)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "Stripe";
                stripe.transform.SetParent(trap.transform);
                stripe.transform.localPosition = new Vector3(-0.3f + i * 0.3f, 0f, 0.52f);
                stripe.transform.localScale = new Vector3(0.08f, 0.9f, 0.05f);
                Object.DestroyImmediate(stripe.GetComponent<Collider>());
                stripe.GetComponent<Renderer>().material = CreateMat(WallYellow, 0.5f);
            }

            var triggerCollider = trap.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(1.2f, 1f, 1.5f);

            var pushWall = trap.AddComponent<PushWallTrap>();
            var so = new SerializedObject(pushWall);
            so.FindProperty("alwaysOn").boolValue = true;
            so.ApplyModifiedProperties();
        }

        // --- Spike Pillars (orange/red from concept art) ---
        {
            GameObject spikeZone = new GameObject("Spike Zone");
            spikeZone.transform.SetParent(trapParent.transform);
            spikeZone.transform.position = new Vector3(10f, 0f, 7f);

            for (int i = 0; i < 4; i++)
            {
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spike.name = $"Spike {i}";
                spike.transform.SetParent(spikeZone.transform);
                spike.transform.localPosition = new Vector3(i * 0.7f - 1.05f, 0.75f, 0f);
                spike.transform.localScale = new Vector3(0.3f, 0.75f, 0.3f);
                Object.DestroyImmediate(spike.GetComponent<Collider>());
                spike.GetComponent<Renderer>().material = CreateMat(
                    Color.Lerp(WallOrange, WallRed, i / 3f), 0.4f);

                // Pointed top
                GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = "Tip";
                tip.transform.SetParent(spike.transform);
                tip.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                tip.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);
                Object.DestroyImmediate(tip.GetComponent<Collider>());
                tip.GetComponent<Renderer>().material = CreateMat(WallRed * 0.8f, 0.3f);
            }
        }

        // --- Slime Zone (green goop) ---
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trap.name = "Slime Zone";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(6f, 0.05f, 10f);
            trap.transform.localScale = new Vector3(4f, 0.1f, 4f);
            trap.GetComponent<Renderer>().material = CreateMat(new Color(0.3f, 0.95f, 0.15f), 0.9f);

            var col = trap.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            trap.AddComponent<SlimeZone>();

            // Slime bubbles
            for (int i = 0; i < 5; i++)
            {
                GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bubble.name = "SlimeBubble";
                bubble.transform.SetParent(trap.transform);
                bubble.transform.localPosition = new Vector3(
                    Random.Range(-0.4f, 0.4f), Random.Range(0.5f, 1.5f), Random.Range(-0.4f, 0.4f));
                bubble.transform.localScale = Vector3.one * Random.Range(0.3f, 0.8f);
                Object.DestroyImmediate(bubble.GetComponent<Collider>());
                bubble.GetComponent<Renderer>().material = CreateMat(
                    new Color(0.2f, 0.9f, 0.1f, 0.6f), 0.9f);
            }
        }

        // --- Spinner Trap (purple beam) ---
        {
            GameObject spinnerBase = new GameObject("Spinner Trap");
            spinnerBase.transform.SetParent(trapParent.transform);
            spinnerBase.transform.position = new Vector3(10f, 0f, 13f);
            spinnerBase.tag = "Trap";

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(spinnerBase.transform);
            pole.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            pole.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
            Object.DestroyImmediate(pole.GetComponent<Collider>());
            pole.GetComponent<Renderer>().material = CreateMat(Color.gray, 0.7f);

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Beam";
            beam.transform.SetParent(spinnerBase.transform);
            beam.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            beam.transform.localScale = new Vector3(6f, 0.5f, 0.35f);

            var beamMat = CreateMat(PlayerPurple, 0.5f);
            beamMat.SetColor("_EmissionColor", PlayerPurple * 0.5f);
            beamMat.EnableKeyword("_EMISSION");
            beam.GetComponent<Renderer>().material = beamMat;

            var beamCol = beam.GetComponent<Collider>();
            if (beamCol != null) beamCol.isTrigger = true;
            var trigger = beam.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1f, 1.5f, 1.5f);

            spinnerBase.AddComponent<SpinnerTrap>();
        }

        // --- Conveyor Belt (gray metallic with arrows) ---
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trap.name = "Conveyor Belt";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(15f, 0.05f, 17f);
            trap.transform.localScale = new Vector3(2f, 0.1f, 6f);
            trap.GetComponent<Renderer>().material = CreateMat(new Color(0.45f, 0.45f, 0.5f), 0.7f);

            // Arrow markings
            for (int i = 0; i < 3; i++)
            {
                GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arrow.name = "Arrow";
                arrow.transform.SetParent(trap.transform);
                arrow.transform.localPosition = new Vector3(0f, 0.55f, -0.3f + i * 0.3f);
                arrow.transform.localScale = new Vector3(0.3f, 0.1f, 0.08f);
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                Object.DestroyImmediate(arrow.GetComponent<Collider>());
                arrow.GetComponent<Renderer>().material = CreateMat(WallYellow, 0.5f);
            }

            var col = trap.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            trap.AddComponent<ConveyorBelt>();
        }
    }

    // ==================== DELIVERY MACHINE (Big Mouth from concept art) ====================

    static void CreateDeliveryMachine()
    {
        GameObject machine = new GameObject("Delivery Machine");
        machine.transform.position = new Vector3(2f, 0f, 5f);
        machine.tag = "DeliveryZone";

        // --- Machine body (yellow/blue box) ---
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Machine Body";
        body.transform.SetParent(machine.transform);
        body.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        body.transform.localScale = new Vector3(2.5f, 3f, 2f);
        body.GetComponent<Renderer>().material = CreateMat(DeliveryYellow, 0.4f);

        // --- Blue top section (head) ---
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Machine Head";
        head.transform.SetParent(machine.transform);
        head.transform.localPosition = new Vector3(0f, 3.3f, 0f);
        head.transform.localScale = new Vector3(2.6f, 1.2f, 2.1f);
        head.GetComponent<Renderer>().material = CreateMat(DeliveryBlue, 0.4f);
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // --- Screen on head (shows TOTAL ASSETS counter) ---
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Counter Screen";
        screen.transform.SetParent(head.transform);
        screen.transform.localPosition = new Vector3(0f, 0.15f, 0.51f);
        screen.transform.localScale = new Vector3(0.7f, 0.5f, 0.02f);
        Object.DestroyImmediate(screen.GetComponent<Collider>());
        var screenMat = CreateMat(new Color(0.05f, 0.12f, 0.08f), 0.9f);
        screenMat.SetColor("_EmissionColor", ScreenGreen * 0.3f);
        screenMat.EnableKeyword("_EMISSION");
        screen.GetComponent<Renderer>().material = screenMat;

        // --- Mouth opening (dark hole with red lips) ---
        GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mouth.name = "Mouth";
        mouth.transform.SetParent(body.transform);
        mouth.transform.localPosition = new Vector3(0f, 0f, 0.51f);
        mouth.transform.localScale = new Vector3(0.55f, 0.35f, 0.02f);
        Object.DestroyImmediate(mouth.GetComponent<Collider>());
        mouth.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.05f, 0.05f), 0.2f);

        // --- Eyes on machine (cute face) ---
        CreateMachineEye(body.transform, new Vector3(-0.2f, 0.25f, 0.51f));
        CreateMachineEye(body.transform, new Vector3(0.2f, 0.25f, 0.51f));

        // --- Side knobs (orange decorations like concept art) ---
        CreateKnob(head.transform, new Vector3(-0.55f, 0f, 0f), WallOrange);
        CreateKnob(head.transform, new Vector3(0.55f, 0f, 0f), WallOrange);

        // --- Antenna on top ---
        GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "Antenna";
        antenna.transform.SetParent(head.transform);
        antenna.transform.localPosition = new Vector3(0.3f, 0.55f, 0f);
        antenna.transform.localScale = new Vector3(0.06f, 0.2f, 0.06f);
        Object.DestroyImmediate(antenna.GetComponent<Collider>());
        antenna.GetComponent<Renderer>().material = CreateMat(Color.gray, 0.7f);

        GameObject antennaBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        antennaBall.name = "Antenna Ball";
        antennaBall.transform.SetParent(antenna.transform);
        antennaBall.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        antennaBall.transform.localScale = new Vector3(3f, 1.5f, 3f);
        Object.DestroyImmediate(antennaBall.GetComponent<Collider>());
        antennaBall.GetComponent<Renderer>().material = CreateMat(WallRed, 0.5f);

        // --- Trigger zone for delivery ---
        GameObject triggerZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        triggerZone.name = "Delivery Trigger";
        triggerZone.transform.SetParent(machine.transform);
        triggerZone.transform.localPosition = new Vector3(0f, 1f, 1.5f);
        triggerZone.transform.localScale = new Vector3(3f, 2f, 2f);
        triggerZone.GetComponent<Renderer>().enabled = false; // Invisible trigger
        var trigCol = triggerZone.GetComponent<Collider>();
        trigCol.isTrigger = true;
        triggerZone.tag = "DeliveryZone";

        triggerZone.AddComponent<DeliveryZone>();

        // Light
        GameObject lightObj = new GameObject("Machine Light");
        lightObj.transform.SetParent(machine.transform);
        lightObj.transform.localPosition = new Vector3(0f, 4f, 1f);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = DeliveryYellow;
        light.intensity = 3f;
        light.range = 6f;
    }

    static void CreateMachineEye(Transform parent, Vector3 localPos)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Eye";
        eye.transform.SetParent(parent);
        eye.transform.localPosition = localPos;
        eye.transform.localScale = new Vector3(0.12f, 0.12f, 0.05f);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        eye.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.1f, 0.1f), 0.9f);
    }

    static void CreateKnob(Transform parent, Vector3 localPos, Color color)
    {
        GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knob.name = "Knob";
        knob.transform.SetParent(parent);
        knob.transform.localPosition = localPos;
        knob.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        Object.DestroyImmediate(knob.GetComponent<Collider>());
        knob.GetComponent<Renderer>().material = CreateMat(color, 0.5f);
    }

    // ==================== GATHER ZONE (Portal) ====================

    static void CreateGatherZone()
    {
        GameObject zone = new GameObject("Gather Zone (Portal)");
        zone.transform.position = new Vector3(18f, 0f, 18f);
        zone.tag = "GatherZone";

        // Base platform
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = "Platform";
        platform.transform.SetParent(zone.transform);
        platform.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        platform.transform.localScale = new Vector3(3f, 0.1f, 3f);
        Object.DestroyImmediate(platform.GetComponent<Collider>());

        var platMat = CreateMat(PlayerPurple, 0.7f);
        platMat.SetColor("_EmissionColor", PlayerPurple * 0.8f);
        platMat.EnableKeyword("_EMISSION");
        platform.GetComponent<Renderer>().material = platMat;

        // Portal ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Portal Ring";
        ring.transform.SetParent(zone.transform);
        ring.transform.localPosition = new Vector3(0f, 2f, 0f);
        ring.transform.localScale = new Vector3(2f, 0.15f, 2f);
        Object.DestroyImmediate(ring.GetComponent<Collider>());

        var ringMat = CreateMat(PlayerPurple, 0.9f);
        ringMat.SetColor("_EmissionColor", PlayerPurple * 2f);
        ringMat.EnableKeyword("_EMISSION");
        ring.GetComponent<Renderer>().material = ringMat;

        // Inner glow
        GameObject innerGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerGlow.name = "Portal Glow";
        innerGlow.transform.SetParent(ring.transform);
        innerGlow.transform.localPosition = Vector3.zero;
        innerGlow.transform.localScale = new Vector3(0.7f, 0.7f, 0.2f);
        Object.DestroyImmediate(innerGlow.GetComponent<Collider>());

        var glowMat = CreateMat(new Color(0.8f, 0.5f, 1f), 0.95f);
        glowMat.SetColor("_EmissionColor", new Color(0.8f, 0.5f, 1f) * 3f);
        glowMat.EnableKeyword("_EMISSION");
        innerGlow.GetComponent<Renderer>().material = glowMat;

        // Light
        GameObject lightObj = new GameObject("Portal Light");
        lightObj.transform.SetParent(zone.transform);
        lightObj.transform.localPosition = new Vector3(0f, 2f, 0f);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = PlayerPurple;
        light.intensity = 5f;
        light.range = 8f;

        // Trigger collider
        var boxTrigger = zone.AddComponent<BoxCollider>();
        boxTrigger.isTrigger = true;
        boxTrigger.size = new Vector3(3f, 4f, 3f);
        boxTrigger.center = new Vector3(0f, 2f, 0f);

        zone.AddComponent<GatherZone>();
    }

    // ==================== GAME MANAGERS ====================

    static void CreateGameManagers()
    {
        GameObject managers = new GameObject("--- Game Managers ---");

        GameObject gm = new GameObject("GameManager");
        gm.transform.SetParent(managers.transform);
        gm.AddComponent<GameManager>();

        GameObject rm = new GameObject("RoundManager");
        rm.transform.SetParent(managers.transform);
        rm.AddComponent<RoundManager>();

        GameObject sm = new GameObject("ScoreManager");
        sm.transform.SetParent(managers.transform);
        sm.AddComponent<ScoreManager>();

        GameObject os = new GameObject("ObjectiveSystem");
        os.transform.SetParent(managers.transform);
        os.AddComponent<ObjectiveSystem>();

        GameObject spm = new GameObject("SpawnPointManager");
        spm.transform.SetParent(managers.transform);
        spm.AddComponent<SpawnPointManager>();

        GameObject am = new GameObject("AudioManager");
        am.transform.SetParent(managers.transform);
        am.AddComponent<AudioManager>();
        var audioSource = am.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        var soAudio = new SerializedObject(am.GetComponent<AudioManager>());
        soAudio.FindProperty("musicSource").objectReferenceValue = audioSource;
        soAudio.ApplyModifiedProperties();
    }

    // ==================== HELPERS ====================

    static Material CreateMat(Color color, float smoothness)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }
}
