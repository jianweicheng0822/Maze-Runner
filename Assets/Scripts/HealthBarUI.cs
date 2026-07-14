using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    Image _fillImage;
    Image _bgImage;
    PlayerHealth _playerHealth;

    public void Setup(PlayerHealth health)
    {
        _playerHealth = health;
        _playerHealth.HealthChanged += UpdateBar;

        CreateHealthBar();
        UpdateBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
    }

    void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.HealthChanged -= UpdateBar;
    }

    void CreateHealthBar()
    {
        // Canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Background bar
        GameObject bgObj = new GameObject("HealthBarBG");
        bgObj.transform.SetParent(canvasObj.transform, false);

        _bgImage = bgObj.AddComponent<Image>();
        _bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var bgRect = _bgImage.rectTransform;
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(30, -30);
        bgRect.sizeDelta = new Vector2(250, 25);

        // Fill bar
        GameObject fillObj = new GameObject("HealthBarFill");
        fillObj.transform.SetParent(bgObj.transform, false);

        _fillImage = fillObj.AddComponent<Image>();
        _fillImage.color = new Color(0.8f, 0.2f, 0.2f);

        var fillRect = _fillImage.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.offsetMin = new Vector2(3, 3);
        fillRect.offsetMax = new Vector2(-3, -3);

        // Health text
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(bgObj.transform, false);

        var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "HP";
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Left;
        tmp.fontStyle = TMPro.FontStyles.Bold;

        var textRect = tmp.rectTransform;
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(0, 1);
        textRect.pivot = new Vector2(0, 0);
        textRect.anchoredPosition = new Vector2(0, 4);
        textRect.sizeDelta = new Vector2(100, 20);
    }

    void UpdateBar(int current, int max)
    {
        if (_fillImage == null) return;

        float ratio = (float)current / max;
        _fillImage.rectTransform.anchorMax = new Vector2(ratio, 1);
        _fillImage.rectTransform.offsetMax = new Vector2(-3, -3);

        // Change color based on health
        if (ratio > 0.5f)
            _fillImage.color = new Color(0.2f, 0.8f, 0.3f);
        else if (ratio > 0.25f)
            _fillImage.color = new Color(0.9f, 0.7f, 0.1f);
        else
            _fillImage.color = new Color(0.8f, 0.2f, 0.2f);
    }
}
