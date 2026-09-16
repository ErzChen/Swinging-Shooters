using UnityEngine;

public class MechFollow : MonoBehaviour
{
    [Header("References")]
    public MimicTransform pelvis;
    public Transform playerTransform;
    public Transform viewPoint;

    [Header("Following")]
    public bool canFollow = false;
    public bool bodyCanFollow = true;

    [Header("Rotation Settings")]
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;
    public float rotationDirection = 0f;
    public float bodyRotationDirection = 0f;
    public float rotationSpeed = 3f;
    public Vector3 pelvisUprightOffset = new Vector3(0f, 90f, 0f);

    private MechHealth mechHealth;
    private Quaternion currentBodyHorizontal;
    private float smoothedVertical;

    void Start()
    {
        mechHealth = GetComponent<MechHealth>();
        currentBodyHorizontal = Quaternion.Euler(0f, 180f, 0f);
        smoothedVertical = 0f;
    }

    void Update()
    {
        if (pelvis != null)
        {
            pelvis.isFollowing = canFollow;
        }

        if (mechHealth == null || mechHealth.currentHealth <= 0 || !canFollow)
        {
            return;
        }

        if (playerTransform == null || viewPoint == null)
        {
            return;
        }

        Vector3 direction = playerTransform.position - viewPoint.position;
        float horizontalDistance = new Vector3(direction.x, 0f, direction.z).magnitude;

        float verticalAngle = Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;

        Quaternion targetTransformRot = Quaternion.LookRotation(-flatDirection) * Quaternion.Euler(0f, rotationDirection, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetTransformRot, Time.deltaTime * rotationSpeed);

        float bodyAngleToPlayer = Mathf.Atan2(flatDirection.x, flatDirection.z) * Mathf.Rad2Deg;
        Quaternion targetHorizontal = Quaternion.Euler(0f, bodyAngleToPlayer + bodyRotationDirection, 0f);
        currentBodyHorizontal = Quaternion.Slerp(currentBodyHorizontal, targetHorizontal, Time.deltaTime * rotationSpeed);

        if (bodyCanFollow)
            smoothedVertical = Mathf.LerpAngle(smoothedVertical, -verticalAngle, Time.deltaTime * rotationSpeed);

        if (pelvis != null)
        {
            Vector3 directionToPlayer = playerTransform.position - pelvis.transform.position;
            Quaternion lookAt = Quaternion.FromToRotation(-Vector3.right, directionToPlayer);
            Quaternion combined = lookAt * Quaternion.Euler(pelvisUprightOffset);
            Vector3 euler = combined.eulerAngles;
            euler.z = 90f;
            combined = Quaternion.Euler(euler);

            pelvis.targetRotation = Quaternion.Slerp(pelvis.targetRotation, combined, Time.deltaTime * rotationSpeed);
        }
    }
}