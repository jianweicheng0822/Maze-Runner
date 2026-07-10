using System.Collections.Generic;
using UnityEngine;

namespace MazeRunner.Items
{
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<CollectibleItem> itemPrefabs = new List<CollectibleItem>();
        [SerializeField] private bool useChildTransformsAsSpawnPoints = true;

        private List<Transform> spawnPoints = new List<Transform>();
        private List<GameObject> spawnedItems = new List<GameObject>();

        void Awake()
        {
            GatherSpawnPoints();
        }

        private void GatherSpawnPoints()
        {
            spawnPoints.Clear();

            if (useChildTransformsAsSpawnPoints)
            {
                foreach (Transform child in transform)
                {
                    spawnPoints.Add(child);
                }
            }

            // If no child spawn points, use this transform as the sole spawn point
            if (spawnPoints.Count == 0)
            {
                spawnPoints.Add(transform);
            }
        }

        /// <summary>
        /// Spawns items at each spawn point, randomly selected from the prefab list.
        /// Called by the round manager at the start of a round.
        /// </summary>
        public void SpawnItems()
        {
            if (itemPrefabs == null || itemPrefabs.Count == 0)
            {
                Debug.LogWarning($"[ItemSpawner] No item prefabs assigned on {gameObject.name}.");
                return;
            }

            foreach (Transform point in spawnPoints)
            {
                CollectibleItem prefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];
                GameObject spawned = Instantiate(prefab.gameObject, point.position, point.rotation);
                spawnedItems.Add(spawned);
            }
        }

        /// <summary>
        /// Destroys all items that were spawned by this spawner and clears the list.
        /// </summary>
        public void ClearSpawnedItems()
        {
            foreach (GameObject item in spawnedItems)
            {
                if (item != null)
                    Destroy(item);
            }
            spawnedItems.Clear();
        }
    }
}
