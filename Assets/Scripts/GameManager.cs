using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Colors")]
    public Color playerColor = new Color(0.2f, 0.5f, 1f);

    MazeGenerator _maze;
    bool _levelComplete;
    TextMeshProUGUI _winText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _maze = GetComponent<MazeGenerator>();
        _maze.Generate();
        _maze.BuildVisuals();

        SpawnPlayer();
        CreateWinUI();
    }

    void SpawnPlayer()
    {
        Vector2Int entrance = _maze.Entrance;

        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(entrance.x, entrance.y, 0);

        // Sprite
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = playerColor;
        sr.sortingOrder = 2;
        player.transform.localScale = Vector3.one * 0.7f;

        // Physics
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Collider (circle, slightly smaller than sprite)
        var col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        // Movement script
        player.AddComponent<PlayerMovement>();

        // Camera follow
        var camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.SetTarget(player.transform);
            // Snap camera to player immediately
            Camera.main.transform.position = new Vector3(entrance.x, entrance.y, -10);
        }
    }

    void CreateWinUI()
    {
        GameObject canvasObj = new GameObject("WinCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        GameObject textObj = new GameObject("WinText");
        textObj.transform.SetParent(canvasObj.transform, false);

        _winText = textObj.AddComponent<TextMeshProUGUI>();
        _winText.text = "LEVEL COMPLETE!";
        _winText.fontSize = 64;
        _winText.color = new Color(0.2f, 0.8f, 0.3f);
        _winText.alignment = TextAlignmentOptions.Center;
        _winText.fontStyle = FontStyles.Bold;

        var rect = _winText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700, 150);

        textObj.SetActive(false);
    }

    public void OnLevelComplete()
    {
        if (_levelComplete) return;
        _levelComplete = true;

        _winText.gameObject.SetActive(true);

        // Freeze player
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;
        }
    }

    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        Color[] colors = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                colors[y * size + x] = dist < radius - 1 ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
