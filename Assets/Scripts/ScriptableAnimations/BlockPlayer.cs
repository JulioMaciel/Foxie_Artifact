using System.Collections.Generic;
using Controller;
using StaticData;
using Tools;
using UnityEngine;
using UnityEngine.AI;

namespace ScriptableAnimations
{
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
            player = GameObject.FindWithTag(Tags.Player);
            agent = GetComponent<NavMeshAgent>();
            animator = gameObject.GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        void OnEnable()
        {
            var moveControl = player.GetComponent<MoveControl>();
            moveControl.OnMove += ReactToPlayerMovement;
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
        
            agent.TrySetWorldDestination(newPos);
        }
    
        void SyncAnimation()
        {
            var arrivedExpectedAxis = followPlayerXPosition && isSameXPositionToPlayer || followPlayerZPosition && isSameZPositionToPlayer;
            animator.SetFloat(AnimParam.MoveSpeed, arrivedExpectedAxis ? 0 : 0.9f);
        }

        void EmitWarningSound()
        {
            if (warningAudioClips.Count < 1) return;
        
            var rndClip = warningAudioClips[Random.Range(0, warningAudioClips.Count)];
            audioSource.PlayClip(rndClip);
        }
    
        void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(Tags.Player)) return;
        
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
            return Vector3.Dot(transform.forward, (player.transform.position - transform.position).normalized) > 0.7;
        }
    }
}