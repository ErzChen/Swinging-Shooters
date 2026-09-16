using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public class Shooting : MonoBehaviour
{
    [Header("Weapon References")]
    public GameObject rifle;
    public GameObject pistol;
    public Transform rifleSpawnPoint;
    public Transform pistolSpawnPoint;
    public GameObject projectilePrefab;
    public GameObject crosshair;
    public GameObject spawnedProjectile;
    public GameObject explosion;

    [Header("Weapon State")]
    public EquippedWeapon equippedWeapon;
    public enum EquippedWeapon { Pistol, Rifle };

    [Header("Shoot Settings")]
    public float projectileSpeed = 50f;
    public int raycastDistance;
    public int pistolSpread = 1;

    [Header("Shoot Manager")]
    public int shotsFired;

    [Header("Sounds")]
    public AudioSource shootSource;
    public AudioClip pistolClip;
    public AudioClip rifleClip;

    private Camera cam;
    private DamageDisplay damageDisplay;
    private RaycastHit hit;
    public Ray gunRay;
    private Animator animator;
    private GameObject projectileContainer;
    private Image[] crosshairPieces;
    private Transform activeSpawnPoint;
    private bool canShoot;
    private int raycastLayerMask;

    private void Start()
    {
        cam = Camera.main;
        canShoot = true;
        damageDisplay = this.GetComponent<DamageDisplay>();
        raycastLayerMask = ~LayerMask.GetMask("Ignore Raycast");
        projectileContainer = GameObject.Find("ObjectCache");
        crosshair = GameObject.Find("Crosshair");
        crosshairPieces = crosshair.GetComponentsInChildren<Image>(true);
        EquipPistol();
    }

    private void FixedUpdate()
    {   
        crosshairUpdate();

        if (Input.GetKeyDown(KeyCode.Alpha1) && equippedWeapon != EquippedWeapon.Pistol)
        {
            EquipPistol();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && GameManager.Instance.rifleUnlocked && equippedWeapon != EquippedWeapon.Rifle)
        {
            EquipRifle();
        }
        if (Input.GetMouseButton(0) && canShoot)
        {
            Shoot(hit);
            canShoot = false;
            StartCoroutine(ShootCooldown());
        }
    }

    private void EquipPistol()
    {
        pistol.SetActive(true);
        rifle.SetActive(false);
        equippedWeapon = EquippedWeapon.Pistol;
        raycastDistance = 50;
        activeSpawnPoint = pistolSpawnPoint;
        animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("Equip");
        StopAllCoroutines();
        canShoot = false;
        StartCoroutine(ShootCooldown(true));
    }

    private void EquipRifle()
    {
        pistol.SetActive(false);
        rifle.SetActive(true);
        equippedWeapon = EquippedWeapon.Rifle;
        raycastDistance = 210;
        activeSpawnPoint = rifleSpawnPoint;
        animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("Equip");
        StopAllCoroutines();
        canShoot = false;
        StartCoroutine(ShootCooldown());
    }

    public void Shoot(RaycastHit hit)
    {
        shotsFired += 1;

        Vector3 targetPoint = gunRay.GetPoint(100f);
        Vector3 fireDirection = targetPoint - activeSpawnPoint.position;

        Vector3 spreadOffset = Vector3.zero;
        if (equippedWeapon == EquippedWeapon.Pistol)
        {
            spreadOffset = new Vector3(
                Random.Range(-pistolSpread, pistolSpread),
                Random.Range(-pistolSpread, pistolSpread),
                Random.Range(-pistolSpread, pistolSpread)
            );
            fireDirection += spreadOffset;
            shootSource.clip = pistolClip;
        } 
        else
        {
            shootSource.clip = rifleClip;
        }
        shootSource.PlayOneShot(shootSource.clip);

        animator.SetTrigger("Shoot");
        spawnedProjectile = Instantiate(projectilePrefab, activeSpawnPoint.position, Quaternion.LookRotation(fireDirection));
        spawnedProjectile.transform.parent = projectileContainer.transform;
        ProjectileMovement projectileMovement = spawnedProjectile.GetComponent<ProjectileMovement>();
        if (projectileMovement != null) 
        {
            projectileMovement.firedRay = gunRay;
        }

        Ray spreadRay = new Ray(gunRay.origin, gunRay.direction + spreadOffset.normalized * 0.05f);
        Physics.Raycast(spreadRay, out hit, raycastDistance, raycastLayerMask);

        if (hit.collider != null)
        {
            if (explosion != null)
            {
                GameObject spawnedExplosion = Instantiate(explosion, hit.point, Quaternion.identity);
                ParticleSystem ps = spawnedExplosion.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                }
                Destroy(spawnedExplosion, 1f);
            }

            damageDisplay.enemy = hit.transform.gameObject;

            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealth hitEnemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (hitEnemy == null)
                {
                    return;
                }
                if (hitEnemy.currentHealth != 0 && equippedWeapon == EquippedWeapon.Pistol)
                {
                    hitEnemy.TakeDamage(20);
                    damageDisplay.ShowDamage(20);
                }
            }
            else if (hit.collider.CompareTag("Target"))
            {
                TargetMovement target = hit.collider.GetComponentInParent<TargetMovement>();
                if (!target.isHit)
                {
                    damageDisplay.ShowDamage(1);
                }
                target.Hit();
                
            }
            else if (hit.collider.CompareTag("Missile"))
            {
                EnemyProjectileMovement missileMovement = hit.collider.GetComponent<EnemyProjectileMovement>();
                if (missileMovement.canDamage)
                {
                    missileMovement.Explode();
                    damageDisplay.ShowDamage(1);
                }
            }
            else if (hit.collider.CompareTag("Boss"))
            {
                MechHealth mechHealth = hit.collider.GetComponentInParent<MechHealth>();
                if (mechHealth.isDestroyed)
                {
                    return;
                }
                MechMovement mechMovement = hit.collider.GetComponentInParent<MechMovement>();
                if (mechMovement.currentState == MechMovement.MoveState.Sleep)
                {
                    mechHealth.TakeDamage(200);
                    damageDisplay.ShowDamage(200, true);
                }
                else
                {
                    mechHealth.TakeDamage(20);
                    damageDisplay.ShowDamage(20);
                }
                
            }
            if (hit.collider.CompareTag("WeakPoint"))
            {
                WeakPointDisplay weakPointDisplay = hit.collider.GetComponent<WeakPointDisplay>();
                if (weakPointDisplay.activeInstance != null)
                {
                    weakPointDisplay.Hit();
                }
            }
        }
    }

    public void crosshairUpdate()
    {
        gunRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Color targetColor = Color.white;

        if (Physics.Raycast(gunRay, out hit, raycastDistance, raycastLayerMask)) 
        {
            if (hit.collider.CompareTag("Enemy") 
                || hit.collider.CompareTag("Target") 
                || hit.collider.CompareTag("Missile") 
                || hit.collider.CompareTag("Boss")
                || hit.collider.CompareTag("WeakPoint")) 
            {
                targetColor = new Color(1f, 0.1f, 0.1f, 1f); 
            }
        }

        foreach (Image image in crosshairPieces) 
        {
            image.color = targetColor;
        }
    }

    private IEnumerator ShootCooldown(bool weaponEquip = false)
    {
        if (equippedWeapon == EquippedWeapon.Rifle || weaponEquip)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else if (equippedWeapon == EquippedWeapon.Pistol)
        {
            yield return new WaitForSeconds(0.1f);
        }

        canShoot = true;
    }
}