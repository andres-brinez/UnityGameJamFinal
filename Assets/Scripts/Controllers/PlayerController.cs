using System.Security.Cryptography;
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

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint; // Punto de referencia para los pies
    [SerializeField] private float groundCheckRadius = 0.2f; // Radio de la esfera de detección
    [SerializeField] private LayerMask groundLayer;

    [Header("Idle Animation Settings")]
    [SerializeField] private float idleTimeout = 5f;
    [SerializeField] private string idle2AnimationTrigger = "Idle2";

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    public Animator animator;
    private Camera mainCamera;
    private PlayerAimThrow aimThrowController;

    public bool isGrounded = false;
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

    private string powerUpName = "PowerUp1"; // Nombre del powerup (un solo tipo)


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
        CheckGroundStatus();
        jumpInput = Input.GetButtonDown("Jump");
        HandleCrouch();
        HandleRotation();
        HandleCameraRotation();
        UpdateAnimations();
        HandleIdleAnimation();

        playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, crouchTransitionSpeed * Time.deltaTime);
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            PowerUpThrow(); // Llama a la función para lanzar el powerup
        }
    }
    void FixedUpdate()
    {
        Move();
        Jump();
    }

    void CheckGroundStatus()
    {
        if (groundCheckPoint == null)
        {
            Debug.LogError("GroundCheckPoint no está asignado en el Inspector!");
            return;
        }

        // Usamos OverlapSphere para detectar suelo de manera más precisa
        isGrounded = Physics.OverlapSphere(groundCheckPoint.position, groundCheckRadius, groundLayer).Length > 0;

        // Debug visual en el Editor
        Debug.DrawRay(groundCheckPoint.position, Vector3.down * groundCheckRadius,
                     isGrounded ? Color.green : Color.red);
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
            // Solo activa IsWalkingCrouched si realmente se está moviendo
            animator.SetBool("IsWalkingCrouched", Mathf.Abs(currentSpeed) > 0.1f);
            animator.SetBool("IsRunning", false);
        }
        else
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && (!aimThrowController || !aimThrowController.IsAiming);
            speed = isRunning ? runSpeed : walkSpeed;
            animator.SetBool("IsRunning", isRunning && Mathf.Abs(currentSpeed) > 0.1f);
            animator.SetBool("IsWalkingCrouched", false);
        }
    }

    void HandleCrouch()
    {
        //Si mantiene cntrl presionado se agacha, si no se levanta
        if (aimThrowController && aimThrowController.IsAiming) return;

        bool wantToCrouch = Input.GetKey(KeyCode.LeftControl);

        if (wantToCrouch != isCrouching)
        {
            isCrouching = wantToCrouch;

            if (isCrouching)
            {
                targetHeight = crouchHeight;
                targetCenter = crouchCenter;
            }
            else
            {
                targetHeight = standingHeight;
                targetCenter = standingCenter;
            }

            animator.SetBool("IsCrouching", isCrouching);
        }
    }
    void Jump()
    {
        if (jumpInput && isGrounded && !isCrouching && (!aimThrowController || !aimThrowController.IsAiming))
        {
            // Cancelar gravedad acumulada
            rb.AddForce(-Physics.gravity * rb.mass, ForceMode.Force);

            // Aplicar salto
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            animator.SetTrigger("JumpTrigger");
            isGrounded = false;
        }
    }
    void PowerUpThrow()
    {
        int count = InventoryManager.Instance.GetPowerUpCount(powerUpName);

        if (count > 0)
        {
            InventoryManager.Instance.RemovePowerUp(powerUpName);
            Debug.Log("PowerUp lanzado. Cantidad restante: " + InventoryManager.Instance.GetPowerUpCount(powerUpName));
        }
        else
        {
            // Si el jugador no tiene powerups, muestra un mensaje de error
            Debug.Log("No tienes powerups disponibles para lanzar.");
        }
    }

    void UpdateAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(currentSpeed));
        animator.SetBool("IsGrounded", isGrounded);

        // Añade esta línea para asegurarte de que IsWalkingCrouched se desactiva cuando no hay movimiento
        if (Mathf.Abs(currentSpeed) < 0.1f)
        {
            animator.SetBool("IsWalkingCrouched", false);
        }

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
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
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
}