using System;
using System.Collections;
using Cameras;
using Controller;
using Managers;
using ScriptableObjects;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameEvents
{
    public class WakeUpEvent : MonoBehaviour
    {
        [SerializeField] Volume postProcessingVolume;
        [SerializeField] DialogueItem welcomeFarmerDialogue;
        [SerializeField] DialogueItem snakeHuntDialogue;
        [SerializeField] DialogueItem snakeFoundDialogue;
        [SerializeField] AudioClip goldieBarkClip;
        [SerializeField] Transform goldieWelcomeFarmerSpot;
        [SerializeField] Transform farmerWaveSpot;
        [SerializeField] Transform farmerWorkSpot;
        [SerializeField] Transform goldieSnakePigSpot;
        [SerializeField] Transform cameraWakeUpSpot;

        Animator goldieAnimator;
        Animator playerAnimator;
        Animator farmerAnimator;
        MoveControl playerMoveControl;
        NavMeshAgent goldieNavmesh;
        NavMeshAgent farmerNavMesh;
        AudioSource goldieAudio;
        
        Camera mainCamera;
        GameObject player;
        GameObject goldie;
        GameObject snake;
        GameObject farmer;
    
        bool isVignetteCleaned;
        bool isSaturationCleaned;

        void Awake()
        {
            SetObjects();
            GetComponents();
        }

        void SetObjects()
        {
            mainCamera = Camera.main;
            player = Entity.Instance.player;
            goldie = Entity.Instance.goldie;
            farmer = Entity.Instance.farmer;
            snake = Entity.Instance.snake;
        }

        void GetComponents()
        {
            goldieAnimator = goldie.GetComponent<Animator>();
            playerAnimator = player.GetComponent<Animator>();
            farmerAnimator = farmer.GetComponent<Animator>();
            playerMoveControl = player.GetComponent<MoveControl>();
            goldieNavmesh = goldie.GetComponent<NavMeshAgent>();
            farmerNavMesh = farmer.GetComponent<NavMeshAgent>();
            goldieAudio = goldie.GetComponent<AudioSource>();
        }

        void Start()
        {
            playerAnimator.SetTrigger(AnimParam.Fox.Sleep);
            DialogueManager.Instance.StartDialogue(welcomeFarmerDialogue);
            playerMoveControl.enabled = false;
            postProcessingVolume.gameObject.SetActive(true);
        }

        void OnEnable()
        {
            DialogueManager.Instance.OnEventToTrigger += OnEventToTrigger;
            QuestPointerManager.Instance.OnApproachTarget += OnEventToTrigger;
        }

        void OnEventToTrigger(EventToTrigger eventToTrigger)
        {
            switch (eventToTrigger)
            {
                case EventToTrigger.CleanSleepingPP: StartCoroutine(CleanPostProcessing()); break;
                case EventToTrigger.MoveWakingCamera: MoveCameraWhileWakeUp(); break;
                case EventToTrigger.EnablePlayerControl: StartCoroutine(EnablePlayerControl()); break;
                case EventToTrigger.StartFarmerAnimation: StartCoroutine(FarmerLeaveHouse()); break;
                case EventToTrigger.SetSnakeTarget: SetSnakeTarget(); break;
                case EventToTrigger.ShowSnakeFoundDialogue: DialogueManager.Instance.StartDialogue(snakeFoundDialogue); break;
                case EventToTrigger.StartAttackSnakeEvent: StartAttackSnake(); break;
            }
        }

        IEnumerator CleanPostProcessing()
        {
            StartCoroutine(CleanVignette());
            StartCoroutine(CleanSaturation());
        
            while (!isVignetteCleaned || !isSaturationCleaned)
                yield return null;

            postProcessingVolume.gameObject.SetActive(false);
        }
    
        IEnumerator CleanVignette()
        {
            if (!postProcessingVolume.profile.TryGet<Vignette>(out var vig)) yield break;

            const float intensityTarget = 0f;
            var speed = .5f * Time.deltaTime;
            var tolerance = Math.Abs(vig.intensity.value) * 0.05;
        
            while (Math.Abs(vig.intensity.value - intensityTarget) > tolerance)
            {
                var newIntensity = Mathf.Lerp(vig.intensity.value, intensityTarget, speed);
                vig.intensity.Override(newIntensity);
                yield return null;
            }

            isVignetteCleaned = true;
        }
    
        IEnumerator CleanSaturation()
        {
            if (!postProcessingVolume.profile.TryGet<ColorAdjustments>(out var adj)) yield break;
        
            const float colorSaturationTarget = 0f;
            var speed = 2 * Time.deltaTime;
            var tolerance = Math.Abs(adj.saturation.value) * 0.05;
        
            while (Math.Abs(adj.saturation.value - colorSaturationTarget) > tolerance)
            {
                var newSaturation = Mathf.Lerp(adj.saturation.value, colorSaturationTarget, speed);
                adj.saturation.Override(newSaturation);
                yield return null;
            }
        
            isSaturationCleaned = true;
        }

        void MoveCameraWhileWakeUp()
        {
            StartCoroutine(MoveCameraToWakeUpSpot());
            StartCoroutine(RotateCameraToWakeUpSpot());
            GoldieBarks();
        }

        void GoldieBarks()
        {
            goldieAnimator.SetTrigger(AnimParam.Goldie.Bark);
            goldieAudio.PlayClip(goldieBarkClip);   
        }

        IEnumerator MoveCameraToWakeUpSpot()
        {
            while (!mainCamera.transform.position.IsCloseEnough(cameraWakeUpSpot.transform.position))
            {
                var speed = Time.deltaTime;
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraWakeUpSpot.transform.position, speed);
                yield return null;
            }
        }

        IEnumerator RotateCameraToWakeUpSpot()
        {
            while (mainCamera.transform.rotation.normalized != cameraWakeUpSpot.transform.rotation.normalized)
            {
                var speed = Time.deltaTime;
                mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, cameraWakeUpSpot.transform.rotation, speed);
                yield return null;
            }
        }

        IEnumerator EnablePlayerControl()
        {
            mainCamera.GetComponent<FreeCameraControl>().enabled = true;
            playerAnimator.SetTrigger(AnimParam.Fox.StandUp);
            yield return playerAnimator.WaitAnimationStart(AnimClip.Idle);

            playerMoveControl.enabled = true;

            StartCoroutine(MoveGoldieToWelcomeSpot());
        }

        IEnumerator MoveGoldieToWelcomeSpot()
        {
            StartCoroutine(goldieNavmesh.MoveAnimating(goldieAnimator, goldieWelcomeFarmerSpot.position));
            yield return goldieNavmesh.WaitToArrive();
        
            goldie.transform.LookAt(farmer.transform);
            GoldieBarks();
        
            QuestPointerManager.Instance.SetNewTarget(goldie, EventToTrigger.StartFarmerAnimation, "Follow Goldie");
        }

        IEnumerator FarmerLeaveHouse()
        {
            StartCoroutine(farmerNavMesh.MoveAnimating(farmerAnimator, farmerWaveSpot.position));
        
            yield return farmerNavMesh.WaitToArrive();
        
            farmer.transform.LookAt(goldie.transform);
            farmerAnimator.SetTrigger(AnimParam.Human.Wave);
            yield return new WaitForSeconds(3);
        
            StartCoroutine(farmerNavMesh.MoveAnimating(farmerAnimator, farmerWorkSpot.position));
            yield return new WaitForSeconds(1);
            StartCoroutine(PlayFarmerAnimationWork());

            DialogueManager.Instance.StartDialogue(snakeHuntDialogue);
        }

        IEnumerator PlayFarmerAnimationWork()
        {
            yield return farmerNavMesh.WaitToArrive();
            farmerAnimator.SetBool(AnimParam.Human.IsWorking, true);
        }

        void SetSnakeTarget()
        {
            StartCoroutine(goldieNavmesh.MoveAnimating(goldieAnimator, goldieSnakePigSpot.position));
            QuestPointerManager.Instance.SetNewTarget(snake, EventToTrigger.ShowSnakeFoundDialogue, "Search for snakes around the chicks");
        }

        void StartAttackSnake()
        {
            GetComponent<AttackSnakeEvent>().enabled = true;
            Destroy(this);
        }

        void OnDisable()
        {
            DialogueManager.Instance.OnEventToTrigger -= OnEventToTrigger;
            QuestPointerManager.Instance.OnApproachTarget -= OnEventToTrigger;
        }
    }
}
