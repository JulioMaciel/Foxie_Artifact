using UnityEngine;

namespace UI
{
    public class QuestPointerHandler : MonoBehaviour
    {
        [SerializeField] Camera freeCamera;
    
        Transform target;

        void Update()
        {
            var direction = (freeCamera.transform.position - target.transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(-direction);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}