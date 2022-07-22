using System;
using StaticData;
using Tools;
using UnityEngine;

namespace Controller
{
    public class MoveControl : MonoBehaviour
    {
        [SerializeField] float playerSpeed = 5f;
        [SerializeField] float jumpHeight = 1f;
        [SerializeField] float rotationSpeed = 25f;
        [SerializeField] Transform freeCamera;
        [SerializeField] AudioClip stepGrassClip;
        [SerializeField] AudioClip stepEarthClip;
        [SerializeField] GameObject stepDustEffect;
    
        const float Gravity = -10f;

        CharacterController controller;
        Animator animator;
        AudioSource audioSource;

        Vector3 direction;
        Vector3 playerVelocity;
        Vector3 moveDirection;
        bool toJump;
    
        public event Action<Transform> OnMove;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }
        // TODO: footprint sound effect by animation clip + dust (run only)

        void Update()
        {
            direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            toJump = Input.GetButtonDown("Jump");
        }

        void LateUpdate()
        {
            if (direction != Vector3.zero)
            {
                Move();
                Rotate();
            }
            animator.SetFloat(AnimParam.MoveSpeed, direction.normalized.magnitude);
        
            if (toJump && CanJump()) 
                Jump();

            HandleGravity();
        }

        void HandleGravity()
        {
            if (controller.isGrounded && playerVelocity.y < 0) 
                playerVelocity.y = 0f;
        
            playerVelocity.y += Gravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }

        bool CanJump()
        {
            if (playerVelocity.y > 0) return false;
        
            var hasHit = Physics.Raycast(transform.position, Vector3.down, 0.1f, Masks.Terrain);
            return hasHit;
        }

        void Jump()
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * Gravity);
            animator.SetTrigger(AnimParam.Fox.Jump);
        }

        void Move()
        {
            var camDir = freeCamera.transform.TransformDirection(direction);
            var flatMovement = playerSpeed * Time.deltaTime * camDir;
            moveDirection = new Vector3(flatMovement.x, moveDirection.y, flatMovement.z);
            controller.Move(moveDirection);
            OnMove?.Invoke(transform);
        }

        void Rotate()
        {
            var currentPosition = transform.position;
            var inputVector = currentPosition + moveDirection;
            var forwardV3 = inputVector - currentPosition;
        
            if (forwardV3 == Vector3.zero) return;
        
            var targetRotation = Quaternion.LookRotation(forwardV3);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
        }

        /*
         * Called by animation clip
         * TODO: only dust when over earth by masking
         * TODO: Pooling system
         */
        void Step()
        {
            audioSource.PlayClip(stepGrassClip);
            var dust = Instantiate(stepDustEffect);
            dust.transform.position = gameObject.transform.position;
        }
    }
}