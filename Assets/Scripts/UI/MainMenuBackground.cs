using UnityEngine;

namespace MazeRunner.UI
{
    public class MainMenuBackground : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed = 8f;
        [SerializeField] private float bobAmplitude = 0.3f;
        [SerializeField] private float bobSpeed = 1.2f;

        private Vector3 startPosition;

        void Start()
        {
            startPosition = transform.position;
        }

        void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            Vector3 pos = startPosition;
            pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = pos;
        }
    }
}
