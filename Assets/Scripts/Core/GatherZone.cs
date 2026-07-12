using System.Collections.Generic;
using UnityEngine;

namespace MazeRunner.Core
{
    public class GatherZone : MonoBehaviour
    {
        [SerializeField] private GameObject visualIndicator; // portal/gate visual
        [SerializeField] private ParticleSystem activationEffect;

        private readonly HashSet<GameObject> arrivedPlayers = new HashSet<GameObject>();
        private bool isActive;
        private int totalPlayerCount;

        public bool IsActive => isActive;
        public int ArrivedCount => arrivedPlayers.Count;
        public int TotalPlayers => totalPlayerCount;

        public event System.Action<int, int> OnPlayerArrived; // arrived, total
        public event System.Action OnAllPlayersGathered;

        void Start()
        {
            SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChange;
            }
        }

        private void HandleStateChange(GameState state)
        {
            if (state == GameState.GatherUp)
                SetActive(true);
            else if (state == GameState.RoundEnd || state == GameState.Countdown)
                Reset();
        }

        public void SetActive(bool active)
        {
            isActive = active;
            if (visualIndicator != null)
                visualIndicator.SetActive(active);

            if (active && activationEffect != null)
                activationEffect.Play();
        }

        public void SetTotalPlayers(int count)
        {
            totalPlayerCount = count;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            if (!other.CompareTag(GameTags.Player)) return;

            if (arrivedPlayers.Add(other.gameObject))
            {
                OnPlayerArrived?.Invoke(arrivedPlayers.Count, totalPlayerCount);

                if (arrivedPlayers.Count >= totalPlayerCount && totalPlayerCount > 0)
                {
                    OnAllPlayersGathered?.Invoke();
                    GameManager.Instance?.OnAllPlayersGathered();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!isActive) return;
            if (!other.CompareTag(GameTags.Player)) return;

            if (arrivedPlayers.Remove(other.gameObject))
            {
                OnPlayerArrived?.Invoke(arrivedPlayers.Count, totalPlayerCount);
            }
        }

        public void Reset()
        {
            arrivedPlayers.Clear();
            SetActive(false);
        }
    }
}
