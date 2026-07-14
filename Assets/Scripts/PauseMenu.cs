using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    GameObject _panel;
    bool _paused;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
            Toggle();
    }

    void Toggle()
    {
        _paused = !_paused;

        if (_panel == null)
            CreateUI();

        _panel.SetActive(_paused);
        Time.timeScale = _paused ? 0f : 1f;
    }

    void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("PauseCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark overlay
        _panel = new GameObject("PausePanel");
        _panel.transform.SetParent(canvasObj.transform, false);

        var panelImg = _panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);

        var panelRect = panelImg.rectTransform;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Title
        CreateText(_panel.transform, "PAUSED", 72, Color.white, new Vector2(0, 120));

        // Buttons
        CreateButton(_panel.transform, "Resume", new Vector2(0, 20), OnResume);
        CreateButton(_panel.transform, "Restart", new Vector2(0, -60), OnRestart);
        CreateButton(_panel.transform, "Quit", new Vector2(0, -140), OnQuit);
    }

    void CreateText(Transform parent, string text, float fontSize, Color color, Vector2 pos)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        var rect = tmp.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(400, 80);
    }

    void CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(label + "Button");
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.3f);

        var rect = btnImg.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(300, 60);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // Hover colors
        var colors = btn.colors;
        colors.normalColor = new Color(0.25f, 0.25f, 0.3f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.2f);
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var textRect = tmp.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    void OnResume()
    {
        Toggle();
    }

    void OnRestart()
    {
        Time.timeScale = 1f;
        LevelManager.Reset();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnQuit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
