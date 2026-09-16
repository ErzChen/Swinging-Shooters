using UnityEngine;

public class MechFootsteps : MonoBehaviour
{
    private MechMovement mechMovement;

    void Start()
    {
        mechMovement = GetComponentInParent<MechMovement>();
    }

    public void OnFootstep()
    {
        mechMovement.OnFootstep();
    }
}
