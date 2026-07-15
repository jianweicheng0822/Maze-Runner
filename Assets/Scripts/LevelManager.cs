using UnityEngine;

public static class LevelManager
{
    public const int MaxLevel = 20;

    public static int CurrentLevel { get; private set; } = 1;

    // Per-level tuning table
    // (mazeSize, spikes, spirits, alcoves, pointLight, flashlight, loopPct, chaseSpeed, drainRate)
    static readonly (int maze, int spikes, int spirits, int alcoves,
                      float pointLight, float flashlight, float loopPct,
                      float chaseSpeed, float drainRate)[] Table =
    {
        //  maze  spk  spr  alc  pLight  flash  loops  chase  drain
        (   11,    3,   1,   5,  3.00f,  5.5f,  0.15f, 2.4f,   6f ),  // Lv1
        (   11,    4,   2,   5,  2.80f,  5.2f,  0.14f, 2.5f,   6f ),  // Lv2
        (   13,    5,   2,   6,  2.60f,  5.0f,  0.13f, 2.6f,   7f ),  // Lv3
        (   13,    6,   3,   6,  2.45f,  4.7f,  0.12f, 2.6f,   7f ),  // Lv4
        (   15,    8,   4,   6,  2.30f,  4.4f,  0.12f, 2.7f,   7f ),  // Lv5
        (   15,   10,   5,   6,  2.20f,  4.1f,  0.11f, 2.7f,   8f ),  // Lv6
        (   17,   12,   6,   7,  2.10f,  3.8f,  0.10f, 2.8f,   8f ),  // Lv7
        (   17,   14,   7,   7,  2.00f,  3.5f,  0.10f, 2.8f,   8f ),  // Lv8
        (   19,   16,   8,   7,  1.90f,  3.3f,  0.09f, 2.9f,   9f ),  // Lv9
        (   19,   18,   9,   7,  1.80f,  3.1f,  0.09f, 2.9f,   9f ),  // Lv10
        (   21,   20,  10,   7,  1.70f,  2.9f,  0.08f, 3.0f,  10f ),  // Lv11
        (   21,   22,  11,   7,  1.60f,  2.8f,  0.08f, 3.0f,  10f ),  // Lv12
        (   23,   24,  13,   8,  1.50f,  2.7f,  0.07f, 3.1f,  10f ),  // Lv13
        (   23,   26,  14,   8,  1.45f,  2.6f,  0.07f, 3.1f,  11f ),  // Lv14
        (   25,   28,  16,   8,  1.40f,  2.5f,  0.06f, 3.2f,  11f ),  // Lv15
        (   25,   30,  18,   8,  1.35f,  2.5f,  0.06f, 3.2f,  12f ),  // Lv16
        (   27,   32,  20,   8,  1.30f,  2.5f,  0.05f, 3.3f,  12f ),  // Lv17
        (   27,   34,  22,   8,  1.25f,  2.5f,  0.05f, 3.3f,  13f ),  // Lv18
        (   29,   36,  24,   8,  1.20f,  2.5f,  0.04f, 3.4f,  14f ),  // Lv19
        (   29,   38,  26,   8,  1.15f,  2.5f,  0.04f, 3.5f,  15f ),  // Lv20
    };

    static int Index => Mathf.Clamp(CurrentLevel - 1, 0, Table.Length - 1);

    public static int MazeSize => Table[Index].maze;
    public static int SpikeCount => Table[Index].spikes;
    public static int BeamCount => Table[Index].spirits;
    public static int AlcoveCount => Table[Index].alcoves;
    public static float PointLightRadius => Table[Index].pointLight;
    public static float FlashlightRange => Table[Index].flashlight;
    public static float LoopPercent => Table[Index].loopPct;
    public static float ChaseSpeed => Table[Index].chaseSpeed;
    public static float DrainRate => Table[Index].drainRate;

    public static void NextLevel()
    {
        if (CurrentLevel < MaxLevel)
            CurrentLevel++;
    }

    public static void Reset()
    {
        CurrentLevel = 1;
    }
}
