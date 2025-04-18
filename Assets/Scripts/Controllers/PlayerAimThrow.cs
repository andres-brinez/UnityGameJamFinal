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

        if (animator == null) Debug.LogError("El Animator no est� asignado.");
        if (trajectoryLine == null) Debug.LogError("LineRenderer no est� asignado.");

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
        // Solo permitir apuntar si hay frascos disponibles
        if (InventoryManager.Instance.GetPowerUpCount("Frasco") <= 0)
        {
            if (IsAiming) StopAiming();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartAiming();
        }

        if (IsAiming)
        {
            // Ajuste del �ngulo SIEMPRE activo (incluso en movimiento)
            HandleDynamicAim();
            UpdateTrajectory();
        }

        if (Input.GetMouseButtonUp(1))
        {
            StopAiming();
        }
    }

    void HandleDynamicAim()
    {
        // Solo ajuste vertical con el mouse (�ngulo de lanzamiento)
        float mouseY = Input.GetAxis("Mouse Y") * aimSensitivity;
        currentThrowAngle = Mathf.Clamp(currentThrowAngle - mouseY, minThrowAngle, maxThrowAngle);

        // Reducir sensibilidad al moverse para mejor control
        if (playerController && Mathf.Abs(playerController.currentSpeed) > 0.1f)
        {
            mouseY *= 0.7f; // 30% menos sensible si el jugador est� en movimiento
        }
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

        bool hitEnemy = false;
        int enemyLayerMask = 1 << 8; // Capa 8 = Enemy

        for (float t = 0; t <= simulationTime; t += trajectoryUpdateInterval)
        {
            Vector3 point = startPosition + startVelocity * t + 0.5f * Physics.gravity * t * t;
            points.Add(point);

            // Detecci�n de enemigos (capa 8)
            if (Physics.Linecast(previousPoint, point, out RaycastHit hit, enemyLayerMask))
            {
                points[points.Count - 1] = hit.point;
                hitEnemy = true;
                break;
            }

            // Detecci�n de otros objetos (opcional)
            if (Physics.Linecast(previousPoint, point, out RaycastHit defaultHit))
            {
                points[points.Count - 1] = defaultHit.point;
                break;
            }

            previousPoint = point;
        }

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());

        // Cambia el color solo si choca con enemigo
        if (hitEnemy)
        {
            trajectoryLine.material.color = Color.red; // Rojo al chocar
        }
        else
        {
            trajectoryLine.material.color = Color.white; // Blanco por defecto
        }
    }

    void ExecuteThrow()
    {
        // Verificar frascos disponibles
        if (InventoryManager.Instance.GetPowerUpCount("Frasco") <= 0)
        {
            Debug.Log("No hay frascos disponibles para lanzar");
            return;
        }

        if (projectilePrefab == null || throwPoint == null) return;

        // Lanzar el frasco
        GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 throwDirection = CalculateThrowDirection();

        projectile.transform.rotation = Quaternion.LookRotation(throwDirection);
        rb.AddForce(throwDirection * currentThrowForce, ForceMode.Impulse);

        // Restar un frasco del inventario
        InventoryManager.Instance.RemovePowerUp("Frasco");

    }
    //Metodos para agregar sonidos por eventos de animacion
    public void SoundThrow()
    {
        AudioManager.Instance.PlayFX("SoundThrow");
    }
    public void SoundAim()
    {
        AudioManager.Instance.PlayFX("SoundAim");
    }
}