using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 11;
    public int height = 11;

    [Header("Traps")]
    public int spikeCount = 5;
    public int beamCount = 2;
    public Color spikeColor = new Color(0.9f, 0.2f, 0.2f);
    public Color beamColor = new Color(0.9f, 0.5f, 0.1f);

    [Header("Colors")]
    public Color wallColor = new Color(0.25f, 0.25f, 0.3f);
    public Color floorColor = new Color(0.9f, 0.88f, 0.82f);
    public Color exitColor = new Color(0.2f, 0.8f, 0.3f);

    // 0 = wall, 1 = floor
    public int[,] Grid => _grid;
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

        // Exit door emits a faint glow — only visible when nearby
        var exitLight = exitObj.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        exitLight.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
        exitLight.pointLightOuterRadius = 1.8f;
        exitLight.pointLightInnerRadius = 0.3f;
        exitLight.intensity = 0.4f;
        exitLight.color = new Color(0.3f, 0.9f, 0.4f); // faint green glow
        exitLight.falloffIntensity = 0.7f;

        // Place traps: beams first, then spikes avoiding beam areas
        var safePath = FindShortestPath();
        var beamTiles = PlaceBeams(mazeParent, square, safePath);
        PlaceSpikes(mazeParent, square, safePath, beamTiles);
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

    void PlaceSpikes(Transform parent, Sprite square, HashSet<Vector2Int> safePath, HashSet<Vector2Int> beamTiles)
    {
        int minSpacing = 3;
        var candidates = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y] != 1) continue;
                Vector2Int pos = new Vector2Int(x, y);
                if (pos == _entrance || pos == _exit) continue;
                if (safePath.Contains(pos)) continue;
                if (beamTiles.Contains(pos)) continue;
                if (Mathf.Abs(x - _entrance.x) + Mathf.Abs(y - _entrance.y) <= 2) continue;
                candidates.Add(pos);
            }
        }

        // Shuffle candidates
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (candidates[i], candidates[rand]) = (candidates[rand], candidates[i]);
        }

        // Pick spikes with minimum spacing between each other
        var placed = new List<Vector2Int>();
        foreach (var pos in candidates)
        {
            if (placed.Count >= spikeCount) break;

            bool tooClose = false;
            foreach (var existing in placed)
            {
                if (Mathf.Abs(pos.x - existing.x) + Mathf.Abs(pos.y - existing.y) < minSpacing)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            placed.Add(pos);

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

    HashSet<Vector2Int> PlaceBeams(Transform parent, Sprite square, HashSet<Vector2Int> safePath)
    {
        var beamTiles = new HashSet<Vector2Int>();
        int minDistFromEntrance = 5;
        int minSpacingBetween = 6;

        // Collect candidate floor tiles (not entrance, not exit, not too close to start)
        var candidates = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (_grid[x, y] != 1) continue;
                Vector2Int pos = new Vector2Int(x, y);
                if (pos == _entrance || pos == _exit) continue;
                if (Mathf.Abs(x - _entrance.x) + Mathf.Abs(y - _entrance.y) < minDistFromEntrance) continue;
                candidates.Add(pos);
            }
        }

        // Shuffle
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (candidates[i], candidates[rand]) = (candidates[rand], candidates[i]);
        }

        // Pick with spacing
        var placed = new List<Vector2Int>();
        foreach (var pos in candidates)
        {
            if (placed.Count >= beamCount) break;

            bool tooClose = false;
            foreach (var existing in placed)
            {
                if (Mathf.Abs(pos.x - existing.x) + Mathf.Abs(pos.y - existing.y) < minSpacingBetween)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            placed.Add(pos);
            beamTiles.Add(pos);

            CreateGlareSpirit(parent, pos);
        }

        return beamTiles;
    }

    void CreateGlareSpirit(Transform parent, Vector2Int tile)
    {
        float beamLength = 3f;
        Sprite beamSprite = CreateBeamSprite();

        GameObject spiritObj = new GameObject($"GlareSpirit_{tile.x}_{tile.y}");
        spiritObj.transform.parent = parent;
        spiritObj.transform.position = new Vector3(tile.x, tile.y, 0);

        // Kinematic Rigidbody2D for movement and compound collider
        var rb = spiritObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Eye body — glowing circle
        GameObject eyeObj = new GameObject("EyeBody");
        eyeObj.transform.SetParent(spiritObj.transform, false);
        eyeObj.transform.localScale = Vector3.one * 0.35f;
        var eyeSr = eyeObj.AddComponent<SpriteRenderer>();
        eyeSr.sprite = CreateCircleSprite();
        eyeSr.color = new Color(0.35f, 0.3f, 0.25f, 0.9f); // dark, only visible when lit
        eyeSr.sortingOrder = 3;

        // Eye body trigger — contact with this starts the chase
        var eyeCol = eyeObj.AddComponent<CircleCollider2D>();
        eyeCol.isTrigger = true;
        eyeCol.radius = 1.2f;

        // Pupil — slightly lighter so it contrasts when lit
        GameObject pupilObj = new GameObject("Pupil");
        pupilObj.transform.SetParent(eyeObj.transform, false);
        pupilObj.transform.localScale = Vector3.one * 0.4f;
        var pupilSr = pupilObj.AddComponent<SpriteRenderer>();
        pupilSr.sprite = CreateCircleSprite();
        pupilSr.color = new Color(0.15f, 0.08f, 0.08f);
        pupilSr.sortingOrder = 4;

        // Beam cone visual — dim, only revealed by player's light
        GameObject coneObj = new GameObject("BeamCone");
        coneObj.transform.SetParent(spiritObj.transform, false);
        coneObj.transform.localPosition = Vector3.zero;
        coneObj.transform.localScale = new Vector3(0.6f, beamLength, 1f);

        var coneSr = coneObj.AddComponent<SpriteRenderer>();
        coneSr.sprite = beamSprite;
        coneSr.color = new Color(0.4f, 0.25f, 0.15f, 0.2f); // nearly invisible in darkness
        coneSr.sortingOrder = 1;

        // BeamTrap handles movement, rotation, and damage
        var trap = spiritObj.AddComponent<BeamTrap>();
        trap.Setup(_grid, width, height, tile);
        trap.SetPupil(pupilSr);
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

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                colors[y * size + x] = dist < radius - 1 ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    Sprite CreateBeamSprite()
    {
        int w = 16, h = 64;
        Texture2D tex = new Texture2D(w, h);
        Color[] colors = new Color[w * h];

        float centerX = w / 2f;

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            // Beam widens from narrow base to wider tip
            float halfWidth = Mathf.Lerp(1f, centerX * 0.8f, t);
            // Fades toward the tip
            float alphaY = 1f - t * 0.6f;

            for (int x = 0; x < w; x++)
            {
                float distFromCenter = Mathf.Abs(x - centerX);
                if (distFromCenter <= halfWidth)
                {
                    float edgeFade = 1f - (distFromCenter / halfWidth) * 0.6f;
                    colors[y * w + x] = new Color(1f, 1f, 1f, alphaY * edgeFade);
                }
                else
                {
                    colors[y * w + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        // Pivot at bottom-center so beam extends upward from emitter
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16);
    }
}
