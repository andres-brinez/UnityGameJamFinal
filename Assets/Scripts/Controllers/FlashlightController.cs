using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Configuración Linterna")]
    [SerializeField] private Light flashlight;
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    [SerializeField] private bool toggleMode = false; // Cambiar a true para el modo toggle tradicional

    // Propiedad pública para verificar el estado
    public bool IsFlashlightOn { get; private set; }

    private void Awake()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }
        flashlight.enabled = false;
        IsFlashlightOn = false;
    }

    private void Update()
    {
        // Verificar si tenemos linterna en el inventario
        bool hasFlashlight = InventoryManager.Instance.GetPowerUpCount("Linterna") > 0;

        if (!hasFlashlight)
        {
            if (flashlight.enabled)
            {
                flashlight.enabled = false;
                IsFlashlightOn = false;
            }
            return;
        }

        if (toggleMode)
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleFlashlight();
            }
        }
        else
        {
            // Modo mantener presionado
            if (Input.GetKey(toggleKey))
            {
                if (!IsFlashlightOn)
                {
                    TurnOnFlashlight();
                }
            }
            else
            {
                if (IsFlashlightOn)
                {
                    TurnOffFlashlight();
                }
            }
        }
    }

    private void ToggleFlashlight()
    {
        IsFlashlightOn = !IsFlashlightOn;
        flashlight.enabled = IsFlashlightOn;

        // AudioManager.Instance.PlaySFX(IsFlashlightOn ? "flashlight_on" : "flashlight_off");
    }

    private void TurnOnFlashlight()
    {
        IsFlashlightOn = true;
        flashlight.enabled = true;
        // AudioManager.Instance.PlaySFX("flashlight_on");
    }

    private void TurnOffFlashlight()
    {
        IsFlashlightOn = false;
        flashlight.enabled = false;
        // AudioManager.Instance.PlaySFX("flashlight_off");
    }
}