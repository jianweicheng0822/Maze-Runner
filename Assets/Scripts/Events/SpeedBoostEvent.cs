using UnityEngine;

namespace MazeRunner.Events
{
    [CreateAssetMenu(fileName = "SpeedBoostEvent", menuName = "MazeRunner/Events/Speed Boost")]
    public class SpeedBoostEvent : MazeEvent
    {
        [SerializeField] private float speedMultiplier = 1.5f;

        private Player.PlayerController[] players;

        public override void Activate()
        {
            players = Object.FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players)
                p.SpeedMultiplier = speedMultiplier;

            Debug.Log("[Event] Speed Boost activated!");
        }

        public override void Deactivate()
        {
            if (players == null) return;
            foreach (var p in players)
            {
                if (p != null) p.SpeedMultiplier = 1f;
            }
            Debug.Log("[Event] Speed Boost ended.");
        }
    }
}
