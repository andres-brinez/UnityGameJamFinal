using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;
    public float detectionRange = 2f;

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
    private bool isChasing = false;

    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0f;  

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("No se encontró el componente Animator en el enemigo.");
        }

        animator.SetBool("isWalking", true);

        SetRandomDestination();  // Empieza el patrullaje con destino aleatorio


    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si el jugador entra en el rango de detección, comienza a perseguirlo
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
            agent.speed = patrolSpeed;  
            agent.stoppingDistance = 0f; 
            agent.velocity = Vector3.zero;  // Detenemos la velocidad al patrullar

            
        }

        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolDelay || agent.remainingDistance < 0.5f)
        {
            SetRandomDestination();
            patrolTimer = 0f;
        }

        animator.SetBool("IsWalking", true);
        animator.SetBool("IsAttacking", false);
    }

    // 🔹 Persigue al jugador
    private void ChasePlayer()
    {
        if (!isChasing)
        {
            isChasing = true;
            agent.speed = chaseSpeed;  
            agent.stoppingDistance = stoppingDistance; 

            animator.SetBool("IsWalking", false);
            animator.SetBool("IsAttacking", true);


        }

        float distanceToTarget = Vector3.Distance(transform.position, player.position);

        // Si está cerca del jugador (dentro del stoppingDistance), lo detiene
        if (distanceToTarget <= stoppingDistance)
        {
            agent.velocity = Vector3.zero;  // Se detiene
        }
        else
        {
            agent.SetDestination(player.position);  // Sigue al jugador si está fuera de stoppingDistance
        }
    }

    // 🔹 Genera un punto aleatorio dentro del radio de patrullaje
    private void SetRandomDestination()
    {
        Vector3 randomPoint = RandomNavSphere(transform.position, patrolRadius);
        agent.SetDestination(randomPoint);  // Asigna un destino aleatorio dentro del radio de patrullaje
    }

    // 🔹 Encuentra un punto aleatorio en el NavMesh dentro de un radio
    private Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector2 randomDirection = Random.insideUnitCircle * distance;
        Vector3 finalPosition = new Vector3(origin.x + randomDirection.x, origin.y, origin.z + randomDirection.y);

        if (NavMesh.SamplePosition(finalPosition, out NavMeshHit hit, distance, NavMesh.AllAreas))
        {
            return hit.position;  // Devuelve una posición válida en el NavMesh
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
