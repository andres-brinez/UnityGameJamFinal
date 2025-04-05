using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    public Image healthBar;
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Damage Effect")]
    public Image damageEffect;
    [Range(0f, 1f)] public float maxAlpha = 0.7f;
    private float targetAlpha = 0f;
    public float fadeSpeed = 5f;

    [Header("Regeneration")]
    public float regenerationDelay = 5f;
    public float regenerationRate = 10f;
    private float timeSinceLastDamage = 0f;
    private bool isRegenerating = false;

    [Header("References")]
    public Animator barAnimator;

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
        if (isDead) return; 

        // Actualizar efecto de daño
        UpdateDamageEffect();

        // Regeneración
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
        if (isDead) return; // No recibir daño si ya murio

        currentHealth -= damage;
        CameraController.Instance.MoverCam(3.5f, 3f, 0.8f); // Efewcto vibracion cada que se recibe daño
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
        Debug.Log("Player muerto - La regeneración está desactivada");
        // Aqui Game Over
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DamageObject") && !isDead)
        {
            barAnimator.SetBool("DamageUI", true);
            TakeDamage(10f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DamageObject"))
        {
            barAnimator.SetBool("DamageUI", false);
        }
    }

}