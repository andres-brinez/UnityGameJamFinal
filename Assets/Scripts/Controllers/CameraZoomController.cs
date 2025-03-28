using UnityEngine;
using Cinemachine;

public class CameraZoomController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float normalDistance = 4f;
    [SerializeField] private float aimDistance = 2.5f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float normalShoulderOffset = 0.5f;
    [SerializeField] private float aimShoulderOffset = 0.2f;

    private Cinemachine3rdPersonFollow followComponent;
    private float targetDistance;
    private float targetShoulderOffset;
    private float currentZoomVelocity;
    private float currentOffsetVelocity;

    void Start()
    {
        if (virtualCamera != null)
        {
            followComponent = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            if (followComponent != null)
            {
                targetDistance = normalDistance;
                targetShoulderOffset = normalShoulderOffset;
                followComponent.CameraDistance = normalDistance;
                followComponent.ShoulderOffset.x = normalShoulderOffset;
            }
            else
            {
                Debug.LogError("No se encontró el componente 3rdPersonFollow en la Virtual Camera");
            }
        }
        else
        {
            Debug.LogError("Virtual Camera no asignada en el inspector");
        }
    }

    void Update()
    {
        if (followComponent != null)
        {
            // Suavizar el zoom
            followComponent.CameraDistance = Mathf.SmoothDamp(
                followComponent.CameraDistance,
                targetDistance,
                ref currentZoomVelocity,
                1f / zoomSpeed
            );

            // Suavizar el offset del hombro
            float currentOffset = followComponent.ShoulderOffset.x;
            float newOffset = Mathf.SmoothDamp(
                currentOffset,
                targetShoulderOffset,
                ref currentOffsetVelocity,
                1f / zoomSpeed
            );

            followComponent.ShoulderOffset = new Vector3(newOffset, followComponent.ShoulderOffset.y, followComponent.ShoulderOffset.z);
        }
    }

    public void SetAimZoom(bool isAiming)
    {
        if (isAiming)
        {
            targetDistance = aimDistance;
            targetShoulderOffset = aimShoulderOffset;
        }
        else
        {
            targetDistance = normalDistance;
            targetShoulderOffset = normalShoulderOffset;
        }
    }
}