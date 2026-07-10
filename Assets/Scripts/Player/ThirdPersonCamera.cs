using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeRunner.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 8f;
        [SerializeField] private float height = 4f;
        [SerializeField] private float followSmoothTime = 0.15f;

        [Header("Orbit")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 60f;

        [Header("Wall Avoidance")]
        [SerializeField] private float wallCheckRadius = 0.3f;
        [SerializeField] private LayerMask collisionLayers;

        private float yaw;
        private float pitch = 20f;
        private Vector3 currentVelocity;
        private Vector2 lookInput;

        public void SetTarget(Transform t)
        {
            target = t;
        }

        void Start()
        {
            if (target != null)
            {
                Vector3 dir = transform.position - target.position;
                yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            if (target == null) return;

            yaw += lookInput.x * mouseSensitivity;
            pitch -= lookInput.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = target.position + Vector3.up * height
                                      + rotation * (Vector3.back * distance);

            // Wall collision avoidance
            Vector3 dirFromTarget = desiredPosition - (target.position + Vector3.up * height);
            float desiredDist = dirFromTarget.magnitude;

            if (Physics.SphereCast(target.position + Vector3.up * height, wallCheckRadius,
                dirFromTarget.normalized, out RaycastHit hit, desiredDist, collisionLayers))
            {
                desiredPosition = target.position + Vector3.up * height
                                  + dirFromTarget.normalized * (hit.distance - wallCheckRadius);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition,
                ref currentVelocity, followSmoothTime);
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }
    }
}
