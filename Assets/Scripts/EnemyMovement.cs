using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public float moveRadius = 10f; // Radio de movimiento dentro de la zona
    public float waitTime = 3f; // Tiempo de espera entre movimientos

    private NavMeshAgent agent;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Obtener el componente NavMeshAgent
        timer = waitTime; // Iniciar el temporizador
        MoveToNewPosition(); // Mover al enemigo a una nueva posición inicial
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            timer -= Time.deltaTime; // Reducir el tiempo de espera
            if (timer <= 0f)
            {
                MoveToNewPosition(); // Elegir nueva posición
                timer = waitTime; // Reiniciar el temporizador
            }
        }
    }

    void MoveToNewPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius; 
        randomDirection += transform.position; // Mover el punto aleatorio alrededor del enemigo

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, moveRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position); // Enviar al enemigo a la nueva posición
        }
    }
}
