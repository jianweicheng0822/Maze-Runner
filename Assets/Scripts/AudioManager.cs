using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    AudioSource _bgmSource;
    AudioSource _footstepSource;
    AudioSource _spiritAmbientSource;
    AudioSource _heartbeatSource;
    AudioSource _sfxSource;

    AudioClip _spikeHitClip;
    AudioClip _deathClip;
    AudioClip _levelCompleteClip;

    // Footstep timing
    float _footstepTimer;
    float _walkInterval = 0.4f;
    float _sprintInterval = 0.25f;

    // Cached references
    PlayerMovement _playerMovement;
    PlayerHealth _playerHealth;

    void Awake()
    {
        Instance = this;

        _bgmSource = CreateSource("BGM", true, 0.3f);
        _footstepSource = CreateSource("Footstep", true, 0.5f);
        _spiritAmbientSource = CreateSource("SpiritAmbient", true, 0f);
        _heartbeatSource = CreateSource("Heartbeat", true, 0f);
        _sfxSource = CreateSource("SFX", false, 0.7f);

        LoadClips();
    }

    void Start()
    {
        // Start BGM
        if (_bgmSource.clip != null)
            _bgmSource.Play();
    }

    void Update()
    {
        CachePlayerRefs();
        UpdateFootsteps();
        UpdateHeartbeat();
        UpdateSpiritAmbient();
    }

    AudioSource CreateSource(string name, bool loop, float volume)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.loop = loop;
        source.volume = volume;
        source.playOnAwake = false;
        return source;
    }

    void LoadClips()
    {
        _bgmSource.clip = Resources.Load<AudioClip>("Audio/bgm");
        _footstepSource.clip = Resources.Load<AudioClip>("Audio/footstep");
        _spiritAmbientSource.clip = Resources.Load<AudioClip>("Audio/spirit_ambient");
        _heartbeatSource.clip = Resources.Load<AudioClip>("Audio/heartbeat");

        _spikeHitClip = Resources.Load<AudioClip>("Audio/spike_hit");
        _deathClip = Resources.Load<AudioClip>("Audio/death");
        _levelCompleteClip = Resources.Load<AudioClip>("Audio/level_complete");
    }

    void CachePlayerRefs()
    {
        if (_playerMovement != null) return;
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;
        _playerMovement = player.GetComponent<PlayerMovement>();
        _playerHealth = player.GetComponent<PlayerHealth>();
    }

    // ---- FOOTSTEPS ----

    void UpdateFootsteps()
    {
        if (_playerMovement == null || _footstepSource.clip == null) return;

        float speedRatio = _playerMovement.SpeedRatio;
        if (speedRatio < 0.01f)
        {
            _footstepSource.Stop();
            _footstepTimer = 0f;
            return;
        }

        bool sprinting = _playerMovement.IsSprinting;
        _footstepSource.pitch = sprinting ? 1.4f : 1.0f;

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0f)
        {
            _footstepSource.Stop();
            _footstepSource.Play();
            _footstepTimer = sprinting ? _sprintInterval : _walkInterval;
        }
    }

    // ---- HEARTBEAT ----

    void UpdateHeartbeat()
    {
        if (_playerHealth == null || _heartbeatSource.clip == null) return;

        float hpRatio = (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth;

        if (hpRatio >= 0.5f)
        {
            if (_heartbeatSource.isPlaying)
                _heartbeatSource.Stop();
            return;
        }

        if (!_heartbeatSource.isPlaying)
            _heartbeatSource.Play();

        // Volume: silent at 50%, loud at 0%
        // Maps 0.5 -> 0.0 volume, 0.0 -> 1.0 volume
        _heartbeatSource.volume = 1f - (hpRatio / 0.5f);

        // Pitch: normal above 25%, faster below 25%
        if (hpRatio < 0.25f)
        {
            // Maps 0.25 -> 1.0 pitch, 0.0 -> 1.5 pitch
            _heartbeatSource.pitch = 1f + 0.5f * (1f - hpRatio / 0.25f);
        }
        else
        {
            _heartbeatSource.pitch = 1f;
        }
    }

    // ---- SPIRIT AMBIENT ----

    void UpdateSpiritAmbient()
    {
        if (_spiritAmbientSource.clip == null) return;

        var player = _playerMovement != null ? _playerMovement.transform : null;
        if (player == null) return;

        float nearestDist = float.MaxValue;
        var spirits = FindObjectsByType<BeamTrap>(FindObjectsSortMode.None);
        foreach (var spirit in spirits)
        {
            float dist = Vector2.Distance(player.position, spirit.transform.position);
            if (dist < nearestDist)
                nearestDist = dist;
        }

        float maxRange = 6f;
        if (nearestDist < maxRange)
        {
            if (!_spiritAmbientSource.isPlaying)
                _spiritAmbientSource.Play();
            _spiritAmbientSource.volume = 1f - (nearestDist / maxRange);
        }
        else
        {
            _spiritAmbientSource.volume = 0f;
        }
    }

    // ---- SFX ONE-SHOTS ----

    public void PlaySpikeHit()
    {
        if (_spikeHitClip != null)
            _sfxSource.PlayOneShot(_spikeHitClip);
    }

    public void PlayDeath()
    {
        StopAll();
        if (_deathClip != null)
            _sfxSource.PlayOneShot(_deathClip);
    }

    public void PlayLevelComplete()
    {
        StopAll();
        if (_levelCompleteClip != null)
            _sfxSource.PlayOneShot(_levelCompleteClip);
    }

    public void StopAll()
    {
        _bgmSource.Stop();
        _footstepSource.Stop();
        _spiritAmbientSource.Stop();
        _heartbeatSource.Stop();
    }
}
