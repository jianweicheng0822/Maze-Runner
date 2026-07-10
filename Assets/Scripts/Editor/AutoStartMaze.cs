using UnityEngine;
using MazeRunner.Maze;

/// <summary>
/// Simple helper that triggers maze generation on Start.
/// Attached automatically by the editor setup script.
/// </summary>
public class AutoStartMaze : MonoBehaviour
{
    void Start()
    {
        var builder = GetComponent<MazeBuilder>();
        if (builder != null)
        {
            builder.GenerateAndBuild();
            Debug.Log("Maze generated! Seed-based procedural maze is ready.");
        }
    }
}
