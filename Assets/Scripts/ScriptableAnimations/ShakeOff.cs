using UnityEngine;
using UnityEngine.UI;

namespace ScriptableAnimations
{
    public class ShakeOff : MonoBehaviour
    {
        [SerializeField] float limit = 5f;
        [SerializeField] Slider slider;
    
        const float LeastSmooth = 0.02f;
        const float MostSmooth = 0.07f;

        float smoothDiff;
        Vector3 velocity = Vector3.zero;
        Vector3 min; 
        Vector3 max;
        bool isGoingMax;
        Vector3 target;

        void Start()
        {
            var pos = gameObject.transform.localPosition;
            min = new Vector3(pos.x - limit, pos.y, pos.z);
            max = new Vector3(pos.x + limit, pos.y, pos.z);
            smoothDiff = MostSmooth - LeastSmooth;
        }

        void Update()
        {
            SetTarget();
            var scaledSmooth = MostSmooth - smoothDiff * slider.value;
            var smoothed = Vector3.SmoothDamp(gameObject.transform.localPosition, target, ref velocity, scaledSmooth);
            gameObject.transform.localPosition = smoothed;
        }

        void SetTarget()
        {
            var current = gameObject.transform.localPosition;
            if (isGoingMax)
            {
                if (Vector3.Distance(current, max) < 0.1f)
                {
                    isGoingMax = false;
                    target = min;
                }
                else
                    target = max;

            }
            else // if is going Min
            {
                if (Vector3.Distance(current, min) < 0.1f)
                {
                    isGoingMax = true;
                    target = max;
                }
                else
                    target = min;
            }
        }
    }
}
