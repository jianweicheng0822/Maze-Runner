using UnityEngine;
using TMPro;
using MazeRunner.Core;

namespace MazeRunner.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Player.PlayerInteraction playerInteraction;

        void Start()
        {
            if (playerInteraction != null)
                playerInteraction.OnTargetChanged += OnTargetChanged;

            if (promptPanel != null)
                promptPanel.SetActive(false);
        }

        private void OnTargetChanged(IInteractable target)
        {
            if (target != null)
            {
                if (promptPanel != null) promptPanel.SetActive(true);
                if (promptText != null) promptText.text = target.GetPromptText();
            }
            else
            {
                if (promptPanel != null) promptPanel.SetActive(false);
            }
        }

        public void SetPlayer(Player.PlayerInteraction interaction)
        {
            if (playerInteraction != null)
                playerInteraction.OnTargetChanged -= OnTargetChanged;

            playerInteraction = interaction;

            if (playerInteraction != null)
                playerInteraction.OnTargetChanged += OnTargetChanged;
        }
    }
}
