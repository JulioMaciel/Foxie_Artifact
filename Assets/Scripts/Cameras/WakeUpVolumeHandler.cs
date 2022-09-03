using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Cameras
{
    public class WakeUpVolumeHandler : MonoBehaviour
    {
        Volume postProcessingVolume;
    
        bool isVignetteCleaned;
        bool isSaturationCleaned;

        void Awake()
        {
            postProcessingVolume = GetComponent<Volume>();
        }

        public void StartWakingUpEffect()
        {
            StartCoroutine(CleanPostProcessing());
        }
        
        IEnumerator CleanPostProcessing()
        {
            StartCoroutine(CleanVignette());
            StartCoroutine(CleanSaturation());
        
            while (!isVignetteCleaned || !isSaturationCleaned)
                yield return null;

            gameObject.SetActive(false);
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
    }
}