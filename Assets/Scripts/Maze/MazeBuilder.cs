using UnityEngine;

namespace MazeRunner.Maze
{
    public class MazeBuilder : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float cellSize = 4f;
        [SerializeField] private MazeTheme theme;

        [Header("Generation")]
        [SerializeField] private int mazeWidth = 10;
        [SerializeField] private int mazeHeight = 10;
        [SerializeField] private int seed = -1; // -1 = random
        [SerializeField] private float loopChance = 0.15f;

        private MazeCell[,] mazeData;
        private Transform mazeParent;

        public MazeCell[,] MazeData => mazeData;
        public float CellSize => cellSize;
        public int Width => mazeWidth;
        public int Height => mazeHeight;

        public event System.Action<MazeCell[,]> OnMazeBuilt;

        public void GenerateAndBuild()
        {
            ClearMaze();

            int usedSeed = seed >= 0 ? seed : Random.Range(0, int.MaxValue);

            var generator = new MazeGenerator();
            mazeData = generator.Generate(mazeWidth, mazeHeight, usedSeed, loopChance);

            BuildMaze();
            OnMazeBuilt?.Invoke(mazeData);
        }

        public void GenerateAndBuild(int networkSeed)
        {
            ClearMaze();
            var generator = new MazeGenerator();
            mazeData = generator.Generate(mazeWidth, mazeHeight, networkSeed, loopChance);
            BuildMaze();
            OnMazeBuilt?.Invoke(mazeData);
        }

        private void BuildMaze()
        {
            mazeParent = new GameObject("Maze").transform;
            mazeParent.SetParent(transform);

            float wallHeight = theme != null ? theme.wallHeight : 3f;

            for (int x = 0; x < mazeWidth; x++)
            {
                for (int z = 0; z < mazeHeight; z++)
                {
                    Vector3 cellCenter = new Vector3(x * cellSize, 0f, z * cellSize);
                    var cell = mazeData[x, z];

                    // Floor
                    SpawnPrefab(GetFloorPrefab(cell.cellType), cellCenter, Quaternion.identity);

                    // Walls
                    if (cell.wallNorth)
                        SpawnWall(cellCenter + new Vector3(0f, wallHeight * 0.5f, cellSize * 0.5f),
                            Quaternion.identity);
                    if (cell.wallSouth)
                        SpawnWall(cellCenter + new Vector3(0f, wallHeight * 0.5f, -cellSize * 0.5f),
                            Quaternion.identity);
                    if (cell.wallEast)
                        SpawnWall(cellCenter + new Vector3(cellSize * 0.5f, wallHeight * 0.5f, 0f),
                            Quaternion.Euler(0f, 90f, 0f));
                    if (cell.wallWest)
                        SpawnWall(cellCenter + new Vector3(-cellSize * 0.5f, wallHeight * 0.5f, 0f),
                            Quaternion.Euler(0f, 90f, 0f));

                    // Special cell objects
                    SpawnCellObject(cell, cellCenter);
                }
            }
        }

        private void SpawnWall(Vector3 position, Quaternion rotation)
        {
            GameObject prefab = theme != null && theme.wallPrefab != null
                ? theme.wallPrefab
                : null;

            if (prefab != null)
            {
                var wall = Instantiate(prefab, position, rotation, mazeParent);
                wall.tag = Core.GameTags.Wall;
            }
            else
            {
                // Fallback: create primitive wall
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float wallHeight = theme != null ? theme.wallHeight : 3f;
                wall.transform.localScale = new Vector3(cellSize, wallHeight, 0.3f);
                wall.transform.position = position;
                wall.transform.rotation = rotation;
                wall.transform.SetParent(mazeParent);
                wall.tag = Core.GameTags.Wall;
                wall.isStatic = true;

                if (theme != null && theme.wallMaterial != null)
                    wall.GetComponent<Renderer>().material = theme.wallMaterial;
            }
        }

        private void SpawnPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab != null)
            {
                Instantiate(prefab, position, rotation, mazeParent);
            }
            else
            {
                // Fallback floor
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                floor.transform.position = position + Vector3.down * 0.1f;
                floor.transform.SetParent(mazeParent);
                floor.tag = Core.GameTags.Ground;
                floor.isStatic = true;

                if (theme != null && theme.floorMaterial != null)
                    floor.GetComponent<Renderer>().material = theme.floorMaterial;
            }
        }

        private GameObject GetFloorPrefab(CellType cellType)
        {
            if (theme == null) return null;
            return theme.floorPrefab;
        }

        private void SpawnCellObject(MazeCell cell, Vector3 cellCenter)
        {
            if (theme == null) return;

            switch (cell.cellType)
            {
                case CellType.SpawnPoint:
                    if (theme.spawnPointPrefab != null)
                        Instantiate(theme.spawnPointPrefab, cellCenter, Quaternion.identity, mazeParent);
                    break;
                case CellType.DeliveryZone:
                    if (theme.deliveryZonePrefab != null)
                        Instantiate(theme.deliveryZonePrefab, cellCenter, Quaternion.identity, mazeParent);
                    break;
                case CellType.GatherZone:
                    if (theme.gatherZonePrefab != null)
                        Instantiate(theme.gatherZonePrefab, cellCenter, Quaternion.identity, mazeParent);
                    break;
            }
        }

        public void ClearMaze()
        {
            if (mazeParent != null)
                Destroy(mazeParent.gameObject);
        }

        public Vector3 CellToWorld(int x, int z)
        {
            return new Vector3(x * cellSize, 0f, z * cellSize);
        }
    }
}
