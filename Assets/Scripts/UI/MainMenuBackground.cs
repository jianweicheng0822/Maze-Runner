using UnityEngine;

namespace MazeRunner.UI
{
    public class MainMenuBackground : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed = 8f;
        [SerializeField] private float bobAmplitude = 0.3f;
        [SerializeField] private float bobSpeed = 1.2f;
        [SerializeField] private float phaseOffset = 0f;
        [SerializeField] private bool rotateOnX = false;
        [SerializeField] private bool rotateOnZ = false;

        private Vector3 startPosition;

        void Start()
        {
            startPosition = transform.position;

            if (phaseOffset == 0f)
                phaseOffset = transform.position.x * 0.7f + transform.position.z * 0.3f;
        }

        void Update()
        {
            Vector3 rotationAxis = Vector3.up;
            if (rotateOnX) rotationAxis += Vector3.right * 0.5f;
            if (rotateOnZ) rotationAxis += Vector3.forward * 0.3f;
            rotationAxis.Normalize();

            transform.Rotate(rotationAxis, rotateSpeed * Time.deltaTime, Space.World);

            Vector3 pos = startPosition;
            pos.y += Mathf.Sin((Time.time + phaseOffset) * bobSpeed) * bobAmplitude;
            transform.position = pos;
        }
    }
}
