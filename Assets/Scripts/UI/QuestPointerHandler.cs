using UnityEngine;

namespace UI
{
    public class QuestPointerHandler : MonoBehaviour
    {
        Camera mainCamera;
        Transform target;

        void Awake()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            var direction = (mainCamera.transform.position - target.transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(-direction);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}