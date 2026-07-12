using UnityEngine;
using UnityEngine.UI;

namespace MazeRunner.UI
{
    public class TitleGlowEffect : MonoBehaviour
    {
        [SerializeField] private Image glowImage;
        [SerializeField] private float minAlpha = 0.15f;
        [SerializeField] private float maxAlpha = 0.35f;
        [SerializeField] private float pulseSpeed = 2f;

        void Update()
        {
            if (glowImage == null) return;

            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            Color c = glowImage.color;
            c.a = alpha;
            glowImage.color = c;
        }
    }
}
