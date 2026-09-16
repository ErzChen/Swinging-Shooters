using UnityEngine; 

public class HollowSphere : MonoBehaviour 
{ 
    private SphereCollider sphere; 
    private float radius; 
    private Rigidbody objRB; 
    private Transform myTransform;
    private Transform parentTransform;

    void Start() 
    { 
        myTransform = transform;
        objRB = GetComponent<Rigidbody>(); 
        sphere = GetComponentInParent<SphereCollider>(); 
        parentTransform = sphere.transform;
        radius = sphere.radius; 
    } 

    void FixedUpdate() 
    { 
        Vector3 offset = myTransform.position - parentTransform.position;
        
        if (offset.sqrMagnitude > radius * radius) 
        { 
            Vector3 normal = offset.normalized;
            objRB.velocity = Vector3.Reflect(objRB.velocity, normal); 
            objRB.position = parentTransform.position + normal * radius * 0.99f; 
        } 

        if (objRB.velocity.sqrMagnitude > 100f) 
        { 
            objRB.velocity = Vector3.ClampMagnitude(objRB.velocity, 10f); 
        } 
    } 
}
