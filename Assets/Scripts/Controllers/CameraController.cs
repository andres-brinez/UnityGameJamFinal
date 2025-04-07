using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private CinemachineBasicMultiChannelPerlin CinemachineBasicMultiChannelPerlin;
    private float tiempoMov;
    private float tiempoMovTotal;
    private float intensidadInicial;
    private CinemachineBasicMultiChannelPerlin noise;
    private void Awake()
    {
        Instance = this;
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        CinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
    private void Start()
    {
        {
            // Obtener el componente de noise
            noise = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            // Configurar ambos valores a 0 al inicio
            if (noise != null)
            {
                noise.m_AmplitudeGain = 0f;
                noise.m_FrequencyGain = 0f;
            }
        }
    }
    public void MoverCam (float intensidad, float frecuencia, float tiempo)
    {
        CinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensidad;
        CinemachineBasicMultiChannelPerlin.m_FrequencyGain = frecuencia;
        intensidadInicial = intensidad;
        tiempoMovTotal = tiempo;
        tiempoMov = tiempo;
    }
    private void Update()
    {
        if(tiempoMov > 0)
        {
            tiempoMov -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(intensidadInicial, 0, 1 -(tiempoMov/ tiempoMovTotal));
        }
    }

}
