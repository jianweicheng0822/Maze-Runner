using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using MazeRunner.Core;
using MazeRunner.Player;
using MazeRunner.Items;
using MazeRunner.Traps;
using MazeRunner.Maze;

/// <summary>
/// One-click test scene setup for Maze Runner.
/// Menu: Maze Runner > Setup Test Scene
/// Creates a playable scene with player, items, traps, delivery zone, and a mini maze.
/// </summary>
public class MazeRunnerSceneSetup : EditorWindow
{
    [MenuItem("Maze Runner/Setup Test Scene (Full) %#t")]
    public static void SetupFullTestScene()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Setup tags and layers first
        SetupTagsAndLayers();

        // Build the scene
        CreateLighting();
        CreateGround();
        GameObject player = CreatePlayer();
        CreateMazeWalls();
        CreateItems();
        CreateTraps();
        CreateDeliveryZone();
        CreateGatherZone();
        CreateGameManagers();

        // Select the player so the user can see it
        Selection.activeGameObject = player;

        // Save the scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/TestScene.unity");

        Debug.Log("=== Maze Runner Test Scene Created! Press Play to test. ===\n" +
                  "Controls: WASD = Move, Mouse = Look, Space = Jump, E = Interact/Drop");
    }

    [MenuItem("Maze Runner/Setup Mini Maze (Procedural) %#m")]
    public static void SetupProceduralMaze()
    {
        SetupTagsAndLayers();

        CreateLighting();

        // Create maze builder
        GameObject mazeObj = new GameObject("MazeSystem");
        var builder = mazeObj.AddComponent<MazeBuilder>();

        // Use serialized object to set private serialized fields
        var so = new SerializedObject(builder);
        so.FindProperty("cellSize").floatValue = 4f;
        so.FindProperty("mazeWidth").intValue = 8;
        so.FindProperty("mazeHeight").intValue = 8;
        so.FindProperty("seed").intValue = 42;
        so.FindProperty("loopChance").floatValue = 0.15f;
        so.ApplyModifiedProperties();

        GameObject player = CreatePlayer();
        CreateGameManagers();

        // The maze generates at runtime via MazeBuilder.GenerateAndBuild()
        // Add a simple starter script
        var starter = mazeObj.AddComponent<AutoStartMaze>();

        Selection.activeGameObject = player;

        Debug.Log("=== Procedural Maze Scene Created! Press Play to generate maze. ===");
    }

    static void SetupTagsAndLayers()
    {
        // Add tags
        AddTag("Player");
        AddTag("Ground");
        AddTag("Item");
        AddTag("DeliveryZone");
        AddTag("GatherZone");
        AddTag("Trap");
        AddTag("SpawnPoint");
        AddTag("Wall");

        // Add layer for Ground (layer 6)
        AddLayer(6, "Ground");
        // Add layer for Interactable (layer 7)
        AddLayer(7, "Interactable");
    }

    static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        }

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

    static void CreateLighting()
    {
        // The default scene already has a directional light, tweak it
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.95f, 0.85f);
                light.intensity = 1.2f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                light.shadows = LightShadows.Soft;
            }
        }

        // Ambient
        RenderSettings.ambientIntensity = 0.8f;
    }

    static void CreateGround()
    {
        // Large ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(20f, -0.5f, 20f);
        ground.transform.localScale = new Vector3(60f, 1f, 60f);
        ground.tag = "Ground";
        ground.layer = 6; // Ground layer
        ground.isStatic = true;

        // Give it a green-ish color
        var renderer = ground.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.45f, 0.75f, 0.35f);
        renderer.material = mat;
    }

    static GameObject CreatePlayer()
    {
        // Player body - capsule
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(2f, 1.5f, 2f);
        player.tag = "Player";
        player.layer = 0;

        // Color
        var renderer = player.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.3f, 0.6f, 1f); // Blue player
        renderer.material = mat;

        // Rigidbody
        var rb = player.GetComponent<Rigidbody>();
        if (rb == null) rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Player scripts
        var controller = player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInteraction>();
        player.AddComponent<PlayerInventory>();
        player.AddComponent<PlayerEffectHandler>();
        player.AddComponent<PlayerAnimationController>();
        player.AddComponent<PlayerEffects>();

        // Set ground layer mask on controller via serialized property
        var so = new SerializedObject(controller);
        so.FindProperty("groundLayer").intValue = 1 << 6; // Ground layer
        so.ApplyModifiedProperties();

        // Set interact layer on interaction
        var interaction = player.GetComponent<PlayerInteraction>();
        var soInteract = new SerializedObject(interaction);
        soInteract.FindProperty("interactLayer").intValue = 1 << 7; // Interactable layer
        soInteract.ApplyModifiedProperties();

        // Input System - PlayerInput component
        var playerInput = player.AddComponent<PlayerInput>();

        // Try to find the input actions asset
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

        // Camera setup
        var cam = Camera.main;
        if (cam != null)
        {
            var camScript = cam.gameObject.AddComponent<ThirdPersonCamera>();
            cam.gameObject.AddComponent<CameraShake>();

            var soCam = new SerializedObject(camScript);
            soCam.FindProperty("target").objectReferenceValue = player.transform;
            soCam.FindProperty("collisionLayers").intValue = ~0; // All layers
            soCam.ApplyModifiedProperties();

            cam.transform.position = player.transform.position + new Vector3(0f, 4f, -8f);
        }

        // Eyes (cosmetic - two small white spheres)
        CreateEye(player.transform, new Vector3(-0.15f, 0.35f, 0.4f));
        CreateEye(player.transform, new Vector3(0.15f, 0.35f, 0.4f));

        return player;
    }

    static void CreateEye(Transform parent, Vector3 localPos)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Eye";
        eye.transform.SetParent(parent);
        eye.transform.localPosition = localPos;
        eye.transform.localScale = Vector3.one * 0.15f;
        Object.DestroyImmediate(eye.GetComponent<Collider>()); // No collider on eyes

        var renderer = eye.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.white;
        renderer.material = mat;

        // Pupil
        GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pupil.name = "Pupil";
        pupil.transform.SetParent(eye.transform);
        pupil.transform.localPosition = new Vector3(0f, 0.1f, 0.35f);
        pupil.transform.localScale = Vector3.one * 0.5f;
        Object.DestroyImmediate(pupil.GetComponent<Collider>());

        var pupilRenderer = pupil.GetComponent<Renderer>();
        var pupilMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        pupilMat.color = Color.black;
        pupilRenderer.material = pupilMat;
    }

    static void CreateMazeWalls()
    {
        GameObject wallParent = new GameObject("Walls");

        // Create a simple L-shaped maze corridor for testing
        CreateWall(wallParent.transform, new Vector3(10f, 1.5f, 0f), new Vector3(20f, 3f, 0.5f), "South Wall");
        CreateWall(wallParent.transform, new Vector3(0f, 1.5f, 10f), new Vector3(0.5f, 3f, 20f), "West Wall");
        CreateWall(wallParent.transform, new Vector3(10f, 1.5f, 20f), new Vector3(20f, 3f, 0.5f), "North Wall");
        CreateWall(wallParent.transform, new Vector3(20f, 1.5f, 10f), new Vector3(0.5f, 3f, 20f), "East Wall");

        // Interior walls for maze feel
        CreateWall(wallParent.transform, new Vector3(8f, 1.5f, 5f), new Vector3(0.5f, 3f, 10f), "Inner Wall 1");
        CreateWall(wallParent.transform, new Vector3(12f, 1.5f, 12f), new Vector3(8f, 3f, 0.5f), "Inner Wall 2");
        CreateWall(wallParent.transform, new Vector3(16f, 1.5f, 8f), new Vector3(0.5f, 3f, 6f), "Inner Wall 3");
        CreateWall(wallParent.transform, new Vector3(5f, 1.5f, 14f), new Vector3(6f, 3f, 0.5f), "Inner Wall 4");
    }

    static void CreateWall(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.tag = "Wall";
        wall.layer = 6; // Same as ground for camera collision
        wall.isStatic = true;

        var renderer = wall.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.85f, 0.8f, 0.7f); // Stone-ish
        renderer.material = mat;
    }

    static void CreateItems()
    {
        GameObject itemParent = new GameObject("Items");

        CreateCollectibleItem(itemParent.transform, new Vector3(5f, 1f, 5f), "Gem", ItemType.Gem,
            new Color(0f, 1f, 0.5f), 100);
        CreateCollectibleItem(itemParent.transform, new Vector3(15f, 1f, 5f), "Key", ItemType.Key,
            new Color(1f, 0.85f, 0f), 150);
        CreateCollectibleItem(itemParent.transform, new Vector3(18f, 1f, 15f), "Battery", ItemType.Battery,
            new Color(0.2f, 0.8f, 1f), 75);
        CreateCollectibleItem(itemParent.transform, new Vector3(3f, 1f, 18f), "Banana", ItemType.Banana,
            new Color(1f, 1f, 0f), 50);
        CreateCollectibleItem(itemParent.transform, new Vector3(10f, 1f, 15f), "Golden Idol", ItemType.GoldenIdol,
            new Color(1f, 0.7f, 0f), 300);
    }

    static void CreateCollectibleItem(Transform parent, Vector3 position, string name, ItemType type,
        Color color, int points)
    {
        // Item body
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = Vector3.one * 0.5f;
        item.tag = "Item";
        item.layer = 7; // Interactable

        var renderer = item.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.8f);
        renderer.material = mat;

        // Rigidbody
        var rb = item.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Glow light
        GameObject lightObj = new GameObject("Glow");
        lightObj.transform.SetParent(item.transform);
        lightObj.transform.localPosition = Vector3.zero;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 2f;
        light.range = 3f;

        // CollectibleItem script
        var collectible = item.AddComponent<CollectibleItem>();
        var so = new SerializedObject(collectible);
        so.FindProperty("itemType").enumValueIndex = (int)type;
        so.FindProperty("pointValue").intValue = points;
        so.FindProperty("glowLight").objectReferenceValue = light;
        so.ApplyModifiedProperties();
    }

    static void CreateTraps()
    {
        GameObject trapParent = new GameObject("Traps");

        // Push Wall Trap
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trap.name = "PushWall Trap";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(12f, 1.5f, 5f);
            trap.transform.localScale = new Vector3(3f, 3f, 0.5f);
            trap.tag = "Trap";

            var renderer = trap.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.9f, 0.2f, 0.2f); // Red = danger
            renderer.material = mat;

            // Trigger collider (slightly larger than visual)
            var triggerCollider = trap.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(1.2f, 1f, 1.5f);

            var pushWall = trap.AddComponent<PushWallTrap>();
            var so = new SerializedObject(pushWall);
            so.FindProperty("alwaysOn").boolValue = true;
            so.ApplyModifiedProperties();
        }

        // Launcher Trap
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trap.name = "Launcher Pad";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(14f, 0.15f, 10f);
            trap.transform.localScale = new Vector3(2f, 0.15f, 2f);
            trap.tag = "Trap";

            var renderer = trap.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.5f, 0f); // Orange
            renderer.material = mat;

            var col = trap.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var triggerCol = trap.AddComponent<CapsuleCollider>();
            triggerCol.isTrigger = true;

            trap.AddComponent<LauncherTrap>();
        }

        // Slime Zone
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trap.name = "Slime Zone";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(6f, 0.05f, 10f);
            trap.transform.localScale = new Vector3(4f, 0.1f, 4f);

            var renderer = trap.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.9f, 0.1f, 0.7f); // Green slime
            mat.SetFloat("_Surface", 1); // Transparent
            renderer.material = mat;

            var col = trap.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            trap.AddComponent<SlimeZone>();
        }

        // Spinner Trap
        {
            // Base pole
            GameObject spinnerBase = new GameObject("Spinner Trap");
            spinnerBase.transform.SetParent(trapParent.transform);
            spinnerBase.transform.position = new Vector3(10f, 0f, 10f);
            spinnerBase.tag = "Trap";

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(spinnerBase.transform);
            pole.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            pole.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            Object.DestroyImmediate(pole.GetComponent<Collider>());

            // Beam
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Beam";
            beam.transform.SetParent(spinnerBase.transform);
            beam.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            beam.transform.localScale = new Vector3(6f, 0.5f, 0.3f);

            var beamRenderer = beam.GetComponent<Renderer>();
            var beamMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            beamMat.color = new Color(0.8f, 0.2f, 0.8f); // Purple
            beamRenderer.material = beamMat;

            // Trigger on beam
            var beamCol = beam.GetComponent<Collider>();
            if (beamCol != null) beamCol.isTrigger = true;
            var trigger = beam.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1f, 1.5f, 1.5f);

            var spinner = spinnerBase.AddComponent<SpinnerTrap>();
        }

        // Conveyor Belt
        {
            GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trap.name = "Conveyor Belt";
            trap.transform.SetParent(trapParent.transform);
            trap.transform.position = new Vector3(15f, 0.05f, 17f);
            trap.transform.localScale = new Vector3(2f, 0.1f, 6f);

            var renderer = trap.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.4f, 0.4f, 0.5f); // Gray metallic
            renderer.material = mat;

            var col = trap.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            trap.AddComponent<ConveyorBelt>();
        }
    }

    static void CreateDeliveryZone()
    {
        // Delivery zone near player spawn
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zone.name = "Delivery Zone";
        zone.transform.position = new Vector3(2f, 0.1f, 5f);
        zone.transform.localScale = new Vector3(3f, 0.1f, 3f);
        zone.tag = "DeliveryZone";

        var renderer = zone.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.5f, 1f, 0.5f); // Blue glow
        renderer.material = mat;

        var col = zone.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var delivery = zone.AddComponent<DeliveryZone>();

        // Light indicator
        GameObject lightObj = new GameObject("Zone Light");
        lightObj.transform.SetParent(zone.transform);
        lightObj.transform.localPosition = Vector3.up * 20f; // scale is 0.1 so this is 2 units
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.3f, 0.6f, 1f);
        light.intensity = 3f;
        light.range = 5f;
    }

    static void CreateGatherZone()
    {
        // Gather zone at far end
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zone.name = "Gather Zone (Portal)";
        zone.transform.position = new Vector3(18f, 0.1f, 18f);
        zone.transform.localScale = new Vector3(3f, 0.1f, 3f);
        zone.tag = "GatherZone";

        var renderer = zone.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1f, 0.3f, 1f, 0.5f); // Purple portal
        renderer.material = mat;

        var col = zone.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var gather = zone.AddComponent<GatherZone>();

        // Portal visual (a ring)
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Torus);
        ring.name = "Portal Ring";
        ring.transform.SetParent(zone.transform);
        ring.transform.localPosition = Vector3.up * 15f; // Scaled
        ring.transform.localScale = new Vector3(15f, 15f, 15f);
        Object.DestroyImmediate(ring.GetComponent<Collider>());

        var ringRenderer = ring.GetComponent<Renderer>();
        var ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ringMat.color = new Color(0.8f, 0.2f, 1f);
        ringMat.SetFloat("_Smoothness", 0.9f);
        ringRenderer.material = ringMat;

        // Light
        GameObject lightObj = new GameObject("Portal Light");
        lightObj.transform.SetParent(zone.transform);
        lightObj.transform.localPosition = Vector3.up * 20f;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.8f, 0.2f, 1f);
        light.intensity = 5f;
        light.range = 8f;
    }

    static void CreateGameManagers()
    {
        // Game managers
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
        // Wire up music source
        var soAudio = new SerializedObject(am.GetComponent<AudioManager>());
        soAudio.FindProperty("musicSource").objectReferenceValue = audioSource;
        soAudio.ApplyModifiedProperties();
    }
}
