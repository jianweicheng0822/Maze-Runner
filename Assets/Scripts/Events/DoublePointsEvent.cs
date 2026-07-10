using UnityEngine;

namespace MazeRunner.Events
{
    [CreateAssetMenu(fileName = "DoublePointsEvent", menuName = "MazeRunner/Events/Double Points")]
    public class DoublePointsEvent : MazeEvent
    {
        private static bool isDoublePoints;
        public static bool IsActive => isDoublePoints;

        public override void Activate()
        {
            isDoublePoints = true;
            Debug.Log("[Event] Double Points activated!");
        }

        public override void Deactivate()
        {
            isDoublePoints = false;
            Debug.Log("[Event] Double Points ended.");
        }
    }
}
