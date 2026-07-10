using UnityEngine;
using UnityEngine.InputSystem;
using MazeRunner.Core;

namespace MazeRunner.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveForce = 40f;
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float groundDrag = 5f;
        [SerializeField] private float airDrag = 0.5f;
        [SerializeField] private float airControlMultiplier = 0.4f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Carry Penalty")]
        [SerializeField] private float carrySpeedMultiplier = 0.7f;

        private Rigidbody rb;
        private Vector2 moveInput;
        private bool jumpRequested;
        private bool isGrounded;
        private bool wasGrounded;
        private Transform cameraTransform;

        // External modifiers (from effects/traps)
        private float speedMultiplier = 1f;

        public bool IsGrounded => isGrounded;
        public bool IsCarrying { get; set; }
        public float CurrentSpeed => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
        public Rigidbody Rigidbody => rb;
        public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }

        public event System.Action OnLanded;
        public event System.Action OnJumped;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            cameraTransform = Camera.main != null ? Camera.main.transform : transform;
        }

        void Update()
        {
            CheckGround();

            rb.linearDamping = isGrounded ? groundDrag : airDrag;

            if (!wasGrounded && isGrounded)
                OnLanded?.Invoke();

            wasGrounded = isGrounded;
        }

        void FixedUpdate()
        {
            Move();

            if (jumpRequested && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                OnJumped?.Invoke();
                jumpRequested = false;
            }
        }

        private void Move()
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

            float effectiveForce = moveForce * speedMultiplier;
            if (IsCarrying) effectiveForce *= carrySpeedMultiplier;
            if (!isGrounded) effectiveForce *= airControlMultiplier;

            rb.AddForce(moveDirection * effectiveForce, ForceMode.Force);

            // Clamp horizontal speed
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float effectiveMax = maxSpeed * speedMultiplier;
            if (IsCarrying) effectiveMax *= carrySpeedMultiplier;

            if (horizontalVel.magnitude > effectiveMax)
            {
                Vector3 clamped = horizontalVel.normalized * effectiveMax;
                rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
            }

            // Rotate toward movement direction
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            }
        }

        private void CheckGround()
        {
            Vector3 origin = transform.position + Vector3.up * groundCheckRadius;
            isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down,
                out _, groundCheckDistance + groundCheckRadius, groundLayer);
        }

        // Input System callbacks
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started && isGrounded)
                jumpRequested = true;
        }

        public void ApplyKnockback(Vector3 force)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }

        public void SetCameraTransform(Transform cam)
        {
            cameraTransform = cam;
        }
    }
}
