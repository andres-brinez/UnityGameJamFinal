using UnityEngine;
using System.Collections;

public class CollectableItem : MonoBehaviour
{
    [Header("Configuraci�n B�sica")]
    public string itemName;
    public GameObject pickupText;
    public Animator playerAnimator;
    public string collectAnimationName = "Pickup";

    [Header("Tiempos")]
    public float animationDelay = 0.5f; // Tiempo antes de empezar animaci�n
    public float postAnimationDelay = 0.5f; // Tiempo despues de animaci�n

    public PlayerController playerController;

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
        //Al presionar la E hara la animacion de recolectar y apaga el script del playerController para evitar movimientos mientras recolectamos
        if (isInRange && Input.GetKeyDown(KeyCode.E) && !isCollecting)
        {
            playerController.enabled = false;
            StartCoroutine(CollectItemRoutine());
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

    private IEnumerator CollectItemRoutine()
    {
        isCollecting = true;

        // 1. Desactivar el texto de recolecci�n
        if (pickupText != null)
        {
            pickupText.SetActive(false);
        }

        // Peque�a pausa antes de la animaci�n (opcional)
        yield return new WaitForSeconds(animationDelay);

        // 2. Reproducir la animaci�n de recoleccion
        if (playerAnimator != null)
        {
            playerAnimator.Play(collectAnimationName);

            // Esperar a que la animaci�n termine
            yield return new WaitForSeconds(GetAnimationLength(playerAnimator, collectAnimationName));
        }

        // Peque�a pausa despu�s de la animaci�n (opcional)
        yield return new WaitForSeconds(postAnimationDelay);

        // 3. A�adir el objeto al inventario en el futuro
        //InventoryManager.instance.AddItem(itemName);

        // 4. Desactivamos el objeto recolectable y el scrip se vuelve activar para seguir moviendose
        playerController.enabled = true;
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