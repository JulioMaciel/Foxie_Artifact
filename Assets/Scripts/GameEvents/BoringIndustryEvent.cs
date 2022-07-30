using System.Collections;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.AI;

namespace GameEvents
{
    public class BoringIndustryEvent : MonoBehaviour
    {
        /*
         * TODO: 2 guys in the vehicle
         * animation car moving -> freeze following camera, wait to arrive
         * sounds + dust effect 
         */

        [SerializeField] GameObject boringCar;
        [SerializeField] GameObject boringDriver;
        [SerializeField] GameObject boringPassenger;
        [SerializeField] Transform[] roadTracks;
        [SerializeField] ParticleSystem roadDustParticles;
        [SerializeField] AudioClip carOnRoadClip;
        [SerializeField] AudioClip carIdleClip;
        [SerializeField] AudioClip carDoorOpenClip;
        [SerializeField] AudioClip carDoorCloseClip;

        Camera mainCamera;
        NavMeshAgent boringCarNavMesh;
        Animator carWheelsAnimator;
        Animator boringDriverAnimator;
        Animator boringPassengerAnimator;
        AudioSource boringCarAudio;

        int currentTrackIndex = 0;
        float carDustSinceLast;
        float carDustFrequency = .1f;

        void OnEnable()
        {
            FindObjects();
            GetComponents();

            StartCoroutine(MoveBoringCarToFarm());

            DebugMode.Instance.EnableDebugCamera(boringCar.transform);
        }

        void FindObjects()
        {
            mainCamera = Camera.main;
        }

        void GetComponents()
        {
            boringCarNavMesh = boringCar.GetComponent<NavMeshAgent>();
            carWheelsAnimator = boringCar.GetComponentInChildren<Animator>();
            boringDriverAnimator = boringDriver.GetComponent<Animator>();
            boringPassengerAnimator = boringPassenger.GetComponent<Animator>();
            boringCarAudio = boringCar.GetComponent<AudioSource>();
        }

        IEnumerator MoveBoringCarToFarm()
        {
            boringCar.gameObject.SetActive(true);
            boringCarAudio.PlayClip(carOnRoadClip, true);
            carWheelsAnimator.SetBool(AnimParam.Car.IsMovingForward, true);
            while (roadTracks.Length > currentTrackIndex)
            {
                if (carDustSinceLast >= carDustFrequency)
                {
                    var dust = Instantiate(roadDustParticles.gameObject);
                    dust.transform.position = boringCar.transform.position;
                    Destroy(dust, 5);
                    carDustSinceLast = 0;
                }
                else
                    carDustSinceLast += Time.deltaTime;

                var nextTrack = roadTracks[currentTrackIndex];
                boringCarNavMesh.TrySetWorldDestination(nextTrack.position);
                currentTrackIndex++;
                yield return boringCarNavMesh.WaitToArrive(5);
                boringCarNavMesh.ResetPath();
            }
            carWheelsAnimator.SetBool(AnimParam.Car.IsMovingForward, false);
            carWheelsAnimator.enabled = false;
            boringCarAudio.PlayClip(carIdleClip, true);
        }

        //IEnumerator LeaveCar()
        //{
            
        //}
    }
}