using UnityEngine;

public class EnemyCheck : MonoBehaviour
{
    [Header("References")]
    public EnemiesLeft enemiesLeft;
    public SceneLoader loader;
    public GameManager gameManager;
    public GameObject portal;

    [Header("Group Settings")]
    public GameObject subGroupManager;
    public bool isSubGroup = false;

    [Header("Enemies")]
    public GameObject[] enemies;
    public bool outlineLastEnemies;
    public int lastAmount = 0;
    public int enemiesDefeated;
    public int totalEnemies;
    public int totalEnemiesDefeated;

    [Header("Level")]
    public int level;
    public bool levelComplete = false;
    public bool nearlyCleared = false;

    private EnemyCheck rootChecker;

    void Start()
    {
        int childCount = transform.childCount;
        enemies = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            enemies[i] = transform.GetChild(i).gameObject;
        }

        rootChecker = GetRootChecker();

        if (!isSubGroup)
        {
            totalEnemies = CountAllEnemies(transform);

            if (enemiesLeft != null)
            {
                enemiesLeft.enemyCount = totalEnemies;
                enemiesLeft.UpdateDisplay();
            }
        }
    }

    public void RegisterSubGroup(EnemyCheck subGroup)
    {
        int newEnemies = CountAllEnemies(subGroup.transform);
        totalEnemies += newEnemies;

        if (enemiesLeft != null)
        {
            enemiesLeft.enemyCount += newEnemies;
            enemiesLeft.UpdateDisplay();
        }
    }

    public void ReportEnemyDefeated()
    {
        enemiesDefeated++;
        nearlyCleared = enemies.Length == enemiesDefeated + 1;
        rootChecker.totalEnemiesDefeated++;

        if (rootChecker.enemiesLeft != null)
        {
            rootChecker.enemiesLeft.LoseEnemy();
        }

        if (outlineLastEnemies && lastAmount > 0)
        {
            int remaining = enemies.Length - enemiesDefeated;
            if (remaining <= lastAmount)
            {
                ApplyOutlineToRemaining();
            }
        }

        if (isSubGroup && enemies.Length <= enemiesDefeated)
        {
            EnemyCheck parentGroup = transform.parent?.GetComponent<EnemyCheck>();
            if (parentGroup != null)
            {
                parentGroup.enemiesDefeated += 1;
            }

            if (portal != null)
            {
                portal.SetActive(true);
            }
            Destroy(subGroupManager);
            Destroy(gameObject);
        }
    }

    private void ApplyOutlineToRemaining()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) 
            {
                continue;
            }

            foreach (Transform child in enemy.GetComponentsInChildren<Transform>(false))
            {
                if (!child.CompareTag("Enemy")) 
                {
                    continue;
                }

                Outline outline = child.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = child.gameObject.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineColor = Color.red;
                    outline.OutlineWidth = 5f;
                }
            }
        }
    }

    void Update()
    {
        if (!isSubGroup && !levelComplete)
        {
            if (totalEnemies > 0 && totalEnemies <= totalEnemiesDefeated)
            {
                if (portal != null)
                {
                    portal.SetActive(true);
                }
                loader.isActive = true;
                GameManager.Instance.CompleteLevel(level);
                levelComplete = true;
            }
        }
    }

    private EnemyCheck GetRootChecker()
    {
        EnemyCheck current = this;
        while (current.transform.parent != null)
        {
            EnemyCheck parent = current.transform.parent.GetComponent<EnemyCheck>();
            if (parent == null) 
            {
                break;
            }
            current = parent;
        }
        return current;
    }

    private int CountAllEnemies(Transform group)
    {
        int count = 0;
        foreach (Transform child in group)
        {
            if (!child.gameObject.activeInHierarchy) 
            {
                continue; 
            }

            EnemyCheck subCheck = child.GetComponent<EnemyCheck>();
            if (subCheck != null)
            {
                count += CountAllEnemies(child);
            }
            else
            {
                count++;
            }
        }
        return count;
    }
}