using UnityEngine;

public class DamageTextFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 cameraOffset = new Vector3(3f, 3f, 0f);
    private Camera cam;
    private RectTransform rectTransform;

    void Start()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (target == null) 
        {
            return;
        }

        Vector3 worldOffset = cam.transform.TransformDirection(cameraOffset);
        Vector3 screenPos = cam.WorldToScreenPoint(target.position + worldOffset);
        bool behindCamera = screenPos.z < 0;
        bool offScreen = screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;
        if (behindCamera || offScreen)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            rectTransform.position = screenPos;
        }
    }
}