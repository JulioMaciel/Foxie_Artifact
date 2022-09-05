using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AttackSnakeHandler : MonoBehaviour
    {
        [SerializeField] Slider awarenessSlider;
        [SerializeField] Image fillAreaAwarenessSlider;
        [SerializeField] RectTransform suspicionBar;
        
        [SerializeField] Color32 awarenessInitialColor = new(255, 238, 196, 255);
        [SerializeField] Color32 awarenessFinalColor = new(246, 74, 57, 255);
        
        Image suspiciousImage;

        void Awake()
        {
            suspiciousImage = suspicionBar.GetComponent<Image>();
        }

        public void ShowCanvas()
        {
            gameObject.SetActive(true);
        }

        public void UpdateAwarenessUI(float awarenessLevel)
        {
            var scaledAwareness = awarenessLevel / 100;
            awarenessSlider.value = scaledAwareness;
        
            var nextColor = Color32.Lerp(awarenessInitialColor,awarenessFinalColor, scaledAwareness);
            fillAreaAwarenessSlider.color = nextColor;
        }

        public void UpdateSuspicionUI(float suspicionLevel)
        {
            var scaledSuspicion = suspicionLevel / 100;
            suspicionBar.localScale = new Vector3(1f, scaledSuspicion, 1f);
        
            var currentAlpha = Mathf.Lerp(115, 215, scaledSuspicion);
            var tempColor = (Color32)suspiciousImage.color;
            tempColor.a = (byte)currentAlpha;
            suspiciousImage.color = tempColor;
        }

        public void HideCanvas()
        {
            gameObject.SetActive(false);
        }
    }
}
