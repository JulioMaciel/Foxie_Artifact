using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AnimalWanderer))]

public class IdleEvent : MonoBehaviour
{
    [SerializeField] [Range(0, 10)] float frequency = 1;
    [SerializeField] AudioClip audioClip;

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
        animator.SetTrigger(Anim.IdleEvent);

        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();            
        }
        
        willTrigger = false;
    }
}