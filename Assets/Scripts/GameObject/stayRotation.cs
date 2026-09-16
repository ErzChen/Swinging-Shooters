using UnityEngine;

public class stayRotation : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
    }
}
