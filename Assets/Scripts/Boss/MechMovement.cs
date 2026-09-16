using System.Collections;
using UnityEngine;

public class MechMovement : MonoBehaviour
{
    [Header("Movement")]
    public WeakPointDisplay leftLeg;
    public WeakPointDisplay rightLeg;
    public Transform anchorPoint;
    public Transform rootTransform;
    public enum MoveState { Sleep, Wake, Idle, Walk, WalkBack, WalkRight, WalkLeft, Run, RunJump }
    public MoveState currentState;
    public bool canMove = false;
    public float movementRadius;
    public float movementDistance;
    public float distance;
    public float idealDistance = 100f;
    public float idealDistanceTolerance = 25f;
    public float runJumpDuration = 5f;
    public float deadzone = 10f;

    [Header("Sound")]
    public AudioSource movementSource;
    
    private MechFollow mechFollow;
    private MechHealth mechHealth;
    private MechShoot mechShoot;
    private Animator animator;
    private Transform playerTransform;
    private Collider rushCollider;
    private Coroutine restoreWeakPointCoroutine;
    private float currentSpeed;
    private float runJumpTimer = 0f;
    private bool waitingForWakeComplete = false;
    private Coroutine wakeTimerCoroutine;

    void Start()
    {
        mechFollow = GetComponent<MechFollow>();
        mechHealth = GetComponent<MechHealth>();
        mechShoot = GetComponent<MechShoot>();
        animator = GetComponentInChildren<Animator>();
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        rushCollider = GetComponent<BoxCollider>();
        SetState(MoveState.Wake);
    }

