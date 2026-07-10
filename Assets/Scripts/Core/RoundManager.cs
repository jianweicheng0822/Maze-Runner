using UnityEngine;
using MazeRunner.Items;

namespace MazeRunner.Core
{
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager Instance { get; private set; }

        [Header("Round Settings")]
        [SerializeField] private RoundObjective[] roundObjectives;
        [SerializeField] private int startingRound = 0;

        private int currentRoundIndex;
        private int itemsDelivered;
        private float roundTimer;
        private bool roundActive;

        public RoundObjective CurrentObjective =>
            roundObjectives != null && currentRoundIndex < roundObjectives.Length
                ? roundObjectives[currentRoundIndex]
                : null;

        public int ItemsDelivered => itemsDelivered;
        public float RoundTimer => roundTimer;
        public int CurrentRound => currentRoundIndex + 1;
        public bool RoundActive => roundActive;

        public event System.Action<int> OnRoundStarted;
        public event System.Action<int> OnRoundEnded;
        public event System.Action<int, int> OnItemDelivered; // current, target

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
            currentRoundIndex = startingRound;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChange;
            }
        }

        void Update()
        {
            if (!roundActive) return;

            var objective = CurrentObjective;
            if (objective != null && objective.timeLimit > 0)
            {
                roundTimer -= Time.deltaTime;
                if (roundTimer <= 0f)
                {
                    roundTimer = 0f;
                    EndRound();
                }
            }
        }

        private void HandleStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    StartRound();
                    break;
                case GameState.RoundEnd:
                    EndRound();
                    break;
            }
        }

        public void StartRound()
        {
            itemsDelivered = 0;
            roundActive = true;

            var objective = CurrentObjective;
            if (objective != null)
                roundTimer = objective.timeLimit;

            // Spawn items via ItemSpawners in scene
            var spawners = FindObjectsByType<ItemSpawner>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
                spawner.SpawnItems();

            OnRoundStarted?.Invoke(currentRoundIndex);
        }

        public void RegisterItemDelivered()
        {
            itemsDelivered++;

            var objective = CurrentObjective;
            int target = objective != null ? objective.targetItemCount : 5;
            OnItemDelivered?.Invoke(itemsDelivered, target);

            if (itemsDelivered >= target)
            {
                GameManager.Instance?.OnCollectionComplete();
            }
        }

        private void EndRound()
        {
            roundActive = false;

            // Award time bonus
            var objective = CurrentObjective;
            if (objective != null && roundTimer > 0)
            {
                int timeBonus = Mathf.FloorToInt(roundTimer) * objective.timeBonusPerSecond;
                // Time bonus distributed to all players would go through ScoreManager
            }

            OnRoundEnded?.Invoke(currentRoundIndex);
            currentRoundIndex++;
        }
    }
}
