using UnityEngine;
using UnityEngine.InputSystem;
using MazeRunner.Core;

namespace MazeRunner.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private float interactRadius = 0.5f;
        [SerializeField] private LayerMask interactLayer;
        [SerializeField] private Transform carryPoint;

        private IInteractable currentTarget;
        private PlayerInventory inventory;

        public Transform CarryPoint => carryPoint;
        public IInteractable CurrentTarget => currentTarget;

        public event System.Action<IInteractable> OnTargetChanged;

        void Start()
        {
            inventory = GetComponent<PlayerInventory>();

            if (carryPoint == null)
            {
                GameObject cp = new GameObject("CarryPoint");
                cp.transform.SetParent(transform);
                cp.transform.localPosition = new Vector3(0f, 1.5f, 0.8f);
                carryPoint = cp.transform;
            }
        }

        void Update()
        {
            ScanForInteractable();
        }

        private void ScanForInteractable()
        {
            IInteractable found = null;

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 direction = transform.forward;

            if (Physics.SphereCast(origin, interactRadius, direction, out RaycastHit hit,
                interactRange, interactLayer))
            {
                found = hit.collider.GetComponentInParent<IInteractable>();
                if (found != null && !found.CanInteract(this))
                    found = null;
            }

            if (found != currentTarget)
            {
                currentTarget = found;
                OnTargetChanged?.Invoke(currentTarget);
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            if (currentTarget != null && currentTarget.CanInteract(this))
            {
                currentTarget.Interact(this);
            }
            else if (inventory != null && inventory.HasItem)
            {
                inventory.Drop();
            }
        }
    }
}
