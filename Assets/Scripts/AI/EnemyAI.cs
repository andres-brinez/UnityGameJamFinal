using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Player Detection")]
    public float detectionRange = 12f;
    public float attackRange = 2f;
    public LayerMask playerLayer;

    [Header("Movement Settings")]
    public float patrolSpeed = 40f;
    public float chaseSpeed = 50f;
    public float rotationSpeed = 5f;

    [Header("Braking System")]
    public float brakingDistance = 3f;
    public float brakingSharpness = 2f;

    [Header("Patrol Settings")]
    public float patrolRadius = 13f;
    public float patrolDelay = 6f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private bool isChasing;
    private float patrolTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = patrolSpeed;
        agent.angularSpeed = 360f; // Rotación rápida
        StartCoroutine(WaitForPlayer());
    }

    void Update()
    {
        if (player == null) return;

        bool playerDetected = Physics.CheckSphere(transform.position, detectionRange, playerLayer);

        if (playerDetected) ChasePlayer();
        else if (isChasing) StartCoroutine(ReturnToPatrol(1.5f));
        else Patrol();
    }

    private void ChasePlayer()
    {
        if (!isChasing)
        {
            isChasing = true;
            agent.speed = chaseSpeed;
            animator.SetBool("isWalking", true);
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // Sistema de frenado progresivo
        if (distance <= brakingDistance)
        {
            float speedFactor = Mathf.Clamp01(distance / brakingDistance);
            agent.speed = Mathf.Lerp(0.5f, chaseSpeed, speedFactor * brakingSharpness);
        }
        else
        {
            agent.speed = chaseSpeed;
        }

        LookAtPlayer();

        if (distance <= attackRange) AttackPlayer();
        else KeepChasing();
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("IsAttacking", true);
    }

    private void KeepChasing()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.SetBool("IsAttacking", false);
    }

    private void Patrol()
    {
        if (isChasing)
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            agent.isStopped = false;
        }

        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolDelay || agent.remainingDistance < 0.5f)
        {
            SetRandomDestination();
            patrolTimer = 0f;
        }

        animator.SetBool("isWalking", true);
        animator.SetBool("IsAttacking", false);
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime * (isChasing ? 2f : 1f)
            );
        }
    }

    private IEnumerator ReturnToPatrol(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!Physics.CheckSphere(transform.position, detectionRange, playerLayer))
            Patrol();
    }

    private IEnumerator WaitForPlayer()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void SetRandomDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * patrolRadius;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detección (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Radio de patrullaje (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Rango de frenado (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, brakingDistance);

        // Rango de ataque (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}