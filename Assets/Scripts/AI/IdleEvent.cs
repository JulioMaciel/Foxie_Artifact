using System;
using System.Collections.Generic;
using StaticData;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AnimalWanderer))]

    public class IdleEvent : MonoBehaviour
    {
        [SerializeField] [Range(0, 10)] float frequency = 1;
        [SerializeField] List<AudioClip> audioClips;
        [SerializeField] bool triggerAnimation = true;

        AudioSource audioSource;
        AnimalWanderer wanderer;
        Animator animator;
    
        bool willTrigger;

        bool IsMidIdle => Math.Abs(wanderer.wanderTimer / 2 - wanderer.Timer) < 0.1f;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            wanderer = GetComponent<AnimalWanderer>();
            animator = gameObject.GetComponent<Animator>();
        }

        void Update()
        {
            if (wanderer.Timer == 0) ShouldTriggerNext();
        
            if (willTrigger && IsMidIdle) Trigger();
        }

        void ShouldTriggerNext()
        {
            willTrigger = Random.Range(0, 10) <= frequency;
        }

        void Trigger()
        {
            if (triggerAnimation) 
                animator.SetTrigger(AnimParam.IdleEvent);

            if (audioClips.Count >= 1)
            {
                var rndClip = audioClips[Random.Range(0, audioClips.Count)];
                audioSource.clip = rndClip;
                audioSource.Play();            
            }
        
            willTrigger = false;
        }
    }
}