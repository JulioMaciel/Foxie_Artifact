using System.Collections;
using Tools;
using UnityEngine;

namespace Cameras
{
    public class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] Transform cameraDestination;
        
        Camera gameplayCamera;
        bool hasMovedCamera;
        bool hasRotatedCamera;

        void Awake()
        {
            gameplayCamera = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            if (hasMovedCamera && hasRotatedCamera)
                enabled = false;
        }

        public void ChangeToThirdPerson()
        {
            StartCoroutine(MoveCameraToWakeUpSpot());
            StartCoroutine(RotateCameraToWakeUpSpot());
            Destroy(this, 10);
        }
        
        IEnumerator MoveCameraToWakeUpSpot()
        {
            while (!gameplayCamera.transform.position.IsCloseEnough(cameraDestination.transform.position))
            {
                var speed = Time.deltaTime;
                gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, cameraDestination.transform.position, speed);
                yield return null;
            }
            hasMovedCamera = true;
        }

        IEnumerator RotateCameraToWakeUpSpot()
        {
            while (!IsRotationCloseEnough(gameplayCamera.transform, cameraDestination.transform))
            {
                var speed = Time.deltaTime;
                gameplayCamera.transform.rotation = Quaternion.Lerp(gameplayCamera.transform.rotation, cameraDestination.transform.rotation, speed);
                yield return null;
            }
            hasRotatedCamera = true;
        }

        static bool IsRotationCloseEnough(Transform q1, Transform q2)
        {
            return Quaternion.Angle(q1.rotation, q2.rotation) < 1;
        }
    }
}