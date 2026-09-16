using System.Collections;
using UnityEngine;
using System.Diagnostics;

public class GrapplingHook : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float maxGrappleDistance = 100.0f;
    public float maximumSpeed = 100.0f;
    public float minimumSpeed = 50.0f;
    public float distanceToStop = 2.0f;
    public float grappleCooldown = 30.0f;
    public LayerMask layerMask;

    [Header("Deceleration")]
    public float deceleration = 2500.0f;
    public float deceleratingTime = 1.4f;

    [Header("Rope")]
    public Rope rope;
    public Transform ropeStart;

    [Header("Crosshair")]
    public RectTransform crosshairSpinningPart;
    public float crosshairSpinSpeed = 200.0f;

    [Header("State")]
    public bool isGrappling = false;
    private bool isBlocked = false;

    [Header("Timer")]
    public Stopwatch ungrappleTimeElapsed;

    [Header("Sound")]
    public AudioSource grappleSource;

    private PlayerMovement PlayerMovement;
    private SwingHook SwingHook;
    private Coroutine decelerateCoroutine;
    private Rigidbody rigidBody;
    private Transform grapplePoint;
    private RaycastHit raycastHit;
    private string firstPassHitName;
    private int baseSegmentCount;
    private Vector3 grappleDirection;
    private float targetDistance;
    private float decelerateTimer = 0.0f;
    private float decelerateDuration;
    private float currentSpeed;

    public bool IsGrappling
    {
        get { return isGrappling; }
    }

    private void Start()
    {
        layerMask = ~LayerMask.GetMask("Ignore Raycast");
        baseSegmentCount = rope.segmentCount;
        SwingHook = this.GetComponent<SwingHook>();
        PlayerMovement = this.GetComponent<PlayerMovement>();
        rigidBody = this.GetComponent<Rigidbody>();
        crosshairSpinningPart = GameObject.Find("Crosshair").transform.Find("SpinningPieces").GetComponent<RectTransform>();
        ungrappleTimeElapsed = new System.Diagnostics.Stopwatch();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) 
        {
            UnGrapple();
            return;
        }
        
        UpdateCrosshair();
        UpdateGrappleInput();
    }

    private void UpdateCrosshair()
    {
        if (crosshairSpinningPart == null) return;

        if (Physics.Raycast(PlayerCamera.cam.transform.position, PlayerCamera.cam.transform.forward, out raycastHit, maxGrappleDistance, layerMask)
            && raycastHit.collider.CompareTag("Obstacle"))
        {
            crosshairSpinningPart.gameObject.SetActive(true);
            crosshairSpinningPart.Rotate(Vector3.forward * crosshairSpinSpeed * Time.deltaTime);
        }
        else
        {
            crosshairSpinningPart.gameObject.SetActive(false);
        }
    }

    private void UpdateGrappleInput()
    {
        if (!isGrappling && !SwingHook.isSwinging)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentSpeed = minimumSpeed;
                Grapple();
            }
        }
        else
        {
            if (!Input.GetKey(KeyCode.E))
            {
                if (isGrappling)
                {
                    ungrappleTimeElapsed.Restart();
                }
                UnGrapple();
            }

            GrappleUpdate();
        }
    }

    public void Grapple()
    {
        if (isBlocked) return;

        if (Physics.Raycast(PlayerCamera.cam.transform.position, PlayerCamera.cam.transform.forward, out raycastHit, maxGrappleDistance, layerMask)
            && raycastHit.collider.CompareTag("Obstacle"))
        {
            grapplePoint = new GameObject().transform;
            grapplePoint.position = raycastHit.point;
            grapplePoint.parent = raycastHit.collider.transform;

            if (decelerateCoroutine != null)
            {
                StopCoroutine(decelerateCoroutine);
                decelerateCoroutine = null;
                decelerateTimer = 0.0f;
            }


            PlayerMovement.DisableMovement();
            rope.segmentCount = Mathf.Max(2, (int)((raycastHit.distance / maxGrappleDistance) * baseSegmentCount));
            rope.Grapple(ropeStart.position, raycastHit.point);

            rigidBody.useGravity = false;
            isGrappling = true;
            grappleSource.PlayOneShot(grappleSource.clip);
        }
    }

    public void UnGrapple()
    {
        if (!isGrappling)
        {
            return;
        }

        PlayerMovement.currentFallingSpeed = PlayerMovement.fallingSpeed;

        float capturedDistance = grapplePoint != null 
            ? Vector3.Distance(transform.position, grapplePoint.position) 
            : targetDistance;

        if (grapplePoint != null)
        {
            Destroy(grapplePoint.gameObject);
        }

        if (decelerateTimer == 0.0f)
        {
            decelerateCoroutine = StartCoroutine(Decelerate(capturedDistance));
        }
        else
        {
            decelerateTimer = 0.0f;
        }

        PlayerMovement.EnableMovement();
        rope.UnGrapple();

        isBlocked = true;
        Invoke("UnblockGrapple", grappleCooldown);

        rigidBody.useGravity = true;
        isGrappling = false;
    }

    private void UnblockGrapple()
    {
        isBlocked = false;
    }

    private IEnumerator Decelerate(float capturedDistance)
    {
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        decelerateDuration = deceleratingTime 
            * Mathf.Clamp01(capturedDistance / 10.0f)  
            * Mathf.Clamp01(rigidBody.velocity.magnitude / 30.0f);

        for (; decelerateTimer < decelerateDuration; decelerateTimer += Time.deltaTime)
        {
            rigidBody.AddForce(-rigidBody.velocity.normalized * deceleration * (1.0f - decelerateTimer / decelerateDuration) * Mathf.Clamp01(rigidBody.velocity.sqrMagnitude / 400.0f) * Time.deltaTime, ForceMode.Acceleration);
            yield return waitForEndOfFrame;
        }

        decelerateTimer = 0.0f;
    }

    private void GrappleUpdate()
    {
        if (grapplePoint == null)
        {
            return;
        }

        currentSpeed = Mathf.Clamp(currentSpeed + 0.2f, minimumSpeed, maximumSpeed);

        targetDistance = Vector3.Distance(transform.position, grapplePoint.position);
        rope.segmentCount = (int)((targetDistance / maxGrappleDistance) * baseSegmentCount);
        grappleDirection = (grapplePoint.position - transform.position).normalized;

        rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, grappleDirection * currentSpeed * Mathf.Clamp01(targetDistance / (4.0f * distanceToStop)), Time.deltaTime);

        rope.UpdateStart(ropeStart.position);
        rope.UpdateEnd(grapplePoint.position);
        rope.UpdateGrapple();
    }

    private Vector3 ClampMagnitude(Vector3 vector, float maxMagnitude)
    {
        if (vector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            vector = vector.normalized * maxMagnitude;
        }

        return vector;
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle") && isGrappling)
        {
            transform.parent = other.transform;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            transform.parent = null;
        }
    }
}