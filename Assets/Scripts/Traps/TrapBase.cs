using UnityEngine;
using MazeRunner.Core;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// Abstract base class for all traps. Provides activation lifecycle,
    /// cooldown management, auto-reset, and player detection via trigger colliders.
    /// </summary>
    public abstract class TrapBase : MonoBehaviour
    {
        [Header("Trap Base Settings")]
        [SerializeField] protected float cooldownDuration = 1f;
        [SerializeField] protected bool autoReset = true;
        [SerializeField] protected float resetDelay = 3f;

        protected bool isActive = true;
        private float cooldownTimer;
        private bool isOnCooldown;

        public bool IsActive => isActive;

        protected virtual void Update()
        {
            if (isOnCooldown)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    isOnCooldown = false;
                }
            }
        }

        /// <summary>
        /// Called when the trap should begin its active behavior.
        /// </summary>
        public abstract void Activate();

        /// <summary>
        /// Called when the trap should stop its active behavior.
        /// </summary>
        public abstract void Deactivate();

        /// <summary>
        /// Called when a player enters this trap's trigger collider.
        /// </summary>
        protected abstract void OnPlayerHit(PlayerController player);

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            if (isOnCooldown) return;

            if (other.CompareTag(GameTags.Player))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player == null)
                    player = other.GetComponentInParent<PlayerController>();

                if (player != null)
                {
                    OnPlayerHit(player);
                    StartCooldown();
                }
            }
        }

        /// <summary>
        /// Starts the cooldown timer so the trap does not trigger again immediately.
        /// </summary>
        protected void StartCooldown()
        {
            isOnCooldown = true;
            cooldownTimer = cooldownDuration;
        }

        /// <summary>
        /// Schedules an auto-reset after the configured resetDelay.
        /// Call this from subclasses after a trap fires and needs to restore itself.
        /// </summary>
        protected void ScheduleAutoReset()
        {
            if (autoReset)
            {
                Invoke(nameof(PerformReset), resetDelay);
            }
        }

        private void PerformReset()
        {
            Activate();
        }

        /// <summary>
        /// Calculates a knockback direction from this trap toward the given player,
        /// normalized on the horizontal plane with a configurable upward component.
        /// </summary>
        protected Vector3 CalculateKnockbackDirection(Vector3 trapPosition, Vector3 playerPosition, float upwardBias = 0.3f)
        {
            Vector3 direction = playerPosition - trapPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
            direction.y = upwardBias;
            return direction.normalized;
        }
    }
}
