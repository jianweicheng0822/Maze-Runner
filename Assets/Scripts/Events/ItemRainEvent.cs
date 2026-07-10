using UnityEngine;

namespace MazeRunner.Events
{
    [CreateAssetMenu(fileName = "ItemRainEvent", menuName = "MazeRunner/Events/Item Rain")]
    public class ItemRainEvent : MazeEvent
    {
        public override void Activate()
        {
            var spawners = Object.FindObjectsByType<Items.ItemSpawner>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
                spawner.SpawnItems();

            Debug.Log("[Event] Item Rain activated!");
        }

        public override void Deactivate()
        {
            // Items stay - no cleanup needed
        }
    }
}
