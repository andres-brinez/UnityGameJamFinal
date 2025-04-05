using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("Player Detection")]
    public float detectionRange = 12f;
    public float attackRange = 2f;
    public float stoppingDistance = 1.5f;
    public LayerMask playerLayer;

    [Header("Movement Settings")]
    public float patrolSpeed = 4f;
    public float chaseSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Braking System")]
    public float brakingDistance = 5f; 
    public float brakingSharpness = 2f; 

    [Header("Waypoint Patrol")]
    public List<Transform> waypoints;
    public float waypointStopTime = 2f;

    [Header("Hide Detection")]
    public float hideCheckRadius = 15f;
    public LayerMask bushLayer;

    [Header("Confusion Settings")]
    public float confusionDuration = 3f;
    public Animator enemyAnimator;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private int currentWaypointIndex = 0;
    private float waypointTimer = 0f;
    private bool isChasing;
    private bool playerIsHiding;
    private bool wasPlayerHidingLastFrame;
    private bool isConfused;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = patrolSpeed;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 0; // Control manual

        StartCoroutine(WaitForPlayer());

        if (waypoints.Count > 0)
            GoToRandomWaypoint();
    }

    void Update()
    {
        if (player == null || waypoints.Count == 0) return;

        wasPlayerHidingLastFrame = playerIsHiding;
        CheckIfPlayerIsHiding();

        // Solo detecta al jugador si NO está escondido
        bool playerDetected = Physics.CheckSphere(transform.position, detectionRange, playerLayer) && !playerIsHiding;

        if (playerDetected)
        {
            if (!isChasing)
            {
                isChasing = true;
                agent.speed = chaseSpeed;
            }
            ChasePlayer();
        }
        else if (isChasing)
        {
            if (playerIsHiding && !wasPlayerHidingLastFrame)
            {
                StartCoroutine(PlayConfusionAndReturnToPatrol());
            }
            else
            {
                StartCoroutine(ReturnToPatrol());
            }
        }
        else
        {
            Patrol();
        }
    }

    private void CheckIfPlayerIsHiding()
    {
        Collider[] bushes = Physics.OverlapSphere(transform.position, hideCheckRadius, bushLayer);
        playerIsHiding = false;

        foreach (var bush in bushes)
        {
            HidePlayer bushController = bush.GetComponent<HidePlayer>();
            if (bushController != null && bushController.IsPlayerHiding)
            {
                playerIsHiding = true;
                break;
            }
        }
    }
    private void ForceReturnToPatrol()
    {
        StopAllCoroutines(); // Detiene cualquier retorno a patrulla en progreso
        isChasing = false;
        agent.speed = patrolSpeed;
        GoToRandomWaypoint();
    }
    private IEnumerator PlayConfusionAndReturnToPatrol()
    {
        // Entrar en estado de confusión
        isConfused = true;
        enemyAnimator.SetBool("IsConfused", true);
        agent.isStopped = true;

        // Reproducir sonido de confusion
        // AudioManager.Instance.PlaySFX("enemy_confused");

        // Esperar la duración de la animación
        yield return new WaitForSeconds(confusionDuration);

        // Salir del estado de confusión
        enemyAnimator.SetBool("IsConfused", false);
        isConfused = false;

        // Volver a patrullar
        ForceReturnToPatrol();
    }
    private void ChasePlayer()
    {
        if (isConfused) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Sistema de frenado progresivo
        if (distanceToPlayer <= brakingDistance && distanceToPlayer > stoppingDistance)
        {
            float speedFactor = Mathf.Clamp01((distanceToPlayer - stoppingDistance) / (brakingDistance - stoppingDistance));
            agent.speed = Mathf.Lerp(0.5f, chaseSpeed, speedFactor * brakingSharpness);
        }
        else if (distanceToPlayer > brakingDistance)
        {
            agent.speed = chaseSpeed;
        }

        // Movimiento y detencion
        if (distanceToPlayer > stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("isWalking", true);
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("isWalking", false);
        }

        LookAtPlayer();

        // Ataque
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            animator.SetBool("IsAttacking", false);
        }
    }

    private void AttackPlayer()
    {
        if (isConfused) return;
        animator.SetBool("IsAttacking", true);
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        animator.SetBool("isWalking", true);
        animator.SetBool("IsAttacking", false);

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            waypointTimer += Time.deltaTime;

            if (waypointTimer >= waypointStopTime)
            {
                GoToRandomWaypoint();
                waypointTimer = 0f;
            }
        }
    }

    private void GoToRandomWaypoint()
    {
        if (waypoints.Count == 0) return;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, waypoints.Count);
        } while (newIndex == currentWaypointIndex && waypoints.Count > 1);

        currentWaypointIndex = newIndex;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    private IEnumerator ReturnToPatrol()
    {
        yield return new WaitForSeconds(1.5f);

        if (!Physics.CheckSphere(transform.position, detectionRange, playerLayer))
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            GoToRandomWaypoint();
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private IEnumerator WaitForPlayer()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, detectionRange);

        // Zona de frenado (amarillo)
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, brakingDistance);

        // Stopping distance (verde)
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, stoppingDistance);
    }
}