using UnityEngine;
using MazeRunner.Core;
using MazeRunner.Player;

namespace MazeRunner.Traps
{
    /// <summary>
    /// A zone that slows players while they are inside it.
    /// Applies a SlowEffect via the PlayerEffectHandler system.
    /// </summary>
    public class SlimeZone : MonoBehaviour
    {
        [Header("Slime Settings")]
        [SerializeField] private float slowMultiplier = 0.4f;
        [SerializeField] private float effectDuration = 0.5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GameTags.Player))
            {
                ApplySlowEffect(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTags.Player))
            {
                ApplySlowEffect(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // The effect is time-based and will expire naturally via PlayerEffectHandler.
            // No explicit removal needed on exit since the short duration handles cleanup.
        }

        private void ApplySlowEffect(Collider other)
        {
            PlayerEffectHandler effectHandler = other.GetComponent<PlayerEffectHandler>();
            if (effectHandler == null)
                effectHandler = other.GetComponentInParent<PlayerEffectHandler>();

            if (effectHandler != null)
            {
                effectHandler.AddEffect(new SlowEffect(slowMultiplier, effectDuration));
            }
        }

        /// <summary>
        /// A speed-reduction effect applied to players inside the slime zone.
        /// Non-stackable: re-entering or staying refreshes the duration.
        /// </summary>
        public class SlowEffect : IPlayerEffect
        {
            private readonly float slowMultiplier;
            private readonly float duration;

            public string EffectName => "SlimeSlow";
            public float Duration => duration;
            public bool IsStackable => false;

            public SlowEffect(float slowMultiplier, float duration)
            {
                this.slowMultiplier = slowMultiplier;
                this.duration = duration;
            }

            public void Apply(PlayerController player)
            {
                player.SpeedMultiplier *= slowMultiplier;
            }

            public void Remove(PlayerController player)
            {
                player.SpeedMultiplier /= slowMultiplier;
            }

            public void Tick(PlayerController player, float deltaTime)
            {
                // No per-tick behavior needed.
            }
        }
    }
}
