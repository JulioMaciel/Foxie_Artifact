using System;
using StaticData;
using UnityEngine;

namespace Controller
{
    public class MoveControl : MonoBehaviour
    {
        [SerializeField] float playerSpeed = 5f;
        [SerializeField] float jumpHeight = 1f;
        [SerializeField] float rotationSpeed = 25f;
    
        const float Gravity = -10f;

        Camera mainCamera;
        CharacterController controller;
        Animator animator;
        Vector3 direction;
        Vector3 playerVelocity;
        Vector3 moveDirection;
        bool toJump;
        bool isStopped;
    
        public event Action<Transform> OnMove;

        void Awake()
        {
            mainCamera = Camera.main;
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (isStopped) return;
            
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
        
            var hasHit = Physics.Raycast(transform.position, Vector3.down, 0.1f, Layers.Terrain);
            return hasHit;
        }

        void Jump()
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * Gravity);
            animator.SetTrigger(AnimParam.Fox.Jump);
        }

        void Move()
        {
            var camDir = mainCamera.transform.TransformDirection(direction);
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

        public void Stop()
        {
            isStopped = true;
            direction = Vector3.zero;
            toJump = false;
        }

        public void Resume()
        {
            isStopped = false;
        }
    }
}