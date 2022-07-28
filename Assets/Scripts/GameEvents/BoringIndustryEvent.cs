using System;
using System.Collections;
using System.Collections.Generic;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.AI;

namespace GameEvents
{
    public class BoringIndustryEvent : MonoBehaviour
    {
        /*
         * USING TIMELINE!!!
         * TODO: 2 guys in the vehicle
         * animation car moving -> freeze following camera, wait to arrive
         * sounds + dust effect 
         */

        [SerializeField] Transform[] roadTracks;
        //[SerializeField] GameObject debugObj;

        Camera mainCamera;
        GameObject boringCar;
        GameObject boringDriver;
        GameObject boringPassenger;
        
        NavMeshAgent boringCarNavMesh;
        Animator boringCarAnimator;
        Animator boringDriverAnimator;
        Animator boringPassengerAnimator;

        int currentTrackIndex = 0;

        void OnEnable()
        {
            FindObjects();
            GetComponents();
        }

        void FindObjects()
        {
            mainCamera = Camera.main;
            boringCar = GameObject.FindWithTag(Tags.BoringCar);
            boringDriver = GameObject.FindWithTag(Tags.BoringDriver);
            boringPassenger = GameObject.FindWithTag(Tags.BoringPassenger);
        }

        void GetComponents()
        {
            boringCarNavMesh = boringCar.GetComponent<NavMeshAgent>();
            boringDriverAnimator = boringDriver.GetComponent<Animator>();
            boringPassengerAnimator = boringPassenger.GetComponent<Animator>();
            boringCarAnimator = boringDriver.GetComponent<Animator>();
        }

        void Start()
        {
            boringDriverAnimator.SetTrigger(AnimParam.Human.SitDown);
            boringPassengerAnimator.SetTrigger(AnimParam.Human.SitDown);
            StartCoroutine(MoveBoringCarToFarm());
        }

        IEnumerator MoveBoringCarToFarm()
        {
            while (roadTracks.Length != currentTrackIndex)
            {
                var nextTrack = roadTracks[currentTrackIndex];
                boringCarNavMesh.TrySetWorldDestination(nextTrack.position);
                currentTrackIndex++;
                yield return boringCarNavMesh.WaitToArrive(3);                
            }
            //boringCarAnimator.
        }
    }
}