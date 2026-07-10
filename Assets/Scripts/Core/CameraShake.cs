using UnityEngine;

namespace MazeRunner.Core
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private float shakeDuration;
        private float shakeMagnitude;
        private float dampingSpeed = 3f;
        private Vector3 originalLocalPosition;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            originalLocalPosition = transform.localPosition;
        }

        void Update()
        {
            if (shakeDuration > 0f)
            {
                float x = (Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f) * 2f * shakeMagnitude;
                float y = (Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f) * 2f * shakeMagnitude;

                transform.localPosition = originalLocalPosition + new Vector3(x, y, 0f);

                shakeDuration -= Time.deltaTime * dampingSpeed;
            }
            else
            {
                shakeDuration = 0f;
                transform.localPosition = originalLocalPosition;
            }
        }

        public void Shake(float duration = 0.3f, float magnitude = 0.15f)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }
    }
}
