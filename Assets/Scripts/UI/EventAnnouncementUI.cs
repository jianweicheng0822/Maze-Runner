using UnityEngine;
using TMPro;
using MazeRunner.Events;

namespace MazeRunner.UI
{
    public class EventAnnouncementUI : MonoBehaviour
    {
        [SerializeField] private GameObject announcementPanel;
        [SerializeField] private TextMeshProUGUI announcementText;
        [SerializeField] private UnityEngine.UI.Image eventIcon;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private CanvasGroup canvasGroup;

        private float hideTimer;
        private bool showing;

        void Start()
        {
            if (RandomEventManager.Instance != null)
            {
                RandomEventManager.Instance.OnEventStarted += ShowAnnouncement;
                RandomEventManager.Instance.OnEventEnded += ShowEndAnnouncement;
            }

            if (announcementPanel != null)
                announcementPanel.SetActive(false);
        }

        void Update()
        {
            if (!showing) return;

            hideTimer -= Time.deltaTime;

            if (canvasGroup != null)
            {
                float fadeStart = 0.5f;
                if (hideTimer < fadeStart)
                    canvasGroup.alpha = Mathf.Clamp01(hideTimer / fadeStart);
            }

            if (hideTimer <= 0f)
            {
                showing = false;
                if (announcementPanel != null)
                    announcementPanel.SetActive(false);
            }
        }

        private void ShowAnnouncement(MazeEvent evt)
        {
            if (announcementPanel != null) announcementPanel.SetActive(true);
            if (announcementText != null) announcementText.text = evt.announcementText;
            if (eventIcon != null && evt.icon != null) eventIcon.sprite = evt.icon;
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            hideTimer = displayDuration;
            showing = true;
        }

        private void ShowEndAnnouncement(MazeEvent evt)
        {
            if (announcementPanel != null) announcementPanel.SetActive(true);
            if (announcementText != null) announcementText.text = $"{evt.eventName} ended!";
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            hideTimer = 2f;
            showing = true;
        }
    }
}
