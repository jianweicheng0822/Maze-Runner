using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float smoothSpeed = 8f;
    [SerializeField] float zoomSize = 5f;

    Transform _target;
    Camera _cam;

    void Start()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = zoomSize;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desired = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
