using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    public float speedX;
    public float speedY;

    [Header("Sine Wave Settings")]
    public float frequency;
    public float amplitude;

    [Header("Movement Bounds")]
    public float boundsMinX;
    public float boundsMaxX;
    public float boundsMinY;
    public float boundsMaxY;

    [Header("Dependencies")]
    public Rigidbody rb;
    public EnemyCheck enemyCheck;

    [Header("Movement Axes")]
    public bool movingX;
    public bool movingY;

    [Header("Movement Mode")]
    public bool isSineMode;
    public bool isLinearMode;

    public bool isHit = false;

    private Vector3 initialPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        enemyCheck = transform.parent.GetComponent<EnemyCheck>();
        initialPosition = transform.position;
    }

    void Update()
    {
        if (isHit)
        {
            return;
        }

        Vector3 pos = transform.position;

        if (isLinearMode)
        {
            if (movingX)
            {
                if (pos.x >= boundsMaxX)
                {
                    speedX = -Mathf.Abs(speedX);
                }
                if (pos.x <= boundsMinX)
                {
                    speedX = Mathf.Abs(speedX);
                }
                pos.x += speedX * Time.deltaTime;
            }

            if (movingY)
            {
                if (pos.y >= boundsMaxY)
                {
                    speedY = -Mathf.Abs(speedY);
                }
                if (pos.y <= boundsMinY)
                {
                    speedY = Mathf.Abs(speedY);
                }
                pos.y += speedY * Time.deltaTime;
            }
        }

        if (isSineMode)
        {
            if (movingX)
            {
                if (pos.y >= boundsMaxY && speedY > 0)
                {
                    speedY = -Mathf.Abs(speedY);
                }
                if (pos.y <= boundsMinY && speedY < 0)
                {
                    speedY = Mathf.Abs(speedY);
                }
                pos.x = initialPosition.x + Mathf.Sin(Time.time * frequency) * amplitude;
                pos.y += speedY * Time.deltaTime;
            }

            if (movingY)
            {
                if (pos.x >= boundsMaxX && speedX > 0)
                {
                    speedX = -Mathf.Abs(speedX);
                }
                if (pos.x <= boundsMinX && speedX < 0)
                {
                    speedX = Mathf.Abs(speedX);
                }
                pos.y = initialPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
                pos.x += speedX * Time.deltaTime;
            }
        }

        transform.position = pos;
    }

    public void Hit()
    {
        isHit = true;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
        rb.useGravity = true;
        Invoke("DestroyTarget", 5f);
    }

    void DestroyTarget()
    {
        enemyCheck.ReportEnemyDefeated();
        Destroy(gameObject);
    }
}