using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4f;

    Rigidbody2D _rb;
    Vector2 _moveInput;

    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;

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

        if (_moveInput != Vector2.zero)
            LastMoveDirection = _moveInput.normalized;
    }

    void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
