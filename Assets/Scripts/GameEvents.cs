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
    [SerializeField] Dialogue startingDialogue;
    [SerializeField] GameObject player;
    [SerializeField] GameObject goldie;
    [SerializeField] AudioClip goldieBarkClip;
    [SerializeField] Transform goldieWelcomeFarmerSpot;
    [SerializeField] GameObject farmer;
    [SerializeField] Transform farmerWaveSpot;
    [SerializeField] Transform farmerWorkSpot;

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
        DialogueManager.Instance.StartDialogue(startingDialogue);
    }

    void OnEnable()
    {
        DialogueManager.Instance.OnEndMessage += OnEndMessage;
        QuestPointerManager.Instance.OnApproachTarget += OnApproachQuestTarget;
    }

    void OnEndMessage(Quest quest, int messageIndex)
    {
        switch (quest)
        {
            case Quest.WelcomeFarmer:        
                switch (messageIndex)
                {
                    case 0: StartCoroutine(CleanPostProcessing()); break;
                    case 1: MoveToControlCamera(); break;
                    case 2: StartCoroutine(EnablePlayerControl()); break;
                }
                break;
            case Quest.HuntSnake:
                break;
        }
    }

    void OnApproachQuestTarget(Quest quest)
    {
        switch (quest)
        {
            case Quest.WelcomeFarmer: StartCoroutine(FarmerLeaveHouse()); break;
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
        
        QuestPointerManager.Instance.SetNewTarget(goldie.transform, "Follow Goldie to welcome the farmer", Quest.WelcomeFarmer);
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
        
        // dialogue kill snakes
    }

    IEnumerator PlayFarmerAnimationWork()
    {
        yield return farmerNavMesh.WaitToArrive();
        farmerAnimator.SetBool(AnimParam.Human.isWorking, true);
    }
    
    void OnDisable()
    {
        DialogueManager.Instance.OnEndMessage -= OnEndMessage;
        QuestPointerManager.Instance.OnApproachTarget -= OnApproachQuestTarget;
    }
}
