using UnityEngine;

public class Part2Manager : MonoBehaviour
{
    public EnemyCheck rootCheck;
    public EnemyCheck previousCheck;
    public EnemyCheck subCheck;
    public Part2Manager previousManager;
    public GameObject platform2;
    public GameObject enemies2;

    private bool activated = false;

    void Start()
    {
        if (enemies2 != null)
        {
            subCheck = enemies2.GetComponent<EnemyCheck>();
        }
    }

    void Update()
    {
        if (!activated && previousCheck != null && previousCheck.nearlyCleared)
        {
            activated = true;

            if (platform2 != null)
            {
                platform2.SetActive(true);
            }

            if (enemies2 != null)
            {
                enemies2.SetActive(true);
                if (subCheck != null)
                {
                    rootCheck.RegisterSubGroup(subCheck);
                }
            }
        }
        else if (previousManager != null && previousCheck == null)
        {
            previousCheck = previousManager.subCheck;
        }
    }
}