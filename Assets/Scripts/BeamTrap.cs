using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BeamTrap : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float patrolSpeed = 1.5f;
    [SerializeField] float chaseSpeed = 2.8f;

    [Header("Chase")]
    [SerializeField] float drainRate = 8f;         // HP/light drained per second when close
    [SerializeField] float drainRange = 1.2f;       // distance within which draining occurs
    [SerializeField] float maxChaseDuration = 6f;   // seconds before giving up
    [SerializeField] float pathRecalcInterval = 0.4f;

    [Header("Patrol")]
    [SerializeField] float stopDurationMin = 1f;
    [SerializeField] float stopDurationMax = 2.5f;
    [SerializeField] float stopChance = 0.15f;
    [SerializeField] float turnAtJunctionChance = 0.3f;

    enum State { Patrol, Chase }
    State _state = State.Patrol;

    int[,] _grid;
    int _gridWidth, _gridHeight;

    Vector2Int _currentTile;
    Vector2Int _targetTile;
    Vector2Int _facingDir;
    float _stopTimer;
    Rigidbody2D _rb;
    Transform _player;
    PlayerMovement _playerMovement;
    PlayerHealth _playerHealth;
    SpriteRenderer _pupilSr;
    float _targetAngle;
    float _currentAngle;

    // Chase pathfinding
    float _nextPathCalcTime;
    float _chaseStartTime;
    float _drainAccumulator;
    List<Vector2Int> _chasePath = new List<Vector2Int>();
    int _chasePathIndex;

    // Safe alcoves — spirits won't enter these tiles
    HashSet<Vector2Int> _safeTiles;

    static readonly Vector2Int[] CardinalDirs = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public void Setup(int[,] grid, int gridWidth, int gridHeight, Vector2Int startTile, HashSet<Vector2Int> safeTiles)
    {
        _grid = grid;
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _safeTiles = safeTiles ?? new HashSet<Vector2Int>();
        _currentTile = startTile;
        _targetTile = startTile;

        var open = GetOpenDirections(startTile);
        _facingDir = open.Count > 0 ? open[Random.Range(0, open.Count)] : Vector2Int.up;
        _currentAngle = DirToAngle(_facingDir);
        _targetAngle = _currentAngle;

        transform.position = new Vector3(startTile.x, startTile.y, 0);
    }

    public void SetPupil(SpriteRenderer pupil)
    {
        _pupilSr = pupil;
    }

    public void SetLevelParams(float newChaseSpeed, float newDrainRate)
    {
        chaseSpeed = newChaseSpeed;
        drainRate = newDrainRate;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        FindPlayer();

        if (_state == State.Patrol)
            UpdatePatrol();
        else
            UpdateChase();

        UpdateRotation();
        UpdatePupil();
    }

    // ---- PLAYER LOOKUP ----

    void FindPlayer()
    {
        if (_player != null) return;
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerMovement = playerObj.GetComponent<PlayerMovement>();
            _playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    // ---- PATROL STATE ----

    void UpdatePatrol()
    {
        if (_stopTimer > 0)
        {
            _stopTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 targetPos = new Vector3(_targetTile.x, _targetTile.y, 0);
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, patrolSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);

        if (Vector3.Distance(newPos, targetPos) < 0.05f)
        {
            _currentTile = _targetTile;
            ChooseNextPatrolTile();
        }
    }

    void ChooseNextPatrolTile()
    {
        var open = GetOpenDirections(_currentTile);
        if (open.Count == 0) return;

        Vector2Int backward = new Vector2Int(-_facingDir.x, -_facingDir.y);
        bool canForward = open.Contains(_facingDir);

        if (canForward && (open.Count <= 2 || Random.value > turnAtJunctionChance))
        {
            _targetTile = _currentTile + _facingDir;
        }
        else
        {
            var choices = new List<Vector2Int>();
            foreach (var dir in open)
            {
                if (dir != backward)
                    choices.Add(dir);
            }
            if (choices.Count == 0)
                choices.Add(backward);

            var chosen = choices[Random.Range(0, choices.Count)];
            _facingDir = chosen;
            _targetTile = _currentTile + chosen;
        }

        _targetAngle = DirToAngle(_facingDir);

        if (Random.value < stopChance)
            _stopTimer = Random.Range(stopDurationMin, stopDurationMax);
    }

    // ---- CHASE STATE ----

    void EnterChase()
    {
        _state = State.Chase;
        _chaseStartTime = Time.time;
        _nextPathCalcTime = 0f;
        _drainAccumulator = 0f;
        _chasePath.Clear();
    }

    void UpdateChase()
    {
        if (_player == null || _playerHealth == null)
        {
            ExitChase();
            return;
        }

        // Give up after max chase duration
        if (Time.time - _chaseStartTime > maxChaseDuration)
        {
            ExitChase();
            return;
        }

        // Give up if player is in a safe alcove
        Vector2Int playerTile = new Vector2Int(
            Mathf.RoundToInt(_player.position.x),
            Mathf.RoundToInt(_player.position.y)
        );
        if (_safeTiles != null && _safeTiles.Contains(playerTile))
        {
            ExitChase();
            return;
        }

        // Drain light/HP when close
        float distToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distToPlayer < drainRange)
        {
            _drainAccumulator += drainRate * Time.fixedDeltaTime;
            if (_drainAccumulator >= 1f)
            {
                int dmg = Mathf.FloorToInt(_drainAccumulator);
                _drainAccumulator -= dmg;
                _playerHealth.TakeDamage(dmg);
            }
        }

        // Recalculate path periodically
        if (Time.time >= _nextPathCalcTime)
        {
            _nextPathCalcTime = Time.time + pathRecalcInterval;
            RecalcChasePath();
        }

        // Follow path
        if (_chasePath.Count > 0 && _chasePathIndex < _chasePath.Count)
        {
            Vector2Int nextTile = _chasePath[_chasePathIndex];
            Vector3 nextPos = new Vector3(nextTile.x, nextTile.y, 0);
            Vector3 newPos = Vector3.MoveTowards(transform.position, nextPos, chaseSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            // Update facing direction
            Vector2Int moveDir = nextTile - _currentTile;
            if (moveDir != Vector2Int.zero)
            {
                _facingDir = moveDir;
                _targetAngle = DirToAngle(_facingDir);
            }

            if (Vector3.Distance(newPos, nextPos) < 0.05f)
            {
                _currentTile = nextTile;
                _chasePathIndex++;
            }
        }
    }

    void ExitChase()
    {
        _state = State.Patrol;
        _chasePath.Clear();
        _targetTile = _currentTile;

        // Pick a new patrol direction
        var open = GetOpenDirections(_currentTile);
        if (open.Count > 0)
        {
            _facingDir = open[Random.Range(0, open.Count)];
            _targetTile = _currentTile + _facingDir;
            _targetAngle = DirToAngle(_facingDir);
        }
    }

    void RecalcChasePath()
    {
        if (_player == null) return;

        Vector2Int playerTile = new Vector2Int(
            Mathf.RoundToInt(_player.position.x),
            Mathf.RoundToInt(_player.position.y)
        );

        _chasePath = BFSPath(_currentTile, playerTile);
        _chasePathIndex = 0;

        // Skip first tile if it's our current position
        if (_chasePath.Count > 1 && _chasePath[0] == _currentTile)
            _chasePathIndex = 1;
    }

    List<Vector2Int> BFSPath(Vector2Int start, Vector2Int end)
    {
        var visited = new bool[_gridWidth, _gridHeight];
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end)
            {
                // Trace path
                var path = new List<Vector2Int>();
                var node = end;
                while (node != start)
                {
                    path.Add(node);
                    node = parent[node];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            foreach (var dir in CardinalDirs)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;
                if (nx < 0 || nx >= _gridWidth || ny < 0 || ny >= _gridHeight) continue;
                if (visited[nx, ny] || _grid[nx, ny] == 0) continue;
                // Spirits cannot enter safe alcoves
                var nextPos = new Vector2Int(nx, ny);
                if (_safeTiles != null && _safeTiles.Contains(nextPos)) continue;

                visited[nx, ny] = true;
                parent[new Vector2Int(nx, ny)] = current;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return new List<Vector2Int>(); // no path found
    }

    // ---- COLLISION / CONTACT ----

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Contact with spirit — it latches on and starts chasing
        if (_state == State.Patrol)
            EnterChase();
    }

    // ---- VISUALS ----

    void UpdateRotation()
    {
        _currentAngle = Mathf.LerpAngle(_currentAngle, _targetAngle, Time.fixedDeltaTime * 8f);
        _rb.MoveRotation(_currentAngle);
    }

    void UpdatePupil()
    {
        if (_pupilSr == null) return;
        _pupilSr.color = _state == State.Chase
            ? new Color(0.6f, 0.1f, 0.05f) // dark red when chasing
            : new Color(0.15f, 0.08f, 0.08f); // dark when patrolling
    }

    float DirToAngle(Vector2Int dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    }

    List<Vector2Int> GetOpenDirections(Vector2Int tile)
    {
        var result = new List<Vector2Int>();
        foreach (var dir in CardinalDirs)
        {
            int nx = tile.x + dir.x;
            int ny = tile.y + dir.y;
            if (nx < 0 || nx >= _gridWidth || ny < 0 || ny >= _gridHeight) continue;
            if (_grid[nx, ny] != 1) continue;
            // Spirits won't patrol into safe alcoves
            if (_safeTiles != null && _safeTiles.Contains(new Vector2Int(nx, ny))) continue;
            result.Add(dir);
        }
        return result;
    }
}
