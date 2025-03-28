using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerController))]
public class PlayerAimThrow : MonoBehaviour
{
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

    [Header("Camera Zoom Settings")]
    [SerializeField] private CameraZoomController cameraZoomController;
    [SerializeField] private float aimDistanceChangeSpeed = 5f;

    [Header("Charged Throw Settings")]
    [SerializeField] private float maxChargeTime = 1.5f;
    [SerializeField] private float maxChargeMultiplier = 2f;
    [SerializeField] private float chargeSpeed = 1f;
    [SerializeField] private float minThrowReleaseTime = 0.15f;

    private Animator animator;
    private Camera mainCamera;
    private PlayerController playerController;

    public bool IsAiming { get; private set; } = false;
    private bool isCharging = false;
    private float currentThrowAngle = 45f;
    private float currentThrowDistance = 10f;
    private float currentCharge = 0f;
    private float currentThrowForce;
    private float mouseDownTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        playerController = GetComponent<PlayerController>();

        if (animator == null) Debug.LogError("El Animator no está asignado.");
        if (trajectoryLine == null) Debug.LogError("LineRenderer no está asignado.");

        trajectoryLine.enabled = false;

        currentThrowAngle = Mathf.Lerp(minThrowAngle, maxThrowAngle, 0.5f);
        currentThrowDistance = Mathf.Lerp(minThrowDistance, maxThrowDistance, 0.5f);
        currentThrowForce = baseThrowForce;

        if (aimDirectionReference == null)
        {
            aimDirectionReference = new GameObject("Aim Direction Reference");
            aimDirectionReference.transform.SetParent(transform);
        }
    }

    void Update()
    {
        HandleAiming();
        HandleChargedThrow();

        if (IsAiming)
        {
            UpdateAimDirectionReference();
            UpdateTrajectory();
        }
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

    void HandleAiming()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartAiming();
        }

        if (IsAiming)
        {
            HandleDynamicAim();

            if (Mathf.Abs(playerController ? playerController.currentSpeed : 0f) > 0.1f)
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
        if (!IsAiming) return;

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
        if (animator != null)
        {
            animator.SetBool("IsChargingThrow", true);
        }
    }

    void ContinueCharging()
    {
        currentCharge += Time.deltaTime * chargeSpeed;
        currentCharge = Mathf.Clamp(currentCharge, 0f, maxChargeTime);
        currentThrowForce = baseThrowForce * (1 + (maxChargeMultiplier - 1) * (currentCharge / maxChargeTime));
        if (animator != null)
        {
            animator.SetFloat("ChargeAmount", currentCharge / maxChargeTime);
        }
    }

    void ReleaseThrow()
    {
        ExecuteThrow();
        isCharging = false;
        currentCharge = 0f;
        currentThrowForce = baseThrowForce;
        if (animator != null)
        {
            animator.SetBool("IsChargingThrow", false);
            animator.SetTrigger("Throw");
        }
    }

    void CancelThrow()
    {
        isCharging = false;
        currentCharge = 0f;
        currentThrowForce = baseThrowForce;
        if (animator != null)
        {
            animator.SetBool("IsChargingThrow", false);
        }
    }

    void StartAiming()
    {
        IsAiming = true;
        if (animator != null)
        {
            animator.SetBool("IsAiming", true);
        }
        trajectoryLine.enabled = true;
        if (cameraZoomController != null)
        {
            cameraZoomController.SetAimZoom(true);
        }
    }

    void StopAiming()
    {
        IsAiming = false;
        isCharging = false;
        currentCharge = 0f;
        if (animator != null)
        {
            animator.SetBool("IsAiming", false);
            animator.SetBool("IsChargingThrow", false);
        }
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
        if (projectilePrefab == null || throwPoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 throwDirection = CalculateThrowDirection();

        projectile.transform.rotation = Quaternion.LookRotation(throwDirection);
        rb.AddForce(throwDirection * currentThrowForce, ForceMode.Impulse);
    }
}