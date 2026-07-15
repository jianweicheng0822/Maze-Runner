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
    bool _gameOver;
    TextMeshProUGUI _winText;
    TextMeshProUGUI _deathText;
    TextMeshProUGUI _levelText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Apply level settings to maze generator
        _maze = GetComponent<MazeGenerator>();
        _maze.width = LevelManager.MazeSize;
        _maze.height = LevelManager.MazeSize;
        _maze.spikeCount = LevelManager.SpikeCount;
        _maze.beamCount = LevelManager.BeamCount;
        _maze.alcoveCount = LevelManager.AlcoveCount;
        _maze.loopPercent = LevelManager.LoopPercent;
        _maze.spiritChaseSpeed = LevelManager.ChaseSpeed;
        _maze.spiritDrainRate = LevelManager.DrainRate;

        _maze.Generate();
        _maze.BuildVisuals();

        SpawnPlayer();
        CreateUI();

        // Ensure there is an AudioListener in the scene
        if (FindAnyObjectByType<AudioListener>() == null)
            Camera.main.gameObject.AddComponent<AudioListener>();

        gameObject.AddComponent<PauseMenu>();
        gameObject.AddComponent<AudioManager>();
    }

    void SpawnPlayer()
    {
        Vector2Int entrance = _maze.Entrance;

        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(entrance.x, entrance.y, 0);

        // Sprite
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadTransparentSprite("Sprites/bobo", 128);
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

        // Map tracker
        var mapTracker = gameObject.AddComponent<MapTracker>();
        mapTracker.Setup(_maze.Grid, _maze.width, _maze.height,
            _maze.Entrance, _maze.Exit, player.transform);

        // Fog of war with level-scaled values
        var fog = GetComponent<FogOfWar>();
        if (fog != null)
        {
            fog.SetLevelParams(LevelManager.PointLightRadius, LevelManager.FlashlightRange);
            fog.Setup(player);
        }

        // Light trail
        var lightTrail = gameObject.AddComponent<LightTrail>();
        lightTrail.Setup(player.transform);

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

        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Level indicator (top right)
        _levelText = CreateText(canvasObj.transform, $"Level {LevelManager.CurrentLevel}",
            Color.white, 28);
        var lvlRect = _levelText.rectTransform;
        lvlRect.anchorMin = new Vector2(1, 1);
        lvlRect.anchorMax = new Vector2(1, 1);
        lvlRect.pivot = new Vector2(1, 1);
        lvlRect.anchoredPosition = new Vector2(-30, -30);
        lvlRect.sizeDelta = new Vector2(200, 40);
        _levelText.alignment = TextAlignmentOptions.Right;

        // Win text (center, hidden)
        _winText = CreateText(canvasObj.transform,
            $"LEVEL {LevelManager.CurrentLevel} COMPLETE!\n<size=32>Next level...</size>",
            new Color(0.2f, 0.8f, 0.3f), 64);
        _winText.gameObject.SetActive(false);

        // Death text (center, hidden)
        _deathText = CreateText(canvasObj.transform,
            "YOUR LIGHT FADED\n<size=32>Restarting...</size>",
            new Color(0.9f, 0.2f, 0.2f), 64);
        _deathText.gameObject.SetActive(false);
    }

    TextMeshProUGUI CreateText(Transform parent, string text, Color color, float fontSize)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
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
        if (_gameOver) return;
        _gameOver = true;
        _levelComplete = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLevelComplete();

        if (LevelManager.CurrentLevel >= LevelManager.MaxLevel)
        {
            _winText.text = "YOU ESCAPED THE MAZE!\n<size=32>Congratulations!</size>";
            _winText.gameObject.SetActive(true);
            FreezePlayer();
            LevelManager.Reset();
            Invoke(nameof(RestartLevel), 3f);
        }
        else
        {
            _winText.gameObject.SetActive(true);
            FreezePlayer();
            Invoke(nameof(GoToNextLevel), 1.5f);
        }
    }

    void GoToNextLevel()
    {
        LevelManager.NextLevel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void HandlePlayerDied()
    {
        if (_gameOver) return;
        _gameOver = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeath();

        _deathText.gameObject.SetActive(true);
        FreezePlayer();

        LevelManager.Reset();
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

    // Load a sprite from a .bytes file in Resources and strip white background
    Sprite LoadTransparentSprite(string path, float pixelsPerUnit)
    {
        // Load as TextAsset (.bytes) for full read/write pixel access
        var asset = Resources.Load<TextAsset>(path + ".png");
        if (asset == null)
        {
            Debug.LogWarning($"Sprite not found: {path}.png.bytes, using fallback circle");
            return CreateFallbackCircle();
        }

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(tex, asset.bytes);

        // Strip white background
        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].r > 0.9f && pixels[i].g > 0.9f && pixels[i].b > 0.9f)
                pixels[i] = Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    Sprite CreateFallbackCircle()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        Color[] colors = new Color[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                colors[y * size + x] = dist < radius - 1 ? Color.white : Color.clear;
            }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
