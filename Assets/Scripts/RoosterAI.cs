using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AnimalWanderer))]

public class RoosterAI : MonoBehaviour
{
    [SerializeField] [Range(0, 10)] float crowFrequency = 1;
    [SerializeField] AudioClip crowAudio;

    AudioSource audioSource;
    AnimalWanderer wanderer;
    Animator animator;
    
    bool willCrow;

    bool IsMidIdle => Math.Abs(wanderer.wanderTimer / 2 - wanderer.Timer) < 0.1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        wanderer = GetComponent<AnimalWanderer>();
        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (wanderer.Timer == 0) WillCrow();
        
        if (willCrow && IsMidIdle) Crow();
    }

    void WillCrow()
    {
        willCrow = Random.Range(0, 10) <= crowFrequency;
    }

    void Crow()
    {
        animator.SetTrigger(Anim.Crow);
        
        audioSource.clip = crowAudio;
        audioSource.Play();
        
        willCrow = false;
    }
}