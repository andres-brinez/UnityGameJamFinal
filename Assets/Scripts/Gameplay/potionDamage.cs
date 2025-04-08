using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public int damage = 15; // Daño que hace el proyectil.

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss")) // Asegúrate de que el Boss tenga el tag "Boss".
        {
            HealthSystem boss = other.GetComponent<HealthSystem>();
            if (boss != null)
            {
                boss.TakeDamage(damage); // Resta vida al boss.
            }
            Destroy(gameObject); // Destruye el proyectil al impactar.
        }
    }
}