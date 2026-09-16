using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    public static Health PlayerInstance;

    [Header("Health")]
    public float currentHealth;

    [Header("UI References")]
    public Image HealthBar;
    public TMP_Text HealthText;
    public GameObject DeathBackgroundObj;
    public Image DeathBackground;
    public GameObject DeathScreenText;

    [Header("Settings")]
    public float FadeSpeed = 20.0f;
    public float HealthBarTransitionSpeed = 5.0f;

    [Header("Gameover")]
    public bool isGameOver;

    [Header("Sound")]
    public AudioSource healthSource;
    public AudioClip hitClip;

    private const float MaxHealth = 100f;
    private float displayedHealth;

    private void Start()
    {
        PlayerInstance = this;
        currentHealth = MaxHealth;
        displayedHealth = MaxHealth;
        HealthBar = GameObject.Find("PlayerHealth").GetComponent<Image>();
        HealthText = GameObject.Find("PlayerHealthText").GetComponent<TMP_Text>();
        DeathBackgroundObj = GameObject.Find("Death_bg");
        DeathBackground = DeathBackgroundObj.GetComponent<Image>();
        DeathScreenText = GameObject.Find("Death Screen").gameObject.transform.Find("Death_text").gameObject;
        DeathBackgroundObj.SetActive(false);
        DeathScreenText.SetActive(false);
    }

    private void Update()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, MaxHealth);
        HealthText.text = Mathf.CeilToInt(currentHealth).ToString();
        displayedHealth = Mathf.Lerp(displayedHealth, currentHealth, Time.deltaTime * HealthBarTransitionSpeed);
        HealthBar.fillAmount = displayedHealth / MaxHealth;

        if (currentHealth == 0f && !isGameOver)
        {
            TriggerDeath();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Void"))
        {
            currentHealth = 0f;
        }
        else if (other.CompareTag("Health"))
        {
            currentHealth += 25;
            healthSource.PlayOneShot(healthSource.clip);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthSource.PlayOneShot(hitClip);
    }

    private void TriggerDeath()
    {
        isGameOver = true;
        DeathBackgroundObj.SetActive(true);
        StartCoroutine(FadeImage(DeathBackground, 0f, 0.9f, FadeSpeed));
        FreezeRigidbody();
        gameObject.tag = "Untagged";
    }

    private void FreezeRigidbody()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = image.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;

        if (DeathScreenText != null)
        {
            DeathScreenText.SetActive(true);
        }
    }
}