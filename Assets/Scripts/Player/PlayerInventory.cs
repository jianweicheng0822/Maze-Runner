using UnityEngine;
using MazeRunner.Items;

namespace MazeRunner.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        private CollectibleItem currentItem;
        private PlayerController playerController;
        private PlayerInteraction playerInteraction;

        public bool HasItem => currentItem != null;
        public CollectibleItem CurrentItem => currentItem;

        public event System.Action<CollectibleItem> OnItemPickedUp;
        public event System.Action<CollectibleItem> OnItemDropped;
        public event System.Action<CollectibleItem> OnItemDelivered;

        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerInteraction = GetComponent<PlayerInteraction>();
        }

        /// <summary>
        /// Picks up the given item, attaching it to the carry point.
        /// </summary>
        public void PickUp(CollectibleItem item)
        {
            if (currentItem != null)
                return;

            currentItem = item;
            currentItem.PickUp(playerInteraction.CarryPoint);

            if (playerController != null)
                playerController.IsCarrying = true;

            OnItemPickedUp?.Invoke(currentItem);
        }

        /// <summary>
        /// Drops the current item at the player's feet.
        /// </summary>
        public void Drop()
        {
            if (currentItem == null)
                return;

            CollectibleItem dropped = currentItem;
            Vector3 dropPosition = transform.position + transform.forward * 0.5f;
            currentItem.Drop(dropPosition);
            currentItem = null;

            if (playerController != null)
                playerController.IsCarrying = false;

            OnItemDropped?.Invoke(dropped);
        }

        /// <summary>
        /// Delivers (consumes) the current item. Returns the item reference before destroying it.
        /// Called by DeliveryZone on trigger.
        /// </summary>
        public CollectibleItem Deliver()
        {
            if (currentItem == null)
                return null;

            CollectibleItem delivered = currentItem;
            currentItem = null;

            if (playerController != null)
                playerController.IsCarrying = false;

            OnItemDelivered?.Invoke(delivered);

            Destroy(delivered.gameObject);
            return delivered;
        }
    }
}
