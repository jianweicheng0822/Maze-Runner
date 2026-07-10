using UnityEngine;
using MazeRunner.Core;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// A constant-force zone that pushes player Rigidbodies in a configurable direction.
    /// </summary>
    public class ConveyorBelt : MonoBehaviour
    {
        [Header("Conveyor Settings")]
        [SerializeField] private Vector3 pushDirection = Vector3.forward;
        [SerializeField] private float strength = 8f;
        [SerializeField] private ForceMode forceMode = ForceMode.Force;

        private Vector3 normalizedDirection;

        private void Start()
        {
            normalizedDirection = pushDirection.normalized;
        }

        private void OnValidate()
        {
            normalizedDirection = pushDirection.normalized;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(GameTags.Player)) return;

            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                // Use the transform's rotation so the conveyor direction
                // follows the object's orientation in the scene.
                Vector3 worldDirection = transform.TransformDirection(normalizedDirection);
                rb.AddForce(worldDirection * strength, forceMode);
            }
        }
    }
}
