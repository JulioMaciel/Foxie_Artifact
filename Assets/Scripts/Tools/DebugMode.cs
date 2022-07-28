using GameEvents;
using UnityEngine;

namespace Tools
{
    public class DebugMode : MonoBehaviour
    {
        [SerializeField] MainGameEvent startAtEvent;
        [SerializeField] Camera boringIndustryDebugCamera;
    
        void Start()
        {
            switch (startAtEvent)
            {
                case MainGameEvent.AttackSnake:
                    GetComponent<WakeUpEvent>().enabled = false;
                    GetComponent<AttackSnakeEvent>().enabled = true;
                    break;
                case MainGameEvent.BoringIndustry:
                    GetComponent<WakeUpEvent>().enabled = false;
                    GetComponent<BoringIndustryEvent>().enabled = true;
                    Camera.main.gameObject.SetActive(false);
                    boringIndustryDebugCamera.gameObject.SetActive(true);
                    break;
            }
        }

        public enum MainGameEvent
        {
            WakeUp,
            AttackSnake,
            BoringIndustry,
        }
    }
}
