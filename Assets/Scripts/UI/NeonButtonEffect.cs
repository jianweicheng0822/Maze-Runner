using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MazeRunner.UI
{
    public class NeonButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image borderImage;
        [SerializeField] private Image backgroundImage;

        private static readonly Color IdleBorderColor = new Color(0.2f, 0.9f, 0.9f, 0.6f);
        private static readonly Color HoverBorderColor = new Color(0.2f, 0.9f, 0.9f, 1f);
        private static readonly Color IdleBackgroundColor = new Color(0.05f, 0.08f, 0.15f, 0.75f);
        private static readonly Color HoverBackgroundColor = new Color(0.08f, 0.12f, 0.2f, 0.85f);
        private static readonly Color DisabledBorderColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        private Vector3 originalScale;
        private float targetScale = 1f;
        private Color targetBorderColor;
        private Color targetBackgroundColor;
        private bool isDisabled;
        private float flashTimer;

        void Awake()
        {
            originalScale = transform.localScale;
            targetBorderColor = IdleBorderColor;
            targetBackgroundColor = IdleBackgroundColor;
        }

        void Start()
        {
            var button = GetComponentInParent<Button>();
            if (button != null)
                isDisabled = !button.interactable;

            if (isDisabled)
            {
                targetBorderColor = DisabledBorderColor;
                if (borderImage != null)
                    borderImage.color = DisabledBorderColor;
            }
            else
            {
                if (borderImage != null)
                    borderImage.color = IdleBorderColor;
            }

            if (backgroundImage != null)
                backgroundImage.color = IdleBackgroundColor;
        }

        void Update()
        {
            if (isDisabled) return;

            float speed = 8f * Time.unscaledDeltaTime;

            if (borderImage != null)
                borderImage.color = Color.Lerp(borderImage.color, targetBorderColor, speed);

            if (backgroundImage != null)
                backgroundImage.color = Color.Lerp(backgroundImage.color, targetBackgroundColor, speed);

            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * targetScale, speed * 1.5f);

            if (flashTimer > 0f)
            {
                flashTimer -= Time.unscaledDeltaTime;
                if (backgroundImage != null)
                {
                    float flash = flashTimer / 0.1f;
                    backgroundImage.color = Color.Lerp(backgroundImage.color, Color.white, flash * 0.3f);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isDisabled) return;
            targetScale = 1.03f;
            targetBorderColor = HoverBorderColor;
            targetBackgroundColor = HoverBackgroundColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isDisabled) return;
            targetScale = 1f;
            targetBorderColor = IdleBorderColor;
            targetBackgroundColor = IdleBackgroundColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isDisabled) return;
            targetScale = 0.97f;
            flashTimer = 0.1f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isDisabled) return;
            targetScale = 1.03f;
        }
    }
}
