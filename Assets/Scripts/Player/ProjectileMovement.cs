using System.Collections;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    public Shooting shooting;
    public ProjectileDetection projectileDetection;
    public float speed = 800f;
    public Ray firedRay;

    private Transform projectileTransform;
    private Shooting.EquippedWeapon equippedWeapon;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        shooting = player.GetComponent<Shooting>();
        projectileDetection = GetComponentInChildren<ProjectileDetection>();
        projectileTransform = gameObject.transform.GetChild(0);

        equippedWeapon = shooting.equippedWeapon; 

        if (projectileDetection != null)
        {
            projectileDetection.firedRay = firedRay;
        }

        if (equippedWeapon == Shooting.EquippedWeapon.Pistol)
        {
            Destroy(shooting.spawnedProjectile, 0.2f);
        }
        else
        {
            StartCoroutine(AnimateLaser(0f));
        }
    }

    void Update()
    {
        if (equippedWeapon != Shooting.EquippedWeapon.Rifle)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    IEnumerator AnimateLaser(float currentLength)
    {
        Vector3 initialScale = projectileTransform.localScale;
        Vector3 initialPosition = projectileTransform.localPosition;
        float maxLength = 210f;

        RaycastHit[] hits = Physics.RaycastAll(firedRay, 210f, ~LayerMask.GetMask("Ignore Raycast"));
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        string[] pierceableTags = { "Target", "Missile", "Enemy", "WeakPoint", "Boss", "Player", "Weapon" };

        foreach (RaycastHit hit in hits)
        {
            bool piercable = false;
            foreach (string tag in pierceableTags)
            {
                if (hit.collider.CompareTag(tag))
                {
                    piercable = true;
                    break;
                }
            }

            if (!piercable)
            {
                maxLength = hit.distance;
                break;
            }
        }

        while (currentLength < maxLength)
        {
            currentLength = Mathf.Min(currentLength + 10f, maxLength);

            projectileTransform.localScale = new Vector3(initialScale.x, initialScale.y * currentLength, initialScale.z);
            projectileTransform.localPosition = new Vector3(initialPosition.x, initialPosition.y, initialPosition.z + currentLength * 0.5f);

            yield return null;
        }

        yield return new WaitForSeconds(0.4f);

        float retractLength = currentLength;
        while (retractLength > 0f)
        {
            retractLength = Mathf.Max(retractLength - 10f, 0f);

            projectileTransform.localScale = new Vector3(initialScale.x, initialScale.y * retractLength, initialScale.z);
            projectileTransform.localPosition = new Vector3(initialPosition.x, initialPosition.y, initialPosition.z + maxLength - retractLength * 0.5f);

            yield return null;
        }

        Destroy(projectileTransform.gameObject);
    }
}