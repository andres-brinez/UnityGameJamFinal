using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("Configuración")]
    public int maxHealth = 100;
    public int currentHealth;
    public Animator animator;

    [Header("UI")]
    public Image healthBarFill;

    [Header("Objetos para apagar")]
    public GameObject[] particlesBoss;


    public bool IsDead { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();

        if (currentHealth <= 0) Die();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        IsDead = true;
        if (healthBarFill != null)
            healthBarFill.transform.parent.gameObject.SetActive(false);

        animator.SetTrigger("Die");
        foreach (GameObject obj in particlesBoss)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Destruir todos los enemigos invocados
        if (TryGetComponent<BossController>(out var bossController))
        {
            bossController.DestroyAllSummonedEnemies();
        }
    }
}