using UnityEngine;

namespace MazeRunner.Maze
{
    public class MazeTrapPlacer : MonoBehaviour
    {
        [SerializeField] private MazeBuilder mazeBuilder;
        [SerializeField] private MazeTheme theme;
        [SerializeField] [Range(0f, 1f)] private float trapDensity = 0.3f;

        private Transform trapParent;

        void Start()
        {
            if (mazeBuilder != null)
                mazeBuilder.OnMazeBuilt += PlaceTraps;
        }

        public void PlaceTraps(MazeCell[,] grid)
        {
            if (theme == null || theme.trapPrefabs == null || theme.trapPrefabs.Length == 0) return;

            if (trapParent != null) Destroy(trapParent.gameObject);
            trapParent = new GameObject("Traps").transform;
            trapParent.SetParent(transform);

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (grid[x, z].cellType != CellType.TrapZone) continue;
                    if (Random.value > trapDensity) continue;

                    Vector3 pos = mazeBuilder.CellToWorld(x, z);
                    var prefab = theme.trapPrefabs[Random.Range(0, theme.trapPrefabs.Length)];
                    Instantiate(prefab, pos, Quaternion.identity, trapParent);
                }
            }
        }
    }
}