    void Update()
    {
        if (Time.timeScale == 0f) 
        {
            return;
        }

        if (waitingForWakeComplete)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("SleepToDefault") && info.normalizedTime >= 1f)
            {
                waitingForWakeComplete = false;
                OnWakeComplete();
            }
        }

        if (!leftLeg.isActive && !rightLeg.isActive)
        {
            if (currentState != MoveState.Sleep && !waitingForWakeComplete)
            {
                rootTransform.localPosition = Vector3.zero;
                SetState(MoveState.Sleep);
            }
            return; 
        }

        if (!canMove || mechHealth.isDestroyed || waitingForWakeComplete)
        {
            return;
        }

        if (leftLeg.isFixed ^ rightLeg.isFixed)
        {
            canMove = false;
            CancelInvoke("Rush");          
            mechFollow.canFollow = true;  
            mechShoot.canShoot = true;
            StartCoroutine(RestoreCooldown()); 
            SetState(MoveState.Idle);
            return;
        }

        movementDistance = Vector3.Distance(transform.position, anchorPoint.position);
        distance = Vector3.Distance(transform.position, playerTransform.position);

        if ((movementDistance >= movementRadius || distance < 25f) && currentState != MoveState.RunJump)
        {
            SetState(MoveState.Idle);
            mechFollow.rotationDirection = 90f;
            canMove = false;
            CancelInvoke("Rush");
            Invoke("Rush", 3f);
            return;
        }

        if (runJumpTimer > 0f)
        {
            runJumpTimer -= Time.deltaTime;
            transform.position += transform.right * currentSpeed * Time.deltaTime;
            return;
        }
        else if (currentState == MoveState.RunJump)
        {
            rushCollider.enabled = false;
            mechFollow.canFollow = true;
            mechShoot.canShoot = true;
            SetState(MoveState.Idle);
        }

        UpdateStates();

        if (currentState == MoveState.WalkRight)
        {
            transform.RotateAround(playerTransform.position, Vector3.up, -currentSpeed * Time.deltaTime);
        }
        else if (currentState == MoveState.WalkLeft)
        {
            transform.RotateAround(playerTransform.position, Vector3.up, currentSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += transform.right * currentSpeed * Time.deltaTime;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && currentState == MoveState.RunJump && canMove)
        {
            Health health = other.GetComponent<Health>();
            health.TakeDamage(1);
        }
    }

    public void OnFootstep()
    {
        if (currentState == MoveState.Sleep || currentState == MoveState.Wake || currentState == MoveState.Idle)
        {    
            return;
        }

        movementSource.PlayOneShot(movementSource.clip);
    }

    void Rush()
    {
        Vector3 predictedPosition = transform.position + transform.right * 30f * runJumpDuration;
        if (Vector3.Distance(predictedPosition, anchorPoint.position) > movementRadius)
        {
            canMove = true;
            return;
        }

        canMove = true;
        runJumpTimer = runJumpDuration;
        rushCollider.enabled = true;
        mechFollow.canFollow = false;
        mechShoot.canShoot = false;
        SetState(MoveState.RunJump);
    }

    void UpdateStates()
    {
        float walkRightExitDistance = currentState == MoveState.WalkRight 
            ? idealDistance - deadzone 
            : idealDistance;

        float walkExitDistance = currentState == MoveState.Walk
            ? idealDistance + idealDistanceTolerance - deadzone
            : idealDistance + idealDistanceTolerance;

        if (distance > idealDistance + idealDistanceTolerance + 100f && currentState != MoveState.Run)
        {
            SetState(MoveState.Run);
        }
        else if ((distance > walkExitDistance
            || (currentState == MoveState.Run && distance > idealDistance + idealDistanceTolerance - deadzone))
            && currentState != MoveState.Walk)
        {
            SetState(MoveState.Walk);
        }
        else if (distance > walkRightExitDistance 
            && currentState != MoveState.WalkRight
            && currentState != MoveState.Walk   
            && currentState != MoveState.Run)
        {
            SetState(MoveState.WalkRight);
        }
        else if (distance <= walkRightExitDistance && currentState != MoveState.WalkBack)
        {
            SetState(MoveState.WalkBack);
        }
    }

    public void SetState(MoveState newState)
    {
        if (currentState == newState) 
        {
            return; 
        }

        if (waitingForWakeComplete && newState != MoveState.Sleep) 
        {
            return;
        }

        currentState = newState;
        PlayAnimationForState(newState);

        switch (newState)
        {
            case MoveState.Sleep:     
                StartSleep();            
                break;
            case MoveState.Wake:      
                StartWake();             
                break;
            case MoveState.Idle:      
                StartIdling();           
                break;
            case MoveState.Walk:      
                StartWalking();          
                break;
            case MoveState.WalkBack:  
                StartWalkingBackward();  
                break;
            case MoveState.WalkRight: 
                StartWalkingRight();     
                break;
            case MoveState.WalkLeft:  
                StartWalkingLeft();      
                break;
            case MoveState.Run:       
                StartRunning();          
                break;
            case MoveState.RunJump:   
                StartRunJumping();       
                break;
        }
    }

    void PlayAnimationForState(MoveState state)
    {
        animator.ResetTrigger("Sleep");
        animator.ResetTrigger("Wake");
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("WalkBack");
        animator.ResetTrigger("Run");
        animator.ResetTrigger("Jump");

        switch (state)
        {
            case MoveState.Sleep:     
                animator.SetTrigger("Sleep");   
                break;
            case MoveState.Wake:      
                animator.SetTrigger("Wake");    
                break;
            case MoveState.Idle:      
                animator.SetTrigger("Idle");    
                break;
            case MoveState.Walk:      
                animator.SetTrigger("Walk");    
                break;
            case MoveState.WalkBack:  
                animator.SetTrigger("WalkBack"); 
                break;
            case MoveState.WalkRight: 
                animator.SetTrigger("Walk");    
                break;
            case MoveState.WalkLeft:  
                animator.SetTrigger("Walk");    
                break;
            case MoveState.Run:       
                animator.SetTrigger("Run");     
                break;
            case MoveState.RunJump:   
                animator.SetTrigger("Jump");    
                break;
        }
    }


    void StartSleep()
    {
        if (restoreWeakPointCoroutine != null)
        {
            StopCoroutine(restoreWeakPointCoroutine);
            restoreWeakPointCoroutine = null;
        }

        if (wakeTimerCoroutine != null)
        {
            StopCoroutine(wakeTimerCoroutine);
            wakeTimerCoroutine = null;
        }

        mechFollow.rotationDirection = 90f;
        currentSpeed = 0f;
        canMove = false;
        mechFollow.canFollow = false;
        mechShoot.canShoot = false;
        runJumpTimer = 0f;
        rushCollider.enabled = false;
        CancelInvoke("Rush");

        if (!mechHealth.isDestroyed)
        {
            wakeTimerCoroutine = StartCoroutine(WakeTimer());
        }

        leftLeg.StopCoroutine(nameof(WeakPointDisplay.RestoreCooldown));
        rightLeg.StopCoroutine(nameof(WeakPointDisplay.RestoreCooldown));
        leftLeg.isFixed = false;
        rightLeg.isFixed = false;
    }

    void StartWake()
    {
        if (restoreWeakPointCoroutine != null)
        {
            StopCoroutine(restoreWeakPointCoroutine);
            restoreWeakPointCoroutine = null;
        }

        mechFollow.rotationDirection = 90f;
        leftLeg.isActive = true;
        leftLeg.isFixed = true;
        rightLeg.isActive = true;
        rightLeg.isFixed = true;
        restoreWeakPointCoroutine = StartCoroutine(RestoreWeakPoints());
        waitingForWakeComplete = true;
    }

    IEnumerator BeginWakePolling()
    {
        yield return null;
        waitingForWakeComplete = true;
    }

    public void OnWakeComplete()
    {
        mechFollow.canFollow = true;
        mechShoot.canShoot = true;
        mechHealth.canTakeDamage = true;
        canMove = true;
    }

    void StartIdling()
    {
        currentSpeed = 0f;
        animator.SetTrigger("Idle");
        mechFollow.rotationDirection = 90f;
    }

    void StartWalking()
    {
        currentSpeed = 10;
        animator.SetTrigger("Walk");
        mechFollow.rotationDirection = 90f;
    }

    void StartWalkingBackward()
    {
        currentSpeed = -20;
        animator.SetTrigger("WalkBack");
        mechFollow.rotationDirection = 90f;
    }

    void StartWalkingRight()
    {
        currentSpeed = 10f;
        animator.SetTrigger("Walk");
        mechFollow.rotationDirection = 180f;
    }

    void StartWalkingLeft()
    {
        currentSpeed = 10f;
        animator.SetTrigger("Walk");
        mechFollow.rotationDirection = 0f;
    }

    void StartRunning()
    {
        currentSpeed = 20f;
        animator.SetTrigger("Run");
        mechFollow.rotationDirection = 90f;
    }

    void StartRunJumping()
    {
        currentSpeed = 30f;
        animator.SetTrigger("Jump");
        mechFollow.rotationDirection = 90f;
    }

    IEnumerator RestoreWeakPoints()
    {
        yield return new WaitForSeconds(30f);
        leftLeg.RestoreDisplay();
        rightLeg.RestoreDisplay();
    }

    IEnumerator WakeTimer()
    {
        yield return new WaitForSeconds(5f);
        wakeTimerCoroutine = null;
        SetState(MoveState.Wake);
    }

    IEnumerator RestoreCooldown()
    {
        yield return new WaitForSeconds(leftLeg.restoreDuration);
        canMove = true;
    }
}
