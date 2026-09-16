using System.Collections;
using UnityEngine;

public class MechShoot : MonoBehaviour
{
    [Header("Properties")]
    public Transform viewPoint;
    public float distance;
    public bool canShoot;

    [Header("Projectiles")]
    public GameObject shortProjectile;
    public GameObject smallProjectile;
    public GameObject bigProjectile;
    public GameObject mortarProjectile;

    [Header("Canons")]
    public Transform shortCanonPoint;
    public Transform smallCanonAPointA;
    public Transform smallCanonAPointB;
    public Transform smallCanonBPointA;
    public Transform smallCanonBPointB;
    public Transform bigCanonAPointA;
    public Transform bigCanonAPointB;
    public Transform bigCanonBPointA;
    public Transform bigCanonBPointB;
    public Transform mortarPointA;
    public Transform mortarPointB;
    public bool shortCanonEnabled = false;
    public bool smallCanonsEnabled = false;
    public bool bigCanonsEnabled = false;
    public bool mortarEnabled = false;

    [Header("Sound")]
    public AudioSource shootSource;
    public AudioClip shortCanonClip;
    public AudioClip smallCanonClip;
    public AudioClip bigCanonClip;
    public AudioClip mortarCanonClip;

    private MechFollow mechFollow;
    private MechMovement mechMovement;
    private Animator smallCanonAAnimator;
    private Animator smallCanonBAnimator;
    private Animator bigCanonAAnimator;
    private Animator bigCanonBAnimator;
    private Transform playerTransform;
    private bool isVisible;
    private bool shortCanonHasCooled = true;
    private bool smallCanonsHasCooled = true;
    private bool bigCanonsHasCooled = true;
    private bool mortarHasCooled = true;

    void Start()
    {
        mechFollow = GetComponent<MechFollow>();
        mechMovement = GetComponent<MechMovement>();
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        smallCanonAAnimator = GameObject.Find("SmallCanonA").GetComponent<Animator>();
        smallCanonBAnimator = GameObject.Find("SmallCanonB").GetComponent<Animator>();
        bigCanonAAnimator = GameObject.Find("BigCanonA").GetComponent<Animator>();
        bigCanonBAnimator = GameObject.Find("BigCanonB").GetComponent<Animator>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) 
        {
            return;
        }
        
        if (!canShoot)
        {
            return;
        }

        distance = Vector3.Distance(transform.position, playerTransform.position);
        Vector3 direction = playerTransform.position - viewPoint.position;

        if (Physics.Raycast(viewPoint.position, direction, out RaycastHit hit))
        {
            isVisible = hit.collider.CompareTag("Player");
        }

        UpdateCanons();

