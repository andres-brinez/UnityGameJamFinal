using UnityEngine;
using System.Collections;

public class CollectableItem : MonoBehaviour
{
    [Header("Configuraci�n B�sica")]
    public string itemName;
    public GameObject pickupText;
    public string collectAnimationName = "Pickup";

    [Header("Tiempos")]
    public float animationDelay = 0.5f; // Tiempo antes de empezar animaci�n
    public float postAnimationDelay = 0.5f; // Tiempo despu�s de animaci�n

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
        // Si est� en rango, se presiona E y no se est� recolectando
        if (isInRange && Input.GetKeyDown(KeyCode.E) && !isCollecting)
        {
            // Buscar al jugador m�s cercano dentro del trigger
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
                        break; // Solo interact�a con el primer jugador encontrado
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

        // 1. Desactivar el texto de recolecci�n
        if (pickupText != null)
        {
            pickupText.SetActive(false);
        }

        // 2. Desactivar el movimiento del jugador
        targetController.enabled = false;

        // Peque�a pausa antes de la animaci�n (opcional)
        yield return new WaitForSeconds(animationDelay);

        // 3. Reproducir la animaci�n de recolecci�n
        if (targetAnimator != null)
        {
            targetAnimator.Play(collectAnimationName);

            // Esperar a que la animaci�n termine
            yield return new WaitForSeconds(GetAnimationLength(targetAnimator, collectAnimationName));
        }

        // Peque�a pausa despu�s de la animaci�n (opcional)
        yield return new WaitForSeconds(postAnimationDelay);

        // 4. (OPCIONAL) A�adir el objeto al inventario
        //InventoryManager.instance.AddItem(itemName);

        // 5. Reactivar el movimiento del jugador y desactivar el objeto
        targetController.enabled = true;
        gameObject.SetActive(false);

        isCollecting = false;
    }

    // M�todo para obtener la duraci�n de una animaci�n
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
        return 0f; // Valor por defecto si no encuentra la animaci�n
    }
}