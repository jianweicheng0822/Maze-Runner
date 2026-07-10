using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MazeRunner.Core;

namespace MazeRunner.UI
{
    public class ScoreboardUI : MonoBehaviour
    {
        [SerializeField] private GameObject scoreboardPanel;
        [SerializeField] private Transform entryContainer;
        [SerializeField] private GameObject entryPrefab;

        private readonly List<GameObject> entries = new List<GameObject>();

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            if (scoreboardPanel != null)
                scoreboardPanel.SetActive(false);
        }

        private void OnStateChanged(GameState state)
        {
            bool show = state == GameState.RoundEnd || state == GameState.GameOver;
            if (scoreboardPanel != null)
                scoreboardPanel.SetActive(show);

            if (show) RefreshScoreboard();
        }

        private void RefreshScoreboard()
        {
            foreach (var entry in entries)
                Destroy(entry);
            entries.Clear();

            if (ScoreManager.Instance == null || entryPrefab == null || entryContainer == null) return;

            var leaderboard = ScoreManager.Instance.GetLeaderboard();
            int rank = 1;

            foreach (var score in leaderboard)
            {
                var entry = Instantiate(entryPrefab, entryContainer);
                var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{rank}";
                    texts[1].text = score.playerName;
                    texts[2].text = score.totalScore.ToString();
                }

                entries.Add(entry);
                rank++;
            }
        }
    }
}
