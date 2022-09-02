using Managers;
using UnityEngine;

namespace UI
{
    public class QuestPointerHandler : MonoBehaviour
    {
        Camera gameplayCamera;
        Transform target;

        void Awake()
        {
            gameplayCamera = Entity.Instance.gamePlayCamera;
        }

        void Update()
        {
            var direction = (gameplayCamera.transform.position - target.transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(-direction);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}