using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MazeRunner.Core;

namespace MazeRunner.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerEntryPrefab;
        [SerializeField] private UnityEngine.UI.Button startButton;
        [SerializeField] private TextMeshProUGUI playerCountText;

        private readonly List<GameObject> playerEntries = new List<GameObject>();

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnStateChanged(GameState state)
        {
            if (lobbyPanel != null)
                lobbyPanel.SetActive(state == GameState.Lobby);
        }

        public void AddPlayer(string playerName)
        {
            if (playerEntryPrefab == null || playerListContainer == null) return;

            var entry = Instantiate(playerEntryPrefab, playerListContainer);
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = playerName;

            playerEntries.Add(entry);
            UpdatePlayerCount();
        }

        public void RemovePlayer(int index)
        {
            if (index < 0 || index >= playerEntries.Count) return;
            Destroy(playerEntries[index]);
            playerEntries.RemoveAt(index);
            UpdatePlayerCount();
        }

        private void UpdatePlayerCount()
        {
            if (playerCountText != null)
                playerCountText.text = $"Players: {playerEntries.Count}/8";
        }

        private void OnStartClicked()
        {
            GameManager.Instance?.StartGame();
        }
    }
}
