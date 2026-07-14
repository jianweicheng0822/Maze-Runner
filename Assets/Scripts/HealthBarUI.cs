using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] float barWidth = 0.8f;
    [SerializeField] float barHeight = 0.08f;
    [SerializeField] float offsetY = 0.55f;

    SpriteRenderer _bgRenderer;
    SpriteRenderer _fillRenderer;
    Transform _fillTransform;
    PlayerHealth _playerHealth;

    public void Setup(PlayerHealth health)
    {
        _playerHealth = health;
        _playerHealth.HealthChanged += UpdateBar;

        CreateHealthBar(health.transform);
    }

    void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.HealthChanged -= UpdateBar;
    }

    void CreateHealthBar(Transform player)
    {
        Sprite square = CreateSquareSprite();

        // Container that follows player
        GameObject container = new GameObject("HealthBar");
        container.transform.SetParent(player, false);
        container.transform.localPosition = new Vector3(0, offsetY, 0);

        // Background (dark)
        GameObject bgObj = new GameObject("BG");
        bgObj.transform.SetParent(container.transform, false);
        bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        _bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        _bgRenderer.sprite = square;
        _bgRenderer.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        _bgRenderer.sortingOrder = 10;

        // Fill (colored)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(container.transform, false);
        fillObj.transform.localPosition = new Vector3(-barWidth / 2f, 0, 0);
        fillObj.transform.localScale = new Vector3(barWidth, barHeight * 0.7f, 1f);
        _fillTransform = fillObj.transform;

        _fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite = square;
        _fillRenderer.color = new Color(0.2f, 0.8f, 0.3f);
        _fillRenderer.sortingOrder = 11;
    }

    void UpdateBar(int current, int max)
    {
        if (_fillRenderer == null) return;

        float ratio = (float)current / max;

        // Scale the fill bar horizontally
        var scale = _fillTransform.localScale;
        scale.x = barWidth * ratio;
        _fillTransform.localScale = scale;

        // Keep fill anchored to the left
        var pos = _fillTransform.localPosition;
        pos.x = -barWidth / 2f + (barWidth * ratio) / 2f;
        _fillTransform.localPosition = pos;

        // Color based on health
        if (ratio > 0.5f)
            _fillRenderer.color = new Color(0.2f, 0.8f, 0.3f);
        else if (ratio > 0.25f)
            _fillRenderer.color = new Color(0.9f, 0.7f, 0.1f);
        else
            _fillRenderer.color = new Color(0.8f, 0.2f, 0.2f);
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
