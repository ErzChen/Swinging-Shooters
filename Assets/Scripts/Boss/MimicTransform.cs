using UnityEngine;

public class MimicTransform : MonoBehaviour
{
    public Transform mimicTransform;
    public bool isFollowing = false;
    public float positionSmoothSpeed = 0f;
    public Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (mimicTransform == null)
        {
            return;
        }

        if (positionSmoothSpeed > 0f)
        {
            transform.position = Vector3.Lerp(transform.position, mimicTransform.position, Time.deltaTime * positionSmoothSpeed);
        }
        else
        {
            transform.position = mimicTransform.position;
        }

        if (isFollowing)
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = mimicTransform.rotation;
        }

        if (transform.parent == mimicTransform.parent)
        {
            transform.localScale = mimicTransform.localScale;
        }
    }
}