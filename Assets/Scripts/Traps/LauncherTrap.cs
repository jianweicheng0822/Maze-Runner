using UnityEngine;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// A floor pad that launches players upward on contact.
    /// </summary>
    public class LauncherTrap : TrapBase
    {
        [Header("Launcher Settings")]
        [SerializeField] private float launchForce = 15f;
        [SerializeField] private Vector3 launchDirection = Vector3.up;

        private void Start()
        {
            launchDirection = launchDirection.normalized;
        }

        public override void Activate()
        {
            isActive = true;
        }

        public override void Deactivate()
        {
            isActive = false;
        }

        protected override void OnPlayerHit(PlayerController player)
        {
            // Zero out the player's current vertical velocity so the launch is consistent.
            Rigidbody rb = player.Rigidbody;
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;

            player.ApplyKnockback(launchDirection * launchForce);
        }
    }
}
