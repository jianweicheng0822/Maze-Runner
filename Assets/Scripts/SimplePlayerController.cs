using UnityEngine;

// =============================================================
// SimplePlayerController.cs
// Attach this script to your "Player" Cube.
//
// What it does:
//   Lets you move a 3D Cube around using WASD or Arrow keys.
//   Uses Rigidbody physics so the cube collides with walls.
// =============================================================
public class SimplePlayerController : MonoBehaviour
{
    // moveSpeed shows up as a number field in Unity's Inspector.
    // Higher = faster. Start with 6 and tweak to your liking.
    [Header("Movement Settings")]
    [Tooltip("How fast the player moves.")]
    public float moveSpeed = 6f;

    // Private variables - the script manages these, you don't touch them.
    private Rigidbody rb;        // Reference to the physics component
    private Vector3 moveInput;   // Stores keyboard input each frame

    // ==========================================================
    // Start() runs ONCE when the game begins.
    // We grab a reference to the Rigidbody component here.
    // ==========================================================
    void Start()
    {
        // Find the Rigidbody component on this same GameObject
        rb = GetComponent<Rigidbody>();

        // If you forgot to add a Rigidbody in Unity, this error
        // will show up in the Console tab at the bottom
        if (rb == null)
        {
            Debug.LogError("No Rigidbody on " + gameObject.name +
                           "! Add one in the Inspector.");
        }

        // Freeze rotation so the cube doesn't tumble when it
        // bumps into things. We only want it to SLIDE.
        rb.freezeRotation = true;
    }

    // ==========================================================
    // Update() runs every single frame (60+ times per second).
    // We read keyboard input here.
    // ==========================================================
    void Update()
    {
        // GetAxisRaw("Horizontal"):
        //   A or Left Arrow  = -1
        //   D or Right Arrow  = +1
        //   Nothing pressed   =  0
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // GetAxisRaw("Vertical"):
        //   S or Down Arrow  = -1
        //   W or Up Arrow    = +1
        //   Nothing pressed  =  0
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Build a direction vector from the input
        // X = left/right, Y = 0 (no flying!), Z = forward/back
        moveInput = new Vector3(horizontalInput, 0f, verticalInput);

        // normalized prevents diagonal movement from being faster.
        // Without this, pressing W+D moves at 1.41x speed.
        moveInput = moveInput.normalized;
    }

    // ==========================================================
    // FixedUpdate() runs at a fixed interval (default 50x/sec).
    // All physics calculations go here, NOT in Update().
    // ==========================================================
    void FixedUpdate()
    {
        // New position = current + direction * speed * time step
        Vector3 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        // MovePosition is better than setting transform.position
        // because it properly handles physics collisions
        rb.MovePosition(newPosition);
    }
}
