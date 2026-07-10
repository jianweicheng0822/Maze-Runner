using UnityEngine;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// A rotating beam that sweeps an area. Players hit by the beam
    /// receive knockback perpendicular to the beam direction.
    /// </summary>
    public class SpinnerTrap : TrapBase
    {
        [Header("Spinner Settings")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float knockbackForce = 10f;

        private void Start()
        {
            rotationAxis = rotationAxis.normalized;
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
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
            // Calculate knockback perpendicular to the beam.
            // The beam extends along the trap's local X (or Z) axis;
            // the perpendicular direction in the sweep plane pushes the player outward.
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude < 0.001f)
            {
                toPlayer = transform.forward;
            }

            // Get the beam's current forward direction projected onto the horizontal plane.
            Vector3 beamDir = transform.right;
            beamDir.y = 0f;
            beamDir.Normalize();

            // The perpendicular component relative to the beam pushes the player away.
            Vector3 perpendicular = toPlayer - Vector3.Dot(toPlayer, beamDir) * beamDir;

            if (perpendicular.sqrMagnitude < 0.001f)
            {
                perpendicular = toPlayer.normalized;
            }
            else
            {
                perpendicular.Normalize();
            }

            // Add a slight upward component so the player is swept off the ground.
            Vector3 knockback = (perpendicular + Vector3.up * 0.25f).normalized * knockbackForce;
            player.ApplyKnockback(knockback);
        }
    }
}
