using UnityEngine;
using System.Collections;

public class SawTrap : MonoBehaviour
{
    [SerializeField] int damage = 25;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float damageCooldown = 1f;
    [SerializeField] float knockbackForce = 10f;

    Vector3 _pointA;
    Vector3 _pointB;
    bool _movingToB = true;
    float _nextDamageTime;

    public void SetPatrolPoints(Vector3 a, Vector3 b)
    {
        _pointA = a;
        _pointB = b;
        transform.position = a;
    }

    void Update()
    {
        Vector3 target = _movingToB ? _pointB : _pointA;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
            _movingToB = !_movingToB;

        // Rotate for visual effect
        transform.Rotate(0, 0, 360 * Time.deltaTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < _nextDamageTime) return;

        if (!other.TryGetComponent<PlayerHealth>(out var health)) return;
        if (!other.TryGetComponent<Rigidbody2D>(out var rb)) return;
        if (!other.TryGetComponent<PlayerMovement>(out var movement)) return;

        health.TakeDamage(damage);
        _nextDamageTime = Time.time + damageCooldown;

        // Knockback away from saw
        Vector2 knockDir = (other.transform.position - transform.position).normalized;
        if (knockDir == Vector2.zero)
            knockDir = -movement.LastMoveDirection;

        StartCoroutine(ApplyKnockback(rb, movement, knockDir));

        // Flash player red
        if (other.TryGetComponent<SpriteRenderer>(out var playerSr))
            StartCoroutine(FlashSprite(playerSr));

        // Camera shake
        var cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null)
            cam.Shake(0.2f, 0.2f);
    }

    IEnumerator ApplyKnockback(Rigidbody2D rb, PlayerMovement movement, Vector2 dir)
    {
        movement.enabled = false;
        rb.linearVelocity = dir * knockbackForce;

        yield return new WaitForSeconds(0.25f);

        rb.linearVelocity = Vector2.zero;
        movement.enabled = true;
    }

    IEnumerator FlashSprite(SpriteRenderer sr)
    {
        Color original = sr.color;
        sr.color = new Color(1f, 0.3f, 0.3f);
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }
}
