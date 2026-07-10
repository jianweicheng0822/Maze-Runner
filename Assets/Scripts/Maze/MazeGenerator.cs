using System.Collections.Generic;
using UnityEngine;

namespace MazeRunner.Maze
{
    public class MazeGenerator
    {
        private MazeCell[,] grid;
        private int width;
        private int height;
        private System.Random rng;

        public MazeCell[,] Generate(int width, int height, int seed, float loopChance = 0.15f)
        {
            this.width = width;
            this.height = height;
            rng = new System.Random(seed);

            grid = new MazeCell[width, height];

            // Initialize all cells with all walls
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    grid[x, z] = new MazeCell();
                    grid[x, z].x = x;
                    grid[x, z].z = z;
                    grid[x, z].InitWalls();
                }
            }

            // Recursive backtracking
            var stack = new Stack<Vector2Int>();
            var start = new Vector2Int(0, 0);
            grid[0, 0].visited = true;
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var neighbors = GetUnvisitedNeighbors(current.x, current.y);

                if (neighbors.Count > 0)
                {
                    var next = neighbors[rng.Next(neighbors.Count)];
                    RemoveWallBetween(current.x, current.y, next.x, next.y);
                    grid[next.x, next.y].visited = true;
                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }

            // Loop injection - remove random walls to create multiple paths
            int wallsToRemove = Mathf.FloorToInt(width * height * loopChance);
            for (int i = 0; i < wallsToRemove; i++)
            {
                int x = rng.Next(width);
                int z = rng.Next(height);
                int dir = rng.Next(4);

                switch (dir)
                {
                    case 0 when z < height - 1:
                        grid[x, z].wallNorth = false;
                        grid[x, z + 1].wallSouth = false;
                        break;
                    case 1 when z > 0:
                        grid[x, z].wallSouth = false;
                        grid[x, z - 1].wallNorth = false;
                        break;
                    case 2 when x < width - 1:
                        grid[x, z].wallEast = false;
                        grid[x + 1, z].wallWest = false;
                        break;
                    case 3 when x > 0:
                        grid[x, z].wallWest = false;
                        grid[x - 1, z].wallEast = false;
                        break;
                }
            }

            // Assign special cell types
            AssignCellTypes();

            return grid;
        }

        private void AssignCellTypes()
        {
            // Spawn point at start
            grid[0, 0].cellType = CellType.SpawnPoint;

            // Delivery zone near spawn
            grid[1, 0].cellType = CellType.DeliveryZone;

            // Gather zone at far corner
            grid[width - 1, height - 1].cellType = CellType.GatherZone;

            // Find dead ends and intersections for items and traps
            var deadEnds = new List<Vector2Int>();
            var intersections = new List<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (grid[x, z].cellType != CellType.Normal) continue;

                    if (grid[x, z].IsDeadEnd)
                        deadEnds.Add(new Vector2Int(x, z));
                    else if (grid[x, z].IsIntersection)
                        intersections.Add(new Vector2Int(x, z));
                }
            }

            // Place items at dead ends
            int itemCount = Mathf.Min(deadEnds.Count, width * height / 10);
            Shuffle(deadEnds);
            for (int i = 0; i < itemCount && i < deadEnds.Count; i++)
            {
                var pos = deadEnds[i];
                grid[pos.x, pos.y].cellType = CellType.ItemSpawn;
            }

            // Place traps at intersections/corridors
            int trapCount = Mathf.Min(intersections.Count / 2, width * height / 15);
            Shuffle(intersections);
            for (int i = 0; i < trapCount && i < intersections.Count; i++)
            {
                var pos = intersections[i];
                grid[pos.x, pos.y].cellType = CellType.TrapZone;
            }
        }

        private List<Vector2Int> GetUnvisitedNeighbors(int x, int z)
        {
            var neighbors = new List<Vector2Int>();

            if (z < height - 1 && !grid[x, z + 1].visited) neighbors.Add(new Vector2Int(x, z + 1));
            if (z > 0 && !grid[x, z - 1].visited) neighbors.Add(new Vector2Int(x, z - 1));
            if (x < width - 1 && !grid[x + 1, z].visited) neighbors.Add(new Vector2Int(x + 1, z));
            if (x > 0 && !grid[x - 1, z].visited) neighbors.Add(new Vector2Int(x - 1, z));

            return neighbors;
        }

        private void RemoveWallBetween(int x1, int z1, int x2, int z2)
        {
            if (x2 == x1 + 1) { grid[x1, z1].wallEast = false; grid[x2, z2].wallWest = false; }
            if (x2 == x1 - 1) { grid[x1, z1].wallWest = false; grid[x2, z2].wallEast = false; }
            if (z2 == z1 + 1) { grid[x1, z1].wallNorth = false; grid[x2, z2].wallSouth = false; }
            if (z2 == z1 - 1) { grid[x1, z1].wallSouth = false; grid[x2, z2].wallNorth = false; }
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
