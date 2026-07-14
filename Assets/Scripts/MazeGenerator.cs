using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 11;
    public int height = 11;

    [Header("Traps")]
    public int spikeCount = 5;
    public Color spikeColor = new Color(0.9f, 0.2f, 0.2f);

    [Header("Colors")]
    public Color wallColor = new Color(0.25f, 0.25f, 0.3f);
    public Color floorColor = new Color(0.9f, 0.88f, 0.82f);
    public Color exitColor = new Color(0.2f, 0.8f, 0.3f);

    // 0 = wall, 1 = floor
    int[,] _grid;
    Vector2Int _entrance;
    Vector2Int _exit;

    public Vector2Int Entrance => _entrance;
    public Vector2Int Exit => _exit;

    public void Generate()
    {
        // Ensure odd dimensions for the algorithm
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        _grid = new int[width, height];

        // Start all as walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _grid[x, y] = 0;

        // Carve maze using DFS
        CarveMaze(1, 1);

        // Pick random entrance and exit with maximum path distance
        PickEntranceAndExit();
    }

    void CarveMaze(int startX, int startY)
    {
        var stack = new Stack<Vector2Int>();
        _grid[startX, startY] = 1;
        stack.Push(new Vector2Int(startX, startY));

        // Directions: up, down, left, right (step of 2)
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

                if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && _grid[nx, ny] == 0)
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

                _grid[wallX, wallY] = 1;
                _grid[nextX, nextY] = 1;

                stack.Push(new Vector2Int(nextX, nextY));
            }
            else
            {
                stack.Pop();
            }
        }
    }

    void PickEntranceAndExit()
    {
        // Collect all floor tiles
        var floorTiles = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (_grid[x, y] == 1)
                    floorTiles.Add(new Vector2Int(x, y));

        // Try multiple random pairs, keep the one with longest path
        Vector2Int bestEntrance = floorTiles[0];
        Vector2Int bestExit = floorTiles[floorTiles.Count - 1];
        int bestDist = 0;

        int attempts = 50;
        for (int i = 0; i < attempts; i++)
        {
            var a = floorTiles[Random.Range(0, floorTiles.Count)];
            var b = floorTiles[Random.Range(0, floorTiles.Count)];
            if (a == b) continue;

            int dist = BFSDistance(a, b);
            if (dist > bestDist)
            {
                bestDist = dist;
                bestEntrance = a;
                bestExit = b;
            }
        }

        _entrance = bestEntrance;
        _exit = bestExit;
    }

    int BFSDistance(Vector2Int start, Vector2Int end)
    {
        var visited = new bool[width, height];
        var queue = new Queue<(Vector2Int pos, int dist)>();
        queue.Enqueue((start, 0));
        visited[start.x, start.y] = true;

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            var (pos, dist) = queue.Dequeue();
            if (pos == end) return dist;

            foreach (var dir in dirs)
            {
                int nx = pos.x + dir.x;
                int ny = pos.y + dir.y;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (visited[nx, ny] || _grid[nx, ny] == 0) continue;

                visited[nx, ny] = true;
                queue.Enqueue((new Vector2Int(nx, ny), dist + 1));
            }
        }

        return 0;
    }

    public void BuildVisuals()
    {
        Sprite square = CreateSquareSprite();
        Transform mazeParent = new GameObject("Maze").transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.parent = mazeParent;
                cell.transform.position = new Vector3(x, y, 0);

                var sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sortingOrder = 0;

                if (_grid[x, y] == 0)
                {
                    // Wall
                    sr.color = wallColor;
                    var col = cell.AddComponent<BoxCollider2D>();
                    cell.layer = LayerMask.NameToLayer("Default");
                }
                else
                {
                    // Floor
                    sr.color = floorColor;
                }
            }
        }

        // Create exit marker
        GameObject exitObj = new GameObject("Exit");
        exitObj.transform.parent = mazeParent;
        exitObj.transform.position = new Vector3(_exit.x, _exit.y, 0);

        var exitSr = exitObj.AddComponent<SpriteRenderer>();
        exitSr.sprite = square;
        exitSr.color = exitColor;
        exitSr.sortingOrder = 1;

        var exitCol = exitObj.AddComponent<BoxCollider2D>();
        exitCol.isTrigger = true;
        exitCol.size = Vector2.one * 0.5f;

        exitObj.AddComponent<ExitDoor>();

        // Place spike traps on random floor tiles
        PlaceSpikes(mazeParent, square);
    }

    HashSet<Vector2Int> FindShortestPath()
    {
        var path = new HashSet<Vector2Int>();
        var visited = new bool[width, height];
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();

        queue.Enqueue(_entrance);
        visited[_entrance.x, _entrance.y] = true;

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == _exit)
            {
                // Trace back the path
                var node = _exit;
                while (node != _entrance)
                {
                    path.Add(node);
                    node = parent[node];
                }
                path.Add(_entrance);
                return path;
            }

            foreach (var dir in dirs)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (visited[nx, ny] || _grid[nx, ny] == 0) continue;

                visited[nx, ny] = true;
                parent[new Vector2Int(nx, ny)] = current;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return path;
    }

    void PlaceSpikes(Transform parent, Sprite square)
    {
        var safePath = FindShortestPath();
        var floorTiles = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y] != 1) continue;
                Vector2Int pos = new Vector2Int(x, y);
                if (pos == _entrance || pos == _exit) continue;
                // Don't place on the shortest path
                if (safePath.Contains(pos)) continue;
                // Don't place adjacent to entrance
                if (Mathf.Abs(x - _entrance.x) + Mathf.Abs(y - _entrance.y) <= 2) continue;
                floorTiles.Add(pos);
            }
        }

        // Shuffle and pick
        int count = Mathf.Min(spikeCount, floorTiles.Count);
        for (int i = 0; i < count; i++)
        {
            int rand = Random.Range(i, floorTiles.Count);
            (floorTiles[i], floorTiles[rand]) = (floorTiles[rand], floorTiles[i]);
        }

        for (int i = 0; i < count; i++)
        {
            Vector2Int pos = floorTiles[i];

            GameObject spike = new GameObject($"Spike_{pos.x}_{pos.y}");
            spike.transform.parent = parent;
            spike.transform.position = new Vector3(pos.x, pos.y, 0);

            var sr = spike.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = spikeColor;
            sr.sortingOrder = 1;
            spike.transform.localScale = Vector3.one * 0.6f;

            var col = spike.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one * 0.8f;

            spike.AddComponent<SpikeTrap>();
        }
    }

    public bool IsWall(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return true;
        return _grid[x, y] == 0;
    }

    Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }
}