        Shoot();
    }

    void UpdateCanons()
    {
        shortCanonEnabled = false;
        smallCanonsEnabled = false;
        bigCanonsEnabled = false;
        mortarEnabled = false;

        if (distance > 25f && distance < 50f)
        {
            shortCanonEnabled = true;
        }

        if (distance > 50f && distance < 150f)
        {
            smallCanonsEnabled = true;
        }

        if (distance > 100f)
        {
            bigCanonsEnabled = true;
        }

        if (distance > 150f)
        {
            mortarEnabled = true;
        }
    }

    void Shoot()
    {
        if (!isVisible)
        {
            return;
        }

        if (shortCanonEnabled && shortCanonHasCooled)
        {
            StopCoroutine(ShootSmallCanon());
            StartCoroutine(ShootShortCanon());
        }

        if (smallCanonsEnabled && smallCanonsHasCooled)
        {
            StartCoroutine(ShootSmallCanon());
        }

        if (bigCanonsEnabled && bigCanonsHasCooled)
        {
            ShootBigCanon();
        }

        if (mortarEnabled && mortarHasCooled)
        {
            ShootMortar();
        }
    }

    private void SpawnProjectile(Transform spawnTransform, GameObject projectile, float maxAngle = 360f)
    {
        Vector3 directionToPlayer = playerTransform.position - spawnTransform.position;
        Quaternion aimRotation = Quaternion.LookRotation(directionToPlayer);
        Quaternion clampedRotation = Quaternion.RotateTowards(spawnTransform.rotation, aimRotation, maxAngle);
        Instantiate(projectile, spawnTransform.position, clampedRotation);
    }

    IEnumerator ShootShortCanon()
    {
        shortCanonHasCooled = false;
        mechMovement.canMove = false;
        mechFollow.bodyCanFollow = false;
        mechMovement.SetState(MechMovement.MoveState.Idle);
        float prevRotationDirection = mechFollow.bodyRotationDirection;
        shootSource.PlayOneShot(shortCanonClip);
        for (int i = 0; i < 90; i++)
        {
            if (!canShoot)
            {
                break;
            }
            mechFollow.bodyRotationDirection = i - 45;
            yield return new WaitForSeconds(0.02f);
            SpawnProjectile(shortCanonPoint, shortProjectile, 0f);
        }
        mechFollow.bodyCanFollow = true;
        mechFollow.bodyRotationDirection = prevRotationDirection;
        mechMovement.canMove = true;
        StartCoroutine(CoolDown(8.0f, () => shortCanonHasCooled = true));
    }

    IEnumerator ShootSmallCanon()
    {
        smallCanonsHasCooled = false;
        for (int i = 0; i < 80; i++)
        {
            if (!canShoot)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
            SpawnProjectile(smallCanonAPointA, smallProjectile, 90f);
            shootSource.PlayOneShot(smallCanonClip);
            SpawnProjectile(smallCanonAPointB, smallProjectile, 90f);
            shootSource.PlayOneShot(smallCanonClip);
            smallCanonAAnimator.SetTrigger("Shoot");
            Invoke("ShootSmallCanonB", 0.05f);  
        }
        StartCoroutine(CoolDown(2.0f, () => smallCanonsHasCooled = true));
    }

    private void ShootSmallCanonB()
    {
        SpawnProjectile(smallCanonBPointA, smallProjectile, 90f);
        shootSource.PlayOneShot(smallCanonClip);
        SpawnProjectile(smallCanonBPointB, smallProjectile, 90f);
        shootSource.PlayOneShot(smallCanonClip);
        smallCanonBAnimator.SetTrigger("Shoot");
    }

    void ShootBigCanon()
    {
        bigCanonsHasCooled = false;
        SpawnProjectile(bigCanonAPointA, bigProjectile, 90f);
        shootSource.PlayOneShot(bigCanonClip);
        SpawnProjectile(bigCanonAPointB, bigProjectile, 90f);
        shootSource.PlayOneShot(bigCanonClip);
        bigCanonAAnimator.SetTrigger("Shoot");
        Invoke("ShootBigCanonB", 1f);
        StartCoroutine(CoolDown(4.0f, () => bigCanonsHasCooled = true));
    }

    private void ShootBigCanonB()
    {
        SpawnProjectile(bigCanonBPointA, bigProjectile, 90f);
        shootSource.PlayOneShot(bigCanonClip);
        SpawnProjectile(bigCanonBPointB, bigProjectile, 90f);
        shootSource.PlayOneShot(bigCanonClip);
        bigCanonBAnimator.SetTrigger("Shoot");
    }

    void ShootMortar()
    {
        mortarHasCooled = false;
        SpawnProjectile(mortarPointA, mortarProjectile, 90f);
        shootSource.PlayOneShot(mortarCanonClip);
        SpawnProjectile(mortarPointB, mortarProjectile, 90f);
        shootSource.PlayOneShot(mortarCanonClip);
        StartCoroutine(CoolDown(2.0f, () => mortarHasCooled = true));
    }

    IEnumerator CoolDown(float cooldownTime, System.Action onComplete)
    {
        yield return new WaitForSeconds(cooldownTime);
        onComplete?.Invoke();
    }
}
