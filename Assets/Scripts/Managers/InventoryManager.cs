using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    //Diccionario para llevar la cantidad de cada tipo de powerup
    [SerializeField]  private Dictionary<string, int> inventory = new Dictionary<string, int>();

    [Header("PowerUps en el Inventario")]
    [SerializeField] private int totalPowerUps = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPowerUp(string powerUpName)
    {
        if (string.IsNullOrEmpty(powerUpName))
        {
            Debug.LogWarning("Intentaste agregar un powerup con nombre vacío.");
            return;
        }

        if (inventory.ContainsKey(powerUpName))
        {
            // Si el powerup ya está en el inventario, solo aumenta su contador
            inventory[powerUpName]++;
        }
        else
        {
            // Si no está, se agrega al inventario con un contador inicial de 1
            inventory.Add(powerUpName, 1);

        }

        UpdateTotalPowerUps();

        Debug.Log(powerUpName + " agregado al inventario. Cantidad: " + inventory[powerUpName]);
    }

    public int GetPowerUpCount(string powerUpName)
    {

        if (inventory.ContainsKey(powerUpName))
        {
            return inventory[powerUpName];
        }

        return 0; // Si no hay el powerup en el inventario
    }

    public void RemovePowerUp(string powerUpName)
    {
        if (inventory.ContainsKey(powerUpName))
        {
            inventory[powerUpName]--;
            if (inventory[powerUpName] <= 0)
            {
                inventory.Remove(powerUpName); // se elimina si la cantidad llega a 0
            }

            UpdateTotalPowerUps();
        }
    }

    // actualiza el total de powerups en el inventario
    private void UpdateTotalPowerUps()
    {
        totalPowerUps = 0;
        foreach (var powerUp in inventory)
        {
            totalPowerUps += powerUp.Value;
        }
    }
}
