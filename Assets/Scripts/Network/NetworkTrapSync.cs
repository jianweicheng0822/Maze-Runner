#if UNITY_NETCODE
using Unity.Netcode;
using UnityEngine;

namespace MazeRunner.Network
{
    public class NetworkTrapSync : NetworkBehaviour
    {
        private Traps.TrapBase trap;

        void Start()
        {
            trap = GetComponent<Traps.TrapBase>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestActivateServerRpc()
        {
            ActivateTrapClientRpc();
        }

        [ClientRpc]
        private void ActivateTrapClientRpc()
        {
            if (trap != null)
                trap.Activate();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDeactivateServerRpc()
        {
            DeactivateTrapClientRpc();
        }

        [ClientRpc]
        private void DeactivateTrapClientRpc()
        {
            if (trap != null)
                trap.Deactivate();
        }
    }
}
#endif
