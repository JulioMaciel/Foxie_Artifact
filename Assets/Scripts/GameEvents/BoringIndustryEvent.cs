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
        [SerializeField] Transform parkingSpot;
        [SerializeField] ParticleSystem roadDustParticles;
        [SerializeField] AudioClip carOnRoadClip;
        [SerializeField] AudioClip carIdleClip;
        [SerializeField] AudioClip carDoorOpenClip;
        [SerializeField] AudioClip carDoorCloseClip;
        [SerializeField] AudioClip painScream;
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
        bool hasDriverReturnedToCar;
        bool hasPassengerReturnedToCar;

        void OnEnable()
        {
            SetObjects();
            GetComponents();
            
            QuestPointerManager.Instance.OnApproachTarget += OnEventToTrigger;
            DialogueManager.Instance.OnEventToTrigger += OnEventToTrigger;
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

        void OnEventToTrigger(EventToTrigger eventToTrigger)
        {
            switch (eventToTrigger)
            {
                case EventToTrigger.SetGoldieTargetAfterSnake: 
                    QuestPointerManager.Instance.SetNewTarget(goldie, EventToTrigger.ShowBoringCar, "Meet Goldie outside the fence");
                    break;
                case EventToTrigger.ShowBoringCar: ShowBoringCar(); break;
                case EventToTrigger.ShowAttackBoringDialogue: 
                    DialogueManager.Instance.StartDialogue(attackBoringIndustry); break;
                case EventToTrigger.SetDriverAsTarget: SetDriverAsTarget(); break;
                case EventToTrigger.ShowAttackLegsUI: ShowAttackLegsUI(); break;
                case EventToTrigger.ReactDriverToBite: hasAttackedDriver = true; break;
                case EventToTrigger.ReactPassengerToBite: hasAttackedPassenger = true; break;
            }
        }

        void ShowBoringCar()
        {
            mainCamera.GetComponent<FreeCameraControl>().enabled = false;
            var boringCamera = mainCamera.GetComponent<BoringCarCamera>();
            boringCamera.enabled = true;
            boringCamera.OnEventToTrigger += OnEventToTrigger;

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

            yield return driverAnimator.WaitCurrentClipFinish();
            var farmerPos = farmer.transform.position;
            StartCoroutine(driverNavMesh.MoveAnimating(driverAnimator, farmerPos));
            StartCoroutine(passengerNavMesh.MoveAnimating(passengerAnimator, farmerPos));
            yield return driverNavMesh.WaitToArrive(2);
            
            driverNavMesh.transform.LookAt(farmer.transform);
            passengerNavMesh.transform.LookAt(farmer.transform);

            StartCoroutine(KeepBoringAgentArguing(true, driverAnimator, driverAudio, driverNavMesh, driverBalloon));
            StartCoroutine(KeepBoringAgentArguing(false, passengerAnimator, passengerAudio, passengerNavMesh, passengerBalloon));
            StartCoroutine(KeepFarmerArguing());
        }

        IEnumerator KeepBoringAgentArguing(bool isDriver, Animator anim, AudioSource audio, NavMeshAgent navMesh, SpeechBalloonHandler balloon)
        {
            while (isDriver ? !hasAttackedDriver : !hasAttackedPassenger)
            {
                TalkAnimating(Emotion.Angry, anim, audio);
                balloon.ShowBalloon(balloonBoringReasons, 3);
                
                var rndWait = Random.Range(2f, 4f);
                var currentWaiting = 0f;
                while (currentWaiting < rndWait && (isDriver ? !hasAttackedDriver : !hasAttackedPassenger))
                {
                    currentWaiting += Time.deltaTime;
                    yield return null;
                }
            }

            StartCoroutine(ReactToGetAttacked(isDriver, anim, audio, navMesh));
        }

        IEnumerator ReactToGetAttacked(bool isDriver, Animator anim, AudioSource audio, NavMeshAgent navMesh)
        {
            yield return new WaitForSeconds(0.1f);
            audio.PlayClip(painScream);
            anim.SetTrigger(AnimParam.Human.GetHit);
            Debug.Log($"SetTrigger GetHit");
            //yield return anim.WaitNextClipFinish();
            yield return anim.WaitAnimationFinish(AnimClip.Injured_leg);
            Debug.Log($"next clip has finished. go navMesh.MoveAnimating");
            yield return navMesh.MoveAnimating(anim, boringCar.transform.position, 3);
            //yield return navMesh.WaitToArrive(2);
            audio.PlayClip(carDoorOpenClip);
            yield return new WaitForSeconds(0.5f);
            audio.PlayClip(carDoorCloseClip);
            yield return new WaitForSeconds(0.5f);
            Destroy(isDriver ? boringDriver : boringPassenger);

            if (isDriver) hasPassengerReturnedToCar = true;
            else hasDriverReturnedToCar = true;
            
            if (hasPassengerReturnedToCar && hasDriverReturnedToCar) 
                DriveBoringCarAway();
        }
        
        IEnumerator KeepFarmerArguing()
        {
            while (!hasAttackedPassenger || !hasAttackedDriver)
            {
                var willReactSad = Random.Range(0, 2) == 0;

                if (willReactSad)
                {
                    TalkAnimating(Emotion.Sad, farmerAnimator, farmerAudio);
                    farmerBalloon.ShowBalloon(balloonSayingNo, 3);
                }
                else
                {
                    TalkAnimating(Emotion.Scared, farmerAnimator, farmerAudio);
                }
                
                //yield return farmerAnimator.WaitCurrentAnimation();
                //yield return new WaitForSeconds(Random.Range(3f, 6f));
                
                //yield return WaitTimeOrTrigger(Random.Range(3f, 6f), wereBothAgentsAttacked);
                var rndWait = Random.Range(3f, 6f);
                var currentWaiting = 0f;
                while (currentWaiting < rndWait && !(hasAttackedPassenger && hasAttackedDriver))
                {
                    currentWaiting += Time.deltaTime;
                    yield return null;
                }
            }

            yield return new WaitForSeconds(2);
            StartCoroutine(KeepFarmerCheeringAndWaving());
        }

        // IEnumerator WaitTimeOrTrigger(float waitTime, bool breakTrigger) // deveria ser um ref aqui
        // {
        //     var currentWaiting = 0f;
        //     while (currentWaiting < waitTime || breakTrigger)
        //     {
        //         currentWaiting += Time.deltaTime;
        //         yield return null;
        //     }
        // }

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

        void SetDriverAsTarget()
        {
            QuestPointerManager.Instance.SetNewTarget(boringDriver, EventToTrigger.ShowAttackLegsUI, 
                "Approach and attack the boring industry");
        }

        void ShowAttackLegsUI()
        {
            var driverBiteHandler = boringDriver.GetComponentInChildren<BiteHandler>();
            driverBiteHandler.enabled = true;
            driverBiteHandler.OnEventToTrigger += OnEventToTrigger;
            var passengerBiteHandler = boringPassenger.GetComponentInChildren<BiteHandler>();
            passengerBiteHandler.enabled = true;
            passengerBiteHandler.OnEventToTrigger += OnEventToTrigger;
        }

        void DriveBoringCarAway()
        {
            // TODO
            Debug.Log("A equipe rocket está decolando outra vez!!!");
        }

        IEnumerator KeepFarmerCheeringAndWaving()
        {
            while (true) // repeat until closes game
            {
                farmerAnimator.SetTrigger(AnimParam.Human.Cheer);
                yield return new WaitForSeconds(3);
                farmer.transform.LookAt(boringCar.transform);
                farmerAnimator.SetTrigger(AnimParam.Human.Wave);
                yield return new WaitForSeconds(3);
            }
        }

        void OnDisable()
        {
            QuestPointerManager.Instance.OnApproachTarget -= OnEventToTrigger;
            DialogueManager.Instance.OnEventToTrigger -= OnEventToTrigger;
        }

        internal enum Emotion
        {
            Angry,
            Sad,
            Scared
        }
    }
}