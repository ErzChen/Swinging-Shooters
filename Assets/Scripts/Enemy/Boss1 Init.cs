using UnityEngine;

public class Boss1Init : MonoBehaviour
{
    public EnemyCheck rootCheck;
    public EnemyCheck subCheck;
    public Transform playerPos;
    public GameObject boss;
    public GameObject bossMap;

    void Start()
    {
        if (boss != null)
        {
            subCheck = boss.GetComponent<EnemyCheck>();
        }
    }

    void Update()
    {
        if (Mathf.Sqrt(Mathf.Pow(transform.position.x - playerPos.position.x, 2) + Mathf.Pow(transform.position.y - playerPos.position.y, 2) + Mathf.Pow(transform.position.z - playerPos.position.z, 2)) <= 25)
        {
            boss.SetActive(true);
            bossMap.SetActive(true);
            rootCheck.RegisterSubGroup(subCheck);
            Destroy(gameObject);
        }
    }
}
