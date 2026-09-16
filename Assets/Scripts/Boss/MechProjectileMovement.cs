using System.Collections;
using UnityEngine;

public class MechProjectileMovement : MonoBehaviour
{
    public Health playerHealth;
    public BossBigProjectileDetection projectileDetection;
    public string projectileType;
    public bool canDamage = true;
    public bool canMove = true;
    public float speed = 400f;
    public float mortarArcHeight = 3f;
    public float mortarSpeed = 3f;
    public float spreadX = 0f;
    public float spreadY = 0f;
    public GameObject explosion;
    public GameObject explosionMortar;
    public GameObject projectileMesh;
    public Collider sphere;

    private Vector3 mortarTargetPosition;
    private Vector3 mortarVelocity;
    private bool mortarLaunched = false;
    private float mortarGravity;

    void Start()
    {
        playerHealth = Health.PlayerInstance;
        projectileDetection = GetComponent<BossBigProjectileDetection>();

        if (projectileType == "Small")
        {
            spreadX = Random.Range(-0.01f, 0.01f);
            spreadY = Random.Range(-0.01f, 0.01f);
        }

        if (projectileType == "Big")
        {
            StartCoroutine(BigLaserMovement(0f));
            return;
        }

        if (projectileType == "Mortar")
        {
            canDamage = false;
            mortarTargetPosition = playerHealth.transform.position;
            mortarGravity = Mathf.Abs(Physics.gravity.y) * mortarSpeed * mortarSpeed;
            mortarVelocity = CalculateLaunchVelocity(transform.position, mortarTargetPosition);
            mortarLaunched = true;
            explosionMortar.GetComponentInChildren<AudioSource>(true).mute = !GameManager.Instance.audioOn;
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    private Vector3 CalculateLaunchVelocity(Vector3 origin, Vector3 target)
    {
        float peakY = Mathf.Max(origin.y, target.y) + mortarArcHeight;
        float timeUp = Mathf.Sqrt(2f * (peakY - origin.y) / mortarGravity);
        float timeDown = Mathf.Sqrt(2f * (peakY - target.y) / mortarGravity);
        float totalTime = timeUp + timeDown;

        Vector3 displacement = target - origin;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
        Vector3 horizontalVelocity = horizontalDisplacement / totalTime;
        float verticalVelocity = mortarGravity * timeUp;

        return horizontalVelocity + Vector3.up * verticalVelocity;
    }

    void Update()
    {
        if (projectileType == "Big" || !canMove)
        {
            return;
        }

        if (projectileType == "Mortar" && mortarLaunched)
        {
            mortarVelocity += Vector3.down * mortarGravity * Time.deltaTime;
            transform.position += mortarVelocity * Time.deltaTime;

            if (mortarVelocity != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(mortarVelocity);

            float horizontalDistToTarget = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(mortarTargetPosition.x, 0f, mortarTargetPosition.z)
            );

            bool descendingPastTarget = mortarVelocity.y < 0f &&
                                        transform.position.y <= mortarTargetPosition.y + 0.5f;

            if (horizontalDistToTarget < 0.8f && descendingPastTarget)
            {
                Explode();
            }

            return;
        }

        Vector3 moveDirection = transform.forward + transform.right * spreadX + transform.up * spreadY;
        moveDirection = moveDirection.normalized;
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    public void SpawnExplosion(Vector3 point)
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
        if (projectileType != "Mortar" && projectileType != "Big" && other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
        {
            SpawnExplosion(other.ClosestPoint(transform.position));
        }
        if (other.CompareTag("Player") && canDamage)
        {
            if (projectileType == "Short")
            {
                playerHealth.TakeDamage(1);
            }
            else if (projectileType == "Small")
            {
                playerHealth.TakeDamage(1);
                Destroy(gameObject);
            }
            else if (projectileType == "Mortar")
            {
                playerHealth.TakeDamage(10);
                canDamage = false;
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            if (projectileType != "Big")
            {
                if (projectileType == "Mortar")
                {
                    Explode();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    IEnumerator BigLaserMovement(float currentLength)
    {
        Vector3 initialPosition = transform.position;
        Quaternion initialRotation = transform.rotation;

        while (currentLength < 150f)
        {
            if (projectileDetection.stopLaser) break;

            currentLength = Mathf.Min(currentLength + 5f, 150f);

            transform.localScale = new Vector3(1f, 1f, currentLength);
            float worldLength = currentLength * 2f;
            transform.position = initialPosition + transform.forward * (worldLength / 2f);

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        float finalWorldLength = currentLength * 2f;
        Vector3 tipPosition = initialPosition + transform.forward * finalWorldLength;

        float retractLength = currentLength;

        while (retractLength > 0f)
        {
            retractLength = Mathf.Max(retractLength - 5f, 0f);

            transform.localScale = new Vector3(1f, 1f, retractLength);
            float worldLength = retractLength * 2f;
            transform.position = tipPosition - transform.forward * (worldLength / 2f);

            yield return null;
        }

        Destroy(gameObject);
    }

    public void Explode()
    {
        if (!canMove) 
        {
            return;  
        }

        canMove = false;
        mortarLaunched = false;
        projectileMesh.SetActive(false);
        explosionMortar.SetActive(true);
        ParticleSystem explosionParts = explosionMortar.GetComponentInChildren<ParticleSystem>();
        explosionParts.Play();
        sphere.enabled = true;
        canDamage = true;
        Destroy(gameObject, 1f);
    }
}