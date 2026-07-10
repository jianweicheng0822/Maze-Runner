#if UNITY_NETCODE
using Unity.Netcode;
using UnityEngine;
using MazeRunner.Core;

namespace MazeRunner.Network
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Color[] playerColors;

        private int nextColorIndex;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            int playerIndex = (int)clientId;
            Vector3 spawnPos = SpawnPointManager.Instance != null
                ? SpawnPointManager.Instance.GetSpawnPosition(playerIndex)
                : Vector3.zero;

            Quaternion spawnRot = SpawnPointManager.Instance != null
                ? SpawnPointManager.Instance.GetSpawnRotation(playerIndex)
                : Quaternion.identity;

            var player = Instantiate(playerPrefab, spawnPos, spawnRot);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            // Assign color
            if (playerColors != null && playerColors.Length > 0)
            {
                int colorIdx = nextColorIndex % playerColors.Length;
                AssignColorClientRpc(player.GetComponent<NetworkObject>().NetworkObjectId, colorIdx);
                nextColorIndex++;
            }

            // Register in score manager
            ScoreManager.Instance?.RegisterPlayer(playerIndex, $"Player {playerIndex + 1}");
        }

        [ClientRpc]
        private void AssignColorClientRpc(ulong networkObjectId, int colorIndex)
        {
            if (playerColors == null || colorIndex >= playerColors.Length) return;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
            {
                var renderer = netObj.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    var material = renderer.material;
                    material.color = playerColors[colorIndex];
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
#endif
