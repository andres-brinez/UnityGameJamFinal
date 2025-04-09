using UnityEngine;

public class ShadowProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 15f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<HealthBar>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}