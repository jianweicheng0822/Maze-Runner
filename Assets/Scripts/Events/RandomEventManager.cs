using System.Collections.Generic;
using UnityEngine;

namespace MazeRunner.Events
{
    public class RandomEventManager : MonoBehaviour
    {
        public static RandomEventManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float minInterval = 30f;
        [SerializeField] private float maxInterval = 60f;
        [SerializeField] private MazeEvent[] possibleEvents;

        private float nextEventTimer;
        private MazeEvent activeEvent;
        private float activeEventTimer;

        public MazeEvent ActiveEvent => activeEvent;
        public event System.Action<MazeEvent> OnEventStarted;
        public event System.Action<MazeEvent> OnEventEnded;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            ResetTimer();
        }

        void Update()
        {
            if (Core.GameManager.Instance == null ||
                Core.GameManager.Instance.CurrentState != Core.GameState.Playing) return;

            // Active event tick
            if (activeEvent != null)
            {
                activeEvent.Tick(Time.deltaTime);
                activeEventTimer -= Time.deltaTime;
                if (activeEventTimer <= 0f)
                    EndActiveEvent();
                return;
            }

            // Timer for next event
            nextEventTimer -= Time.deltaTime;
            if (nextEventTimer <= 0f)
            {
                TriggerRandomEvent();
                ResetTimer();
            }
        }

        private void TriggerRandomEvent()
        {
            if (possibleEvents == null || possibleEvents.Length == 0) return;

            MazeEvent chosen = WeightedRandom();
            if (chosen == null) return;

            activeEvent = chosen;
            activeEventTimer = chosen.duration;
            chosen.Activate();
            OnEventStarted?.Invoke(chosen);
        }

        private MazeEvent WeightedRandom()
        {
            float totalWeight = 0f;
            foreach (var e in possibleEvents)
                totalWeight += e.weight;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var e in possibleEvents)
            {
                cumulative += e.weight;
                if (roll <= cumulative) return e;
            }

            return possibleEvents[0];
        }

        private void EndActiveEvent()
        {
            if (activeEvent == null) return;
            activeEvent.Deactivate();
            OnEventEnded?.Invoke(activeEvent);
            activeEvent = null;
        }

        private void ResetTimer()
        {
            nextEventTimer = Random.Range(minInterval, maxInterval);
        }

        public void ForceEvent(MazeEvent mazeEvent)
        {
            EndActiveEvent();
            activeEvent = mazeEvent;
            activeEventTimer = mazeEvent.duration;
            mazeEvent.Activate();
            OnEventStarted?.Invoke(mazeEvent);
        }
    }
}
