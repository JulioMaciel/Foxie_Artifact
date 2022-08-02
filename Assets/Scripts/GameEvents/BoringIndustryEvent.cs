using System;
using System.Collections;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

namespace GameEvents
{
    public class BoringIndustryEvent : MonoBehaviour
    {
        [SerializeField] GameObject boringCar;
        [SerializeField] GameObject boringDriver;
        [SerializeField] GameObject boringPassenger;
        [SerializeField] GameObject roadDustParent;
        [SerializeField] Transform parkingSpot;
        [SerializeField] ParticleSystem roadDustParticles;
        [SerializeField] AudioClip carOnRoadClip;
        [SerializeField] AudioClip carIdleClip;
        [SerializeField] AudioClip carDoorOpenClip;
        [SerializeField] AudioClip carDoorCloseClip;
        [SerializeField] AudioClip[] angryClips;
        [SerializeField] AudioClip[] sadClips;
        [SerializeField] AudioClip[] scaredClips;

        Camera mainCamera;
        GameObject farmer;
        
        NavMeshAgent carNavMesh;
        NavMeshAgent driverNavMesh;
        NavMeshAgent passengerNavMesh;
        Animator carWheelsAnimator;
        Animator driverAnimator;
        Animator passengerAnimator;
        Animator farmerAnimator;
        AudioSource carAudio;
        AudioSource driverAudio;
        AudioSource passengerAudio;
        AudioSource farmerAudio;

        ObjectPool<GameObject> poolDust;
        float timeSinceLastDustEmission;
        float carDustFrequency = .2f;
        bool isCarMoving;

        void OnEnable()
        {
            FindObjects();
            GetComponents();

            BuildDustPool();
            StartCoroutine(MoveBoringCarToFarm());

            DebugMode.Instance.EnableDebugCamera(boringCar.transform);
        }

        void Update()
        {
            if (isCarMoving) HandleDustEmission();
        }

        void FindObjects()
        {
            mainCamera = Camera.main;
            farmer = GameObject.FindWithTag(Tags.Farmer);
        }

        void GetComponents()
        {
            carNavMesh = boringCar.GetComponent<NavMeshAgent>();
            driverNavMesh = boringDriver.GetComponent<NavMeshAgent>();
            passengerNavMesh = boringPassenger.GetComponent<NavMeshAgent>();
            carWheelsAnimator = boringCar.GetComponentInChildren<Animator>();
            driverAnimator = boringDriver.GetComponent<Animator>();
            passengerAnimator = boringPassenger.GetComponent<Animator>();
            farmerAnimator = farmer.GetComponent<Animator>();
            carAudio = boringCar.GetComponent<AudioSource>();
            driverAudio = boringDriver.GetComponent<AudioSource>();
            passengerAudio = boringPassenger.GetComponent<AudioSource>();
            farmerAudio = farmer.GetComponent<AudioSource>();
        }

        void BuildDustPool()
        {
            var dustObj = roadDustParticles.gameObject;
            poolDust = new ObjectPool<GameObject>(() =>
                Instantiate(dustObj, roadDustParent.transform),
                dust => dust.SetActive(true),
                dust => dust.SetActive(false),
                Destroy, false, 30, 50
            );
        }

        IEnumerator MoveBoringCarToFarm()
        {
            isCarMoving = true;
            boringCar.gameObject.SetActive(true);
            carAudio.PlayClip(carOnRoadClip, true);
            carWheelsAnimator.SetBool(AnimParam.Car.IsMovingForward, true);
            carNavMesh.SetDestination(parkingSpot.position);
            yield return carNavMesh.WaitToArrive(0.1f);
            carWheelsAnimator.SetBool(AnimParam.Car.IsMovingForward, false);
            carWheelsAnimator.enabled = false;
            carAudio.PlayClip(carIdleClip, true);
            isCarMoving = false;
            StartCoroutine(LeaveCar());
        }

        void HandleDustEmission()
        {
            if (timeSinceLastDustEmission >= carDustFrequency)
            {
                var dust = poolDust.Get();
                dust.transform.position = boringCar.transform.position;
                StartCoroutine(ReleaseDustPoolAfterFinishing(dust));
                timeSinceLastDustEmission = 0;
            }
            else timeSinceLastDustEmission += Time.deltaTime;
        }
        
        IEnumerator ReleaseDustPoolAfterFinishing(GameObject dust)
        {
            var ps = dust.GetComponent<ParticleSystem>();
            while (ps.isPlaying) yield return null;
            poolDust.Release(dust);
        }

        IEnumerator LeaveCar()
        {
            var carPos = boringCar.transform.position;
            boringDriver.SetActive(true);
            boringPassenger.SetActive(true);
            boringDriver.transform.position = carPos;
            boringPassenger.transform.position = carPos;
            
            driverAudio.PlayClip(carDoorOpenClip);
            passengerAudio.PlayClip(carDoorOpenClip);

            var driverLeaveCarSpot  = boringCar.transform.TransformPoint(Vector3.left * 2);
            var passengerLeaveCarSpot  = boringCar.transform.TransformPoint(Vector3.right * 2);
            
            boringDriver.transform.TurnBackOn(boringCar.transform);
            boringPassenger.transform.TurnBackOn(boringCar.transform);

            StartCoroutine(boringDriver.transform.MoveUntilArrive(driverLeaveCarSpot, 2, .025f));
            yield return boringPassenger.transform.MoveUntilArrive(passengerLeaveCarSpot, 2, .025f);

            StartCoroutine(ApproachFarmer());
        }

        IEnumerator ApproachFarmer()
        {
            driverNavMesh.enabled = true;
            passengerNavMesh.enabled = true;

            StartCoroutine(farmer.transform.LookAtSmoothly(boringCar.transform, 1));
            
            driverNavMesh.transform.LookAt(farmer.transform);
            passengerNavMesh.transform.LookAt(farmer.transform);

            TalkAnimating(Emotion.Angry, driverAnimator, driverAudio);

            yield return driverAnimator.WaitCurrentAnimation();
            var farmerPos = farmer.transform.position;
            yield return driverNavMesh.MoveAnimating(driverAnimator, farmerPos);
            yield return passengerNavMesh.MoveAnimating(passengerAnimator, farmerPos);
            yield return driverNavMesh.WaitToArrive(1);
            
            driverNavMesh.transform.LookAt(farmer.transform);
            passengerNavMesh.transform.LookAt(farmer.transform);
            
            TalkAnimating(Emotion.Angry, driverAnimator, driverAudio);
            yield return driverAnimator.WaitCurrentAnimation();
            TalkAnimating(Emotion.Angry, passengerAnimator, passengerAudio);
            yield return passengerAnimator.WaitCurrentAnimation();
            TalkAnimating(Emotion.Sad, farmerAnimator, farmerAudio);
            yield return farmerAnimator.WaitCurrentAnimation();
        }

        void TalkAnimating(Emotion emotion, Animator humanAnim, AudioSource audioSource)
        {
            switch (emotion)
            {
                case Emotion.Angry:
                    humanAnim.SetTrigger(AnimParam.Human.Shout);
                    audioSource.PlayRandomClip(angryClips);
                    break;
                case Emotion.Sad:
                    humanAnim.SetTrigger(AnimParam.Human.SayNo);
                    audioSource.PlayRandomClip(sadClips);
                    break;
                case Emotion.Scared:
                    humanAnim.SetTrigger(AnimParam.Human.Horror);
                    audioSource.PlayRandomClip(scaredClips);
                    break;
            }
        }

        internal enum Emotion
        {
            Angry,
            Sad,
            Scared
        }
    }
}