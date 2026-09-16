using System.Collections;
using TMPro;
using UnityEngine;

public class WeakPointDisplay : MonoBehaviour
{
    public GameObject activeInstance;
    public GameObject crosshair;
    public ParticleSystem explosion;
    public bool isActive;
    public bool isFixed;
    public float restoreDuration = 5f;
    private MechMovement mechMovement;
    private Canvas canvas;
    private Camera cam;
    private AudioSource explosionSource;

    void Start()
    {
        isActive = true;
        isFixed = true;
        cam = Camera.main;
        canvas = GameObject.Find("DamageDisplay").GetComponent<Canvas>();
        explosion = transform.Find("Explosion").GetComponentInChildren<ParticleSystem>();
        mechMovement = GetComponentInParent<MechMovement>();
        activeInstance = Instantiate(crosshair, canvas.transform);
        explosionSource = GetComponentInChildren<AudioSource>();
    }

    void Update()
    {
        if (activeInstance != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);

            bool behindCamera = screenPos.z < 0;
            bool offScreen = screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;

            if (behindCamera || offScreen)
            {
                activeInstance.SetActive(false);
            }
            else
            {
                activeInstance.SetActive(true);
                activeInstance.GetComponent<RectTransform>().position = screenPos;
            }
        }
    }

    public void Hit()
    {
        isActive = false;
        isFixed = false;
        explosionSource.PlayOneShot(explosionSource.clip);
        explosion.Play();
        StopCoroutine(nameof(RestoreCooldown));
        StartCoroutine(nameof(RestoreCooldown));
        Destroy(activeInstance);
    }

    public void RestoreDisplay()
    {
        StopCoroutine(nameof(RestoreCooldown));
        
        if (activeInstance != null)
        {
            Destroy(activeInstance);
        }
        isActive = true;
        isFixed = true;
        activeInstance = Instantiate(crosshair, canvas.transform);
    }

    public IEnumerator RestoreCooldown()
    {
        yield return new WaitForSeconds(restoreDuration);
        isFixed = true;
    }
}
