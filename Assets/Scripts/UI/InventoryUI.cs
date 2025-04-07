using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
public class InventoryUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_Text jarCountText;
    [SerializeField] private TMP_Text flashlightCountText;
    [SerializeField] private TMP_Text leafCountText;

    [Header("Iconos")]
    [SerializeField] private Image jarIcon;
    [SerializeField] private Image flashlightIcon;
    [SerializeField] private Image leafIcon;

    private void Awake()
    {
        // Verificar asignaciones en el Inspector
        if (jarCountText == null) Debug.LogError("Asignar jarCountText en el Inspector");
        if (flashlightCountText == null) Debug.LogError("Asignar flashlightCountText en el Inspector");
        if (leafCountText == null) Debug.LogError("Asignar leafCountText en el Inspector");
        if (jarIcon == null) Debug.LogError("Asignar jarIcon en el Inspector");
        if (flashlightIcon == null) Debug.LogError("Asignar flashlightIcon en el Inspector");
        if (leafIcon == null) Debug.LogError("Asignar leafIcon en el Inspector");
    }

    private IEnumerator Start()
    {
        // Esperar a que InventoryManager se inicialice
        while (InventoryManager.Instance == null)
        {
            yield return null;
        }

        // Suscribirse al evento
        InventoryManager.Instance.OnInventoryChanged += UpdateInventoryUI;
        UpdateInventoryUI();
    }

    private void OnDisable()
    {
        // Desuscribirse para evitar memory leaks
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateInventoryUI;
        }
    }

    public void UpdateInventoryUI()
    {
        // Verificar que InventoryManager existe
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager no está inicializado");
            return;
        }

        // Actualizar UI con verificaciones null
        if (jarCountText != null)
            jarCountText.text = InventoryManager.Instance.GetPowerUpCount("Frasco").ToString();

        if (flashlightCountText != null)
            flashlightCountText.text = InventoryManager.Instance.GetPowerUpCount("Linterna").ToString();

        if (leafCountText != null)
            leafCountText.text = InventoryManager.Instance.GetPowerUpCount("Hoja").ToString();

        if (jarIcon != null)
            jarIcon.gameObject.SetActive(InventoryManager.Instance.GetPowerUpCount("Frasco") > 0);

        if (flashlightIcon != null)
            flashlightIcon.gameObject.SetActive(InventoryManager.Instance.GetPowerUpCount("Linterna") > 0);

        if (leafIcon != null)
            leafIcon.gameObject.SetActive(InventoryManager.Instance.GetPowerUpCount("Hoja") > 0);
    }
}