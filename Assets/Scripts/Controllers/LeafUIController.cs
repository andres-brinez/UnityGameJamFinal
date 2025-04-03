using UnityEngine;

public class LeafUIController : MonoBehaviour
{
    [Header("Configuración Hoja")]
    [SerializeField] private GameObject leafUI;
    [SerializeField] private KeyCode toggleKey = KeyCode.H;

    private void Update()
    {
        // Verificar si tenemos hoja en el inventario
        bool hasLeaf = InventoryManager.Instance.GetPowerUpCount("Hoja") > 0;

        if (hasLeaf && Input.GetKeyDown(toggleKey))
        {
            leafUI.SetActive(!leafUI.activeSelf);
        }
        else if (!hasLeaf && leafUI.activeSelf)
        {
            leafUI.SetActive(false);
        }
    }
}