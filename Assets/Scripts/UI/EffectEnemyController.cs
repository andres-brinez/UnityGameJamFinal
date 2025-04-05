using UnityEngine;
using UnityEngine.UI;

public class EffectEnemyController : MonoBehaviour
{
    [Header("Referencias UI")]
    private Transform player;       
    public Image fearImage;       
    public Image fearFillBar;   

    [Header("Ajustes de Distancia")]
    public float maxDistance = 10f; 
    public float minDistance = 2f;   

    private GameObject[] enemies;

    void Start()
    {
        player = PlayerController.PlayerTransform; 
    }
    void Update()
    {
        // Busca todos los objetos con tag "Enemy" (ajusta el tag según tu juego)
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Si no hay enemigos, resetea los efectos
        if (enemies.Length == 0)
        {
            ResetFearEffect();
            return;
        }

        // Encuentra el enemigo más cercano
        Transform closestEnemy = GetClosestEnemy();

        if (closestEnemy != null)
        {
            // Calcula la distancia entre jugador y enemigo
            float distance = Vector3.Distance(player.position, closestEnemy.position);

            // Normaliza el valor entre 0 y 1 (invertido: entre mas cerca = mayor valor)
            float fearIntensity = CalculateFearIntensity(distance);

            UpdateFearEffect(fearIntensity);
        }
    }

    private void ResetFearEffect()
    {
        // Restablece la transparencia y la barra
        fearImage.color = new Color(fearImage.color.r, fearImage.color.g, fearImage.color.b, 0);
        fearFillBar.fillAmount = 0;
    }

    private float CalculateFearIntensity(float distance)
    {
        // Fórmula para convertir distancia en intensidad (0-1)
        return 1 - Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
    }

    private void UpdateFearEffect(float intensity)
    {
        // Actualiza el alpha de la imagen
        Color imgColor = fearImage.color;
        imgColor.a = intensity;
        fearImage.color = imgColor;

        // Actualiza la barra de fill
        fearFillBar.fillAmount = intensity;
    }

    private Transform GetClosestEnemy()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float currentDistance = Vector3.Distance(player.position, enemy.transform.position);
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closest = enemy.transform;
            }
        }

        return closest;
    }
}