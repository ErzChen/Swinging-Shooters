using UnityEngine;

public class LevitatePlayer : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    public float liftForce = 60f;
    public float playerFallingSpeed;
    void Start()
    {
        PlayerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerFallingSpeed = PlayerMovement.fallingSpeed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement.fallingSpeed = 0;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement.fallingSpeed = playerFallingSpeed;
            PlayerMovement.currentFallingSpeed = playerFallingSpeed;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddForce(Vector3.up * liftForce, ForceMode.Acceleration);
            }
        }
    }
}
