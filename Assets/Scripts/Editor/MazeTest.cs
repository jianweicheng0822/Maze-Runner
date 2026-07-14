using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class MazeTest
{
    [MenuItem("Maze/Run 100 Tests")]
    public static void Run100Tests()
    {
        int passed = 0;
        int failed = 0;
        var errors = new List<string>();

        for (int i = 0; i < 100; i++)
        {
            string error = RunSingleTest(i);
            if (error == null)
            {
                passed++;
            }
            else
            {
                failed++;
                errors.Add($"Test {i}: {error}");
            }
        }

        Debug.Log($"=== Maze Test Results ===");
        Debug.Log($"Passed: {passed} / 100");
        Debug.Log($"Failed: {failed} / 100");

        foreach (var err in errors)
        {
            Debug.LogError(err);
        }

        if (failed == 0)
        {
            Debug.Log("All 100 tests passed!");
        }
    }

    static string RunSingleTest(int seed)
    {
        int width = 11;
        int height = 11;

        // Ensure odd
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        int[,] grid = new int[width, height];

        // Init all walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = 0;

        // Set random seed for reproducibility
        Random.InitState(seed);

        // DFS carve (same logic as MazeGenerator)
        CarveMaze(grid, width, height, 1, 1);

        Vector2Int entrance = new Vector2Int(1, height - 2);
        Vector2Int exit = new Vector2Int(width - 2, 1);

        grid[entrance.x, entrance.y] = 1;
        grid[exit.x, exit.y] = 1;

        // Test 1: Border should all be walls (except if entrance/exit is on border)
        for (int x = 0; x < width; x++)
        {
            if (grid[x, 0] != 0) return $"Bottom border not wall at ({x}, 0)";
            if (grid[x, height - 1] != 0) return $"Top border not wall at ({x}, {height - 1})";
        }
        for (int y = 0; y < height; y++)
        {
            if (grid[0, y] != 0) return $"Left border not wall at (0, {y})";
            if (grid[width - 1, y] != 0) return $"Right border not wall at ({width - 1}, {y})";
        }

        // Test 2: Entrance is on floor
        if (grid[entrance.x, entrance.y] != 1)
            return $"Entrance ({entrance.x}, {entrance.y}) is a wall";

        // Test 3: Exit is on floor
        if (grid[exit.x, exit.y] != 1)
            return $"Exit ({exit.x}, {exit.y}) is a wall";

        // Test 4: Path exists from entrance to exit (BFS)
        if (!HasPath(grid, width, height, entrance, exit))
            return $"No path from entrance to exit";

        // Test 5: At least some floor tiles exist
        int floorCount = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 1) floorCount++;

        if (floorCount < 10)
            return $"Too few floor tiles: {floorCount}";

        return null;
    }

    static void CarveMaze(int[,] grid, int width, int height, int startX, int startY)
    {
        var stack = new Stack<Vector2Int>();
        grid[startX, startY] = 1;
        stack.Push(new Vector2Int(startX, startY));

        Vector2Int[] directions = {
            new Vector2Int(0, 2),
            new Vector2Int(0, -2),
            new Vector2Int(-2, 0),
            new Vector2Int(2, 0)
        };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            var unvisited = new List<Vector2Int>();

            foreach (var dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && grid[nx, ny] == 0)
                {
                    unvisited.Add(dir);
                }
            }

            if (unvisited.Count > 0)
            {
                var chosen = unvisited[Random.Range(0, unvisited.Count)];
                int wallX = current.x + chosen.x / 2;
                int wallY = current.y + chosen.y / 2;
                int nextX = current.x + chosen.x;
                int nextY = current.y + chosen.y;

                grid[wallX, wallY] = 1;
                grid[nextX, nextY] = 1;

                stack.Push(new Vector2Int(nextX, nextY));
            }
            else
            {
                stack.Pop();
            }
        }
    }

    static bool HasPath(int[,] grid, int width, int height, Vector2Int start, Vector2Int end)
    {
        bool[,] visited = new bool[width, height];
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] dirs = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();

            if (pos == end) return true;

            foreach (var dir in dirs)
            {
                int nx = pos.x + dir.x;
                int ny = pos.y + dir.y;

                if (nx >= 0 && nx < width && ny >= 0 && ny < height
                    && !visited[nx, ny] && grid[nx, ny] == 1)
                {
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return false;
    }
}
