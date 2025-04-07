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
    public LayerMask obstacleLayer;

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
    public string confusedAnimationParam = "IsConfused";

    [Header("Flashlight Confusion")]
    public float flashlightConfusionDuration = 5f;
    public float flashlightDetectionAngle = 45f;
    public float flashlightDetectionDistance = 15f;
    public float minFlashlightConfusionCooldown = 10f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private int currentWaypointIndex = 0;
    private float waypointTimer = 0f;
    private bool isChasing;
    private bool playerIsHiding;
    private bool wasPlayerHidingLastFrame;
    private bool isConfused;
    private bool isFlashlightConfused;
    private float lastFlashlightConfusionTime;
    private FlashlightController flashlightController;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = patrolSpeed;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 0;

        flashlightController = FindFirstObjectByType<FlashlightController>();
        StartCoroutine(WaitForPlayer());

        if (waypoints.Count > 0)
            GoToRandomWaypoint();
    }

    void Update()
    {
        if (player == null || waypoints.Count == 0) return;

        wasPlayerHidingLastFrame = playerIsHiding;
        CheckIfPlayerIsHiding();

        // Verificar si la linterna está afectando al enemigo
        if (!isFlashlightConfused && !isConfused &&
            Time.time > lastFlashlightConfusionTime + minFlashlightConfusionCooldown &&
            IsPlayerFlashlightShiningOnEnemy())
        {
            StartCoroutine(FlashlightConfusionRoutine());
        }

        // Solo detecta al jugador si:
        // 1. Está dentro del rango de detección
        // 2. No esta escondido
        // 3. No esta confundido por la linterna
        // 4. No esta en estado de confusión normal
        bool playerDetected = Physics.CheckSphere(transform.position, detectionRange, playerLayer)
                            && !playerIsHiding
                            && !isFlashlightConfused
                            && !isConfused;

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
            // Si el jugador se esconde y no estaba escondido en el frame anterior
            if (playerIsHiding && !wasPlayerHidingLastFrame && !isFlashlightConfused)
            {
                StartCoroutine(PlayConfusionAndReturnToPatrol());
            }
            else if (!isFlashlightConfused && !isConfused)
            {
                StartCoroutine(ReturnToPatrol());
            }
        }
        else
        {
            Patrol();
        }

        UpdateAnimationParameters();
    }

    private void UpdateAnimationParameters()
    {
        // Solo actualizar animaciones si no está confundido
        if (!isConfused && !isFlashlightConfused)
        {
            bool shouldWalk = agent.velocity.magnitude > 0.1f && agent.remainingDistance > agent.stoppingDistance;
            animator.SetBool("isWalking", shouldWalk);
        }
    }

    private bool IsPlayerFlashlightShiningOnEnemy()
    {
        if (flashlightController == null || !flashlightController.IsFlashlightOn)
            return false;

        Vector3 directionToEnemy = transform.position - player.position;
        float distance = directionToEnemy.magnitude;
        float angle = Vector3.Angle(player.forward, directionToEnemy.normalized);

        // Verifica si está dentro del ángulo y distancia de detección
        if (distance <= flashlightDetectionDistance && angle <= flashlightDetectionAngle)
        {
            // Raycast para verificar si hay obstáculos
            if (!Physics.Raycast(player.position, directionToEnemy.normalized, distance, obstacleLayer))
            {
                Debug.DrawLine(player.position, transform.position, Color.yellow, 0.1f);
                return true;
            }
        }
        return false;
    }

    private IEnumerator FlashlightConfusionRoutine()
    {
        isFlashlightConfused = true;
        lastFlashlightConfusionTime = Time.time;
        enemyAnimator.SetBool(confusedAnimationParam, true);
        agent.isStopped = true;

        // AudioManager.Instance.PlaySFX("enemy_flashlight_confused");

        yield return new WaitForSeconds(flashlightConfusionDuration);

        enemyAnimator.SetBool(confusedAnimationParam, false);
        isFlashlightConfused = false;

        ForceReturnToPatrol();
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
        StopAllCoroutines();
        isChasing = false;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        GoToRandomWaypoint();
    }

    private IEnumerator PlayConfusionAndReturnToPatrol()
    {
        isConfused = true;
        enemyAnimator.SetBool(confusedAnimationParam, true);
        agent.isStopped = true;

        // AudioManager.Instance.PlaySFX("enemy_confused");

        yield return new WaitForSeconds(confusionDuration);

        enemyAnimator.SetBool(confusedAnimationParam, false);
        isConfused = false;

        ForceReturnToPatrol();
    }

    private void ChasePlayer()
    {
        if (isConfused || isFlashlightConfused) return;

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

        // Movimiento y detención
        if (distanceToPlayer > stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
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
        if (isConfused || isFlashlightConfused) return;
        animator.SetBool("IsAttacking", true);
    }

    private void Patrol()
    {
        if (isConfused || isFlashlightConfused) return;

        agent.speed = patrolSpeed;
        agent.isStopped = false;

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
        GameObject playerObj = null;
        while (playerObj == null)
        {
            playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                // Asegurarnos de tener la referencia al flashlightController
                if (flashlightController == null)
                {
                    flashlightController = player.GetComponentInChildren<FlashlightController>(true);
                }
            }
            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, detectionRange);

        // Zona de frenado
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, brakingDistance);

        // Stopping distance
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, stoppingDistance);

        // Flashlight detection
        Gizmos.color = new Color(0, 0.5f, 1, 0.2f);
        Gizmos.DrawSphere(transform.position, flashlightDetectionDistance);
    }
    //Metodo para sonidos por eventos de animacion

    public void AttackEnemySound()
    {
        AudioManager.Instance.PlayFX("AttackEnemy");
    }
}