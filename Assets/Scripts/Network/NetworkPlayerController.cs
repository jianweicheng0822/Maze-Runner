#if UNITY_NETCODE
using Unity.Netcode;
using UnityEngine;

namespace MazeRunner.Network
{
    [RequireComponent(typeof(Player.PlayerController))]
    public class NetworkPlayerController : NetworkBehaviour
    {
        [SerializeField] private Player.ThirdPersonCamera cameraPrefab;

        private Player.PlayerController playerController;
        private Player.PlayerInteraction playerInteraction;

        public override void OnNetworkSpawn()
        {
            playerController = GetComponent<Player.PlayerController>();
            playerInteraction = GetComponent<Player.PlayerInteraction>();

            if (IsOwner)
            {
                // Enable input for local player
                enabled = true;

                if (cameraPrefab != null)
                {
                    var cam = Instantiate(cameraPrefab);
                    cam.SetTarget(transform);
                    playerController.SetCameraTransform(cam.transform);
                }
            }
            else
            {
                // Disable input for remote players
                if (playerController != null) playerController.enabled = false;
                if (playerInteraction != null) playerInteraction.enabled = false;
            }
        }
    }
}
#endif
