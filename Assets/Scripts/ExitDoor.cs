using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.OnLevelComplete();
    }
}
