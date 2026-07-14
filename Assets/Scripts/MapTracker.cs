using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapTracker : MonoBehaviour
{
    [SerializeField] float revealRadius = 2.5f;

    bool[,] _explored;
    int _width;
    int _height;
    int[,] _grid;
    Transform _player;
    Vector2Int _entrance;
    Vector2Int _exit;

    // UI
    GameObject _mapPanel;
    RawImage _mapImage;
    Texture2D _mapTexture;
    bool _mapOpen;

    public void Setup(int[,] grid, int width, int height, Vector2Int entrance, Vector2Int exit, Transform player)
    {
        _grid = grid;
        _width = width;
        _height = height;
        _entrance = entrance;
        _exit = exit;
        _player = player;

        _explored = new bool[width, height];
        CreateMapUI();
    }

    void Update()
    {
        if (_player == null) return;

        TrackExplored();

        var kb = Keyboard.current;
        if (kb != null && kb.mKey.wasPressedThisFrame)
            ToggleMap();
    }

    void TrackExplored()
    {
        int px = Mathf.RoundToInt(_player.position.x);
        int py = Mathf.RoundToInt(_player.position.y);
        int r = Mathf.CeilToInt(revealRadius);

        for (int x = px - r; x <= px + r; x++)
        {
            for (int y = py - r; y <= py + r; y++)
            {
                if (x < 0 || x >= _width || y < 0 || y >= _height) continue;
                float dist = Vector2.Distance(new Vector2(px, py), new Vector2(x, y));
                if (dist <= revealRadius)
                    _explored[x, y] = true;
            }
        }
    }

    void ToggleMap()
    {
        _mapOpen = !_mapOpen;
        _mapPanel.SetActive(_mapOpen);

        if (_mapOpen)
        {
            Time.timeScale = 0f;
            RenderMap();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    void CreateMapUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("MapCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark background panel
        _mapPanel = new GameObject("MapPanel");
        _mapPanel.transform.SetParent(canvasObj.transform, false);

        var panelImg = _mapPanel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.85f);

        var panelRect = panelImg.rectTransform;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Map image
        GameObject imgObj = new GameObject("MapImage");
        imgObj.transform.SetParent(_mapPanel.transform, false);

        _mapImage = imgObj.AddComponent<RawImage>();

        var imgRect = _mapImage.rectTransform;
        imgRect.anchorMin = new Vector2(0.5f, 0.5f);
        imgRect.anchorMax = new Vector2(0.5f, 0.5f);
        // Scale to fit screen with padding
        float mapPixelSize = Mathf.Min(800, 800);
        imgRect.sizeDelta = new Vector2(mapPixelSize, mapPixelSize);

        // Title text
        GameObject titleObj = new GameObject("MapTitle");
        titleObj.transform.SetParent(_mapPanel.transform, false);

        var title = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        title.text = "MAP  <size=24>(Press M to close)</size>";
        title.fontSize = 36;
        title.color = Color.white;
        title.alignment = TMPro.TextAlignmentOptions.Center;
        title.fontStyle = TMPro.FontStyles.Bold;

        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(600, 50);

        // Create texture
        _mapTexture = new Texture2D(_width, _height);
        _mapTexture.filterMode = FilterMode.Point;
        _mapImage.texture = _mapTexture;

        _mapPanel.SetActive(false);
    }

    void RenderMap()
    {
        int px = Mathf.RoundToInt(_player.position.x);
        int py = Mathf.RoundToInt(_player.position.y);

        Color unexplored = new Color(0.1f, 0.1f, 0.1f);
        Color wall = new Color(0.3f, 0.3f, 0.35f);
        Color floor = new Color(0.6f, 0.58f, 0.52f);
        Color playerDot = new Color(0.2f, 0.5f, 1f);
        Color exitDot = new Color(0.2f, 0.8f, 0.3f);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Color c;

                if (!_explored[x, y])
                {
                    c = unexplored;
                }
                else if (_grid[x, y] == 0)
                {
                    c = wall;
                }
                else
                {
                    c = floor;
                }

                // Player position
                if (x == px && y == py)
                    c = playerDot;

                // Exit (only if explored)
                if (x == _exit.x && y == _exit.y && _explored[x, y])
                    c = exitDot;

                _mapTexture.SetPixel(x, y, c);
            }
        }

        _mapTexture.Apply();
    }

    void OnDestroy()
    {
        // Ensure time scale is restored
        Time.timeScale = 1f;
    }
}
