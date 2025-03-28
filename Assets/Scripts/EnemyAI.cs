using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;  
    public float detectionRange = 5f;  // Rango de detección
    public float patrolRadius = 10f;   // Radio donde patrulla aleatoriamente
    public float patrolDelay = 3f;     // Tiempo entre cambios de patrulla
    private float patrolTimer = 0f;    

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetRandomDestination();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)  
        {
            // Persigue al jugador
            agent.SetDestination(player.position);
        }
        else  
        {
            // Patrullaje aleatorio
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolDelay || agent.remainingDistance < 0.5f)
            {
                SetRandomDestination();
                patrolTimer = 0f;
            }
        }
    }

    void SetRandomDestination()
    {
        Vector3 randomPoint = RandomNavSphere(transform.position, patrolRadius);
        agent.SetDestination(randomPoint);
    }

    Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector2 randomDirection = Random.insideUnitCircle * distance;
        Vector3 finalPosition = new Vector3(origin.x + randomDirection.x, origin.y, origin.z + randomDirection.y);

        if (NavMesh.SamplePosition(finalPosition, out NavMeshHit hit, distance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return origin; // Si no encuentra un punto válido, se queda en el mismo lugar
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Dibuja el área de detección
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius); // Dibuja el área de patrullaje
    }
}
