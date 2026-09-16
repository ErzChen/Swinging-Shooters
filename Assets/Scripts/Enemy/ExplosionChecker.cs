using UnityEngine;

public class ExplosionChecker : MonoBehaviour
{
    public Health health;

    void Start()
    {
        health = Health.PlayerInstance;
    }

    public void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            health.TakeDamage(20);
            Destroy(gameObject);
        }
    }
}
