using System.Collections;
using Cameras;
using Controller;
using Managers;
using ScriptableObjects;
using StaticData;
using Tools;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace GameEvents
{
    public class BoringIndustryEvent : MonoBehaviour
    {
        [SerializeField] GameObject roadDustParent;
        [SerializeField] DialogueItem boringCarShowedUp;
        [SerializeField] DialogueItem attackBoringIndustry;
        [SerializeField] TargetItem goldieTargetAfterSnake;
        [SerializeField] Transform parkingSpot;
        [SerializeField] ParticleSystem roadDustParticles;
        [SerializeField] AudioClip carOnRoadClip;
        [SerializeField] AudioClip carIdleClip;
        [SerializeField] AudioClip carDoorOpenClip;
        [SerializeField] AudioClip carDoorCloseClip;
        [SerializeField] AudioClip[] angryClips;
        [SerializeField] AudioClip[] sadClips;
        [SerializeField] AudioClip[] scaredClips;
        [SerializeField] BalloonItem balloonBoringReasons;
        [SerializeField] BalloonItem balloonSayingNo;

        Camera mainCamera;
        GameObject player;
        GameObject farmer;
        GameObject goldie;
        GameObject boringCar;
        GameObject boringDriver;
        GameObject boringPassenger;
        
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
        SpeechBalloonHandler farmerBalloon;
        SpeechBalloonHandler driverBalloon;
        SpeechBalloonHandler passengerBalloon;

        ObjectPool<GameObject> poolDust;
        float timeSinceLastDustEmission;
        float carDustFrequency = .2f;
        bool isCarMoving;
        bool hasAttackedDriver;
        bool hasAttackedPassenger;

        void OnEnable()
        {
            SetObjects();
            GetComponents();
            
            QuestPointerManager.Instance.OnApproachTarget += OnApproachQuestTarget;
            DialogueManager.Instance.OnDialogueEvent += OnDialogueEvent;

            //DebugMode.Instance.EnableDebugCamera(boringCar.transform);
        }

        void Update()
        {
            if (isCarMoving) HandleDustEmission();
        }

        void SetObjects()
        {
            mainCamera = Camera.main;
            player = Entity.Instance.player;
            farmer = Entity.Instance.farmer;
            goldie = Entity.Instance.goldie;
            boringCar = Entity.Instance.boringCar;
            boringDriver = Entity.Instance.boringDriver;
            boringPassenger = Entity.Instance.boringPassenger;
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
            farmerBalloon = farmer.GetComponent<SpeechBalloonHandler>();
            driverBalloon = boringDriver.GetComponent<SpeechBalloonHandler>();
            passengerBalloon = boringPassenger.GetComponent<SpeechBalloonHandler>();
        }

        void OnApproachQuestTarget(Target target)
        {
            switch (target)
            {
                case Target.Goldie: ShowBoringCar(); break;
            }
        }

        void OnDialogueEvent(DialogueEvent dialogueEvent)
        {
            switch (dialogueEvent)
            {
                case DialogueEvent.SetGoldieTargetAfterSnake: 
                    QuestPointerManager.Instance.SetNewTarget(goldieTargetAfterSnake, goldie.transform);
                    break;
            }
        }

        void ShowBoringCar()
        {
            mainCamera.GetComponent<FreeCameraControl>().enabled = false;
            mainCamera.GetComponent<BoringCarCamera>().enabled = true;
            
            DialogueManager.Instance.StartDialogue(boringCarShowedUp);
            boringCar.SetActive(true);
            
            BuildDustPool();
            StartCoroutine(MoveBoringCarToFarm());
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
            
            farmerAnimator.SetBool(AnimParam.Human.IsWorking, false);
            farmer.transform.LookAt(boringCar.transform);
            
            driverAudio.PlayClip(carDoorOpenClip);
            passengerAudio.PlayClip(carDoorOpenClip);

            var driverLeaveCarSpot  = boringCar.transform.TransformPoint(Vector3.left * 2);
            var passengerLeaveCarSpot  = boringCar.transform.TransformPoint(Vector3.right * 2);

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
            driverBalloon.ShowBalloon(balloonBoringReasons, 5);

            yield return driverAnimator.WaitCurrentAnimation();
            var farmerPos = farmer.transform.position;
            StartCoroutine(driverNavMesh.MoveAnimating(driverAnimator, farmerPos));
            StartCoroutine(passengerNavMesh.MoveAnimating(passengerAnimator, farmerPos));
            yield return driverNavMesh.WaitToArrive(2);
            
            driverNavMesh.transform.LookAt(farmer.transform);
            passengerNavMesh.transform.LookAt(farmer.transform);

            StartCoroutine(KeepDriverArguing());
            StartCoroutine(KeepPassengerArguing());
            StartCoroutine(KeepFarmerArguing());
        }

        IEnumerator KeepDriverArguing()
        {
            while (!hasAttackedDriver)
            {
                TalkAnimating(Emotion.Angry, driverAnimator, driverAudio);
                driverBalloon.ShowBalloon(balloonBoringReasons, 3);
                yield return driverAnimator.WaitCurrentAnimation();
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }

            // todo: what happens after attacked
        }
        
        IEnumerator KeepPassengerArguing()
        {
            while (!hasAttackedPassenger)
            {
                TalkAnimating(Emotion.Angry, passengerAnimator, passengerAudio);
                passengerBalloon.ShowBalloon(balloonBoringReasons, 3);
                yield return passengerAnimator.WaitCurrentAnimation();
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }  
            
            // todo: what happens after attacked
        }
        
        IEnumerator KeepFarmerArguing()
        {
            while (!hasAttackedPassenger && !hasAttackedDriver)
            {
                var willReactSad = Random.Range(1, 3) == 1;

                if (willReactSad)
                {
                    TalkAnimating(Emotion.Sad, farmerAnimator, farmerAudio);
                    farmerBalloon.ShowBalloon(balloonSayingNo, 3);
                }
                else
                {
                    TalkAnimating(Emotion.Scared, farmerAnimator, farmerAudio);
                }
                
                yield return farmerAnimator.WaitCurrentAnimation();
                yield return new WaitForSeconds(Random.Range(3f, 6f));
            }
            
            // todo: what happens after attacks
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

        void OnDisable()
        {
            QuestPointerManager.Instance.OnApproachTarget -= OnApproachQuestTarget;
            DialogueManager.Instance.OnDialogueEvent -= OnDialogueEvent;
        }

        internal enum Emotion
        {
            Angry,
            Sad,
            Scared
        }
    }
}