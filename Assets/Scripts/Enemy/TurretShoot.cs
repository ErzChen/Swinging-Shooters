using System.Collections;
using UnityEngine;

public class TurretShoot : MonoBehaviour
{
    public GameObject turretType;
    public TurretFollow turretFollow;
    public Transform spawnPointA;
    public Transform spawnPointB;
    public GameObject enemyProjectile;
    public Transform playerTransform;
    public AudioSource turretSource;
    public bool isVisible;
    public bool useSpread;
    public bool hasSecondBarrel;
    public bool canRecoil;
    public Rigidbody rb;
    [SerializeField][Range(0, 1)] public float projectileSpread;

    public bool isCoolingDown;
    public Animator animator;

    void Start()
    {
        animator = GetComponentInParent<Animator>();
        isCoolingDown = false;
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        turretSource = GetComponentInParent<AudioSource>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) 
        {
            return;
        }
        
        Vector3 direction = playerTransform.position - transform.position;

        if (Physics.Raycast(spawnPointA.position, direction, out RaycastHit hit))
        {
            isVisible = hit.collider.CompareTag("Player");
        }

        if (turretFollow.isInRange && !isCoolingDown)
        {
            StartCoroutine(FireSequence());
        }
    }

    public void Spawn()
    {
        if (canRecoil)
        {
            rb.AddForce(-transform.forward.x * 2f, 0f, -transform.forward.z * 2f, ForceMode.Force);
        }

        if (turretType.CompareTag("Canon4"))
        {
            useSpread = true;
            projectileSpread = 0.05f;
        }
        else
        {
            useSpread = false;
            projectileSpread = 0f;
        }

        if (turretType.CompareTag("Canon1") || turretType.CompareTag("Canon3") || turretType.CompareTag("Canon4"))
        {
            SpawnProjectile(spawnPointA.position);
        }
        else if (turretType.CompareTag("Canon5"))
        {
            StartCoroutine(FireCanon5());
        }
        else if (turretType.CompareTag("Canon2"))
        {
            SpawnProjectile(spawnPointA.position);
            SpawnProjectile(spawnPointB.position);
        }
    }

    private void SpawnProjectile(Vector3 spawnPosition)
    {
        GameObject newProjectile = Instantiate(enemyProjectile, spawnPosition, transform.rotation);
        EnemyProjectileMovement movement = newProjectile.GetComponent<EnemyProjectileMovement>();
        movement.turretShoot = this;
        movement.playerTransform = playerTransform;
    }

    IEnumerator FireSequence()
    {
        isCoolingDown = true;

        if (turretType.CompareTag("Canon1"))
        {
            if (isVisible)
            {
                animator.SetTrigger("Shoot");
                turretSource.pitch = 0.5f;
                turretSource.PlayOneShot(turretSource.clip);
            }

            for (int i = 0; i < 20; i++)
            {
                if (isVisible)
                {
                    yield return new WaitForSeconds(0.02f);
                    Spawn();
                }
            }
        }
        else if (turretType.CompareTag("Canon2"))
        {
            for (int i = 0; i < 30; i++)
            {
                if (isVisible)
                {
                    yield return new WaitForSeconds(0.2f);
                    animator.SetTrigger("Shoot1");
                    turretSource.PlayOneShot(turretSource.clip);
                    Spawn();
                }
            }
        }
        else if (turretType.CompareTag("Canon3"))
        {
            if (isVisible)
            {
                animator.SetTrigger("Shoot2");
                turretSource.pitch = 0.5f;
                turretSource.PlayOneShot(turretSource.clip);
            }

            for (int i = 0; i < 20; i++)
            {
                if (isVisible)
                {
                    yield return new WaitForSeconds(0.02f);
                    Spawn();
                }
            }
        }
        else if (turretType.CompareTag("Canon4"))
        {
            if (isVisible)
            {
                animator.SetTrigger("Shoot3");
            }
            
            for (int i = 0; i < 7; i++)
            {
                if (isVisible)
                {
                    yield return new WaitForSeconds(0.02f);
                    turretSource.PlayOneShot(turretSource.clip);
                    Spawn();
                }
            }
        }
        else if (turretType.CompareTag("Canon5"))
        {
            if (isVisible)
            {
                Spawn();
                yield return new WaitForSeconds(6f);
            }
        }

        if (!turretType.CompareTag("Canon4"))
        {
            yield return new WaitForSeconds(2f);
        }

        isCoolingDown = false;
    }

    IEnumerator FireCanon5()
    {
        if (hasSecondBarrel)
        {
            yield return new WaitForSeconds(2f);
        }

        animator.SetTrigger("Shoot4");
        turretSource.PlayOneShot(turretSource.clip);
        SpawnProjectile(spawnPointA.position);
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("Shoot4");
        turretSource.PlayOneShot(turretSource.clip);
        SpawnProjectile(spawnPointB.position);
    }
}