using System;
using System.Collections;
using ScriptObjects;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StartGameEvent : MonoBehaviour
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
    }

    void Start()
    {
        DialogueManager.instance.StartDialogue(startingDialogue);
    }

    void OnEnable()
    {
        DialogueManager.instance.OnEndMessage += OnEndMessage;
    }

    void OnEndMessage(int messageIndex)
    {
        switch (messageIndex)
        {
            case 0: StartCoroutine(CleanPostProcessing()); break;
            case 1: MoveToControlCamera(); break;
            case 2: StartCoroutine(EnablePlayerControl()); break;
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
        var goldieAudio = goldie.GetComponent<AudioSource>();
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
        yield return playerAnimator.WaitAnimationFinishes();
        Debug.Log("AnimParam.StandUp finishes. It has 2 sequential animations."); //TODO

        playerMoveControl.enabled = true;

        StartCoroutine(MoveGoldieToWelcomeSpot());
    }

    IEnumerator MoveGoldieToWelcomeSpot()
    {
        StartCoroutine(goldieNavmesh.MoveWhileAnimating(goldieAnimator, goldieWelcomeFarmerSpot.position));
        yield return goldieNavmesh.WaitToArrive();

        StartCoroutine(farmerNavMesh.MoveWhileAnimating(farmerAnimator, farmerWaveSpot.position));
        
        goldie.transform.LookAt(farmer.transform);
        goldieAnimator.SetTrigger(AnimParam.Goldie.Bark);
        
        WaypointManager.Instance.SetNewTarget(goldie.transform, "Follow Goldie to welcome the farmer");
        
        yield return farmerNavMesh.WaitToArrive();
        
        farmer.transform.LookAt(goldie.transform);
        farmerAnimator.SetTrigger(AnimParam.Human.Wave);
        yield return farmerAnimator.WaitAnimationFinishes();
        
        StartCoroutine(farmerNavMesh.MoveWhileAnimating(farmerAnimator, farmerWorkSpot.position));
        StartCoroutine(PlayFarmerAnimationWork());
    }

    IEnumerator PlayFarmerAnimationWork()
    {
        yield return farmerNavMesh.WaitToArrive();
        farmerAnimator.SetBool(AnimParam.Human.isWorking, true);
    }
}
