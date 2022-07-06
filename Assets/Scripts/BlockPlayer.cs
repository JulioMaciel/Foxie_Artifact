using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Issues:
 * Sometimes doesnt follow player movement; stay awhile and moves again later;
 * - move o navmesh mas nao set animation movespeed to 1  
 * LookAtPlayer is not smooth;
 */
public class BlockPlayer : MonoBehaviour
{
    [SerializeField] bool followPlayerXPosition;
    [SerializeField] bool followPlayerZPosition;
    //[SerializeField] IList<LimitWall> walls;
    [SerializeField] List<AudioClip> warningAudioClips;

    GameObject player;
    NavMeshAgent agent;
    Animator animator;
    AudioSource audioSource;
    
    bool lookAtPlayer;
    bool isSameXPositionToPlayer;
    bool isSameZPositionToPlayer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player");
    }

    void OnEnable()
    {
        var moveControl = player.GetComponent<MoveControl>();
        moveControl.OnMove += ReactToPlayerMovement;

        // foreach (var wall in walls)
        //     wall.OnPlayerTouches += OnPlayerHittingLimitWall;
    }

    void Update()
    {
        var playerPos = player.transform.position;
        var thisPos = transform.position;
        isSameXPositionToPlayer = Mathf.Abs(playerPos.x - thisPos.x) < 1;
        isSameZPositionToPlayer = Mathf.Abs(playerPos.z - thisPos.z) < 1;
        
        SyncAnimation();

        if (lookAtPlayer) SmoothlyLookAtPlayer();
    }

    void ReactToPlayerMovement(Transform playerPosition)
    {
        Vector3 newPos;
        var thisPos = transform.position;

        if (followPlayerXPosition && !isSameXPositionToPlayer)
            newPos = new Vector3(playerPosition.position.x, thisPos.y, thisPos.z);
        else if (followPlayerZPosition && !isSameZPositionToPlayer)
            newPos = new Vector3(thisPos.x, thisPos.y, playerPosition.position.z);
        else return;

        var navMeshPos = GetNearestNavMeshPosition(newPos);
        
        agent.SetDestination(navMeshPos);
    }
    
    void SyncAnimation()
    {
        var arrivedExpectedAxis = followPlayerXPosition && isSameXPositionToPlayer || followPlayerZPosition && isSameZPositionToPlayer;
        animator.SetFloat(Anim.MoveSpeed, arrivedExpectedAxis ? 0 : 0.9f);
    }

    Vector3 GetNearestNavMeshPosition(Vector3 position)
    {
        NavMesh.SamplePosition(position, out var hit, 1.0f, NavMesh.AllAreas);
        return hit.position;
    }

    void EmitWarningSound()
    {
        if (warningAudioClips.Count < 1) return;
        
        var rndClip = warningAudioClips[Random.Range(0, warningAudioClips.Count)];
        audioSource.clip = rndClip;
        audioSource.Play();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        
        EmitWarningSound();
        
        if (!IsFacingPlayer()) lookAtPlayer = true;
    }

    void SmoothlyLookAtPlayer()
    {
        if (IsFacingPlayer())
        {
            lookAtPlayer = false;
            return;
        }
        
        var lTargetDir = player.transform.position - transform.position;
        lTargetDir.y = 0.0f;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lTargetDir), Time.time * 2.5f);
    }

    bool IsFacingPlayer()
    {
        //return Vector3.Angle(player.transform.forward, transform.position - player.transform.position) < 5;
        return Vector3.Dot(transform.forward, (player.transform.position - transform.position).normalized) > 0.7;
    }
}