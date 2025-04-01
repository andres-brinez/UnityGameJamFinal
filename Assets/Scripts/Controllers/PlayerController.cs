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
    [SerializeField] private float groundCheckDistance = 0.2f;
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
        jumpInput = Input.GetKeyDown(KeyCode.Space);
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
        // Calcula el punto más bajo del collider
        float bottomOfCollider = playerCollider.bounds.min.y;
        Vector3 rayStart = new Vector3(transform.position.x, bottomOfCollider + playerCollider.radius, transform.position.z);

        // Dispara un raycast hacia abajo
        isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, groundLayer);

        // Opcional: Dibuja el raycast en el editor para debug
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
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
            animator.SetBool("IsRunning", false); // Asegura que no se active running mientras está agachado
        }
        else
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && (!aimThrowController || !aimThrowController.IsAiming);
            speed = isRunning ? runSpeed : walkSpeed;
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsWalkingCrouched", false); // Asegura que no se active caminar agachado cuando esta de pie
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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            jumpInput = false;
            animator.SetTrigger("JumpTrigger");
        }
    }

    void HandleAiming()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartAiming();
        }

        if (isAiming)
        {
            HandleDynamicAim();

            if (Mathf.Abs(currentSpeed) > 0.1f)
            {
                StopAiming();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            StopAiming();
        }
    }

    void HandleDynamicAim()
    {
        float mouseY = Input.GetAxis("Mouse Y") * aimSensitivity;
        currentThrowAngle = Mathf.Clamp(currentThrowAngle - mouseY, minThrowAngle, maxThrowAngle);

        float distanceInput = 0f;
        if (Input.GetKey(KeyCode.Q)) distanceInput = -1f;
        if (Input.GetKey(KeyCode.E)) distanceInput = 1f;

        currentThrowDistance = Mathf.Clamp(
            currentThrowDistance + distanceInput * aimDistanceChangeSpeed * Time.deltaTime,
            minThrowDistance,
            maxThrowDistance
        );
    }

    void HandleChargedThrow()
    {
        if (!isAiming) return;

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownTime = Time.time;
            StartCharging();
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            ContinueCharging();
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            if (Time.time - mouseDownTime >= minThrowReleaseTime)
            {
                ReleaseThrow();
            }
            else
            {
                CancelThrow();
            }
        }
    }

    void StartCharging()
    {
        isCharging = true;
        currentCharge = 0f;
        animator.SetBool("IsChargingThrow", true);
    }

    void ContinueCharging()
    {
        currentCharge += Time.deltaTime * chargeSpeed;
        currentCharge = Mathf.Clamp(currentCharge, 0f, maxChargeTime);
        currentThrowForce = baseThrowForce * (1 + (maxChargeMultiplier - 1) * (currentCharge / maxChargeTime));
        animator.SetFloat("ChargeAmount", currentCharge / maxChargeTime);
    }

    void ReleaseThrow()
    {
        ExecuteThrow();
        isCharging = false;
        currentCharge = 0f;
        currentThrowForce = baseThrowForce;
        animator.SetBool("IsChargingThrow", false);
        animator.SetTrigger("Throw");
    }

    void CancelThrow()
    {
        isCharging = false;
        currentCharge = 0f;
        currentThrowForce = baseThrowForce;
        animator.SetBool("IsChargingThrow", false);
    }

    void StartAiming()
    {
        isAiming = true;
        animator.SetBool("IsAiming", true);
        trajectoryLine.enabled = true;
        if (cameraZoomController != null)
        {
            cameraZoomController.SetAimZoom(true);
        }
    }

    void StopAiming()
    {
        isAiming = false;
        isCharging = false;
        currentCharge = 0f;
        animator.SetBool("IsAiming", false);
        animator.SetBool("IsChargingThrow", false);
        trajectoryLine.enabled = false;
        if (cameraZoomController != null)
        {
            cameraZoomController.SetAimZoom(false);
        }
    }

    void UpdateTrajectory()
    {
        Vector3 throwDirection = CalculateThrowDirection();
        float displayForce = isCharging ? currentThrowForce : baseThrowForce;
        DrawTrajectory(throwPoint.position, throwDirection * displayForce);
    }

    Vector3 CalculateThrowDirection()
    {
        Vector3 baseDirection = transform.forward;
        Vector3 playerRight = transform.right;
        return Quaternion.AngleAxis(currentThrowAngle, -playerRight) * baseDirection;
    }

    void DrawTrajectory(Vector3 startPosition, Vector3 startVelocity)
    {
        List<Vector3> points = new List<Vector3>();
        float simulationTime = currentThrowDistance / (startVelocity.magnitude * Mathf.Cos(currentThrowAngle * Mathf.Deg2Rad));
        Vector3 previousPoint = startPosition;
        points.Add(previousPoint);

        for (float t = 0; t <= simulationTime; t += trajectoryUpdateInterval)
        {
            Vector3 point = startPosition + startVelocity * t + 0.5f * Physics.gravity * t * t;
            points.Add(point);

            if (Physics.Linecast(previousPoint, point, out RaycastHit hit))
            {
                points[points.Count - 1] = hit.point;
                break;
            }

            previousPoint = point;
        }

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());
    }

    void ExecuteThrow()
    {
        GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 throwDirection = CalculateThrowDirection();

        projectile.transform.rotation = Quaternion.LookRotation(throwDirection);
        rb.AddForce(throwDirection * currentThrowForce, ForceMode.Impulse);
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