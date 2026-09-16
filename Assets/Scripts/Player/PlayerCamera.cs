using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera cam;

    [Header("Camera References")]
    public Transform player;
    public Transform cameraPosition;
    public Health health;
    public GrapplingHook grapplingHook;
    public Shooting shooting;
    public GameObject crosshair;
    public GameObject weapons;

    [Header("Mouse Sensitivity")]
    public float sensitivity = 3;

    [Header("Vertical Look Clamp")]
    public float maxUpAngle = 80;
    public float maxDownAngle = -80;

    [Header("Camera Roll")]
    public float rollAngle = 0.0f;

    [HideInInspector]
    public float mouseX;
    [HideInInspector]
    public float mouseY;

    private GameManager gameManager;
    private Camera cameraComponent;
    private float pitchAngle = 0.0f;

    private void Start()
    {
        cam = this;
        gameManager = GameManager.Instance;
        cameraComponent = GetComponent<Camera>();
        crosshair = GameObject.Find("Crosshair");
    }

    private void Update()
    {
        if (Time.timeScale == 0f) 
        {
            return;
        }
        
        if (!health.isGameOver)
        {
            HandleActiveState();
        }
        else
        {
            HandleDeadState();
        }
    }

    private void HandleActiveState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        sensitivity = gameManager.sensitivity;


        mouseX = Input.GetAxis("Mouse X") * sensitivity;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        pitchAngle -= mouseY;
        pitchAngle = Mathf.Clamp(pitchAngle, maxDownAngle, maxUpAngle);

        player.Rotate(Vector3.up * mouseX, Space.Self);
        transform.rotation = player.rotation * Quaternion.Euler(pitchAngle, 0, rollAngle);
        transform.position = cameraPosition.position;
    }

    private void HandleDeadState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        crosshair.SetActive(false);
        weapons.SetActive(false);
        grapplingHook.enabled = false;
        shooting.enabled = false;
    }
}