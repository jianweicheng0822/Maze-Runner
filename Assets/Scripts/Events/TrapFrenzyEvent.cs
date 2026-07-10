using UnityEngine;

namespace MazeRunner.Events
{
    [CreateAssetMenu(fileName = "TrapFrenzyEvent", menuName = "MazeRunner/Events/Trap Frenzy")]
    public class TrapFrenzyEvent : MazeEvent
    {
        private Traps.TrapBase[] traps;

        public override void Activate()
        {
            traps = Object.FindObjectsByType<Traps.TrapBase>(FindObjectsSortMode.None);
            foreach (var trap in traps)
                trap.Activate();

            Debug.Log("[Event] Trap Frenzy activated!");
        }

        public override void Deactivate()
        {
            Debug.Log("[Event] Trap Frenzy ended.");
        }
    }
}
