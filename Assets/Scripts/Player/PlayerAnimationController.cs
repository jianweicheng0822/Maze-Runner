using UnityEngine;

namespace MazeRunner.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator animator;
        private PlayerController controller;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsCarryingHash = Animator.StringToHash("IsCarrying");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int LandHash = Animator.StringToHash("Land");
        private static readonly int HitHash = Animator.StringToHash("Hit");

        void Start()
        {
            controller = GetComponent<PlayerController>();
            animator = GetComponentInChildren<Animator>();

            if (animator == null) return;

            controller.OnJumped += () => animator.SetTrigger(JumpHash);
            controller.OnLanded += () => animator.SetTrigger(LandHash);
        }

        void Update()
        {
            if (animator == null) return;

            animator.SetFloat(SpeedHash, controller.CurrentSpeed);
            animator.SetBool(IsGroundedHash, controller.IsGrounded);
            animator.SetBool(IsCarryingHash, controller.IsCarrying);
        }

        public void PlayHitAnimation()
        {
            if (animator != null)
                animator.SetTrigger(HitHash);
        }
    }
}
