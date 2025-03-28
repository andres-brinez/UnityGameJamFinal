using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineFreeLookCamera : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private float horizontalSpeed = 2f;
    [SerializeField] private float verticalSpeed = 1f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 70f;
    [SerializeField] private bool invertY = false;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;

    private CinemachineVirtualCamera virtualCam;
    private Cinemachine3rdPersonFollow followComponent;
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 20f;

    private void Awake()
    {
        virtualCam = GetComponent<CinemachineVirtualCamera>();
        followComponent = virtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (followComponent == null)
        {
            Debug.LogError("Configura el Body de la cámara como '3rd Person Follow'");
        }
    }

    private void Update()
    {
        HandleCameraRotation();
        HandleZoom();
    }

    private void HandleCameraRotation()
    {
        if (Input.GetMouseButton(1)) // Rotar con clic derecho
        {
            // Movimiento horizontal
            currentHorizontalAngle += Input.GetAxis("Mouse X") * horizontalSpeed;

            // Movimiento vertical (con inversión opcional)
            float verticalInput = Input.GetAxis("Mouse Y") * (invertY ? 1 : -1);
            currentVerticalAngle += verticalInput * verticalSpeed;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

            // Aplicar rotación
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        // Convertir ángulos a posición de cámara
        float horizontalRad = currentHorizontalAngle * Mathf.Deg2Rad;
        float verticalRad = currentVerticalAngle * Mathf.Deg2Rad;

        // Calcular offset relativo
        followComponent.ShoulderOffset = new Vector3(
            Mathf.Sin(horizontalRad) * 0.5f, // Pequeño desplazamiento lateral
            Mathf.Sin(verticalRad),
            -Mathf.Cos(verticalRad)
        ) * followComponent.CameraDistance;
    }

    private void HandleZoom()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            followComponent.CameraDistance -= Input.mouseScrollDelta.y * zoomSpeed;
            followComponent.CameraDistance = Mathf.Clamp(followComponent.CameraDistance, minDistance, maxDistance);
            UpdateCameraPosition(); // Recalcular posición al hacer zoom
        }
    }
}