using UnityEngine;
using System.Collections;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class RecollectObject : MonoBehaviour
{
    [Header("Configuraci�n B�sica")]
    public string itemName;
    public GameObject pickupText;
    public string collectAnimationName = "Pickup";

    [Header("Cantidad")]
    public int quantity = 1;

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

        if (pickupText != null)
        {
            pickupText.SetActive(false);
        }

        targetController.enabled = false;

        yield return new WaitForSeconds(animationDelay);

        if (targetAnimator != null)
        {
            targetAnimator.Play(collectAnimationName);
            yield return new WaitForSeconds(GetAnimationLength(targetAnimator, collectAnimationName));
        }

        yield return new WaitForSeconds(postAnimationDelay);

        // Solo llama a CollectPotion() si el objeto es una poci�n
        if (gameObject.CompareTag("Potion"))
        {
            LevelManager.instance.CollectPotion();
        }

        // A�ade el objeto al inventario 
        InventoryManager.Instance.AddPowerUp(itemName, quantity);

        targetController.enabled = true;
        gameObject.SetActive(false);

        isCollecting = false;
    }

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