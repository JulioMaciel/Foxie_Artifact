using System;
using System.Collections;
using ScriptObjects;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameEvents : MonoBehaviour
{
    [SerializeField] Volume postProcessingVolume;
    [SerializeField] Camera startingCamera;
    [SerializeField] Camera moveControlCamera;
    [SerializeField] DialogueItem welcomeFarmerDialogue;
    [SerializeField] DialogueItem snakeHuntDialogue;
    [SerializeField] DialogueItem snakeFoundDialogue;
    [SerializeField] DialogueItem checkOutFarmerDialogue;
    [SerializeField] GameObject player;
    [SerializeField] GameObject goldie;
    [SerializeField] GameObject snake;
    [SerializeField] GameObject farmer;
    [SerializeField] AudioClip goldieBarkClip;
    [SerializeField] Transform goldieWelcomeFarmerSpot;
    [SerializeField] Transform farmerWaveSpot;
    [SerializeField] Transform farmerWorkSpot;
    [SerializeField] Transform goldieSnakePigSpot;

    Animator goldieAnimator;
    Animator playerAnimator;
    Animator farmerAnimator;
    MoveControl playerMoveControl;
    NavMeshAgent goldieNavmesh;
    NavMeshAgent farmerNavMesh;
    AudioSource goldieAudio;
    
    bool isVignetteCleaned;
    bool isSaturationCleaned;

    void Awake()
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
        DialogueManager.Instance.StartDialogue(welcomeFarmerDialogue);
    }

    void OnEnable()
    {
        DialogueManager.Instance.OnEndMessage += OnEndMessage;
        QuestPointerManager.Instance.OnApproachTarget += OnApproachQuestTarget;
    }

    void OnEndMessage(Dialogue dialogue, int messageIndex)
    {
        switch (dialogue)
        {
            case Dialogue.WelcomeFarmer:        
                switch (messageIndex)
                {
                    case 0: StartCoroutine(CleanPostProcessing()); break;
                    case 1: MoveToControlCamera(); break;
                    case 2: StartCoroutine(EnablePlayerControl()); break;
                }
                break;
            case Dialogue.HuntSnake: 
                switch (messageIndex) 
                {
                    case 2: SetSnakeTarget(); break;
                }
                break;
            case Dialogue.SnakeFound:
                switch (messageIndex)
                {
                    case 1: StartAttackSnake(); break;
                }
                break;
        }
    }

    void OnApproachQuestTarget(Target target)
    {
        switch (target)
        {
            case Target.GoldieWelcomeFarmer: StartCoroutine(FarmerLeaveHouse()); break;
            case Target.Snake: DialogueManager.Instance.StartDialogue(snakeFoundDialogue);break;
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

    void MoveToControlCamera()
    {
        StartCoroutine(MoveCameraToControlPosition());
        StartCoroutine(RotateCameraToControlRotation());
        GoldieBarks();
    }

    void GoldieBarks()
    {
        goldieAnimator.SetTrigger(AnimParam.Goldie.Bark);
        goldieAudio.clip = goldieBarkClip;
        goldieAudio.Play();   
    }

    IEnumerator MoveCameraToControlPosition()
    {
        while (!startingCamera.transform.position.IsCloseEnough(moveControlCamera.transform.position))
        {
            var speed = Time.deltaTime;
            startingCamera.transform.position = Vector3.Lerp(startingCamera.transform.position, moveControlCamera.transform.position, speed);
            yield return null;
        }
    }

    IEnumerator RotateCameraToControlRotation()
    {
        while (startingCamera.transform.rotation.normalized != moveControlCamera.transform.rotation.normalized)
        {
            var speed = Time.deltaTime;
            startingCamera.transform.rotation = Quaternion.Lerp(startingCamera.transform.rotation, moveControlCamera.transform.rotation, speed);
            yield return null;
        }
    }

    IEnumerator EnablePlayerControl()
    {
        moveControlCamera.gameObject.SetActive(true);
        startingCamera.gameObject.SetActive(false);

        playerAnimator.SetTrigger(AnimParam.StandUp);
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
        
        QuestPointerManager.Instance.SetNewTarget(goldie.transform, "Follow Goldie to welcome the farmer", Target.GoldieWelcomeFarmer);
    }

    IEnumerator FarmerLeaveHouse()
    {
        StartCoroutine(farmerNavMesh.MoveAnimating(farmerAnimator, farmerWaveSpot.position));
        
        yield return farmerNavMesh.WaitToArrive();
        
        farmer.transform.LookAt(goldie.transform);
        farmerAnimator.SetTrigger(AnimParam.Human.Wave);
        yield return farmerAnimator.WaitAnimationFinish(AnimClip.Wave);
        
        StartCoroutine(farmerNavMesh.MoveAnimating(farmerAnimator, farmerWorkSpot.position));
        yield return farmerNavMesh.WaitToArrive();
        StartCoroutine(PlayFarmerAnimationWork());

        DialogueManager.Instance.StartDialogue(snakeHuntDialogue);
    }

    IEnumerator PlayFarmerAnimationWork()
    {
        yield return farmerNavMesh.WaitToArrive();
        farmerAnimator.SetBool(AnimParam.Human.isWorking, true);
    }

    void SetSnakeTarget()
    {
        StartCoroutine(goldieNavmesh.MoveAnimating(goldieAnimator, goldieSnakePigSpot.position));
        QuestPointerManager.Instance.SetNewTarget(snake.transform, "Hunt the snake around the chicks", Target.Snake);
    }

    void StartAttackSnake()
    {
        AttackSnakeEvent.Instance.StartEvent();
        AttackSnakeEvent.Instance.OnEventEnds += OnAttackSnakeEventEnds;
    }

    void OnAttackSnakeEventEnds()
    {
        DialogueManager.Instance.StartDialogue(checkOutFarmerDialogue);
    }
    
    void OnDisable()
    {
        DialogueManager.Instance.OnEndMessage -= OnEndMessage;
        QuestPointerManager.Instance.OnApproachTarget -= OnApproachQuestTarget;
        AttackSnakeEvent.Instance.OnEventEnds -= OnAttackSnakeEventEnds;
    }
}
