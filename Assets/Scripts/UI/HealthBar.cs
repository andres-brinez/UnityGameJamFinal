using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    public Image healthBar;
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [SerializeField] private bool isDead = false;

    [Header("Damage Effect")]
    public Image damageEffect;
    [Range(0f, 1f)] public float maxAlpha = 0.7f;
    private float targetAlpha = 0f;
    public float fadeSpeed = 5f;

    [Header("Regeneration")]
    public float regenerationDelay = 5f;
    public float regenerationRate = 10f;
    private float timeSinceLastDamage = 0f;
    public bool isRegenerating = false;

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthBar();

        if (damageEffect != null)
        {
            Color tempColor = damageEffect.color;
            tempColor.a = 0f;
            damageEffect.color = tempColor;
        }
    }

    void Update()
    {
        if (currentHealth <= 0) {
            isDead = true;
            
        } 
        if (isDead){
            Die();
        }

        // Actualizar efecto de da�o
        UpdateDamageEffect();

        // Regeneraci�n
        HandleHealthRegeneration();
    }

    void UpdateDamageEffect()
    {
        if (damageEffect != null)
        {
            Color currentColor = damageEffect.color;
            if (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
            {
                currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
                damageEffect.color = currentColor;
            }
        }
    }

    void HandleHealthRegeneration()
    {
        if (currentHealth < maxHealth && !isDead)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenerationDelay)
            {
                isRegenerating = true;
                RegenerateHealth();
            }
            else
            {
                isRegenerating = false;
            }
        }
        else
        {
            isRegenerating = false;
            timeSinceLastDamage = 0f;
        }
    }

    void RegenerateHealth()
    {
        float regenerationAmount = regenerationRate * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth + regenerationAmount, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        healthBar.fillAmount = currentHealth / maxHealth;

        if (damageEffect != null)
        {
            float healthLost = 1 - (currentHealth / maxHealth);
            targetAlpha = healthLost * maxAlpha;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // No recibir da�o si ya murio

        currentHealth -= damage;
        CameraController.Instance.MoverCam(3.5f, 3f, 0.8f); // Efewcto vibracion cada que se recibe da�o
        timeSinceLastDamage = 0f;
        isRegenerating = false;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            if (damageEffect != null)
            {
                targetAlpha = maxAlpha;
                Color tempColor = damageEffect.color;
                tempColor.a = maxAlpha;
                damageEffect.color = tempColor;
            }
            Die();
        }
        UpdateHealthBar();
    }

    public void Die()
    {
        // Aqui Game Over
        GameManager.Instance.GameOver();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DamageObject") && !isDead)
        {
            TakeDamage(10f);
        }
    }

}