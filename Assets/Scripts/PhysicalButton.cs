using UnityEngine;
using UnityEngine.Events;

// =============================================================
// PhysicalButton.cs
// Attach this script to your "Button" Cylinder.
//
// Functionality: When the Player enters the trigger area of
// this cylinder, the button sinks down like a cartoon press,
// then fires an event (e.g. activating a trap).
// =============================================================
public class PhysicalButton : MonoBehaviour
{
    // How far the button sinks when pressed
    [Header("Button Settings")]
    [Tooltip("How far the button sinks when pressed")]
    public float sinkAmount = 0.3f;

    // Sink animation speed, higher = faster
    [Tooltip("Sink animation speed")]
    public float sinkSpeed = 3f;

    // ---- Key part: Unity Event System ----
    // This shows a field in the Inspector where you can drag in
    // TrapWall and tell Unity "when the button is pressed, run a function"
    [Header("Button Event")]
    [Tooltip("Event fired when the button is pressed (drag TrapWall here)")]
    public UnityEvent onButtonPressed;

    // Private variables
    private Vector3 originalPosition;   // Remember the button's original position
    private Vector3 sunkPosition;       // The button's position after sinking
    private bool isPressed = false;     // Whether the button has been pressed
    private bool isSinking = false;     // Whether the sink animation is playing

    void Start()
    {
        // Record the initial position
        originalPosition = transform.position;

        // Calculate the sunk position (decreasing Y = sinking toward the ground)
        sunkPosition = originalPosition + Vector3.down * sinkAmount;
    }

    void Update()
    {
        // If sinking, smoothly move toward the target position
        if (isSinking)
        {
            // Lerp = linear interpolation, makes movement smooth instead of instant
            transform.position = Vector3.Lerp(
                transform.position,  // Current position
                sunkPosition,        // Target position
                sinkSpeed * Time.deltaTime  // Speed
            );
        }
    }

    // ==========================================================
    // OnTriggerEnter is a special Unity function.
    // It is called automatically when another object with a
    // Rigidbody enters this object's Trigger Collider.
    // ==========================================================
    void OnTriggerEnter(Collider other)
    {
        // Prevent repeated triggers (button can only be pressed once)
        if (isPressed) return;

        // Check if the entering object is the Player
        // (identified by name, so your cube must be named "Player")
        if (other.gameObject.name == "Player")
        {
            isPressed = true;   // Mark as pressed
            isSinking = true;   // Start the sink animation

            // Print a message to the Console for debugging
            Debug.Log("Button was pressed!");

            // Fire the event! This runs all functions connected in the Inspector
            // (e.g. activating the trap)
            onButtonPressed.Invoke();
        }
    }
}
