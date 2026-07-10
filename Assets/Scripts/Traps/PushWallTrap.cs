using UnityEngine;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// A wall that moves back and forth along a configurable direction,
    /// knocking players on contact. Can be triggered by a ButtonInteractable
    /// or set to always-on mode.
    /// </summary>
    public class PushWallTrap : TrapBase
    {
        [Header("Push Wall Settings")]
        [SerializeField] private Vector3 moveDirection = Vector3.right;
        [SerializeField] private float moveDistance = 4f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float knockbackForce = 12f;
        [SerializeField] private bool alwaysOn = false;

        private Vector3 startPosition;
        private bool isMoving;
        private float moveTimer;

        private void Start()
        {
            startPosition = transform.position;
            moveDirection = moveDirection.normalized;

            if (alwaysOn)
            {
                isMoving = true;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!isMoving) return;

            moveTimer += Time.deltaTime * moveSpeed;
            float offset = Mathf.PingPong(moveTimer, moveDistance);
            transform.position = startPosition + moveDirection * offset;
        }

        public override void Activate()
        {
            isActive = true;
            isMoving = true;
        }

        public override void Deactivate()
        {
            isActive = false;
            isMoving = false;
            transform.position = startPosition;
            moveTimer = 0f;
        }

        protected override void OnPlayerHit(PlayerController player)
        {
            Vector3 knockDir = CalculateKnockbackDirection(transform.position, player.transform.position);
            player.ApplyKnockback(knockDir * knockbackForce);
        }
    }
}
