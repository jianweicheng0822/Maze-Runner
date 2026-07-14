using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] int damage = 20;
    [SerializeField] float damageCooldown = 1f;

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

        health.TakeDamage(damage);
        _nextDamageTime = Time.time + damageCooldown;

        // Brief red flash feedback
        if (_sr != null)
            StartCoroutine(FlashRed());
    }

    System.Collections.IEnumerator FlashRed()
    {
        Color original = _sr.color;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        _sr.color = original;
    }
}
