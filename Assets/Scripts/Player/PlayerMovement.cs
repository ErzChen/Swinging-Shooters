using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform orientation;
    public Camera playerCameraComponent;
    public Tutorial tutorial;

    [Header("Audio")]
    public AudioSource playerSource;
    public AudioClip stepClip;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public float stepIntervalWalk = 0.5f;
    public float stepIntervalRun = 0.3f;
    private float stepTimer;

    [Header("Camera")]
    public float pistolRunFOV = 70f;
    public float rifleRunFOV = 65f;
    public float fastFOV = 80f;
    public float idleFOV = 60f;

    [Header("Movement")]
    public float walkSpeed = 8.0f;
    public float runSpeed = 12.0f;
    public float accelerationSpeed = 10.0f;
    public float maximumPlayerSpeed = 150.0f;
    public bool canWalkWalls;

    [Header("Ground Check")]
    public Transform groundChecker;
    public float groundCheckerDist = 0.2f;

    [Header("Jump")]
    public float jumpForce = 0f;
    public float jumpCooldown = 1.0f;
    public float fallMultiplier = 1.006f;
    public float fallingSpeed = 20f;
    public float currentFallingSpeed;
    public float grappleJumpMultiplier = 3f;
    public int ungrappleJumpTime = 500;
    public bool jumpBlocked = false;

    [Header("Fall Damage")]
    public float minFallSpeed = 50f;      
    public float lethalFallSpeed = 150f;   
    public float maxFallDamage = 90f;   
    public bool canTakeDamage = true;
    public Vector3 lastVelocity;

    [Header("State")]
    public bool isGrounded = false;
    public bool tutorialCheck = false;
    [HideInInspector] public float verticalInput, horizontalInput;

    public bool IsGrounded { get { return isGrounded; } }

    public int layerMask;

    private Shooting shoot;
    private Health health;
    private GrapplingHook grapplingHook;
    private SwingHook swingHook;
    private Rigidbody rb;
    private WallRun wallRun;
    private bool movementEnabled = true;
    private bool canChangeWalls;
    private bool ungrappleJumpAvailable = false;
    private bool ungrappleJumpUsed = false;
    private long ungrappleTimeElapsed;

    private Vector3 gravityDirection;
    private List<float> wallDistances = new List<float>();
    private List<Vector3> wallNormals = new List<Vector3>();

    private bool wasGrounded = false;

    private void Start()
    {
        layerMask = ~LayerMask.GetMask("Ignore Raycast");
        rb = this.GetComponent<Rigidbody>();
        shoot = this.GetComponent<Shooting>();
        health = Health.PlayerInstance;
        grapplingHook = this.GetComponent<GrapplingHook>();
        swingHook = this.GetComponent<SwingHook>();
        wallRun = this.GetComponent<WallRun>();
        gravityDirection = Vector3.up;
        currentFallingSpeed = fallingSpeed;

        if (canWalkWalls)
        {
            canChangeWalls = true;
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }
        
        UpdateGroundedState();

        if (!isGrounded)
        {
            float currentDownSpeed = Vector3.Dot(rb.velocity, -gravityDirection);
            float lastDownSpeed = Vector3.Dot(lastVelocity, -gravityDirection);
            if (currentDownSpeed > lastDownSpeed)
            {
                lastVelocity = rb.velocity;
            }
        }
        else
        {
            lastVelocity = Vector3.zero;
        }


        UpdateTutorial();

        if (grapplingHook.isGrappling)
        {
            lastVelocity = rb.velocity;
            return;
        }

        Vector3 cameraFlat = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up).normalized;
        if (cameraFlat != Vector3.zero)
        {
            orientation.rotation = Quaternion.LookRotation(cameraFlat);
        }
            
        UpdateWeaponSpeeds();

        ungrappleTimeElapsed = grapplingHook.ungrappleTimeElapsed.ElapsedMilliseconds;

        if (ungrappleTimeElapsed < ungrappleJumpTime)
        {
            if (!ungrappleJumpAvailable && !ungrappleJumpUsed)
            {
                Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };
                foreach (Vector3 direction in directions)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, direction, out hit, 2f, LayerMask.GetMask("Obstacle")))
                    {
                        ungrappleJumpAvailable = true;
                        break;
                    }
                }
            }
        }
        else
        {
            ungrappleJumpAvailable = false;
            ungrappleJumpUsed = false; 
        }


        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        rb.velocity = ClampMag(rb.velocity, maximumPlayerSpeed);

        if (!movementEnabled)
        {
            return;
        }

        UpdateFieldOfView();

        if (canWalkWalls)
        {
            rb.useGravity = false;
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, gravityDirection);
            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, gravityDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            if (canChangeWalls)
            {
                WallChange();
            }
        }

        if (isGrounded)
        {
            currentFallingSpeed = fallingSpeed;
            UpdateGroundMovement();
        }
        else
        {
            UpdateAirMovement();
        }
    }

    private void UpdateWeaponSpeeds()
    {
        if (shoot.equippedWeapon == Shooting.EquippedWeapon.Pistol)
        {
            walkSpeed = 20.0f;
            runSpeed = 35.0f;
        }
        else if (shoot.equippedWeapon == Shooting.EquippedWeapon.Rifle)
        {
            walkSpeed = 10.0f;
            runSpeed = 18.0f;
        }
    }

    private void UpdateTutorial()
    {
        if (!tutorialCheck)
        {
            return;
        }

        switch (tutorial.currentStep)
        {
            case 1:
                if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f)
                {
                    tutorial.currentStep = 2;
                }
                break;
            case 2:
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    tutorial.currentStep = 3;
                }
                break;
            case 3:
                if (Input.GetButton("Jump"))
                {
                    tutorial.currentStep = 4;
                }
                break;
            case 4:
                if (grapplingHook.IsGrappling)
                {
                    tutorial.currentStep = 5;
                }
                break;
            case 5:
                if (swingHook.IsSwinging)
                {
                    tutorial.currentStep = 6;
                }
                break;

        }
    }

    private void UpdateGroundedState()
    {   
        if (wallRun != null && wallRun.IsWallRunning)
        {
            isGrounded = false;
            currentFallingSpeed = fallingSpeed;
            wasGrounded = false;
            return;
        }

        if (swingHook != null && swingHook.IsSwinging)
        {
            isGrounded = false;
            currentFallingSpeed = fallingSpeed;
            return;
        }

        RaycastHit hit;
        if (Physics.SphereCast(groundChecker.position + (gravityDirection * 0.5f), 0.5f, -gravityDirection, out hit, 0.55f))
        {
            if (!wasGrounded && (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")))
            {
                if (canTakeDamage)
                {
                    ApplyFallDamage();
                }
                playerSource.PlayOneShot(landClip);
                Invoke("UnblockJump", jumpCooldown);
            }

            isGrounded = true;
            wasGrounded = true;
        }
        else
        {
            isGrounded = false;
            wasGrounded = false;
        }
    }

    private void ApplyFallDamage()
    {
        if (health == null)
        {
            health = Health.PlayerInstance;
        }

        float downwardSpeed = -Vector3.Dot(lastVelocity, gravityDirection);

        if (downwardSpeed < minFallSpeed)
        {
            return;
        }

        if (downwardSpeed >= lethalFallSpeed)
        {
            health.TakeDamage(health.currentHealth);
            return;
        }

        float t = Mathf.InverseLerp(minFallSpeed, lethalFallSpeed, downwardSpeed);
        float damage = Mathf.Round(t * maxFallDamage);
        health.TakeDamage(damage);
    }

    private void UpdateFieldOfView()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (shoot.equippedWeapon == Shooting.EquippedWeapon.Pistol)
            {
                playerCameraComponent.fieldOfView = Mathf.Lerp(playerCameraComponent.fieldOfView, pistolRunFOV, Time.deltaTime * 5);
            }
            else
            {
                playerCameraComponent.fieldOfView = Mathf.Lerp(playerCameraComponent.fieldOfView, rifleRunFOV, Time.deltaTime * 5);
            }
        }
        else if (rb.velocity.magnitude > 50f)
        {
            playerCameraComponent.fieldOfView = Mathf.Lerp(playerCameraComponent.fieldOfView, fastFOV, Time.deltaTime * 5);
        }
        else
        {
            playerCameraComponent.fieldOfView = Mathf.Lerp(playerCameraComponent.fieldOfView, idleFOV, Time.deltaTime * 5);
        }
    }

    private void UpdateGroundMovement()
    {
        if (Input.GetButton("Jump") && (!jumpBlocked || ungrappleJumpAvailable))
        {
            rb.AddForce(jumpForce * gravityDirection, ForceMode.Impulse);
            jumpBlocked = true;
            ungrappleJumpAvailable = false;
            ungrappleJumpUsed = true;
            playerSource.PlayOneShot(jumpClip);
            stepTimer = 0f;                    
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 wallForward = transform.forward;
        Vector3 wallRight = transform.right;
        Vector3 targetMoveDirection = (wallForward * verticalInput + wallRight * horizontalInput).normalized;

        rb.AddForce((targetMoveDirection * currentSpeed - rb.velocity) * accelerationSpeed, ForceMode.Acceleration);

        if (targetMoveDirection.magnitude > 0.1f)
        {
            stepTimer -= Time.fixedDeltaTime;
            if (stepTimer <= 0f)
            {
                playerSource.PlayOneShot(stepClip);
                stepTimer = isRunning ? stepIntervalRun : stepIntervalWalk;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void UpdateAirMovement()
    {
        if (Input.GetButton("Jump") && ungrappleJumpAvailable)
        {
            rb.AddForce(jumpForce * gravityDirection * grappleJumpMultiplier, ForceMode.Impulse);
            lastVelocity = Vector3.zero;
            ungrappleJumpAvailable = false;
            ungrappleJumpUsed = true;
        }

        Vector3 airForward = Vector3.ProjectOnPlane(playerCamera.forward, gravityDirection).normalized;
        Vector3 airRight = Vector3.ProjectOnPlane(playerCamera.right, gravityDirection).normalized;
        Vector3 relativeInput = (airForward * verticalInput + airRight * horizontalInput).normalized;
        Vector3 lateralVelocity = Vector3.ProjectOnPlane(rb.velocity, gravityDirection);

        if (!swingHook.isSwinging)
        {
            currentFallingSpeed *= fallMultiplier;
        }

        if (lateralVelocity.magnitude < maximumPlayerSpeed)
        {
            rb.AddForce(relativeInput * (walkSpeed * 2f), ForceMode.Acceleration);
        }

        rb.AddForce(-gravityDirection * currentFallingSpeed, ForceMode.Acceleration);
    }

    private void UnblockJump()
    {
        jumpBlocked = false;
    }

    public void EnableMovement()
    {
        movementEnabled = true;
    }

    public void DisableMovement()
    {
        movementEnabled = false;
    }

    public void WallChange()
    {
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };
        wallDistances = new List<float>();
        wallNormals = new List<Vector3>();

        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, 1f, LayerMask.GetMask("Obstacle")))
            {
                wallDistances.Add(hit.distance);
                wallNormals.Add(hit.normal);
            }
        }

        if (wallDistances.Count > 0)
        {
            float smallestDistance = wallDistances[0];
            int smallestIndex = 0;

            for (int index = 1; index < wallDistances.Count; index++)
            {
                if (wallDistances[index] < smallestDistance)
                {
                    smallestDistance = wallDistances[index];
                    smallestIndex = index;
                }
            }

            gravityDirection = wallNormals[smallestIndex];
            canChangeWalls = false;
            StartCoroutine(WallChangeCooldown());
        }
    }

    private IEnumerator WallChangeCooldown()
    {
        yield return new WaitForSeconds(1f);
        canChangeWalls = true;
    }

    private static Vector3 ClampSqrMag(Vector3 vector, float maxSqrMagnitude)
    {
        if (vector.sqrMagnitude > maxSqrMagnitude)
        {
            vector = vector.normalized * Mathf.Sqrt(maxSqrMagnitude);
        }

        return vector;
    }

    private static Vector3 ClampMag(Vector3 vector, float maxMagnitude)
    {
        if (vector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            vector = vector.normalized * maxMagnitude;
        }

        return vector;
    }
}  