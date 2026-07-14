using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSetup
{
    [MenuItem("Maze/Create Game Scene")]
    public static void CreateGameScene()
    {
        // Create a new empty scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
        camObj.transform.position = new Vector3(0, 0, -10);

        // Create GameManager
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // Save the scene
        string path = "Assets/Scenes/GameScene.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, path);

        // Add to build settings
        var buildScenes = EditorBuildSettings.scenes;
        bool alreadyAdded = false;
        foreach (var s in buildScenes)
        {
            if (s.path == path) { alreadyAdded = true; break; }
        }
        if (!alreadyAdded)
        {
            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            buildScenes.CopyTo(newScenes, 0);
            newScenes[buildScenes.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = newScenes;
        }

        Debug.Log("Game scene created at " + path + ". Press Play to start!");
    }
}
