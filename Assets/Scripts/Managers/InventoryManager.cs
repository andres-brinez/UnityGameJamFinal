using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public delegate void InventoryChangedDelegate();
    public event InventoryChangedDelegate OnInventoryChanged;

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

    public void AddPowerUp(string powerUpName, int quantity = 1)  // Cambiado para aceptar cantidad
    {
        if (string.IsNullOrEmpty(powerUpName))
        {
            Debug.LogWarning("Intentaste agregar un powerup con nombre vacío.");
            return;
        }

        if (inventory.ContainsKey(powerUpName))
        {
            // Si el powerup ya está en el inventario, aumenta su contador
            inventory[powerUpName] += quantity;  // Suma la cantidad especificada
        }
        else
        {
            // Si no esta, se agrega al inventario con la cantidad especificada
            inventory.Add(powerUpName, quantity);
        }

        UpdateTotalPowerUps();
        OnInventoryChanged?.Invoke();

        Debug.Log($"{quantity} {powerUpName}(s) agregado(s) al inventario. Total: {inventory[powerUpName]}");
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
                inventory.Remove(powerUpName);
            }

            UpdateTotalPowerUps();
            OnInventoryChanged?.Invoke(); // Esto notificará al InventoryUI
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
