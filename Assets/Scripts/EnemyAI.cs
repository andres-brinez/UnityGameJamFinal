using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;
    public float detectionRange =2f;

    [Header("Movement Speeds")]
    public float patrolSpeed = 1f;
    public float chaseSpeed = 1.2f;

    [Header("Patrol Settings")]
    public float patrolRadius = 3f;
    public float patrolDelay = 6f;
    private float patrolTimer = 0f;

    [Header("Chase Settings")]
    public float stoppingDistance = 0.3f;

    private NavMeshAgent agent;
    [SerializeField] private float currentSpeed;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentSpeed = patrolSpeed;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0f;
        SetRandomDestination();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // 🔹 Lógica de patrullaje
    private void Patrol()
    {
        if (isChasing)
        {
            isChasing = false;
            currentSpeed = patrolSpeed;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0f; // No hay distancia de parada al patrullar
        }

        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolDelay || agent.remainingDistance < 0.5f)
        {
            SetRandomDestination();
            patrolTimer = 0f;
        }
    }

    // 🔹 Persigue al jugador con aceleración progresiva
    private void ChasePlayer()
    {
        if (!isChasing)
        {
            isChasing = true;
            agent.speed = chaseSpeed;
            agent.stoppingDistance = stoppingDistance;
        }

        agent.SetDestination(player.position);
    }

    // 🔹 Genera un punto aleatorio dentro del radio de patrullaje
    private void SetRandomDestination()
    {
        Vector3 randomPoint = RandomNavSphere(transform.position, patrolRadius);
        agent.SetDestination(randomPoint);
    }

    // 🔹 Encuentra un punto aleatorio en el NavMesh dentro de un radio
    private Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector2 randomDirection = Random.insideUnitCircle * distance;
        Vector3 finalPosition = new Vector3(origin.x + randomDirection.x, origin.y, origin.z + randomDirection.y);

        if (NavMesh.SamplePosition(finalPosition, out NavMeshHit hit, distance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return origin; // Si no encuentra un punto válido, se queda en el mismo lugar
    }

    // 🔹 Dibuja los rangos en la vista del editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
