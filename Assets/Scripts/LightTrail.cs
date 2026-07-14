using UnityEngine;
using System.Collections.Generic;

public class LightTrail : MonoBehaviour
{
    [SerializeField] float fadeDuration = 10f;
    [SerializeField] float dropInterval = 0.25f;
    [SerializeField] int maxDots = 200;

    Transform _player;
    Sprite _glowSprite;
    float _nextDropTime;
    List<(SpriteRenderer sr, float spawnTime)> _dots = new List<(SpriteRenderer, float)>();
    Transform _trailParent;

    public void Setup(Transform player)
    {
        _player = player;
        _glowSprite = CreateGlowSprite();

        _trailParent = new GameObject("LightTrail").transform;
    }

    void Update()
    {
        if (_player == null) return;

        // Drop trail dot at intervals
        if (Time.time >= _nextDropTime)
        {
            var movement = _player.GetComponent<PlayerMovement>();
            if (movement != null && movement.SpeedRatio > 0.05f)
            {
                _nextDropTime = Time.time + dropInterval;
                DropDot();
            }
        }

        // Fade and cleanup
        for (int i = _dots.Count - 1; i >= 0; i--)
        {
            var (sr, spawnTime) = _dots[i];
            if (sr == null)
            {
                _dots.RemoveAt(i);
                continue;
            }

            float elapsed = Time.time - spawnTime;
            if (elapsed >= fadeDuration)
            {
                Destroy(sr.gameObject);
                _dots.RemoveAt(i);
                continue;
            }

            float alpha = 1f - (elapsed / fadeDuration);
            alpha = alpha * alpha; // quadratic fade — lingers then disappears
            sr.color = new Color(1f, 0.92f, 0.7f, alpha * 0.25f);
        }
    }

    void DropDot()
    {
        // Remove oldest if at capacity
        if (_dots.Count >= maxDots)
        {
            Destroy(_dots[0].sr.gameObject);
            _dots.RemoveAt(0);
        }

        GameObject dot = new GameObject("TrailDot");
        dot.transform.SetParent(_trailParent, false);
        dot.transform.position = _player.position;
        dot.transform.localScale = Vector3.one * 0.35f;

        var sr = dot.AddComponent<SpriteRenderer>();
        sr.sprite = _glowSprite;
        sr.color = new Color(1f, 0.92f, 0.7f, 0.25f);
        sr.sortingOrder = 0;

        _dots.Add((sr, Time.time));
    }

    Sprite CreateGlowSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - dist / radius);
                alpha *= alpha; // soft radial falloff
                colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
