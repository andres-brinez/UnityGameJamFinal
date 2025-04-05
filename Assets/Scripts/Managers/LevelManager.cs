using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Configuración")]
    public GameObject portalPrefab;
    public Transform portalSpawnPoint;
    public Transform destinationSpawnPoint;
    public GameObject[] objectsToActivateOnEnter;
    public GameObject[] objectsToSpawnOnEnter;
    public GameObject[] objectsToDeactivateOnEnter;

    [Header("UI")]
    public TextMeshProUGUI potionCounterText;

    private int totalPotions;
    private int collectedPotions = 0;
    private GameObject currentPortal;
    private GameObject player;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
        // Contar todas las pociones en la escena
        totalPotions = GameObject.FindGameObjectsWithTag("Potion").Length;
        UpdatePotionUI();
    }

    public void CollectPotion()
    {
        collectedPotions++;
        UpdatePotionUI();

        if (collectedPotions >= totalPotions)
        {
            SpawnPortal();
        }
    }
    void UpdatePotionUI()
    {
        if (potionCounterText != null)
        {
            int remaining = totalPotions - collectedPotions;
            potionCounterText.text = $"Pociones restantes: {remaining}";

            // Opcional: Cambiar color cuando quedan pocas
            if (remaining <= 3)
            {
                potionCounterText.color = Color.yellow;
            }
            if (remaining == 0)
            {
                potionCounterText.color = Color.green;
            }
        }
    }
    void SpawnPortal()
    {
        if (currentPortal == null && portalPrefab != null && portalSpawnPoint != null)
        {
            currentPortal = Instantiate(portalPrefab, portalSpawnPoint.position, portalSpawnPoint.rotation);
            potionCounterText.text = "¡Portal activado!";
        }
    }

    public void EnterPortal()
    {
        if (player != null)
        {
            // Desactivar player
            player.SetActive(false);

            // Mover player a la nueva posición
            player.transform.position = destinationSpawnPoint.position;
            player.transform.rotation = destinationSpawnPoint.rotation;

            // Activar player de nuevo
            player.SetActive(true);

            // Manejar objetos a activar/desactivar/spawnear
            HandleObjects();

            // Destruir el portal
            if (currentPortal != null)
            {
                Destroy(currentPortal);
            }
        }
    }

    void HandleObjects()
    {
        // Activar objetos
        foreach (GameObject obj in objectsToActivateOnEnter)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Desactivar objetos
        foreach (GameObject obj in objectsToDeactivateOnEnter)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Instanciar nuevos objetos
        foreach (GameObject obj in objectsToSpawnOnEnter)
        {
            if (obj != null) Instantiate(obj, obj.transform.position, obj.transform.rotation);
        }
    }
}