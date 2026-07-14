using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;

    Vector2Int _gridPos = new Vector2Int(0, 0);
    Vector3 _targetWorldPos;
    bool _moving;

    void Start()
    {
        _targetWorldPos = transform.position;
    }

    void Update()
    {
        HandleInput();
        MoveToTarget();
        CheckKeyPickup();
    }

    void HandleInput()
    {
        if (_moving) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        int dx = 0, dy = 0;

        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) dy = 1;
        else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) dy = -1;
        else if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) dx = -1;
        else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) dx = 1;

        if (dx == 0 && dy == 0) return;

        int newX = _gridPos.x + dx;
        int newY = _gridPos.y + dy;

        if (GameManager.Instance.IsWithinGrid(newX, newY))
        {
            _gridPos = new Vector2Int(newX, newY);
            _targetWorldPos = GameManager.Instance.GridToWorld(newX, newY);
            _moving = true;
        }
    }

    void MoveToTarget()
    {
        if (!_moving) return;

        transform.position = Vector3.MoveTowards(transform.position, _targetWorldPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.01f)
        {
            transform.position = _targetWorldPos;
            _moving = false;
        }
    }

    void CheckKeyPickup()
    {
        if (_gridPos == GameManager.Instance.KeyPosition)
        {
            GameManager.Instance.OnKeyCollected();
        }
    }
}
