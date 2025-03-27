using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private float maxThrowDistance = 10f;
    [SerializeField] private float trajectoryUpdateInterval = 0.1f;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator animator;
    private Camera mainCamera;

    public bool isGrounded = true;
    private bool jumpInput = false;
    private bool isCrouching = false;
    private bool isAiming = false;

    private float currentSpeed;
    private float currentRotation;
    private float targetHeight;
    private Vector3 targetCenter;

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
    }

    void Update()
    {
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        HandleCrouch();
        HandleRotation();
        HandleAiming();
        HandleThrowing();
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

        // No rotar mientras se apunta
        if (!isAiming)
        {
            transform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime);
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
        // No agacharse mientras se apunta
        if (isAiming) return;

        // Verificar si está en movimiento significativo
        bool isMoving = Mathf.Abs(currentSpeed) > 0.1f || Mathf.Abs(currentRotation) > 0.1f;
        bool isRunning = animator.GetBool("IsRunning");

        // Solo permite cambiar estado de agachado si no está en movimiento activo
        if (!isMoving && !isRunning)
        {
            // Agacharse con LeftControl
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                ToggleCrouch();
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
        // Apuntar con clic derecho (mantener)
        if (Input.GetMouseButtonDown(1))
        {
            StartAiming();
        }

        if (isAiming)
        {
            UpdateTrajectory();

            // Cancelar apuntado si nos movemos
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

    void StartAiming()
    {
        isAiming = true;
        animator.SetBool("IsAiming", true);
        trajectoryLine.enabled = true;
    }

    void StopAiming()
    {
        isAiming = false;
        animator.SetBool("IsAiming", false);
        trajectoryLine.enabled = false;
    }

    void UpdateTrajectory()
    {
        Vector3 throwDirection = CalculateThrowDirection();
        DrawTrajectory(throwPoint.position, throwDirection);
    }

    Vector3 CalculateThrowDirection()
    {
        // Dirección hacia adelante con ángulo de lanzamiento
        Vector3 direction = Quaternion.AngleAxis(throwAngle, -transform.right) * transform.forward;
        return direction.normalized;
    }

    void DrawTrajectory(Vector3 startPosition, Vector3 startVelocity)
    {
        List<Vector3> points = new List<Vector3>();
        float simulationTime = maxThrowDistance / (throwForce * Mathf.Cos(throwAngle * Mathf.Deg2Rad));
        Vector3 previousPoint = startPosition;
        points.Add(previousPoint);

        for (float t = 0; t <= simulationTime; t += trajectoryUpdateInterval)
        {
            Vector3 point = startPosition + startVelocity * throwForce * t + 0.5f * Physics.gravity * t * t;
            points.Add(point);

            // Detectar colisiones
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

    void HandleThrowing()
    {
        if (isAiming && Input.GetMouseButtonDown(0))
        {
            ThrowProjectile();
            animator.SetTrigger("Throw");
            StopAiming();
        }
    }

    void ThrowProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 throwDirection = CalculateThrowDirection();

        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
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