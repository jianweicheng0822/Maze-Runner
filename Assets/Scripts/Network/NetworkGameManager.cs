#if UNITY_NETCODE
using Unity.Netcode;
using UnityEngine;
using MazeRunner.Core;

namespace MazeRunner.Network
{
    public class NetworkGameManager : NetworkBehaviour
    {
        private NetworkVariable<GameState> networkState = new NetworkVariable<GameState>(
            GameState.Lobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<float> networkTimer = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkItemsDelivered = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkMazeSeed = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            networkState.OnValueChanged += OnNetworkStateChanged;

            if (IsServer && GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnLocalStateChanged;
            }
        }

        private void OnLocalStateChanged(GameState newState)
        {
            if (!IsServer) return;
            networkState.Value = newState;
        }

        private void OnNetworkStateChanged(GameState oldState, GameState newState)
        {
            if (IsServer) return;
            GameManager.Instance?.SetState(newState);
        }

        void Update()
        {
            if (!IsServer) return;

            if (RoundManager.Instance != null)
            {
                networkTimer.Value = RoundManager.Instance.RoundTimer;
                networkItemsDelivered.Value = RoundManager.Instance.ItemsDelivered;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestStartGameServerRpc()
        {
            GameManager.Instance?.StartGame();
        }

        public void SetMazeSeed(int seed)
        {
            if (!IsServer) return;
            networkMazeSeed.Value = seed;
        }

        public int GetMazeSeed() => networkMazeSeed.Value;
    }
}
#endif
