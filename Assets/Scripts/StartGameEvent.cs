using System;
using System.Collections;
using ScriptObjects;
using Unity.Mathematics;
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

    bool isVignetteCleaned;
    bool isSaturationCleaned;

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
        var goldieAnimator = goldie.GetComponent<Animator>();
        goldieAnimator.SetTrigger(Anim.Goldie.Bark);
        var goldieAudio = goldie.GetComponent<AudioSource>();
        goldieAudio.clip = goldieBarkClip;
        goldieAudio.Play();   
    }

    IEnumerator MoveCameraToControlPosition()
    {
        while (Vector3.Distance(startingCamera.transform.position, moveControlCamera.transform.position) > .5f)
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

        var playerAnimator = player.GetComponent<Animator>();
        playerAnimator.SetTrigger(Anim.StandUp);
        
        while(!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fox_Idle"))
            yield return null;
        
        var playerMoveControl = player.GetComponent<MoveControl>();
        playerMoveControl.enabled = true;

        StartCoroutine(MoveGoldieToWelcomeSpot());
    }

    IEnumerator MoveGoldieToWelcomeSpot()
    {
        var goldieNavmesh = goldie.GetComponent<NavMeshAgent>();
        var goldieAnim = goldie.GetComponent<Animator>();
        StartCoroutine(goldieNavmesh.MoveWhileAnimating(goldieAnim, goldieWelcomeFarmerSpot));
        
        while (goldieNavmesh.hasPath)
            yield return null;

        var farmerNavMesh = farmer.GetComponent<NavMeshAgent>();
        var farmerAnim = farmer.GetComponent<Animator>();
        StartCoroutine(farmerNavMesh.MoveWhileAnimating(farmerAnim, farmerWaveSpot));
        
        goldie.transform.LookAt(farmer.transform);
        goldieAnim.SetTrigger(Anim.Goldie.Bark);
        
        while (Vector3.Distance(farmer.transform.position, farmerWaveSpot.position) > .5f)
            yield return null;
        
        farmer.transform.LookAt(goldie.transform);
        farmerAnim.SetTrigger(Anim.Human.Wave);
    }
}
