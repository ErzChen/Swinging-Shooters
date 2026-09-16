using UnityEngine;

public class ProjectileDetection : MonoBehaviour
{
    public Shooting shooting;
    public Shooting.EquippedWeapon equippedWeapon;
    public Ray firedRay;
    private DamageDisplay damageDisplay;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        shooting = player.GetComponent<Shooting>();
        damageDisplay = player.GetComponent<DamageDisplay>();
        equippedWeapon = shooting.equippedWeapon;
    }

    private void SpawnExplosion(Vector3 point)
    {
        if (shooting.explosion == null) 
        {
            return;
        }

        GameObject spawnedExplosion = Instantiate(shooting.explosion, point, Quaternion.identity);
        ParticleSystem particleSystem = spawnedExplosion.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        Destroy(spawnedExplosion, 1f);
    }

    public void OnTriggerStay(Collider other)
    {
        if (equippedWeapon == Shooting.EquippedWeapon.Rifle)
        {
            damageDisplay.enemy = other.transform.gameObject;

            bool hitSomething = false;

            if (other.CompareTag("Target"))
            {
                TargetMovement target = other.GetComponentInParent<TargetMovement>();
                target.Hit();
                if (!target.isHit)
                {
                    hitSomething = true;
                    damageDisplay.ShowDamage(1);
                }
            }
            else if (other.CompareTag("Missile"))
            {
                EnemyProjectileMovement movement = other.GetComponent<EnemyProjectileMovement>();
                if (movement.canDamage)
                {
                    hitSomething = true;
                    movement.Explode();
                    damageDisplay.ShowDamage(1);
                }
            }
            else if (other.CompareTag("Enemy"))
            {
                EnemyHealth hitEnemy = other.GetComponentInParent<EnemyHealth>();
                if (hitEnemy.currentHealth != 0)
                {
                    hitSomething = true;
                    hitEnemy.TakeDamage(3);
                    damageDisplay.ShowDamage(3);
                }
            }
            else if (other.CompareTag("Boss"))
            {
                MechHealth mechHealth = other.GetComponentInParent<MechHealth>();
                MechMovement mechMovement = other.GetComponentInParent<MechMovement>();
                hitSomething = true;
                if (mechMovement.currentState == MechMovement.MoveState.Sleep)
                {
                    mechHealth.TakeDamage(30);
                    damageDisplay.ShowDamage(30, true);
                }
                else
                {
                    mechHealth.TakeDamage(3);
                    damageDisplay.ShowDamage(3);
                }
            }

            if (other.CompareTag("WeakPoint"))
            {
                WeakPointDisplay weakPointDisplay = other.GetComponent<WeakPointDisplay>();
                if (weakPointDisplay.activeInstance != null)
                {
                    hitSomething = true;
                    weakPointDisplay.Hit();
                }
            }

            if (other.CompareTag("Obstacle"))
            {
                hitSomething = true;
            }

            if (hitSomething)
            {
                RaycastHit explosionHit;
                if (Physics.Raycast(firedRay, out explosionHit, shooting.raycastDistance, ~LayerMask.GetMask("Ignore Raycast")))
                {
                    SpawnExplosion(explosionHit.point);
                }
            }
        }
    }
}