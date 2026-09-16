using UnityEngine;

public class WeakPointSpin : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.forward * 200f * Time.deltaTime);
    }
}
