using UnityEngine;

public class CircleBoss : MonoBehaviour
{
    public float damage = 5;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HealthBar health = collision.gameObject.GetComponent<HealthBar>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("No se encontró el componente HealthBar en el objeto Player.");
            }
        }
    }
}
