﻿using System.Collections;
using Controller;
using Managers;
using Tools;
using UnityEngine;

namespace Cameras
{
    public class BoringCarCamera : MonoBehaviour
    {
        [SerializeField] float height = 10;
        
        GameObject boringCar;
        FreeCameraControl freeCamera;
        MoveControl playerMoveControl;

        void OnEnable()
        {
            freeCamera = GetComponent<FreeCameraControl>();
            playerMoveControl = Entity.Instance.player.GetComponent<MoveControl>();
            boringCar = Entity.Instance.boringCar;
            
            freeCamera.enabled = false;
            playerMoveControl.enabled = false;

            StartCoroutine(MoveCameraSmoothly(true));
        }

        void LateUpdate()
        {
            transform.LookAt(boringCar.transform);
        }

        IEnumerator MoveCameraSmoothly(bool goUp)
        {
            var currentPos = transform.position;
            var desiredHeight = currentPos.y + height * (goUp ? 1 : -1);
            
            var desiredPosition = new Vector3(currentPos.x, desiredHeight, currentPos.z);
            Debug.DrawLine(currentPos, desiredPosition, Color.magenta, 99);
            yield return transform.MoveUntilArrive(desiredPosition, 1);

            // Move back down after going up 
            if (!goUp) yield break;
            yield return new WaitForSeconds(1);
            yield return MoveCameraSmoothly(false);
            End();
        }

        void End()
        {
            freeCamera.enabled = true;
            playerMoveControl.enabled = true;
            Destroy(this);
        }
    }
}