using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float crouchWalkSpeed = 1.8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float speed;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.25f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0.9f, 0);
    [SerializeField] private float crouchTransitionSpeed = 5f;

    [Header("Camera Settings")]
    [SerializeField] private float cameraRotationSpeed = 2f;
    [SerializeField] private float cameraMaxYAngle = 80f;
    [SerializeField] private float cameraMinYAngle = -80f;

    [Header("Idle Animation Settings")]
    [SerializeField] private float idleTimeout = 5f;
    [SerializeField] private string idle2AnimationTrigger = "Idle2";

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator animator;
    private Camera mainCamera;
    private PlayerAimThrow aimThrowController;

    public bool isGrounded = true;
    private bool jumpInput = false;
    private bool isCrouching = false;
    public float currentSpeed;
    private float currentRotation;
    private float targetHeight;
    private Vector3 targetCenter;
    private float cameraHorizontalRotation = 0f;
    private float cameraVerticalRotation = 0f;
    private float idleTimer = 0f;
    private bool isInIdle2 = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main;
        aimThrowController = GetComponent<PlayerAimThrow>();

        if (rb == null) Debug.LogError("El Rigidbody no está asignado.");
        if (animator == null) Debug.LogError("El Animator no está asignado.");
        if (playerCollider == null) Debug.LogError("El CapsuleCollider no está asignado.");

        rb.freezeRotation = true;
        targetHeight = standingHeight;
        targetCenter = standingCenter;

        cameraHorizontalRotation = transform.eulerAngles.y;
        if (mainCamera != null)
        {
            cameraVerticalRotation = mainCamera.transform.eulerAngles.x;
        }
    }

    void Update()
    {
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        HandleCrouch();
        HandleRotation();
        HandleCameraRotation();
        UpdateAnimations();
        HandleIdleAnimation();

        playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, crouchTransitionSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float moveZ = Input.GetAxis("Vertical");
        currentSpeed = moveZ;

        HandleRun();

        Vector3 velocity = transform.forward * moveZ * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + velocity);
    }

    void HandleRotation()
    {
        if (!aimThrowController || !aimThrowController.IsAiming)
        {
            float rotation = Input.GetAxis("Horizontal");
            currentRotation = rotation;
            transform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime);
            cameraHorizontalRotation = transform.eulerAngles.y;
        }
    }

    void HandleIdleAnimation()
    {
        bool isIdle = Mathf.Abs(currentSpeed) < 0.1f &&
                     Mathf.Abs(currentRotation) < 0.1f &&
                     (!aimThrowController || !aimThrowController.IsAiming) &&
                     !isCrouching &&
                     isGrounded;

        if (isIdle)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTimeout && !isInIdle2)
            {
                animator.SetTrigger(idle2AnimationTrigger);
                isInIdle2 = true;
            }
        }
        else
        {
            idleTimer = 0f;
            isInIdle2 = false;
        }
    }

    void HandleCameraRotation()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.1f || (aimThrowController && aimThrowController.IsAiming))
        {
            float mouseX = Input.GetAxis("Mouse X") * cameraRotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * cameraRotationSpeed;

            cameraHorizontalRotation += mouseX;
            transform.rotation = Quaternion.Euler(0f, cameraHorizontalRotation, 0f);

            cameraVerticalRotation -= mouseY;
            cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, cameraMinYAngle, cameraMaxYAngle);

            if (mainCamera != null)
            {
                mainCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
            }
        }
    }

    void HandleRun()
    {
        if (isCrouching)
        {
            speed = crouchWalkSpeed;
            animator.SetBool("IsWalkingCrouched", Mathf.Abs(currentSpeed) > 0.1f);
        }
        else
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && (!aimThrowController || !aimThrowController.IsAiming);
            speed = isRunning ? runSpeed : walkSpeed;
            animator.SetBool("IsRunning", isRunning);
        }
    }

    void HandleCrouch()
    {
        if (aimThrowController && aimThrowController.IsAiming) return;

        bool isMoving = Mathf.Abs(currentSpeed) > 0.1f || Mathf.Abs(currentRotation) > 0.1f;
        bool isRunning = animator.GetBool("IsRunning");

        if (!isMoving && !isRunning)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                ToggleCrouch();
            }

            if (isCrouching && CanStandUp() && !Input.GetKey(KeyCode.LeftControl))
            {
                ToggleCrouch();
            }
        }
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            targetHeight = crouchHeight;
            targetCenter = crouchCenter;
        }
        else if (CanStandUp())
        {
            targetHeight = standingHeight;
            targetCenter = standingCenter;
        }
        else
        {
            isCrouching = true;
        }

        animator.SetBool("IsCrouching", isCrouching);
    }

    bool CanStandUp()
    {
        return !Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight);
    }

    void Jump()
    {
        if (jumpInput && isGrounded && !isCrouching && (!aimThrowController || !aimThrowController.IsAiming))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            jumpInput = false;
            animator.SetTrigger("JumpTrigger");
        }
    }

    void UpdateAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(currentSpeed));
        animator.SetBool("IsGrounded", isGrounded);

        if (currentRotation > 0.1f)
        {
            animator.SetBool("TurningRight", true);
            animator.SetBool("TurningLeft", false);
            ResetIdleState();
        }
        else if (currentRotation < -0.1f)
        {
            animator.SetBool("TurningLeft", true);
            animator.SetBool("TurningRight", false);
            ResetIdleState();
        }
        else
        {
            animator.SetBool("TurningRight", false);
            animator.SetBool("TurningLeft", false);
        }
    }

    void ResetIdleState()
    {
        idleTimer = 0f;
        if (isInIdle2)
        {
            animator.ResetTrigger(idle2AnimationTrigger);
            isInIdle2 = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}