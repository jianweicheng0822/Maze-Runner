using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using MazeRunner.Core;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// An interactable button that can be pressed by players via interaction
    /// or by walking over it (trigger mode). Supports toggle and one-shot modes.
    /// Fires UnityEvents on activation and deactivation.
    /// </summary>
    public class ButtonInteractable : MonoBehaviour, IInteractable
    {
        [Header("Button Settings")]
        [SerializeField] private float sinkAmount = 0.3f;
        [SerializeField] private float sinkSpeed = 5f;
        [SerializeField] private string promptText = "Press Button";

        [Header("Mode")]
        [SerializeField] private bool isToggle = false;
        [SerializeField] private bool walkOnActivation = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onActivated;
        [SerializeField] private UnityEvent onDeactivated;

        private Vector3 originalPosition;
        private Vector3 sunkPosition;
        private bool isPressed;
        private Coroutine animationRoutine;

        public bool IsPressed => isPressed;

        private void Start()
        {
            originalPosition = transform.localPosition;
            sunkPosition = originalPosition + Vector3.down * sinkAmount;
        }

        // -- IInteractable implementation --

        public void Interact(PlayerInteraction player)
        {
            PerformPress();
        }

        public string GetPromptText()
        {
            if (isToggle && isPressed)
                return "Release Button";
            return promptText;
        }

        public bool CanInteract(PlayerInteraction player)
        {
            // In one-shot mode, the button can only be pressed once.
            if (!isToggle && isPressed)
                return false;

            return true;
        }

        // -- Trigger-based activation (walk-on button) --

        private void OnTriggerEnter(Collider other)
        {
            if (!walkOnActivation) return;

            if (other.CompareTag(GameTags.Player))
            {
                PerformPress();
            }
        }

        // -- Core logic --

        private void PerformPress()
        {
            if (isToggle)
            {
                if (isPressed)
                {
                    // Deactivate
                    isPressed = false;
                    AnimateTo(originalPosition);
                    onDeactivated?.Invoke();
                }
                else
                {
                    // Activate
                    isPressed = true;
                    AnimateTo(sunkPosition);
                    onActivated?.Invoke();
                }
            }
            else
            {
                if (isPressed) return;

                isPressed = true;
                AnimateTo(sunkPosition);
                onActivated?.Invoke();
            }
        }

        /// <summary>
        /// Public reset method so external systems can restore the button.
        /// </summary>
        public void ResetButton()
        {
            isPressed = false;
            AnimateTo(originalPosition);
        }

        private void AnimateTo(Vector3 target)
        {
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);

            animationRoutine = StartCoroutine(SinkAnimation(target));
        }

        private IEnumerator SinkAnimation(Vector3 target)
        {
            while (Vector3.Distance(transform.localPosition, target) > 0.001f)
            {
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition, target, sinkSpeed * Time.deltaTime);
                yield return null;
            }

            transform.localPosition = target;
            animationRoutine = null;
        }
    }
}
