using System.Linq;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationCurve effectOverTime;
    // public AnimationCurve waveCurve;
    // public AnimationCurve effectOverDistance;
    public float waveAmplitude = 5f;
    public float scrollSpeed = 5f;
    public float animationSpeed = 1.5f;

    [Header("Rope Settings")]
    public int segmentCount = 100;

    private LineRenderer lineRenderer;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private float animationProgress;
    private bool isActive;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Grapple(Vector3 start, Vector3 end)
    {
        isActive = true;
        animationProgress = 0f;
        startPoint = start;
        endPoint = end;
    }

    public void UnGrapple()
    {
        isActive = false;
        lineRenderer.positionCount = 0;
    }

    public void UpdateStart(Vector3 start) => startPoint = start;

    public void UpdateEnd(Vector3 end) => endPoint = end;

    public void UpdateGrapple()
    {
        lineRenderer.enabled = isActive;
        if (isActive)
        {
            ProcessRopeWave();
        }
    }

    private void ProcessRopeWave()
    {
        Vector3[] points = new Vector3[segmentCount + 1];

        animationProgress = Mathf.MoveTowards(
            animationProgress,
            1f,
            Mathf.Max(
                Mathf.Lerp(animationProgress, 1f, animationSpeed * Time.deltaTime) - animationProgress,
                0.2f * Time.deltaTime
            )
        );

        points[0] = startPoint;

        Quaternion forwardRotation = Quaternion.LookRotation(endPoint - startPoint);
        Vector3 upDirection = forwardRotation * Vector3.up;

        float timeEffect = EvaluateNormalized(effectOverTime, animationProgress);

        for (int i = 1; i <= segmentCount; i++)
        {
            float segmentT = (float)i / segmentCount;
            float waveT = NormalizeWaveT(segmentT * waveAmplitude);
            float scrollT = WrapPositive(waveT + -scrollSpeed * animationProgress);
            // float waveSample = EvaluateNormalized(waveCurve, scrollT);
            // float distanceEffect = EvaluateNormalized(effectOverDistance, segmentT);
            // float displacement = waveSample * distanceEffect * timeEffect;

            points[i] = LerpAlongRope(segmentT) + upDirection * timeEffect;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    private float NormalizeWaveT(float value)
    {
        if (value <= 1f) 
        {
            return value;
        }

        int wholePart = (int)(value - 1f);
        value -= wholePart;
        if (value > 1f) 
        {
            value -= 1f;
        }
        return value;
    }

    private float WrapPositive(float value)
    {
        if (value >= 0f) 
        {
            return value;
        }

        value -= (int)value;
        if (value < 0f)
        {
            value += 1f;
        }
        return value;
    }

    private Vector3 LerpAlongRope(float t)
    {
        return Vector3.Lerp(startPoint, endPoint, t);
    }

    private static float EvaluateNormalized(AnimationCurve curve, float t)
    {
        float maxTime = curve.keys.Select(k => k.time).Max();
        return curve.Evaluate(t * maxTime);
    }
}