using UnityEngine;
using MazeRunner.Core;
using MazeRunner.Player;

namespace MazeRunner.Items
{
    [RequireComponent(typeof(Collider))]
    public class DeliveryZone : MonoBehaviour
    {
        [Header("Delivery Settings")]
        [SerializeField] private int targetCount = 5;

        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem deliveryBurstEffect;

        private int itemsDelivered;

        public int ItemsDelivered => itemsDelivered;
        public int TargetCount => targetCount;

        public event System.Action<ItemType, int> OnItemDelivered;
        public event System.Action OnTargetReached;

        void Start()
        {
            // Ensure the collider is a trigger
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(GameTags.Player))
                return;

            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null)
                inventory = other.GetComponentInParent<PlayerInventory>();

            if (inventory == null || !inventory.HasItem)
                return;

            // Deliver the item
            CollectibleItem delivered = inventory.Deliver();
            if (delivered == null)
                return;

            itemsDelivered++;

            // Play particle burst
            if (deliveryBurstEffect != null)
                deliveryBurstEffect.Play();

            // Notify round manager
            RoundManager.Instance?.RegisterItemDelivered();

            OnItemDelivered?.Invoke(delivered.ItemType, itemsDelivered);

            // The item is destroyed by PlayerInventory.Deliver(), so we just check target
            if (itemsDelivered >= targetCount)
            {
                OnTargetReached?.Invoke();
            }
        }

        public void ResetZone()
        {
            itemsDelivered = 0;
        }
    }
}
