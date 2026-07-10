using UnityEngine;

namespace MazeRunner.Core
{
    public class ObjectiveSystem : MonoBehaviour
    {
        public static ObjectiveSystem Instance { get; private set; }

        private RoundObjective currentObjective;

        public string PrimaryObjectiveText
        {
            get
            {
                if (currentObjective == null) return "";
                int delivered = RoundManager.Instance != null ? RoundManager.Instance.ItemsDelivered : 0;
                return $"{currentObjective.description} ({delivered}/{currentObjective.targetItemCount})";
            }
        }

        public event System.Action<string> OnObjectiveUpdated;
        public event System.Action<BonusObjective> OnBonusCompleted;

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
            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.OnRoundStarted += OnRoundStarted;
                RoundManager.Instance.OnItemDelivered += OnDelivery;
            }
        }

        private void OnRoundStarted(int roundIndex)
        {
            currentObjective = RoundManager.Instance.CurrentObjective;
            OnObjectiveUpdated?.Invoke(PrimaryObjectiveText);
        }

        private void OnDelivery(int current, int target)
        {
            OnObjectiveUpdated?.Invoke(PrimaryObjectiveText);
        }

        public void CompleteBonusObjective(int index)
        {
            if (currentObjective == null || currentObjective.bonusObjectives == null) return;
            if (index < 0 || index >= currentObjective.bonusObjectives.Length) return;

            var bonus = currentObjective.bonusObjectives[index];
            if (bonus.completed) return;

            bonus.completed = true;
            OnBonusCompleted?.Invoke(bonus);
        }
    }
}
