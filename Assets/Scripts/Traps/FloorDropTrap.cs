using System.Collections;
using UnityEngine;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// A floor tile that warns the player (shaking and color change),
    /// then drops away. Respawns after a configurable delay.
    /// </summary>
    public class FloorDropTrap : TrapBase
    {
        [Header("Floor Drop Settings")]
        [SerializeField] private float warningDuration = 1f;
        [SerializeField] private float dropDistance = 5f;
        [SerializeField] private float dropSpeed = 8f;
        [SerializeField] private float respawnDelay = 4f;
        [SerializeField] private float shakeIntensity = 0.05f;
        [SerializeField] private Color warningColor = Color.red;

        private Vector3 originalPosition;
        private Color originalColor;
        private Renderer tileRenderer;
        private Collider tileCollider;
        private bool isTriggered;

        private void Start()
        {
            originalPosition = transform.position;
            tileCollider = GetComponent<Collider>();
            tileRenderer = GetComponent<Renderer>();

            if (tileRenderer != null && tileRenderer.material != null)
            {
                originalColor = tileRenderer.material.color;
            }
        }

        public override void Activate()
        {
            isActive = true;
            isTriggered = false;
            transform.position = originalPosition;

            if (tileCollider != null)
                tileCollider.enabled = true;

            if (tileRenderer != null)
            {
                tileRenderer.enabled = true;
                tileRenderer.material.color = originalColor;
            }
        }

        public override void Deactivate()
        {
            isActive = false;
        }

        protected override void OnPlayerHit(PlayerController player)
        {
            if (isTriggered) return;
            isTriggered = true;
            StartCoroutine(DropSequence());
        }

        private IEnumerator DropSequence()
        {
            // Warning phase: shake and change color
            float elapsed = 0f;

            if (tileRenderer != null)
                tileRenderer.material.color = warningColor;

            while (elapsed < warningDuration)
            {
                Vector3 shake = new Vector3(
                    Random.Range(-shakeIntensity, shakeIntensity),
                    0f,
                    Random.Range(-shakeIntensity, shakeIntensity)
                );
                transform.position = originalPosition + shake;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Reset position before dropping
            transform.position = originalPosition;

            // Drop phase: disable collider and move down
            if (tileCollider != null)
                tileCollider.enabled = false;

            Vector3 dropTarget = originalPosition + Vector3.down * dropDistance;
            float dropProgress = 0f;

            while (dropProgress < 1f)
            {
                dropProgress += Time.deltaTime * dropSpeed / dropDistance;
                transform.position = Vector3.Lerp(originalPosition, dropTarget, dropProgress);
                yield return null;
            }

            transform.position = dropTarget;

            // Optionally hide the renderer
            if (tileRenderer != null)
                tileRenderer.enabled = false;

            // Wait for respawn
            yield return new WaitForSeconds(respawnDelay);

            // Respawn
            Activate();
        }
    }
}
