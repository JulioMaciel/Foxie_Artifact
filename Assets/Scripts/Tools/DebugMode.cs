using Cameras;
using GameEvents;
using StaticData;
using UnityEngine;

namespace Tools
{
    public class DebugMode : MonoBehaviour
    {
        [SerializeField] GameObject gameEvents;
        [SerializeField] MainGameEvent startAtEvent;
        [SerializeField] Transform farmerWorkSpot;
        
        public static DebugMode Instance;
        void Awake() => Instance = this;
    
        void Start()
        {
            switch (startAtEvent)
            {
                case MainGameEvent.None:
                    gameEvents.GetComponent<WakeUpEvent>().enabled = false;
                    break;
                case MainGameEvent.AttackSnake:
                    gameEvents.GetComponent<WakeUpEvent>().enabled = false;
                    gameEvents.GetComponent<AttackSnakeEvent>().enabled = true;
                    break;
                case MainGameEvent.BoringIndustry:
                    var farmer = GameObject.FindWithTag(Tags.Farmer);
                    farmer.SetActive(false);
                    farmer.transform.position = farmerWorkSpot.position;
                    farmer.SetActive(true);
                    gameEvents.GetComponent<WakeUpEvent>().enabled = false;
                    gameEvents.GetComponent<BoringIndustryEvent>().enabled = true;
                    break;
            }
        }
        
        public void EnableDebugCamera(Transform newTarget)
        {
            Camera.main.gameObject.SetActive(false);
            var debugCam = GetComponentInChildren<DebugCamera>();
            debugCam.gameObject.SetActive(true);
            debugCam.target = newTarget;
        }

        public enum MainGameEvent
        {
            None,
            WakeUp,
            AttackSnake,
            BoringIndustry,
        }
    }
}
