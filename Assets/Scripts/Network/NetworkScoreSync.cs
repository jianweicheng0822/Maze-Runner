#if UNITY_NETCODE
using Unity.Netcode;
using UnityEngine;
using MazeRunner.Core;

namespace MazeRunner.Network
{
    public class NetworkScoreSync : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged += OnScoreChangedServer;
        }

        private void OnScoreChangedServer(int playerId, int newScore)
        {
            SyncScoreClientRpc(playerId, newScore);
        }

        [ClientRpc]
        private void SyncScoreClientRpc(int playerId, int newScore)
        {
            if (IsServer) return;
            // Update local score display
            if (ScoreManager.Instance != null)
            {
                var scores = ScoreManager.Instance.Scores;
                if (scores.ContainsKey(playerId))
                {
                    // Direct field update for client display
                    ScoreManager.Instance.AddScore(playerId,
                        newScore - ScoreManager.Instance.GetScore(playerId));
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= OnScoreChangedServer;
        }
    }
}
#endif
