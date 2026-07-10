using UnityEngine;

namespace MazeRunner.Events
{
    [CreateAssetMenu(fileName = "WallShuffleEvent", menuName = "MazeRunner/Events/Wall Shuffle")]
    public class WallShuffleEvent : MazeEvent
    {
        public override void Activate()
        {
            var builder = Object.FindFirstObjectByType<Maze.MazeBuilder>();
            if (builder != null)
            {
                int newSeed = Random.Range(0, int.MaxValue);
                builder.GenerateAndBuild(newSeed);
            }

            Debug.Log("[Event] Wall Shuffle activated!");
        }

        public override void Deactivate()
        {
            // Walls stay shuffled until round end
        }
    }
}
