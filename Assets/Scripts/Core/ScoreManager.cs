using System.Collections.Generic;
using UnityEngine;

namespace MazeRunner.Core
{
    [System.Serializable]
    public class PlayerScore
    {
        public int playerId;
        public string playerName;
        public int totalScore;
        public int itemsDelivered;
        public int roundsCompleted;

        public PlayerScore(int id, string name)
        {
            playerId = id;
            playerName = name;
        }
    }

    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private readonly Dictionary<int, PlayerScore> scores = new Dictionary<int, PlayerScore>();

        public event System.Action<int, int> OnScoreChanged; // playerId, newTotal
        public IReadOnlyDictionary<int, PlayerScore> Scores => scores;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterPlayer(int playerId, string playerName)
        {
            if (!scores.ContainsKey(playerId))
                scores[playerId] = new PlayerScore(playerId, playerName);
        }

        public void AddScore(int playerId, int points)
        {
            if (!scores.ContainsKey(playerId)) return;
            scores[playerId].totalScore += points;
            OnScoreChanged?.Invoke(playerId, scores[playerId].totalScore);
        }

        public void AddItemDelivered(int playerId)
        {
            if (!scores.ContainsKey(playerId)) return;
            scores[playerId].itemsDelivered++;
        }

        public void CompleteRound(int playerId)
        {
            if (!scores.ContainsKey(playerId)) return;
            scores[playerId].roundsCompleted++;
        }

        public List<PlayerScore> GetLeaderboard()
        {
            var list = new List<PlayerScore>(scores.Values);
            list.Sort((a, b) => b.totalScore.CompareTo(a.totalScore));
            return list;
        }

        public void ResetAllScores()
        {
            scores.Clear();
        }

        public int GetScore(int playerId)
        {
            return scores.ContainsKey(playerId) ? scores[playerId].totalScore : 0;
        }
    }
}
