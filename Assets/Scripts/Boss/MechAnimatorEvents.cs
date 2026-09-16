using UnityEngine;

public class MechAnimatorEvents : MonoBehaviour
{
    private MechMovement mechMovement;

    void Start()
    {
        mechMovement = GetComponentInParent<MechMovement>();
    }

    public void OnWakeComplete()
    {
        mechMovement.OnWakeComplete();
    }
}
