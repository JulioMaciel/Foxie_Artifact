using System.Collections;
using Managers;
using Tools;
using UnityEngine;

namespace Cameras
{
    public class BoringCarCamera : MonoBehaviour
    {
        [SerializeField] float height = 10;
        
        GameObject boringCar;
        //Vector3 initialPosition;

        void OnEnable()
        {
            boringCar = Entity.Instance.boringCar;
            //initialPosition = transform.position;
            
            StartCoroutine(MoveCameraSmoothly(true));
            StartCoroutine(KeepLookingAtCar());
        }

        IEnumerator MoveCameraSmoothly(bool goUp)
        {
            var currentPos = transform.position;
            var desiredHeight = currentPos.y + height * (goUp ? 1 : -1);
            
            var desiredPosition = new Vector3(currentPos.x, desiredHeight, currentPos.z);
            yield return transform.MoveUntilArrive(desiredPosition, 1);

            // Move back down after going up 
            if (!goUp) yield break;
            yield return new WaitForSeconds(5);
            yield return MoveCameraSmoothly(false);
        }

        IEnumerator KeepLookingAtCar()
        {
            yield return transform.LookAtSmoothly(boringCar.transform, 1);
        }
    }
}