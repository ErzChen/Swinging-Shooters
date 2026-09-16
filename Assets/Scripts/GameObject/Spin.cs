using UnityEngine;

public class Spin : MonoBehaviour
{
    public bool isMissle;

    void Update()
    {
        if (isMissle) 
        {
            gameObject.transform.Rotate(0, 1f, 0);
        }
        else
        {
            gameObject.transform.Rotate(0, 0, 1f);
        }
    }
}
