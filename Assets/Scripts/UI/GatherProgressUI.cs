using UnityEngine;
using TMPro;
using MazeRunner.Core;

namespace MazeRunner.UI
{
    public class GatherProgressUI : MonoBehaviour
    {
        [SerializeField] private GameObject gatherPanel;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private UnityEngine.UI.Slider progressSlider;
        [SerializeField] private GatherZone gatherZone;

        void Start()
        {
            if (gatherPanel != null)
                gatherPanel.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            if (gatherZone != null)
                gatherZone.OnPlayerArrived += OnPlayerArrived;
        }

        private void OnStateChanged(GameState state)
        {
            if (gatherPanel != null)
                gatherPanel.SetActive(state == GameState.GatherUp);
        }

        private void OnPlayerArrived(int arrived, int total)
        {
            if (progressText != null)
                progressText.text = $"Gather at portal! {arrived}/{total} players";

            if (progressSlider != null)
                progressSlider.value = total > 0 ? (float)arrived / total : 0f;
        }
    }
}
