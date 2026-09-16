using UnityEngine;

public class EnemyProjectileMovement : MonoBehaviour
{
    public Health playerHealth;
    public TurretShoot turretShoot;
    public Transform playerTransform;
    public ParticleSystem explosionEffect;
    public GameObject rocketVisual;
    public GameObject explosionTrigger;
    public GameObject explosion;
    public string turretTag;
    public bool canDamage = true;
    public bool hasSpread;
    public bool isMissile;
    public float speed = 200f;
    public float spread;

    void Start()
    {
        turretTag = turretShoot.turretType.tag;
        hasSpread = turretShoot.useSpread;

        if (turretTag == "Canon3")
        {
            speed = 350f;
        }

        spread = Random.Range(-turretShoot.projectileSpread, turretShoot.projectileSpread);

        if (isMissile)
        {
            Invoke("Explode", 8f);
            explosionTrigger.GetComponentInChildren<AudioSource>(true).mute = !GameManager.Instance.audioOn;
        }
        else
        {
            Destroy(gameObject, 2f);
        }

        playerHealth = Health.PlayerInstance;
    }

    void Update()
    {
        Vector3 moveDirection;

        if (hasSpread)
        {
            moveDirection = transform.forward + transform.right * spread;
        }
        else
        {
            moveDirection = transform.forward;
        }

        if (isMissile)
        {
            speed = 20f;
            Quaternion targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f);
        }

        if (canDamage)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    private void SpawnExplosion(Vector3 point)
    {
        if (explosion == null) 
        {
            return;
        }

        GameObject spawnedExplosion = Instantiate(explosion, point, Quaternion.identity);
        ParticleSystem particleSystem = spawnedExplosion.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        Destroy(spawnedExplosion, 1f);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (turretTag != "Canon5" 
            && !other.CompareTag("Enemy") 
            && other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
        {
            SpawnExplosion(other.ClosestPoint(transform.position));
        }
        if (other.CompareTag("Player") && canDamage)
        {
            if (turretTag == "Canon1" || turretTag == "Canon3")
            {
                playerHealth.TakeDamage(1);
                Destroy(gameObject);
            }
            else if (turretTag == "Canon2" || turretTag == "Canon4")
            {
                playerHealth.TakeDamage(3);
                Destroy(gameObject);
            }
            else if (turretTag == "Canon5")
            {
                playerHealth.TakeDamage(10);
                Explode();
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    public void Explode()
    {
        rocketVisual.SetActive(false);
        canDamage = false;
        explosionEffect.Play();
        explosionTrigger.SetActive(true);
        Destroy(gameObject, 2f);
    }
}