using UnityEngine;

public class WallRun : MonoBehaviour
{
    [Header("References")]
    public Transform startingPosition;

    [Header("Wall Detection")]
    public LayerMask layerMask;
    public float maxDistanceToWall = 3f;

    [Header("Wall Run")]
    public float minSpeedWhenAttached = 5f;
    public float cameraTiltAngle = 5f;
    public float initialVerticalBoost = 5f;    
    public float verticalDecayRate = 4.5f;        

    [Header("Jump")]
    public float jumpForce = 600f;
    public float jumpWallMultiplier = 0.5f;
    public float jumpForwardMultiplier = 0.3f;
    public float jumpUpMultiplier = 0.2f;       

    [Header("Timings")]
    public float jumpBlockTime = 0.8f;
    public float attachToWallBlockTime = 0.5f;

    public bool IsWallRunning => wallRunning;

    private PlayerMovement PlayerMovement;
    private SwingHook SwingHook;
    private Rigidbody rb;

    private bool wallRunning = false;
    private bool blocked = false;
    private bool jumpAvailable = true;

    private float verticalBoost = 0f;
    private RaycastHit hitInfo;


    private void Start()
    {
        PlayerMovement = GetComponent<PlayerMovement>();
        SwingHook = GetComponent<SwingHook>();
        rb = GetComponent<Rigidbody>();

        if (PlayerMovement.canWalkWalls)
        {
            this.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!rb.useGravity || blocked || SwingHook.isSwinging)
        {
            return;
        }

        if (TryGetWall(transform.right, out hitInfo))
        {
            HandleWallRun(true);
            return;
        }

        if (TryGetWall(-transform.right, out hitInfo))
        {
            HandleWallRun(false);
            return;
        }

        if (wallRunning)
        {
            StopWallRunning();
        }
    }

    private bool TryGetWall(Vector3 direction, out RaycastHit hit)
    {
        return Physics.Raycast(startingPosition.position, direction, out hit, maxDistanceToWall, layerMask) && PlayerMovement.verticalInput >= 0.5f;
    }


    private void HandleWallRun(bool isRightWall)
    {
        if (!wallRunning)
        {
            StartWallRunning();
        }

        ApplyWallRunForces(isRightWall);
    }

    private void StartWallRunning()
    {
        wallRunning = true;
        jumpAvailable = true;
        verticalBoost = initialVerticalBoost;
        rb.velocity = new Vector3(rb.velocity.x, verticalBoost, rb.velocity.z);
    }

    private void StopWallRunning()
    {
        wallRunning = false;
        PlayerCamera.cam.rollAngle = 0f;

        blocked = true;
        Invoke(nameof(UnblockWallRunning), attachToWallBlockTime);
    }

    private void UnblockWallRunning() => blocked = false;

    private void ApplyWallRunForces(bool isRightWall)
    {
        HandleWallJump();
        ApplyVerticalBoost();
        EnforceMinimumSpeed();
        PlayerCamera.cam.rollAngle = isRightWall ? cameraTiltAngle : -cameraTiltAngle;
    }

    private void HandleWallJump()
    {
        if (!jumpAvailable || !Input.GetKey(KeyCode.Space))
        {
            return;
        }

        Vector3 jumpDirection = (hitInfo.normal * jumpWallMultiplier + transform.forward * jumpForwardMultiplier + Vector3.up * jumpUpMultiplier).normalized;

        rb.AddForce(jumpDirection * rb.mass * jumpForce);
        jumpAvailable = false;
        Invoke(nameof(UnblockJump), jumpBlockTime);
    }

    private void ApplyVerticalBoost()
    {
        if (verticalBoost < 0f)
        {
            return;
        }

        rb.velocity = new Vector3(rb.velocity.x, verticalBoost, rb.velocity.z);
        verticalBoost -= verticalDecayRate * Time.fixedDeltaTime;
    }

    private void EnforceMinimumSpeed()
    {
        float horizontalSpeed = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
        if (horizontalSpeed < minSpeedWhenAttached)
        {
            rb.velocity = new Vector3(rb.velocity.x / horizontalSpeed * minSpeedWhenAttached, rb.velocity.y, rb.velocity.z / horizontalSpeed * minSpeedWhenAttached);
        }
    }

    private void UnblockJump() => jumpAvailable = true;
}