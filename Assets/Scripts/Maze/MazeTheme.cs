using UnityEngine;

namespace MazeRunner.Maze
{
    [CreateAssetMenu(fileName = "NewMazeTheme", menuName = "MazeRunner/Maze Theme")]
    public class MazeTheme : ScriptableObject
    {
        [Header("Structural Prefabs")]
        public GameObject wallPrefab;
        public GameObject floorPrefab;
        public GameObject cornerPrefab;

        [Header("Special Prefabs")]
        public GameObject spawnPointPrefab;
        public GameObject deliveryZonePrefab;
        public GameObject gatherZonePrefab;

        [Header("Materials")]
        public Material wallMaterial;
        public Material floorMaterial;

        [Header("Traps")]
        public GameObject[] trapPrefabs;

        [Header("Items")]
        public GameObject[] itemPrefabs;

        [Header("Theme Settings")]
        public string themeName = "Default";
        public Color ambientColor = Color.white;
        public float wallHeight = 3f;
    }
}
