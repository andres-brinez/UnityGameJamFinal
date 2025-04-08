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

    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Contar solo las pociones en la escena objetos con tag "Potion"
        GameObject[] potions = GameObject.FindGameObjectsWithTag("Potion");
        totalPotions = potions.Length;
        UpdatePotionUI();

        // Verificación de debug
        Debug.Log("Total de pociones encontradas: " + totalPotions);
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

            // Cambiar color cuando quedan pocas
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
            player.SetActive(false);

            // Mover player a la nueva posicion en la que estara contra el jefe
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