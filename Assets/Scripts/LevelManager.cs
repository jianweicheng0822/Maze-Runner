using UnityEngine;

public static class LevelManager
{
    public static int CurrentLevel { get; private set; } = 1;

    // Maze size: 11 base + 2 per level
    public static int MazeSize => 11 + (CurrentLevel - 1) * 2;

    // Spikes: 5 base + 2 per level
    public static int SpikeCount => 5 + (CurrentLevel - 1) * 2;

    // Flashlight range shrinks: 5 base - 0.3 per level, min 2.5
    public static float FlashlightRange => Mathf.Max(5f - (CurrentLevel - 1) * 0.3f, 2.5f);

    // Point light radius shrinks slightly
    public static float PointLightRadius => Mathf.Max(2.5f - (CurrentLevel - 1) * 0.15f, 1.5f);

    public static void NextLevel()
    {
        CurrentLevel++;
    }

    public static void Reset()
    {
        CurrentLevel = 1;
    }
}
