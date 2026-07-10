using UnityEngine;

// =============================================================
// MovingTrap.cs
// Attach this script to your "TrapWall" Cube.
//
// Functionality: The trap stays still by default. Once the
// PhysicalButton is pressed, this wall moves back and forth
// (like a swinging pendulum / pushing wall trap).
// =============================================================
public class MovingTrap : MonoBehaviour
{
    // Direction of trap movement (default: left-right along X axis)
    [Header("Trap Movement Settings")]
    [Tooltip("Movement direction, default is left-right")]
    public Vector3 moveDirection = Vector3.right;

    // Distance of back-and-forth movement
    [Tooltip("Movement distance (units)")]
    public float moveDistance = 4f;

    // Movement speed
    [Tooltip("Movement speed")]
    public float moveSpeed = 3f;

    // Private variables
    private Vector3 startPosition;      // Starting position
    private bool isActivated = false;   // Whether the trap is activated

    void Start()
    {
        // Record the initial position
        startPosition = transform.position;
    }

    void Update()
    {
        // If the trap is not activated, do nothing
        if (!isActivated) return;

        // PingPong makes a value bounce between 0 and moveDistance,
        // like a ping-pong ball going back and forth — perfect for traps
        float pingPong = Mathf.PingPong(Time.time * moveSpeed, moveDistance);

        // New position = start position + direction * current offset
        transform.position = startPosition + moveDirection * pingPong;
    }

    // ==========================================================
    // This function is public so it can be called by
    // PhysicalButton's UnityEvent. You need to connect them
    // in the Inspector.
    // ==========================================================
    public void ActivateTrap()
    {
        isActivated = true;
        Debug.Log("Trap activated! Watch out!");
    }
}
