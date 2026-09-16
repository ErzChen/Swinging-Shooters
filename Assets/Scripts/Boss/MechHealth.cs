using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MechHealth : MonoBehaviour
{
    public GameObject portal;
    public Image healthBar;
    public float currentHealth;
    public float maxHealth = 5000f;
    public bool isDestroyed;
    public bool canTakeDamage = false;
    
    private List<GameObject> explosions = new List<GameObject>();
    private MechMovement mechMovement;
    private MechShoot mechShoot;
    private MechFollow mechFollow;
    private GameObject BossCanvas;
    private Animator animator;
    private AudioSource enemySource;

    void Start()
    {
        currentHealth = maxHealth;
        BossCanvas = GameObject.Find("BossCanvas");
        foreach (Transform child in transform) 
        {
            if (child.name.Contains("Explosion"))
            {
                explosions.Add(child.gameObject);
            }
        }
        mechMovement = GetComponent<MechMovement>();
        mechShoot = GetComponent<MechShoot>();
        mechFollow = GetComponent<MechFollow>();
        enemySource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (!canTakeDamage || isDestroyed) 
        {
            return;
        }

        currentHealth -= damage;
        enemySource.Play();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDestroyed = true;
            BossCanvas.SetActive(false);
            portal.SetActive(true);
            if (mechMovement.currentState == MechMovement.MoveState.Sleep)
            {
                animator.SetTrigger("Wake");
            }
            mechMovement.StopAllCoroutines();
            mechMovement.enabled = false;
            mechShoot.enabled = false;
            mechFollow.enabled = false;
            StartCoroutine(ExplodeSequence());
            return;
        }

        UpdateHealthUI();
        UpdateMechPhases();
    }

    private IEnumerator ExplodeSequence()
    {
        foreach (GameObject explosion in explosions)
        {
            ParticleSystem explosionParts = explosion.GetComponentInChildren<ParticleSystem>();
            explosionParts.Play();
            AudioSource explosionSource = explosion.GetComponent<AudioSource>();
            explosionSource.PlayOneShot(explosionSource.clip);
            yield return new WaitForSeconds(0.05f);
        }
        
        animator.ResetTrigger("Wake");
        animator.SetTrigger("Sleep");
    }

    private void UpdateHealthUI()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    private void UpdateMechPhases()
    {
        if (currentHealth < maxHealth * 0.40)
        {
            mechMovement.idealDistance = 150f;
        }
        else if (currentHealth < maxHealth * 0.70)
        {
            mechMovement.idealDistance = 125f;
        }
    }
}