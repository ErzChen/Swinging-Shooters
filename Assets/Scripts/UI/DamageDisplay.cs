using UnityEngine;
using TMPro;

public class DamageDisplay : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3(3f, 3f, 0f);
    public GameObject enemy;

    public GameObject damageTextPrefab;

    private Canvas canvas;
    private DamageTextFollow follow;
    private Camera cam;
    private GameObject activeInstance;
    private GameObject currentEnemy;
    private TextMeshProUGUI activeTMP;
    private int accumulatedDamage = 0;

    void Start()
    {
        cam = Camera.main;
        canvas = GameObject.Find("DamageDisplay").GetComponent<Canvas>();
    }

    public void ShowDamage(int amount, bool crit = false)
    {
        Color textColor = crit ? new Color(1f, 1f, 0f, 1f) : new Color(1f, 1f, 1f, 1f);

        if (activeInstance != null && currentEnemy == enemy)
        {
            accumulatedDamage += amount;

            if (activeTMP != null)
            {
                activeTMP.text = accumulatedDamage.ToString();
                activeTMP.color = textColor; 
            }

            Animator existingAnimator = activeInstance.GetComponent<Animator>();
            if (existingAnimator != null)
            {
                existingAnimator.ResetTrigger("Play");
                existingAnimator.SetTrigger("Play");
            }

            CancelInvoke("ClearInstance"); 
            Invoke("ClearInstance", 2f);
            return;
        }

        CancelInvoke("ClearInstance");
        ClearInstance();

        accumulatedDamage = amount;
        currentEnemy = enemy;

        if (enemy == null) 
        {
            return; 
        }

        Transform enemyTransform = enemy.transform;
        Vector3 screenPos = cam.WorldToScreenPoint(enemyTransform.position + cameraOffset);

        activeInstance = Instantiate(damageTextPrefab, canvas.transform);
        activeInstance.GetComponent<RectTransform>().position = screenPos;

        follow = activeInstance.GetComponent<DamageTextFollow>();
        if (follow != null)
        {
            follow.target = enemyTransform;
            follow.cameraOffset = cameraOffset;
        }

        activeTMP = activeInstance.GetComponent<TextMeshProUGUI>();
        if (activeTMP != null)
        {
            activeTMP.text = accumulatedDamage.ToString();
            activeTMP.color = textColor;
        }

        Animator animator = activeInstance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Play");
        }

        Invoke("ClearInstance", 2f);
    }

    private void ClearInstance()
    {
        if (activeInstance != null)
        {
            Destroy(activeInstance);
            activeInstance = null;
            activeTMP = null;
        }
        currentEnemy = null;
        accumulatedDamage = 0;
    }
}