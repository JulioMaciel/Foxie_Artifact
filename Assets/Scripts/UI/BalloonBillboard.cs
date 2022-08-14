using UnityEngine;

namespace UI
{
    public class BalloonBillboard : MonoBehaviour
    {
        [SerializeField] float distance = .4f;
        
        Camera mainCamera;
        Transform thisHuman;
        float yPos;
        
        const float xMinAngle = 0;
        const float xMaxAngle = 180;
        const float xMaxValue = xMaxAngle - xMinAngle;
        const float zMinAngle = 90;
        const float zMaxAngle = 270;
        const float zMaxValue = zMaxAngle - zMinAngle;
    
        void Start()
        {
            mainCamera = Camera.main;
            thisHuman = transform.parent;
            yPos = transform.localPosition.y;
        }

        void LateUpdate()
        {
            //transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.y, 0);
            //transform.LookAt(transform.position + mainCamera.transform.forward);
            transform.LookAt(mainCamera.transform);

            var angleHumanToCamera = Get360Angle();
            
            // EXPECTED VALUES - TODO unit tests
            // a 0      x +4  z 0
            // a 90     x 0   z -4
            // a 180    x -4  z 0
            // a 270    x 0   z +4  

            // normalizes angles > 180 and > 270 
            var xNormalizedAngle = angleHumanToCamera switch
            {
                > xMaxValue and < xMaxAngle * 1.5f => xMaxValue - (angleHumanToCamera - xMaxValue),
                > xMaxAngle * 1.5f => xMinAngle + (xMaxValue * 2f - angleHumanToCamera),
                _ => angleHumanToCamera
            };
            var xPercentage = (xNormalizedAngle - xMinAngle) / xMaxValue;
            var xDestination = Mathf.Lerp(distance, -distance, xPercentage);

            // normalizes angles < 90 and > 270 
            var zNormalizedAngle = angleHumanToCamera switch
            {
                > zMaxAngle => zMaxAngle - (angleHumanToCamera - zMaxAngle),
                < zMinAngle => zMinAngle + (zMinAngle - angleHumanToCamera),
                _ => angleHumanToCamera
            };
            var zPercentage = (zNormalizedAngle - zMinAngle) / zMaxValue;
            var zDestination = Mathf.Lerp(-distance, distance, zPercentage);

            transform.localPosition = new Vector3(xDestination, yPos, zDestination);
        }

        float Get360Angle()
        {
            var humanTrans = thisHuman.transform;
            var humanPos = humanTrans.position;
            humanPos.y = 0;
            var camPos = mainCamera.transform.position;
            camPos.y = 0;
            var humanForward = humanTrans.forward;
            humanForward.y = 0;
            var humanRight = humanTrans.right;
            humanRight.y = 0;
            var forwardAngle = Vector3.Angle((camPos - humanPos), humanForward);
            var rightAngle = Vector3.Angle((camPos - humanPos), humanRight);
            var angleHumanToCamera = rightAngle > 90 ? 360 - forwardAngle : forwardAngle;
            return angleHumanToCamera;
        }

        float Get180Angle()
        {
            var direction = thisHuman.position - mainCamera.transform.position;
            direction.y = 0;
            return Vector3.Angle(thisHuman.forward, direction);
        }
    }
}
