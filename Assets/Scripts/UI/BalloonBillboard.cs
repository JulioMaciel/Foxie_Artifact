using Managers;
using UnityEngine;

namespace UI
{
    public class BalloonBillboard : MonoBehaviour
    {
        public float distance = .4f;
        
        Camera gameplayCamera;
        Transform thisHuman;
        float yPos;
        
        const float xMinAngle = 0;
        const float xMaxAngle = 180;
        const float zMinAngle = 90;
        const float zMaxAngle = 270;
    
        void Start()
        {
            gameplayCamera = Entity.Instance.gamePlayCamera;
            thisHuman = transform.parent;
            yPos = transform.localPosition.y;
        }

        void LateUpdate()
        {
            transform.LookAt(gameplayCamera.transform);
            SetPosition();
        }

        void SetPosition()
        {
            var angleHumanToCamera = Get360Angle();
            var xDestination = GetPositionX(angleHumanToCamera);
            var zDestination = GetPositionZ(angleHumanToCamera);
            transform.localPosition = new Vector3(xDestination, yPos, zDestination);
        }

        float Get360Angle()
        {
            var humanTrans = thisHuman.transform;
            var humanPos = humanTrans.position;
            humanPos.y = 0;
            var camPos = gameplayCamera.transform.position;
            camPos.y = 0;
            var humanForward = humanTrans.forward;
            humanForward.y = 0;
            var humanRight = humanTrans.right;
            humanRight.y = 0;
            var forwardAngle = Vector3.Angle(camPos - humanPos, humanForward);
            var rightAngle = Vector3.Angle(camPos - humanPos, humanRight);
            var angleHumanToCamera = rightAngle > 90 ? 360 - forwardAngle : forwardAngle;
            return angleHumanToCamera;
        }

        public float GetPositionX(float angleHumanToCamera)
        {
            // normalizes angles > 180 and > 270 
            var xNormalizedAngle = angleHumanToCamera switch
            {
                >= 180 and <= xMaxAngle * 1.5f => 180 - (angleHumanToCamera - 180),
                >= xMaxAngle * 1.5f => xMinAngle + (180 * 2f - angleHumanToCamera),
                _ => angleHumanToCamera
            };
            var xPercentage = (xNormalizedAngle - xMinAngle) / 180;
            var xDestination = Mathf.Lerp(distance, -distance, xPercentage);
            return xDestination;
        }

        public float GetPositionZ(float angleHumanToCamera)
        {
            // normalizes angles < 90 and > 270 
            var zNormalizedAngle = angleHumanToCamera switch
            {
                >= zMaxAngle => zMaxAngle - (angleHumanToCamera - zMaxAngle),
                < zMinAngle => zMinAngle + (zMinAngle - angleHumanToCamera),
                _ => angleHumanToCamera
            };
            var zPercentage = (zNormalizedAngle - zMinAngle) / 180;
            var zDestination = Mathf.Lerp(-distance, distance, zPercentage);
            return zDestination;
        }

        float Get180Angle()
        {
            var direction = thisHuman.position - gameplayCamera.transform.position;
            direction.y = 0;
            return Vector3.Angle(thisHuman.forward, direction);
        }
    }
}
