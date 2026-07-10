#if UNITY_NETCODE
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using MazeRunner.Core;

namespace MazeRunner.Network
{
    public class LobbyManager : NetworkBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        [SerializeField] private int minPlayers = 1;
        [SerializeField] private int maxPlayers = 8;

        private NetworkList<ulong> connectedPlayers;
        private NetworkList<bool> readyStatus;

        public int PlayerCount => connectedPlayers?.Count ?? 0;
        public int MaxPlayers => maxPlayers;

        public event System.Action<int> OnPlayerCountChanged;
        public event System.Action<ulong, bool> OnPlayerReadyChanged;
        public event System.Action OnAllPlayersReady;

        void Awake()
        {
            Instance = this;
            connectedPlayers = new NetworkList<ulong>();
            readyStatus = new NetworkList<bool>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }

            connectedPlayers.OnListChanged += OnPlayersListChanged;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;
            if (connectedPlayers.Count >= maxPlayers) return;

            connectedPlayers.Add(clientId);
            readyStatus.Add(false);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            for (int i = 0; i < connectedPlayers.Count; i++)
            {
                if (connectedPlayers[i] == clientId)
                {
                    connectedPlayers.RemoveAt(i);
                    readyStatus.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnPlayersListChanged(NetworkListEvent<ulong> changeEvent)
        {
            OnPlayerCountChanged?.Invoke(connectedPlayers.Count);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ToggleReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            for (int i = 0; i < connectedPlayers.Count; i++)
            {
                if (connectedPlayers[i] == clientId)
                {
                    readyStatus[i] = !readyStatus[i];
                    OnPlayerReadyChanged?.Invoke(clientId, readyStatus[i]);
                    CheckAllReady();
                    break;
                }
            }
        }

        private void CheckAllReady()
        {
            if (connectedPlayers.Count < minPlayers) return;

            for (int i = 0; i < readyStatus.Count; i++)
            {
                if (!readyStatus[i]) return;
            }

            OnAllPlayersReady?.Invoke();
            GameManager.Instance?.StartGame();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestStartServerRpc()
        {
            if (connectedPlayers.Count >= minPlayers)
                GameManager.Instance?.StartGame();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}
#endif
