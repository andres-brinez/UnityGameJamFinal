using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float crouchWalkSpeed = 1.5f;
    [SerializeField] private float crouchRunSpeed = 3f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float speed;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.25f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0.9f, 0);
    [SerializeField] private float crouchTransitionSpeed = 5f;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    public bool isGrounded = true;
    private bool jumpInput = false;
    private bool isCrouching = false;
    private bool isCrouchRunning = false;

    private Animator animator;
    private float currentSpeed;
    private float currentRotation;
    private float targetHeight;
    private Vector3 targetCenter;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider>();

        if (rb == null) Debug.LogError("El Rigidbody no está asignado.");
        if (animator == null) Debug.LogError("El Animator no está asignado.");
        if (playerCollider == null) Debug.LogError("El CapsuleCollider no está asignado.");

        rb.freezeRotation = true;
        targetHeight = standingHeight;
        targetCenter = standingCenter;
    }

    void Update()
    {
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        HandleCrouch();
        HandleRotation();
        UpdateAnimations();

        // Suavizar transición de agachado
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
        float rotation = Input.GetAxis("Horizontal");
        currentRotation = rotation;

        transform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime);
    }

    void HandleRun()
    {
        if (isCrouching)
        {
            // Correr agachado si se mantiene Shift
            isCrouchRunning = Input.GetKey(KeyCode.LeftShift);
            speed = isCrouchRunning ? crouchRunSpeed : crouchWalkSpeed;
            animator.SetBool("IsRunning", isCrouchRunning);
            animator.SetBool("IsWalkingCrouched", Mathf.Abs(currentSpeed) > 0.1f && !isCrouchRunning);
        }
        else
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            speed = isRunning ? runSpeed : walkSpeed;
            animator.SetBool("IsRunning", isRunning);
        }
    }

    void HandleCrouch()
    {
        // Verificar si está en movimiento significativo
        bool isMoving = Mathf.Abs(currentSpeed) > 0.1f || Mathf.Abs(currentRotation) > 0.1f;
        bool isRunning = animator.GetBool("IsRunning");

        // Solo permite cambiar estado de agachado si no está en movimiento activo
        if (!isMoving && !isRunning)
        {
            // Agacharse con LeftControl
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                // Solo permite agacharse si no está corriendo
                if (!animator.GetBool("IsRunning"))
                {
                    ToggleCrouch();
                }
            }

            // Auto-levantarse
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
            isCrouching = true; // No hay espacio, sigue agachado
        }

        animator.SetBool("IsCrouching", isCrouching);
    }

    bool CanStandUp()
    {
        return !Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight);
    }

    void Jump()
    {
        if (jumpInput && isGrounded && !isCrouching)
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

        // Animaciones de rotación
        if (currentRotation > 0.1f)
        {
            animator.SetBool("TurningRight", true);
            animator.SetBool("TurningLeft", false);
        }
        else if (currentRotation < -0.1f)
        {
            animator.SetBool("TurningLeft", true);
            animator.SetBool("TurningRight", false);
        }
        else
        {
            animator.SetBool("TurningRight", false);
            animator.SetBool("TurningLeft", false);
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