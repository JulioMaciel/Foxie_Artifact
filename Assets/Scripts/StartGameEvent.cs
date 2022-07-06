using System;
using System.Collections;
using ScriptObjects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StartGameEvent : MonoBehaviour
{
    [SerializeField] Volume volumePP;
    [SerializeField] Camera startingCamera;
    [SerializeField] Dialogue startingDialogue;
    
    Camera movementCamera;

    void Start()
    {
        movementCamera = Camera.main;
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
            case 0: StartCoroutine(StartWakeUpPP()); break;
            case 1: StartTransitionToMainCamera(); break;
        }
    }

    IEnumerator StartWakeUpPP()
    {
        var vigIntensityTarget = 0f;
        var vigSmoothnessTarget = 0f;
        var colorSaturationTarget = 0f;
        var speed = .5f * Time.deltaTime;
        
        if (volumePP.profile.TryGet<Vignette>(out var vig))
        {
            var intensityInit = vig.intensity.value;
            var smoothnessInit = vig.smoothness.value;
            
            // bloom.intensity.overrideState = true;
            yield return StartCoroutine(NormalizeVignette(vig, intensityInit, vigIntensityTarget, smoothnessInit, vigSmoothnessTarget, speed));
        }

        // if (volumePP.profile.TryGet<ColorAdjustments>(out var ca))
        // {
        //     var colorSaturationInit = ca.saturation.value;
        //     ca.saturation.value = Mathf.Lerp(-100, 0, speed);
        // }

        yield return null;
    }

    IEnumerator NormalizeVignette(Vignette vig, float intensityInit, float intensityTarget, float smoothnessInit, float smoothnessTarget, float speed)
    {
        while (vig.intensity.value != intensityTarget)
        {
            vig.intensity.value = Mathf.Lerp(intensityInit, intensityTarget, speed);
            vig.smoothness.value = Mathf.Lerp(smoothnessInit, smoothnessTarget, speed);            
        }
        
        yield return null;
    }

    void StartTransitionToMainCamera()
    {
        // after dialogue finishes
        
        StartPlayerControl();
    }

    void StartPlayerControl()
    {
        var moveControl = GetComponent<MoveControl>();
        moveControl.enabled = true;
        var cameraControl = Camera.main;
        if (cameraControl is not null) cameraControl.enabled = true;
    }
}
