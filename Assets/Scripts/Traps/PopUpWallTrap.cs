using System.Collections;
using UnityEngine;

namespace MazeRunner.Traps
{
    /// <summary>
    /// A wall that rises from the floor on a timer or via button activation,
    /// stays for a duration, then sinks back down. Blocks paths temporarily.
    /// </summary>
    public class PopUpWallTrap : TrapBase
    {
        [Header("Pop-Up Wall Settings")]
        [SerializeField] private float riseHeight = 3f;
        [SerializeField] private float riseSpeed = 4f;
        [SerializeField] private float stayDuration = 3f;
        [SerializeField] private bool useTimer = false;
        [SerializeField] private float timerInterval = 5f;

        private Vector3 loweredPosition;
        private Vector3 raisedPosition;
        private Coroutine activeRoutine;
        private bool isRaised;

        private void Start()
        {
            loweredPosition = transform.position;
            raisedPosition = loweredPosition + Vector3.up * riseHeight;

            if (useTimer)
            {
                StartTimerCycle();
            }
        }

        public override void Activate()
        {
            isActive = true;

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(RiseAndFallSequence());
        }

        public override void Deactivate()
        {
            isActive = false;

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(LerpToPosition(loweredPosition));
        }

        protected override void OnPlayerHit(MazeRunner.Player.PlayerController player)
        {
            // The wall blocks movement physically via its collider.
            // No additional knockback needed; it is a passive obstacle.
        }

        private void StartTimerCycle()
        {
            StartCoroutine(TimerCycleRoutine());
        }

        private IEnumerator TimerCycleRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(timerInterval);

                if (isActive)
                {
                    yield return RiseAndFallSequence();
                }
            }
        }

        private IEnumerator RiseAndFallSequence()
        {
            isRaised = true;

            // Rise
            yield return LerpToPosition(raisedPosition);

            // Stay
            yield return new WaitForSeconds(stayDuration);

            // Lower
            yield return LerpToPosition(loweredPosition);

            isRaised = false;
        }

        private IEnumerator LerpToPosition(Vector3 target)
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, riseSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
        }
    }
}
