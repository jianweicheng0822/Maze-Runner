using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FogOfWar : MonoBehaviour
{
    [Header("Ambient")]
    [SerializeField] float ambientIntensity = 0.02f;

    [Header("Player Light (nearby glow)")]
    [SerializeField] float pointLightRadius = 2.5f;
    [SerializeField] float pointLightIntensity = 0.6f;

    [Header("Flashlight (forward cone)")]
    [SerializeField] float flashlightRange = 5f;
    [SerializeField] float flashlightWidth = 2f;
    [SerializeField] float flashlightIntensity = 0.8f;

    Transform _flashlightTransform;
    PlayerMovement _playerMovement;

    public void Setup(GameObject player)
    {
        _playerMovement = player.GetComponent<PlayerMovement>();

        CreateGlobalLight();
        CreatePointLight(player.transform);
        CreateFlashlight(player.transform);
    }

    void CreateGlobalLight()
    {
        GameObject obj = new GameObject("GlobalLight");
        var light = obj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = ambientIntensity;
        light.color = new Color(0.4f, 0.4f, 0.6f);
    }

    void CreatePointLight(Transform player)
    {
        GameObject obj = new GameObject("PlayerLight");
        obj.transform.SetParent(player, false);
        obj.transform.localPosition = Vector3.zero;

        var light = obj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.pointLightOuterRadius = pointLightRadius;
        light.pointLightInnerRadius = pointLightRadius * 0.3f;
        light.intensity = pointLightIntensity;
        light.color = new Color(1f, 0.95f, 0.8f);
        light.falloffIntensity = 0.5f;
    }

    void CreateFlashlight(Transform player)
    {
        // Use a Point Light shaped as an elongated cone via inner/outer radius
        // For a true cone we use Freeform, but Point with offset works well enough
        GameObject obj = new GameObject("Flashlight");
        obj.transform.SetParent(player, false);
        obj.transform.localPosition = Vector3.zero;

        _flashlightTransform = obj.transform;

        // Create a freeform light shaped like a cone
        var light = obj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Freeform;
        light.intensity = flashlightIntensity;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.falloffIntensity = 0.5f;

        // Set cone shape: triangle pointing up (will be rotated by script)
        Vector3[] shapePath = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(-flashlightWidth / 2f, flashlightRange, 0f),
            new Vector3(flashlightWidth / 2f, flashlightRange, 0f)
        };
        light.SetShapePath(shapePath);
    }

    void Update()
    {
        if (_flashlightTransform == null || _playerMovement == null) return;

        Vector2 dir = _playerMovement.LastMoveDirection;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _flashlightTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}
