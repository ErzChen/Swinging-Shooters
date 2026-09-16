using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    public float healthDropChance = 20f;
    public float currentHealth;
    public float maxHealth;
    public bool isDestroyed;
    public Image healthBar;
    public TMP_Text healthText;
    public GameObject turretActive;
    public GameObject turretDestroyed;
    public GameObject healthDrop;
    public EnemyCheck enemyCheck;
    public AudioSource enemySource;

    void Start()
    {
        enemyCheck = GetComponentInParent<EnemyCheck>();
        enemySource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        enemySource.Play();

        if (currentHealth <= 0 && !isDestroyed)
        {
            currentHealth = 0;
            isDestroyed = true;
            turretActive.SetActive(false);
            turretDestroyed.SetActive(true);
            if (Random.Range(0, 100) < healthDropChance)
            {
                Instantiate(healthDrop, transform.position, Quaternion.identity);
            }
            Invoke(nameof(DestroyEnemy), 5f);
        }

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        healthText.text = currentHealth.ToString();
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    public void DestroyEnemy()
    {
        enemyCheck.ReportEnemyDefeated();
        Destroy(gameObject);
    }
}