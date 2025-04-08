using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public GameObject shadowProjectile; // Ataque 1
    public GameObject fireballProjectile; // Ataque 2
    public GameObject enemyPrefab; // Enemigo a invocar
    public Transform projectileSpawnPoint;

    [Header("Configuración")]
    public float moveSpeed = 3.5f;
    public float minDistance = 5f; // Distancia mínima para atacar
    public float maxDistance = 10f; // Distancia máxima para acercarse
    public float rotationSpeed = 10f;

    [Header("Ataques")]
    public float attack1Cooldown = 2f;
    public float attack2Cooldown = 1.5f;
    public float attack3Cooldown = 3f;

    [Header("Ataque 3 - Invocación")]
    public List<Transform> spawnPoints; // Puntos específicos de aparición
    public int enemiesPerSpawnPoint = 1; // Enemigos por punto
    public float enemyLifetime = 15f;
    public float summonDuration = 2f;
    private List<GameObject> summonedEnemies = new List<GameObject>(); // Lista de enemigo
    // Componentes
    private Animator animator;
    private NavMeshAgent agent;
    private HealthSystem health;
    private Transform cameraTransform;
    private bool isAttacking = false;
    private int currentAttackPhase = 1;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthSystem>();
        cameraTransform = Camera.main.transform;

        // Configuración inicial segura
        if (agent.isOnNavMesh)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = minDistance;
            agent.angularSpeed = 0;
        }
        else
        {
            Debug.LogWarning("Boss no está en NavMesh! Revisa la configuración.");
            agent.enabled = false;
        }
    }

    void Update()
    {
        if (health.IsDead || player == null || isAttacking) return;

        UpdateHealthPhase();
        FacePlayer();
        HandleMovement();
    }

    void UpdateHealthPhase()
    {
        float healthPercent = (float)health.currentHealth / health.maxHealth;

        if (healthPercent <= 0.4f) currentAttackPhase = 3;
        else if (healthPercent <= 0.65f) currentAttackPhase = 2;
        else currentAttackPhase = 1;
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void HandleMovement()
    {
        if (!agent.isOnNavMesh || !agent.enabled) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > maxDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("IsWalking", true);
        }
        else if (distance < minDistance)
        {
            Vector3 dirAway = transform.position - player.position;
            Vector3 newPos = transform.position + dirAway.normalized;
            agent.isStopped = false;
            agent.SetDestination(newPos);
            animator.SetBool("IsWalking", true);
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("IsWalking", false);
            StartCoroutine(StartAttack());
        }
    }

    IEnumerator StartAttack()
    {
        isAttacking = true;

        switch (currentAttackPhase)
        {
            case 1:
                animator.SetTrigger("Attack1");
                yield return new WaitForSeconds(attack1Cooldown);
                break;

            case 2:
                animator.SetTrigger("Attack2");
                yield return new WaitForSeconds(attack2Cooldown);
                break;

            case 3:
                // Animación de invocación
                animator.SetTrigger("Summon");
                yield return new WaitForSeconds(summonDuration);

                // Invocar enemigos
                SummonEnemies();

                // Tiempo de recuperación
                yield return new WaitForSeconds(attack3Cooldown - summonDuration);
                break;
        }

        isAttacking = false;
    }

    void SummonEnemies()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No hay puntos de spawn asignados!");
            return;
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            for (int i = 0; i < enemiesPerSpawnPoint; i++)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                summonedEnemies.Add(enemy); // Añadir a la lista

                Destroy(enemy, enemyLifetime);
            }
        }
    }
    public void DestroyAllSummonedEnemies()
    {
        foreach (GameObject enemy in summonedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        summonedEnemies.Clear(); // Limpiar la lista
    }

    // ===== EVENTOS DE ANIMACIÓN ===== //
    public void LaunchShadowProjectile()
    {
        Instantiate(shadowProjectile, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
    }

    public void LaunchFireball()
    {
        GameObject fireball = Instantiate(fireballProjectile, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        fireball.GetComponent<Rigidbody>().AddForce(projectileSpawnPoint.forward * 15f, ForceMode.Impulse);
    }
}