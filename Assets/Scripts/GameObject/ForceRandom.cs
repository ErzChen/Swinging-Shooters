using UnityEngine;

public class ForceRandom : MonoBehaviour
{
    public float maxForce = 100f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 randomDirection = Random.onUnitSphere;
        float randomForce = Random.Range(0f, maxForce);
        if (rb != null) {
            rb.AddForce(randomDirection * randomForce, ForceMode.Force);
            rb.AddTorque(randomDirection * randomForce);
        }
    }
}
