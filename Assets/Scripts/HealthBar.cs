using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{
    public Image healthBar;  //imagen de la barra de vida
    public float maxHealth = 100f;
    private float currentHealth;
    public Animator barAnimator;
    //public GameObject icons;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        UpdateHealthBar();
    }

    public void Die()
    {
        //SceneManager.LoadScene("GameOver");
        Debug.Log("Player muerto");
        //GameManager.GameInstance.GameOverLose();
        //icons.SetActive(false);
        //AudioManager.Instance.musicSource.Stop();
        //AudioManager.Instance.PlaySFX("LoseSound");
        // pantalla de Game Over
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DamageObject"))
        {
            barAnimator.SetBool("DamageUI", true);
            //AudioManager.Instance.PlaySFX("HitSound");
            TakeDamage(10f); // Reduce 20 de vida 
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
