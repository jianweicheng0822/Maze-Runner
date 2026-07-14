using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float sprintMultiplier = 1.6f;

    Rigidbody2D _rb;
    Vector2 _moveInput;
    bool _sprinting;

    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
    public bool IsSprinting => _sprinting && _moveInput.sqrMagnitude > 0.01f;

    // 0 = still, ~0.6 = walking, 1.0 = sprinting
    public float SpeedRatio { get; private set; }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        _moveInput = Vector2.zero;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) _moveInput.y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) _moveInput.y -= 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) _moveInput.x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) _moveInput.x += 1f;

        // Normalize so diagonal movement isn't faster
        if (_moveInput.sqrMagnitude > 1f)
            _moveInput.Normalize();

        _sprinting = kb.leftShiftKey.isPressed;

        if (_moveInput != Vector2.zero)
            LastMoveDirection = _moveInput.normalized;

        SpeedRatio = _moveInput.magnitude * (_sprinting ? 1f : 0.625f);
    }

    void FixedUpdate()
    {
        float speed = _sprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        _rb.MovePosition(_rb.position + _moveInput * speed * Time.fixedDeltaTime);
    }
}
