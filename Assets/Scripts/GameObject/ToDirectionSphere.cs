using UnityEngine;
using System.Collections.Generic;

public class ToDirectionSphere : MonoBehaviour 
{
    public float radius = 1f;
    private GameObject[] targets;
    private Transform playerTransform;
    private Camera playerCamera;
    private Renderer sphereRenderer;
    private Dictionary<GameObject, TargetMovement> targetComponents = new Dictionary<GameObject, TargetMovement>();

    void Start() 
    {
        sphereRenderer = GetComponent<Renderer>();
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) 
        {
            playerTransform = playerObj.transform.Find("Main Camera");
            if (playerTransform != null) 
            {
                playerCamera = playerTransform.GetComponent<Camera>();
            }
        }
        RefreshTargets();
    }

    void Update() 
    {
        if (Time.frameCount % 30 == 0) 
        {
            RefreshTargets();
        }
        UpdatePosition();
    }

    void RefreshTargets()
    {
        targets = GameObject.FindGameObjectsWithTag("Target");
        targetComponents.Clear();
        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                TargetMovement movement = target.GetComponentInChildren<TargetMovement>();
                if (movement != null)
                {
                    targetComponents[target] = movement;
                }
            }
        }
    }

    void UpdatePosition() 
    {
        if (targets == null || targets.Length == 0 || playerTransform == null || playerCamera == null) 
        {
            SetSphereVisibility(false);
            return;
        }

        float closestDist = float.MaxValue;
        Transform closestTarget = null;

        foreach (GameObject target in targets) 
        {
            if (target == null) continue;

            if (!targetComponents.TryGetValue(target, out TargetMovement movement))
            {
                movement = target.GetComponent<TargetMovement>();
                if (movement != null) targetComponents[target] = movement;
            }

            if (movement == null || movement.isHit) 
            {
                continue; 
            }

            float dist = Vector3.Distance(playerTransform.position, target.transform.position);
            if (dist < closestDist) 
            {
                closestDist = dist;
                closestTarget = target.transform;
            }
        }
        if (closestTarget == null)
        {
            SetSphereVisibility(false);
            return;
        }

        SetSphereVisibility(true);

        Vector3 targetPos = closestTarget.position;
        Vector3 viewportPos = playerCamera.WorldToViewportPoint(targetPos);
        
        bool isBehindCamera = viewportPos.z < 0;
        bool isOffScreen = viewportPos.x < 0f || viewportPos.x > 1f || viewportPos.y < 0f || viewportPos.y > 1f;

        if (isBehindCamera || isOffScreen) 
        {
            if (isBehindCamera) 
            {
                viewportPos.x = 1f - viewportPos.x;
                viewportPos.y = 1f - viewportPos.y;
            }

            Vector2 shiftedPos = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);
            float maxDimension = Mathf.Max(Mathf.Abs(shiftedPos.x), Mathf.Abs(shiftedPos.y));
            
            if (maxDimension > 0) 
            {
                shiftedPos /= (maxDimension * 2f);
            }

            viewportPos.x = shiftedPos.x + 0.5f;
            viewportPos.y = shiftedPos.y + 0.5f;
            viewportPos.z = playerCamera.nearClipPlane + 0.1f;

            Vector3 edgeWorldPos = playerCamera.ViewportToWorldPoint(viewportPos);
            Vector3 edgeDirection = (edgeWorldPos - playerTransform.position).normalized;
            transform.position = playerTransform.position + edgeDirection * radius;
        } 
        else 
        {
            Vector3 direction = (targetPos - playerTransform.position).normalized;
            transform.position = playerTransform.position + direction * radius;
        }
    }

    void SetSphereVisibility(bool visible)
    {
        if (sphereRenderer != null && sphereRenderer.enabled != visible)
        {
            sphereRenderer.enabled = visible;
        }
    }
}
