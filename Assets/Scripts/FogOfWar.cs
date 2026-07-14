using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FogOfWar : MonoBehaviour
{
    [Header("Ambient")]
    [SerializeField] float ambientIntensity = 0f;

    [Header("Player Light (nearby glow)")]
    [SerializeField] float pointLightRadius = 2.5f;
    [SerializeField] float pointLightIntensity = 0.6f;

    [Header("Flashlight (forward cone)")]
    [SerializeField] float flashlightRange = 5f;
    [SerializeField] float flashlightWidth = 2f;
    [SerializeField] float flashlightIntensity = 0.8f;

    Transform _flashlightTransform;
    PlayerMovement _playerMovement;
    PlayerHealth _playerHealth;

    Light2D _pointLight;
    Light2D _flashlight;
    float _basePointRadius;
    float _baseFlashRange;
    float _baseFlashWidth;
    float _brightnessMultiplier = 1f;

    public void SetLevelParams(float pointRadius, float flashRange)
    {
        pointLightRadius = pointRadius;
        flashlightRange = flashRange;
    }

    public void Setup(GameObject player)
    {
        _playerMovement = player.GetComponent<PlayerMovement>();
        _playerHealth = player.GetComponent<PlayerHealth>();

        _basePointRadius = pointLightRadius;
        _baseFlashRange = flashlightRange;
        _baseFlashWidth = flashlightWidth;

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

        _pointLight = light;
    }

    void CreateFlashlight(Transform player)
    {
        GameObject obj = new GameObject("Flashlight");
        obj.transform.SetParent(player, false);
        obj.transform.localPosition = Vector3.zero;

        _flashlightTransform = obj.transform;

        var light = obj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Freeform;
        light.intensity = flashlightIntensity;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.falloffIntensity = 0.5f;

        Vector3[] shapePath = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(-flashlightWidth / 2f, flashlightRange, 0f),
            new Vector3(flashlightWidth / 2f, flashlightRange, 0f)
        };
        light.SetShapePath(shapePath);

        _flashlight = light;
    }

    void Update()
    {
        if (_flashlightTransform == null || _playerMovement == null) return;

        // Rotate flashlight to face movement direction
        Vector2 dir = _playerMovement.LastMoveDirection;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _flashlightTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        // Dynamic light radius based on movement
        UpdateBrightness();
    }

    void UpdateBrightness()
    {
        // Movement-based brightness
        float movementMultiplier;
        if (_playerMovement.IsSprinting)
            movementMultiplier = 1.5f;   // sprinting = bright flare
        else if (_playerMovement.SpeedRatio < 0.05f)
            movementMultiplier = 0.55f;  // standing still = light dims
        else
            movementMultiplier = 0.85f;  // walking = slightly dim

        // HP-based brightness — light = life
        float hpRatio = 1f;
        if (_playerHealth != null)
            hpRatio = Mathf.Clamp01((float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth);

        // HP affects base brightness: at 50% HP light is 70%, at 0% HP light is 0%
        float hpMultiplier = Mathf.Lerp(0.15f, 1f, hpRatio);

        float targetMultiplier = movementMultiplier * hpMultiplier;
        _brightnessMultiplier = Mathf.Lerp(_brightnessMultiplier, targetMultiplier, Time.deltaTime * 2f);

        // Point light
        if (_pointLight != null)
        {
            _pointLight.pointLightOuterRadius = _basePointRadius * _brightnessMultiplier;
            _pointLight.pointLightInnerRadius = _pointLight.pointLightOuterRadius * 0.3f;
            _pointLight.intensity = pointLightIntensity * Mathf.Lerp(0.7f, 1.3f, (_brightnessMultiplier - 0.55f) / 0.95f);
        }

        // Flashlight cone shape scales with brightness
        if (_flashlight != null)
        {
            float range = _baseFlashRange * _brightnessMultiplier;
            float width = _baseFlashWidth * _brightnessMultiplier;

            Vector3[] shapePath = new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(-width / 2f, range, 0f),
                new Vector3(width / 2f, range, 0f)
            };
            _flashlight.SetShapePath(shapePath);
            _flashlight.intensity = flashlightIntensity * Mathf.Lerp(0.7f, 1.2f, (_brightnessMultiplier - 0.55f) / 0.95f);
        }
    }
}
