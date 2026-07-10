using System.Collections.Generic;
using UnityEngine;

namespace MazeRunner.Core
{
    public class SpawnPointManager : MonoBehaviour
    {
        public static SpawnPointManager Instance { get; private set; }

        [SerializeField] private Transform[] fixedSpawnPoints;

        private readonly List<Transform> allSpawnPoints = new List<Transform>();

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
            if (fixedSpawnPoints != null)
                allSpawnPoints.AddRange(fixedSpawnPoints);
        }

        public void RegisterSpawnPoint(Transform point)
        {
            if (!allSpawnPoints.Contains(point))
                allSpawnPoints.Add(point);
        }

        public void ClearDynamicSpawnPoints()
        {
            allSpawnPoints.Clear();
            if (fixedSpawnPoints != null)
                allSpawnPoints.AddRange(fixedSpawnPoints);
        }

        public Vector3 GetSpawnPosition(int playerIndex)
        {
            if (allSpawnPoints.Count == 0)
                return Vector3.zero;

            int index = playerIndex % allSpawnPoints.Count;
            return allSpawnPoints[index].position;
        }

        public Quaternion GetSpawnRotation(int playerIndex)
        {
            if (allSpawnPoints.Count == 0)
                return Quaternion.identity;

            int index = playerIndex % allSpawnPoints.Count;
            return allSpawnPoints[index].rotation;
        }
    }
}
