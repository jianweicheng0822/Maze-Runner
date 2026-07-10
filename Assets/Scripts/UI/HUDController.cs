using UnityEngine;
using TMPro;
using MazeRunner.Core;

namespace MazeRunner.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private GameObject hudPanel;

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            if (RoundManager.Instance != null)
                RoundManager.Instance.OnItemDelivered += OnItemDelivered;

            if (ObjectiveSystem.Instance != null)
                ObjectiveSystem.Instance.OnObjectiveUpdated += OnObjectiveUpdated;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
        }

        void Update()
        {
            if (RoundManager.Instance != null && timerText != null)
            {
                float time = RoundManager.Instance.RoundTimer;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private void OnStateChanged(GameState state)
        {
            if (hudPanel != null)
                hudPanel.SetActive(state == GameState.Playing || state == GameState.GatherUp);
        }

        private void OnItemDelivered(int current, int target)
        {
            if (itemCountText != null)
                itemCountText.text = $"{current} / {target}";
        }

        private void OnObjectiveUpdated(string text)
        {
            if (objectiveText != null)
                objectiveText.text = text;
        }

        private void OnScoreChanged(int playerId, int newScore)
        {
            if (scoreText != null)
                scoreText.text = newScore.ToString();
        }
    }
}
