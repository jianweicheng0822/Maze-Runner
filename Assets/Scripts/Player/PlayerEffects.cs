using UnityEngine;

namespace MazeRunner.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerEffects : MonoBehaviour
    {
        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem landingDustPrefab;
        [SerializeField] private ParticleSystem runningDustPrefab;
        [SerializeField] private ParticleSystem hitSparksPrefab;

        [Header("Running Dust")]
        [SerializeField] private float runDustSpeedThreshold = 3f;

        private PlayerController controller;
        private ParticleSystem runningDustInstance;

        void Start()
        {
            controller = GetComponent<PlayerController>();

            controller.OnLanded += OnLanded;

            if (runningDustPrefab != null)
            {
                runningDustInstance = Instantiate(runningDustPrefab, transform);
                runningDustInstance.transform.localPosition = Vector3.zero;
                var emission = runningDustInstance.emission;
                emission.enabled = false;
            }
        }

        void Update()
        {
            if (runningDustInstance == null) return;

            var emission = runningDustInstance.emission;
            emission.enabled = controller.IsGrounded && controller.CurrentSpeed > runDustSpeedThreshold;
        }

        private void OnLanded()
        {
            if (landingDustPrefab != null)
            {
                ParticleSystem dust = Instantiate(landingDustPrefab, transform.position, Quaternion.identity);
                Destroy(dust.gameObject, dust.main.duration + dust.main.startLifetime.constantMax);
            }
        }

        public void PlayHitEffect(Vector3 hitPoint)
        {
            if (hitSparksPrefab != null)
            {
                ParticleSystem sparks = Instantiate(hitSparksPrefab, hitPoint, Quaternion.identity);
                Destroy(sparks.gameObject, sparks.main.duration + sparks.main.startLifetime.constantMax);
            }
        }
    }
}
