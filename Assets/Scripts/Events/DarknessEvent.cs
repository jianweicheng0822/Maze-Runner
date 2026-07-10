using UnityEngine;

namespace MazeRunner.Events
{
    [CreateAssetMenu(fileName = "DarknessEvent", menuName = "MazeRunner/Events/Darkness")]
    public class DarknessEvent : MazeEvent
    {
        [SerializeField] private float dimIntensity = 0.1f;
        [SerializeField] private GameObject flashlightPrefab;

        private float originalAmbientIntensity;
        private GameObject[] spawnedFlashlights;

        public override void Activate()
        {
            originalAmbientIntensity = RenderSettings.ambientIntensity;
            RenderSettings.ambientIntensity = dimIntensity;

            if (flashlightPrefab != null)
            {
                var players = Object.FindObjectsByType<Player.PlayerController>(FindObjectsSortMode.None);
                spawnedFlashlights = new GameObject[players.Length];
                for (int i = 0; i < players.Length; i++)
                {
                    spawnedFlashlights[i] = Object.Instantiate(flashlightPrefab, players[i].transform);
                    spawnedFlashlights[i].transform.localPosition = new Vector3(0f, 1f, 0.5f);
                }
            }

            Debug.Log("[Event] Darkness activated!");
        }

        public override void Deactivate()
        {
            RenderSettings.ambientIntensity = originalAmbientIntensity;

            if (spawnedFlashlights != null)
            {
                foreach (var fl in spawnedFlashlights)
                {
                    if (fl != null) Object.Destroy(fl);
                }
            }

            Debug.Log("[Event] Darkness ended.");
        }
    }
}
