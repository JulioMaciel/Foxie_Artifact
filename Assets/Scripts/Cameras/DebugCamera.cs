using UnityEngine;

namespace Cameras
{
    public class DebugCamera : MonoBehaviour
    {
        public Transform target;
        [SerializeField] float targetHeight = 1;
        [SerializeField] float distance = 10;
        [SerializeField] float cameraMovementSpeed = 4;
        
        float xAngle, yAngle, currentDistance, desiredDistance, correctedDistance;

        void Start()
        {
            var angles = transform.eulerAngles;
            xAngle = angles.x;
            yAngle = angles.y;

            currentDistance = distance;
            desiredDistance = distance;
            correctedDistance = distance;
        }
    
        void LateUpdate()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                xAngle += Input.GetAxis("Mouse X") * cameraMovementSpeed;
                yAngle -= Input.GetAxis("Mouse Y") * cameraMovementSpeed;
            }
        
            FixVerticalAngle();

            var rotation = Quaternion.Euler(yAngle, xAngle, 0);
            correctedDistance = desiredDistance;

            var vTargetOffset = new Vector3(0, -targetHeight, 0);
            
            if (target == null) return;
            
            var tarPos = target.position;
            var position = tarPos - (rotation * Vector3.forward * desiredDistance + vTargetOffset);

            currentDistance = correctedDistance;

            position = tarPos - (rotation * Vector3.forward * currentDistance + vTargetOffset);

            transform.rotation = rotation;
            transform.position = position;
        }

        void FixVerticalAngle()
        {
            switch (yAngle)
            {
                case < -360:
                    yAngle += 360;
                    break;
                case > 360:
                    yAngle -= 360;
                    break;
            }
        }
    }
}