using UnityEngine;
using System.Collections;

public class CollectableItem : MonoBehaviour
{
    [Header("Configuración Básica")]
    public string itemName;
    public GameObject pickupText;
    public string collectAnimationName = "Pickup";

    [Header("Tiempos")]
    public float animationDelay = 0.5f; // Tiempo antes de empezar animación
    public float postAnimationDelay = 0.5f; // Tiempo después de animación

    private bool isInRange = false;
    private bool isCollecting = false;

    private void Start()
    {
        if (pickupText != null)
        {
            pickupText.SetActive(false);
        }
    }

    private void Update()
    {
        // Si está en rango, se presiona E y no se está recolectando
        if (isInRange && Input.GetKeyDown(KeyCode.E) && !isCollecting)
        {
            // Buscar al jugador más cercano dentro del trigger
            Collider[] nearbyPlayers = Physics.OverlapSphere(transform.position, 2f);
            foreach (var playerCollider in nearbyPlayers)
            {
                if (playerCollider.CompareTag("Player"))
                {
                    PlayerController playerController = playerCollider.GetComponent<PlayerController>();
                    Animator playerAnimator = playerCollider.GetComponent<Animator>();

                    if (playerController != null && playerAnimator != null)
                    {
                        StartCoroutine(CollectItemRoutine(playerAnimator, playerController));
                        break; // Solo interactúa con el primer jugador encontrado
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollecting)
        {
            isInRange = true;
            if (pickupText != null)
            {
                pickupText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            if (pickupText != null && !isCollecting)
            {
                pickupText.SetActive(false);
            }
        }
    }

    private IEnumerator CollectItemRoutine(Animator targetAnimator, PlayerController targetController)
    {
        isCollecting = true;

        // 1. Desactivar el texto de recolección
        if (pickupText != null)
        {
            pickupText.SetActive(false);
        }

        // 2. Desactivar el movimiento del jugador
        targetController.enabled = false;

        // Pequeña pausa antes de la animación (opcional)
        yield return new WaitForSeconds(animationDelay);

        // 3. Reproducir la animación de recolección
        if (targetAnimator != null)
        {
            targetAnimator.Play(collectAnimationName);

            // Esperar a que la animación termine
            yield return new WaitForSeconds(GetAnimationLength(targetAnimator, collectAnimationName));
        }

        // Pequeña pausa después de la animación (opcional)
        yield return new WaitForSeconds(postAnimationDelay);

        // 4. (OPCIONAL) Añadir el objeto al inventario
        //InventoryManager.instance.AddItem(itemName);

        // 5. Reactivar el movimiento del jugador y desactivar el objeto
        targetController.enabled = true;
        gameObject.SetActive(false);

        isCollecting = false;
    }

    // Método para obtener la duración de una animación
    private float GetAnimationLength(Animator animator, string animationName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 0f; // Valor por defecto si no encuentra la animación
    }
}