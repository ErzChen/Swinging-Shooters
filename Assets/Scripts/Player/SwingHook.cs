using System.Collections;
using UnityEngine;

public class SwingHook : MonoBehaviour
{
    [Header("Swing Settings")]
    public float springStiffness = 4.5f;
    public float springDamping = 7f;
    public float gravityScale = 2.5f;
    public float ropeShortening = 0.92f;
    public float maxSwingDistance = 25f;
    public float swingCooldown = 0.75f;
    public float thrustForce = 300f;
    public LayerMask layerMask;

    [Header("Rope")]
    public Rope rope;
    public Transform ropeStart;
    public Transform orientation;

    [Header("State")]
    public bool isSwinging = false;
    private bool isBlocked = false;

    [Header("Sound")]
    public AudioSource swingSource;

    private GrapplingHook grapplingHook;
    private PlayerMovement playerMovement;
    private Rigidbody rb;
    private Transform swingPoint;
    private RaycastHit raycastHit;
    private SpringJoint joint;
    private int baseSegmentCount;
    private float ropeLength;
    private Coroutine cooldownCoroutine;
    private Vector3 currentGrapplePosition;

    public bool IsSwinging
    {
        get { return isSwinging; }
    }

    private void Start()
    {
        layerMask = ~LayerMask.GetMask("Ignore Raycast");
        baseSegmentCount = rope.segmentCount;
        grapplingHook = this.GetComponent<GrapplingHook>();
        playerMovement = this.GetComponent<PlayerMovement>();
        rb = this.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) 
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            Swing();
        }
        if (Input.GetKeyUp(KeyCode.Q)) 
        {
            UnSwing();
        }

        CheckForSwingPoints();

        if (joint != null) 
        {
            UpdateSwing();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void CheckForSwingPoints()
    {
        if (joint != null) 
        {
            return;
        }

        RaycastHit hit;
        Physics.Raycast(PlayerCamera.cam.transform.position, PlayerCamera.cam.transform.forward, out hit, maxSwingDistance, layerMask);

        Vector3 realHitPoint;

        if (hit.point != Vector3.zero && hit.collider != null 
            && hit.collider.CompareTag("Obstacle"))
        {
            realHitPoint = hit.point;
        }
        else
        {
            realHitPoint = Vector3.zero;
        }
    }

    public void Swing()
    {
        if (Time.timeScale == 0f) 
        {
            UnSwing();
            return;
        }

        if (isBlocked) 
        {
            return;
        }

        if (grapplingHook.isGrappling) 
        {
            return;
        }

        if (!Physics.Raycast(PlayerCamera.cam.transform.position, PlayerCamera.cam.transform.forward, out raycastHit, maxSwingDistance, layerMask))
        {
            return;
        }

        if (!raycastHit.collider.CompareTag("Obstacle"))
        {
            return;
        }

        swingPoint = new GameObject().transform;
        swingPoint.position = raycastHit.point;
        swingPoint.parent = raycastHit.collider.transform;

        isSwinging = true;

        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint.position;

        float distanceFromPoint = Vector3.Distance(transform.position, swingPoint.position);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;
        joint.spring = springStiffness;
        joint.damper = springDamping;
        joint.massScale = gravityScale;

        ropeLength = raycastHit.distance;
        rope.segmentCount = Mathf.Max(2, (int)((raycastHit.distance / maxSwingDistance) * baseSegmentCount));
        rope.Grapple(ropeStart.position, raycastHit.point);

        currentGrapplePosition = ropeStart.position;
        swingSource.PlayOneShot(swingSource.clip);
    }

    public void UnSwing()
    {
        if (!isSwinging) 
        {
            return;
        }

        isSwinging = false;

        rope.UnGrapple();
        playerMovement.lastVelocity = rb.velocity;

        Destroy(joint);
        joint = null;

        if (swingPoint != null)
        {
            Destroy(swingPoint.gameObject);
        }

        isBlocked = true;

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        cooldownCoroutine = StartCoroutine(SwingCooldownRoutine());
    }

    private IEnumerator SwingCooldownRoutine()
    {
        yield return new WaitForSeconds(swingCooldown);
        isBlocked = false;
        cooldownCoroutine = null;
    }

    private void UpdateSwing()
    {
        if (swingPoint != null)
        {
            joint.connectedAnchor = swingPoint.position;
        }

        Vector3 ropeVec = ropeStart.position - swingPoint.position;
        float bottomAlignment = Mathf.Clamp01(Vector3.Dot(ropeVec.normalized, Vector3.down));
        Vector3 tangent = Vector3.Cross(ropeVec, Vector3.Cross(rb.velocity, ropeVec)).normalized;
        float scaledThrust = thrustForce * bottomAlignment;
        Vector3 flatTangent = Vector3.ProjectOnPlane(tangent, Vector3.up);

        rb.AddForce(tangent * scaledThrust * Time.deltaTime);
        if (flatTangent != Vector3.zero)
        {
            orientation.rotation = Quaternion.Slerp(orientation.rotation, Quaternion.LookRotation(flatTangent), Time.deltaTime * 10f);
        }
    }

    private void DrawRope()
    {
        if (joint == null) 
        {
            return;
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint.position, Time.deltaTime * 8f);

        rope.UpdateStart(ropeStart.position);
        rope.UpdateEnd(currentGrapplePosition);
        rope.UpdateGrapple();
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle") && isSwinging)
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