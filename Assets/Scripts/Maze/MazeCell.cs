namespace MazeRunner.Maze
{
    public enum CellType
    {
        Normal,
        SpawnPoint,
        ItemSpawn,
        TrapZone,
        DeliveryZone,
        GatherZone,
        Empty
    }

    [System.Serializable]
    public struct MazeCell
    {
        public bool wallNorth;
        public bool wallSouth;
        public bool wallEast;
        public bool wallWest;
        public bool visited;
        public CellType cellType;
        public int x;
        public int z;

        public void InitWalls()
        {
            wallNorth = true;
            wallSouth = true;
            wallEast = true;
            wallWest = true;
            visited = false;
            cellType = CellType.Normal;
        }

        public int WallCount
        {
            get
            {
                int count = 0;
                if (wallNorth) count++;
                if (wallSouth) count++;
                if (wallEast) count++;
                if (wallWest) count++;
                return count;
            }
        }

        public bool IsDeadEnd => WallCount >= 3;
        public bool IsIntersection => WallCount <= 1;
        public bool IsCorridor => WallCount == 2;
    }
}
