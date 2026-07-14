using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 11;
    public int height = 11;

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

        // Set entrance (top-left area) and exit (bottom-right area)
        _entrance = new Vector2Int(1, height - 2);
        _exit = new Vector2Int(width - 2, 1);
        _grid[_entrance.x, _entrance.y] = 1;
        _grid[_exit.x, _exit.y] = 1;
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
