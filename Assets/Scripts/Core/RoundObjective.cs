using UnityEngine;

namespace MazeRunner.Core
{
    [CreateAssetMenu(fileName = "NewRoundObjective", menuName = "MazeRunner/Round Objective")]
    public class RoundObjective : ScriptableObject
    {
        [Header("Primary Objective")]
        public string objectiveName = "Collect Items";
        public string description = "Find and deliver items to the base.";
        public int targetItemCount = 5;

        [Header("Time")]
        public float timeLimit = 180f; // 0 = no limit

        [Header("Scoring")]
        public int pointsPerItem = 100;
        public int timeBonusPerSecond = 5;
        public int completionBonus = 500;

        [Header("Bonus Objectives")]
        public BonusObjective[] bonusObjectives;
    }

    [System.Serializable]
    public class BonusObjective
    {
        public string description;
        public int bonusPoints;
        public bool completed;
    }
}
