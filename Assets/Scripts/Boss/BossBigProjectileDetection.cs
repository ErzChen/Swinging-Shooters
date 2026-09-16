using UnityEngine;

public class BossBigProjectileDetection : MonoBehaviour
{
    public bool stopLaser = false;
    public bool isBig = false;
    private MechProjectileMovement movement;
    private Health health;


    void Start()
    {
        movement = GetComponent<MechProjectileMovement>();
        if (movement.projectileType == "Big")
        {
            isBig = true;
        }
    }

    public void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Obstacle")) {
            stopLaser = true;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
        {
            movement.SpawnExplosion(gameObject.GetComponent<Collider>().ClosestPoint(other.transform.position));
        }

        if (other.CompareTag("Player") && isBig)
        {
            if (health == null)
            {
                health = other.GetComponent<Health>();
            }
            health.TakeDamage(1f);
        }
    }
    
}
