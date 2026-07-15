using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] int damage = 20;
    [SerializeField] float damageCooldown = 1f;
    [SerializeField] float knockbackForce = 8f;

    float _nextDamageTime;
    SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < _nextDamageTime) return;

        if (!other.TryGetComponent<PlayerHealth>(out var health)) return;
        if (!other.TryGetComponent<Rigidbody2D>(out var rb)) return;
        if (!other.TryGetComponent<PlayerMovement>(out var movement)) return;

        health.TakeDamage(damage);
        _nextDamageTime = Time.time + damageCooldown;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySpikeHit();

        // Knockback: push player away from trap center
        Vector2 knockDir = (other.transform.position - transform.position).normalized;
        if (knockDir == Vector2.zero)
            knockDir = -movement.LastMoveDirection;

        StartCoroutine(ApplyKnockback(rb, movement, knockDir));

        // Flash player red
        if (other.TryGetComponent<SpriteRenderer>(out var playerSr))
            StartCoroutine(FlashSprite(playerSr, new Color(1f, 0.3f, 0.3f)));

        // Flash trap white
        if (_sr != null)
            StartCoroutine(FlashSprite(_sr, Color.white));

        // Camera shake
        var cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null)
            cam.Shake(0.15f, 0.15f);
    }

    IEnumerator ApplyKnockback(Rigidbody2D rb, PlayerMovement movement, Vector2 dir)
    {
        movement.enabled = false;
        rb.linearVelocity = dir * knockbackForce;

        yield return new WaitForSeconds(0.2f);

        rb.linearVelocity = Vector2.zero;
        movement.enabled = true;
    }

    IEnumerator FlashSprite(SpriteRenderer sr, Color flashColor)
    {
        Color original = sr.color;
        sr.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }
}
