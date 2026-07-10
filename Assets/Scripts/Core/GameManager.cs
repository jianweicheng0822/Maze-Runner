using UnityEngine;

namespace MazeRunner.Core
{
    public enum GameState
    {
        Lobby,
        Countdown,
        Playing,
        CollectionComplete,
        GatherUp,
        RoundEnd,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private float roundEndDisplayTime = 5f;

        private GameState currentState = GameState.Lobby;
        public GameState CurrentState => currentState;

        public event System.Action<GameState> OnStateChanged;
        public event System.Action<float> OnCountdownTick;

        private float stateTimer;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartGame()
        {
            SetState(GameState.Countdown);
        }

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            stateTimer = 0f;

            switch (newState)
            {
                case GameState.Countdown:
                    stateTimer = countdownDuration;
                    break;
                case GameState.RoundEnd:
                    stateTimer = roundEndDisplayTime;
                    break;
            }

            OnStateChanged?.Invoke(currentState);
        }

        void Update()
        {
            switch (currentState)
            {
                case GameState.Countdown:
                    stateTimer -= Time.deltaTime;
                    OnCountdownTick?.Invoke(stateTimer);
                    if (stateTimer <= 0f)
                        SetState(GameState.Playing);
                    break;

                case GameState.RoundEnd:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                        SetState(GameState.Countdown); // Next round starts
                    break;
            }
        }

        public void OnCollectionComplete()
        {
            SetState(GameState.CollectionComplete);
            SetState(GameState.GatherUp);
        }

        public void OnAllPlayersGathered()
        {
            SetState(GameState.RoundEnd);
        }

        public void EndGame()
        {
            SetState(GameState.GameOver);
        }
    }
}
