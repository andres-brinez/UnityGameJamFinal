using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Configuración Linterna")]
    [SerializeField] private Light flashlight;
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    private void Awake()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }
        flashlight.enabled = false;
    }

    private void Update()
    {
        // Verificar si tenemos linterna en el inventario
        bool hasFlashlight = InventoryManager.Instance.GetPowerUpCount("Linterna") > 0;

        if (hasFlashlight && Input.GetKeyDown(toggleKey))
        {
            flashlight.enabled = !flashlight.enabled;
        }
        else if (!hasFlashlight && flashlight.enabled)
        {
            flashlight.enabled = false;
        }
    }
}