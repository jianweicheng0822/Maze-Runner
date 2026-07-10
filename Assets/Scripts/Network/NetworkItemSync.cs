#if UNITY_NETCODE
using Unity.Netcode;
using UnityEngine;

namespace MazeRunner.Network
{
    public class NetworkItemSync : NetworkBehaviour
    {
        private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<ulong> holderClientId = new NetworkVariable<ulong>(
            ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Items.CollectibleItem collectible;

        void Start()
        {
            collectible = GetComponent<Items.CollectibleItem>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPickupServerRpc(ulong requestingClientId)
        {
            // Server resolves race conditions
            if (isPickedUp.Value) return;

            isPickedUp.Value = true;
            holderClientId.Value = requestingClientId;

            PickupClientRpc(requestingClientId);
        }

        [ClientRpc]
        private void PickupClientRpc(ulong pickerClientId)
        {
            if (NetworkManager.Singleton.LocalClientId == pickerClientId) return;
            // Remote clients see the item disappear (it's parented on the picker's side)
            if (collectible != null)
            {
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
                // Visual hide handled by parenting on owner
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDropServerRpc(Vector3 dropPosition)
        {
            isPickedUp.Value = false;
            holderClientId.Value = ulong.MaxValue;

            DropClientRpc(dropPosition);
        }

        [ClientRpc]
        private void DropClientRpc(Vector3 dropPosition)
        {
            transform.position = dropPosition;
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDeliverServerRpc()
        {
            DeliverClientRpc();
            // Despawn on server
            GetComponent<NetworkObject>().Despawn();
        }

        [ClientRpc]
        private void DeliverClientRpc()
        {
            // Visual feedback handled by DeliveryZone
        }
    }
}
#endif
