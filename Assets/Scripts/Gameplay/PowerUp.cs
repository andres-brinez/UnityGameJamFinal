using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private const string powerUpName = "PowerUp1";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.AddPowerUp(powerUpName);
            Destroy(gameObject);
        }
    }
}
