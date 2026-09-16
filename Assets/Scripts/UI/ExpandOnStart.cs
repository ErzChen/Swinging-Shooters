using System.Collections;
using UnityEngine;

public class ExpandOnStart : MonoBehaviour
{
    [SerializeField] private float duration = 1.0f;

    void Start()
    {
        StartCoroutine(ScaleOverTime());
    }

    private IEnumerator ScaleOverTime()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration; 
            
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        transform.localScale = endScale; 
    }

    public IEnumerator ShrinkOverTime()
    {
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = endScale;
    }
}
