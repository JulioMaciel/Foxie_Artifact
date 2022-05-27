using UnityEngine;

public class MoveControl : MonoBehaviour
{
    [SerializeField] float playerSpeed = 5f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float rotationSpeed = 25f;
    [SerializeField] Transform freeCamera;
    
    const float Gravity = -9.81f;

    CharacterController controller;
    Animator animator;

    Vector3 direction;
    Vector3 playerVelocity;
    Vector3 moveDirection;
    bool toJump;

    void Awake()
    {
        controller = gameObject.GetComponent<CharacterController>();
        animator = gameObject.GetComponent<Animator>();
    }

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
        animator.SetFloat(Anim.MoveSpeed, direction.normalized.magnitude);
        
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
        animator.SetTrigger(Anim.Jump);
    }

    void Move()
    {
        var camDir = freeCamera.transform.TransformDirection(direction);
        var flatMovement = playerSpeed * Time.deltaTime * camDir;
        moveDirection = new Vector3(flatMovement.x, moveDirection.y, flatMovement.z);
        controller.Move(moveDirection);
    }

    void Rotate()
    {
        var currentPosition = transform.position;
        var inputVector = currentPosition + moveDirection;
        var targetRotation = Quaternion.LookRotation(inputVector - currentPosition);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
    }
}