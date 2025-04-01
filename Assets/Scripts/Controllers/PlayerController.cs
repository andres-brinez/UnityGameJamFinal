using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

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

    [Header("Throwing Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float baseThrowForce = 10f;
    [SerializeField] private LineRenderer trajectoryLine;

    [Header("Dynamic Aim Settings")]
    [SerializeField] private float minThrowAngle = 5f;
    [SerializeField] private float maxThrowAngle = 85f;
    [SerializeField] private float minThrowDistance = 2f;
    [SerializeField] private float maxThrowDistance = 20f;
    [SerializeField] private float aimSensitivity = 1f;
    [SerializeField] private float trajectoryUpdateInterval = 0.1f;

    [Header("Aim Reference Settings")]
    [SerializeField] private GameObject aimDirectionReference;
    [SerializeField] private float referenceDistance = 2f;

    [Header("Camera Settings")]
    [SerializeField] private float cameraRotationSpeed = 2f;
    [SerializeField] private float cameraMaxYAngle = 80f;
    [SerializeField] private float cameraMinYAngle = -80f;
    [SerializeField] private float aimDistanceChangeSpeed = 5f;
    [SerializeField] private CameraZoomController cameraZoomController;

    [Header("Charged Throw Settings")]
    [SerializeField] private float maxChargeTime = 1.5f;
    [SerializeField] private float maxChargeMultiplier = 2f;
    [SerializeField] private float chargeSpeed = 1f;
    [SerializeField] private float minThrowReleaseTime = 0.15f;

    [Header("Idle Animation Settings")]
    [SerializeField] private float idleTimeout = 5f; // Tiempo para activar idle 2
    [SerializeField] private string idle2AnimationTrigger = "Idle2";
    private float idleTimer = 0f;
    private bool isInIdle2 = false;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator animator;
    private Camera mainCamera;

    public bool isGrounded = true;
    private bool jumpInput = false;
    private bool isCrouching = false;
    private bool isAiming = false;
    private bool isCharging = false;

    private float currentSpeed;
    private float currentRotation;
    private float targetHeight;
    private Vector3 targetCenter;
    private float currentThrowAngle = 45f;
    private float currentThrowDistance = 10f;
    private float cameraHorizontalRotation = 0f;
    private float cameraVerticalRotation = 0f;
    private float currentCharge = 0f;
    private float currentThrowForce;
    private float mouseDownTime;
    private string powerUpName = "PowerUp1"; // Nombre del powerup (un solo tipo)


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main;

        if (rb == null) Debug.LogError("El Rigidbody no está asignado.");
        if (animator == null) Debug.LogError("El Animator no está asignado.");
        if (playerCollider == null) Debug.LogError("El CapsuleCollider no está asignado.");
        if (trajectoryLine == null) Debug.LogError("LineRenderer no está asignado.");

        rb.freezeRotation = true;
        targetHeight = standingHeight;
        targetCenter = standingCenter;
        trajectoryLine.enabled = false;

        currentThrowAngle = Mathf.Lerp(minThrowAngle, maxThrowAngle, 0.5f);
        currentThrowDistance = Mathf.Lerp(minThrowDistance, maxThrowDistance, 0.5f);
        currentThrowForce = baseThrowForce;

        cameraHorizontalRotation = transform.eulerAngles.y;
        if (mainCamera != null)
        {
            cameraVerticalRotation = mainCamera.transform.eulerAngles.x;
        }

        if (aimDirectionReference == null)
        {
            aimDirectionReference = new GameObject("Aim Direction Reference");
            aimDirectionReference.transform.SetParent(transform);
        }
    }

    void Update()
    {
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        HandleCrouch();
        HandleRotation();
        HandleCameraRotation();
        HandleAiming();
        HandleChargedThrow();
        UpdateAnimations();
        HandleIdleAnimation();

        if (isAiming)
        {
            UpdateAimDirectionReference();
            UpdateTrajectory();
        }

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

    void UpdateAimDirectionReference()
    {
        if (mainCamera != null && aimDirectionReference != null)
        {
            Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * referenceDistance;
            aimDirectionReference.transform.position = targetPosition;
            aimDirectionReference.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
        }
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
        if (!isAiming)
        {
            float rotation = Input.GetAxis("Horizontal");
            currentRotation = rotation;
            transform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime);
            cameraHorizontalRotation = transform.eulerAngles.y;
        }
    }
    void HandleIdleAnimation()
    {
        // Verificar si el personaje está quieto (sin movimiento ni rotación)
        bool isIdle = Mathf.Abs(currentSpeed) < 0.1f &&
                     Mathf.Abs(currentRotation) < 0.1f &&
                     !isAiming &&
                     !isCrouching &&
                     isGrounded;

        if (isIdle)
        {
            idleTimer += Time.deltaTime;

            // Activar idle2 después del timeout
            if (idleTimer >= idleTimeout && !isInIdle2)
            {
                animator.SetTrigger(idle2AnimationTrigger);
                isInIdle2 = true;
            }
        }
        else
        {
            // Resetear el temporizador si hay movimiento
            idleTimer = 0f;
            isInIdle2 = false;
        }
    }
    void HandleCameraRotation()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.1f || isAiming)
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
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isAiming;
            speed = isRunning ? runSpeed : walkSpeed;
            animator.SetBool("IsRunning", isRunning);
        }
    }

    void HandleCrouch()
    {
        if (isAiming) return;

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
        if (jumpInput && isGrounded && !isCrouching && !isAiming)
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