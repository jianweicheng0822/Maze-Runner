using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Colors")]
    public Color playerColor = new Color(0.2f, 0.5f, 1f);

    MazeGenerator _maze;
    bool _levelComplete;
    TextMeshProUGUI _winText;
    TextMeshProUGUI _deathText;

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
        CreateUI();
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

        // Collider
        var col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        // Movement
        player.AddComponent<PlayerMovement>();

        // Health
        var health = player.AddComponent<PlayerHealth>();
        health.PlayerDied += HandlePlayerDied;

        // Health bar UI
        var healthBar = gameObject.AddComponent<HealthBarUI>();
        healthBar.Setup(health);

        // Fog of war
        var fog = GetComponent<FogOfWar>();
        if (fog != null) fog.Setup(player);

        // Camera follow
        var camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.SetTarget(player.transform);
            Camera.main.transform.position = new Vector3(entrance.x, entrance.y, -10);
        }
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("UICanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        // Win text
        _winText = CreateCenterText(canvasObj.transform, "LEVEL COMPLETE!",
            new Color(0.2f, 0.8f, 0.3f));
        _winText.gameObject.SetActive(false);

        // Death text
        _deathText = CreateCenterText(canvasObj.transform, "YOU DIED\n<size=32>Restarting...</size>",
            new Color(0.9f, 0.2f, 0.2f));
        _deathText.rectTransform.anchoredPosition = Vector2.zero;
        _deathText.gameObject.SetActive(false);
    }

    TextMeshProUGUI CreateCenterText(Transform parent, string text, Color color)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 64;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        var rect = tmp.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700, 200);

        return tmp;
    }

    public void OnLevelComplete()
    {
        if (_levelComplete) return;
        _levelComplete = true;

        _winText.gameObject.SetActive(true);
        FreezePlayer();
    }

    void HandlePlayerDied()
    {
        if (_levelComplete) return;

        _deathText.gameObject.SetActive(true);
        FreezePlayer();

        Invoke(nameof(RestartLevel), 1.5f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void FreezePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
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
