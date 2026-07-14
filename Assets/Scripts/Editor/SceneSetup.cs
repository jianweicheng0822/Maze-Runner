using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSetup
{
    [MenuItem("Maze/Create Game Scene")]
    public static void CreateGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
        camObj.transform.position = new Vector3(0, 0, -10);
        camObj.AddComponent<CameraFollow>();

        // GameManager + MazeGenerator + FogOfWar
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<MazeGenerator>();
        gmObj.AddComponent<FogOfWar>();
        gmObj.AddComponent<GameManager>();

        // Save
        string path = "Assets/Scenes/GameScene.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, path);

        // Add to build settings
        var buildScenes = EditorBuildSettings.scenes;
        bool found = false;
        foreach (var s in buildScenes)
        {
            if (s.path == path) { found = true; break; }
        }
        if (!found)
        {
            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            buildScenes.CopyTo(newScenes, 0);
            newScenes[buildScenes.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = newScenes;
        }

        Debug.Log("Game scene created. Press Play to start!");
    }
}
