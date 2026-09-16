using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    public bool playerHasTouched = false;
    public float speed;
    public float end;
    public float start;
 
    void Update()
    {
        if (playerHasTouched) {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (transform.position.z <= start) {
            speed *= -1;
        }
        else if (transform.position.z >= end) {
            speed *= -1;
        }
    }
    public void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerHasTouched = true;
        }
    }
}
