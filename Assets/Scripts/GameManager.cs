using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;

    [Header("Colors")]
    public Color cellColorA = new Color(0.85f, 0.85f, 0.85f);
    public Color cellColorB = new Color(0.75f, 0.75f, 0.75f);
    public Color playerColor = new Color(0.2f, 0.5f, 1f);
    public Color keyColor = new Color(1f, 0.85f, 0f);

    public Vector2Int KeyPosition { get; private set; }

    bool _won;
    TextMeshProUGUI _winText;
    Canvas _canvas;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateGrid();
        SpawnKey();
        SpawnPlayer();
        SetupCamera();
        CreateWinUI();
    }

    void CreateGrid()
    {
        Sprite square = CreateSquareSprite();
        Transform gridParent = new GameObject("Grid").transform;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.parent = gridParent;
                cell.transform.position = GridToWorld(x, y);

                var sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.color = (x + y) % 2 == 0 ? cellColorA : cellColorB;
                sr.sortingOrder = 0;
            }
        }
    }

    void SpawnPlayer()
    {
        Sprite square = CreateSquareSprite();

        GameObject player = new GameObject("Player");
        player.transform.position = GridToWorld(0, 0);

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = playerColor;
        sr.sortingOrder = 1;
        // Make the player slightly smaller than a cell
        player.transform.localScale = Vector3.one * 0.8f;

        player.AddComponent<PlayerController>();
    }

    void SpawnKey()
    {
        Sprite square = CreateSquareSprite();

        // Place key away from player start (0,0)
        int kx, ky;
        do
        {
            kx = Random.Range(0, gridWidth);
            ky = Random.Range(0, gridHeight);
        } while (kx + ky < gridWidth / 2); // Ensure some minimum distance

        KeyPosition = new Vector2Int(kx, ky);

        GameObject key = new GameObject("Key");
        key.transform.position = GridToWorld(kx, ky);
        // Rotate 45 degrees to make a diamond shape
        key.transform.rotation = Quaternion.Euler(0, 0, 45);
        key.transform.localScale = Vector3.one * 0.5f;

        var sr = key.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = keyColor;
        sr.sortingOrder = 1;
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = true;
        // Center camera on the grid
        float centerX = (gridWidth - 1) * cellSize / 2f;
        float centerY = (gridHeight - 1) * cellSize / 2f;
        cam.transform.position = new Vector3(centerX, centerY, -10);
        // Size to fit the grid with some padding
        cam.orthographicSize = Mathf.Max(gridWidth, gridHeight) * cellSize / 2f + 1f;
    }

    void CreateWinUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("WinCanvas");
        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        // Create win text (hidden)
        GameObject textObj = new GameObject("WinText");
        textObj.transform.SetParent(canvasObj.transform, false);

        _winText = textObj.AddComponent<TextMeshProUGUI>();
        _winText.text = "YOU WIN!";
        _winText.fontSize = 72;
        _winText.color = new Color(1f, 0.85f, 0f);
        _winText.alignment = TextAlignmentOptions.Center;
        _winText.fontStyle = FontStyles.Bold;

        var rect = _winText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(600, 150);

        textObj.SetActive(false);
    }

    public void OnKeyCollected()
    {
        if (_won) return;
        _won = true;
        _winText.gameObject.SetActive(true);

        // Destroy the key object
        GameObject key = GameObject.Find("Key");
        if (key != null) Destroy(key);
    }

    public bool IsWithinGrid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x * cellSize, y * cellSize, 0);
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
