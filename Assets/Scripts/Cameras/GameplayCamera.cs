using Managers;
using UnityEngine;

namespace Cameras
{
    public class GameplayCamera : MonoBehaviour
    {
        [SerializeField] float targetHeight = 1f;
        [SerializeField] float distance = 5.0f;
        [SerializeField] float offsetFromWall = .1f;
        [SerializeField] float maxDistance = 10;
        [SerializeField] float minDistance = .6f;
        [SerializeField] float cameraMovementSpeed = 4;
        [SerializeField] int yMinLimit = 15;
        [SerializeField] int yMaxLimit = 50;
        [SerializeField] LayerMask collisionLayers = -1;
        
        Transform target;
        float xAngle, yAngle, currentDistance, desiredDistance, correctedDistance;

        void Awake()
        {
            target = Entity.Instance.player.transform;
        }

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
            var tarPos = target.position;
            var position = tarPos - (rotation * Vector3.forward * desiredDistance + vTargetOffset);

            var trueTargetPosition = new Vector3(tarPos.x, tarPos.y, tarPos.z) - vTargetOffset;

            var isCorrected = false;
            if (Physics.Linecast(trueTargetPosition, position, out var collisionHit, collisionLayers.value))
            {
                correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                isCorrected = true;
            }

            if (!isCorrected || correctedDistance > currentDistance)
                currentDistance = Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime);
            else 
                currentDistance = correctedDistance;

            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

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

            yAngle = Mathf.Clamp(yAngle, yMinLimit, yMaxLimit);
        }
    }
}