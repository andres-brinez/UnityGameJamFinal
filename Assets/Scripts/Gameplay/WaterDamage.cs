using UnityEngine;

public class WaterDamage : MonoBehaviour
{
    public float damagePerSecond = 5f;
    private bool isPlayerInWater = false;
    private HealthBar playerHealth;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<HealthBar>();

            if (playerHealth == null)
            {
                Debug.LogError("No se encontró el componente HealthBar en el jugador.");
            }
            else
            {
                isPlayerInWater = true;
                Debug.Log("Jugador entró al agua, comenzará a recibir daño.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInWater = false;
            Debug.Log("Jugador salió del agua, ya no recibe daño.");
        }
    }

    void Update()
    {
        if (isPlayerInWater && playerHealth != null)
        {
            playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
