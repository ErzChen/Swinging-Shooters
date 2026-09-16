using UnityEngine;

public class TurretFollow : MonoBehaviour
{
    public bool isInRange;
    public bool isUpsideDown;
    public GameObject playerTarget;
    public Transform playerTransform;
    public Quaternion baseTargetRotation;
    public Quaternion headTargetRotation;
    public Transform turretHead;

    public EnemyHealth enemyHealth;
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    private const float RotationSpeed = 3f;

    void Update()
    {
        if (isInRange && playerTarget.CompareTag("Player") && enemyHealth.currentHealth > 0)
        {
            Vector3 direction = playerTransform.position - transform.position;
            float horizontalDistance = new Vector3(direction.x, 0f, direction.z).magnitude;
            float verticalAngle = Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;

            if (isUpsideDown)
            {
                verticalAngle = Mathf.Clamp(-verticalAngle, -maxVerticalAngle, -minVerticalAngle);
            }
            else
            {
                verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
            }

            Vector3 headDirection = new Vector3(0f, Mathf.Sin(verticalAngle * Mathf.Deg2Rad), Mathf.Cos(verticalAngle * Mathf.Deg2Rad));

            direction.y = 0f;
            baseTargetRotation = Quaternion.LookRotation(direction);

            if (isUpsideDown)
            {
                baseTargetRotation *= Quaternion.Euler(0f, 0f, 180f);

            }

            headTargetRotation = Quaternion.LookRotation(headDirection);
            turretHead.localRotation = Quaternion.Slerp(turretHead.localRotation, headTargetRotation, Time.deltaTime * RotationSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, baseTargetRotation, Time.deltaTime * RotationSpeed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            playerTarget = other.gameObject;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            playerTarget = null;
            playerTransform = null;
        }
    }
}