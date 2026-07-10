using UnityEngine;
using MazeRunner.Core;
using MazeRunner.Player;

namespace MazeRunner.Items
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CollectibleItem : MonoBehaviour, IInteractable
    {
        [Header("Item Settings")]
        [SerializeField] private ItemType itemType;
        [SerializeField] private float weight = 1f;
        [SerializeField] private int pointValue = 10;

        [Header("Visual - Bob")]
        [SerializeField] private float bobAmplitude = 0.25f;
        [SerializeField] private float bobFrequency = 2f;

        [Header("Visual - Glow")]
        [SerializeField] private Light glowLight;
        [SerializeField] private float glowIntensity = 1.5f;
        [SerializeField] private float glowPulseSpeed = 3f;

        private Rigidbody rb;
        private Collider col;
        private Vector3 startPosition;
        private bool isBobbing = true;
        private float bobTimer;

        public ItemType ItemType => itemType;
        public float Weight => weight;
        public int PointValue => pointValue;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }

        void Start()
        {
            startPosition = transform.position;
            rb.isKinematic = true;
            rb.useGravity = false;

            if (glowLight != null)
                glowLight.intensity = glowIntensity;
        }

        void Update()
        {
            if (isBobbing)
            {
                bobTimer += Time.deltaTime;

                // Hover up and down using a sine wave
                Vector3 bobOffset = Vector3.up * (Mathf.Sin(bobTimer * bobFrequency) * bobAmplitude);
                transform.position = startPosition + bobOffset;

                // Pulse the glow light
                if (glowLight != null)
                {
                    float pulse = Mathf.Lerp(glowIntensity * 0.5f, glowIntensity,
                        (Mathf.Sin(bobTimer * glowPulseSpeed) + 1f) * 0.5f);
                    glowLight.intensity = pulse;
                }

                // Slow rotation for visual flair
                transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
            }
        }

        public bool CanInteract(PlayerInteraction player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            return inventory != null && !inventory.HasItem;
        }

        public void Interact(PlayerInteraction player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.PickUp(this);
            }
        }

        public string GetPromptText()
        {
            return $"E - Pick Up {itemType}";
        }

        /// <summary>
        /// Called by PlayerInventory when picking up this item.
        /// Parents the item to the carry point and disables physics/bobbing.
        /// </summary>
        public void PickUp(Transform carryPoint)
        {
            isBobbing = false;

            transform.SetParent(carryPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            col.enabled = false;

            if (glowLight != null)
                glowLight.intensity = glowIntensity;
        }

        /// <summary>
        /// Called by PlayerInventory when dropping this item.
        /// Unparents and re-enables physics and bobbing.
        /// </summary>
        public void Drop(Vector3 dropPosition)
        {
            transform.SetParent(null);
            transform.position = dropPosition;

            rb.isKinematic = true;
            rb.useGravity = false;
            col.enabled = true;

            // Reset bob origin to the new drop position
            startPosition = dropPosition;
            bobTimer = 0f;
            isBobbing = true;
        }
    }
}
