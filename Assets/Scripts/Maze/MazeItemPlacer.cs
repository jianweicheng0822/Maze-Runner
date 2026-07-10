using UnityEngine;

namespace MazeRunner.Maze
{
    public class MazeItemPlacer : MonoBehaviour
    {
        [SerializeField] private MazeBuilder mazeBuilder;
        [SerializeField] private MazeTheme theme;
        [SerializeField] private int maxItems = 15;

        private Transform itemParent;

        void Start()
        {
            if (mazeBuilder != null)
                mazeBuilder.OnMazeBuilt += PlaceItems;
        }

        public void PlaceItems(MazeCell[,] grid)
        {
            if (theme == null || theme.itemPrefabs == null || theme.itemPrefabs.Length == 0) return;

            if (itemParent != null) Destroy(itemParent.gameObject);
            itemParent = new GameObject("Items").transform;
            itemParent.SetParent(transform);

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int placed = 0;

            for (int x = 0; x < width && placed < maxItems; x++)
            {
                for (int z = 0; z < height && placed < maxItems; z++)
                {
                    if (grid[x, z].cellType != CellType.ItemSpawn) continue;

                    Vector3 pos = mazeBuilder.CellToWorld(x, z) + Vector3.up * 0.5f;
                    var prefab = theme.itemPrefabs[Random.Range(0, theme.itemPrefabs.Length)];
                    Instantiate(prefab, pos, Quaternion.identity, itemParent);
                    placed++;
                }
            }
        }
    }
}
