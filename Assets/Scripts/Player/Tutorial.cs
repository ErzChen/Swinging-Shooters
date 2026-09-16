using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text instructionText;

    [Header("Step Tracking")]
    public int currentStep;

    [Header("Cannons")]
    public GameObject cannonCenter;
    public GameObject cannonLeft;
    public GameObject cannonRight;

    [Header("Cannon Status")]
    public bool cannonCenterDestroyed = false;
    public bool cannonLeftDestroyed = false;
    public bool cannonRightDestroyed = false;

    [Header("References")]
    public GameObject startPortal;
    public GameObject mainPortal;
    public PlayerMovement playerMovement;

    private bool sideCannonsActivated = false;

    void Start()
    {
        currentStep = 1;
        playerMovement.tutorialCheck = true;
    }

    void Update()
    {
        switch (currentStep)
        {
            case 1:
                instructionText.text = "Use the keys W (forward), A (left), S (backward), D (right) to move. Moving near a wall enables wall run.";
                break;

            case 2:
                instructionText.text = "Hold left shift to sprint.";
                break;

            case 3:
                instructionText.text = "Nice! Now use the spacebar to jump.";
                break;

            case 4:
                instructionText.text = "Look at an object and press + hold E to grapple (HINT: Make sure the crosshair is spinning before grappling), you can also jump up a wall after grappling.";
                break;

            case 5:
                instructionText.text = "Look at an object and press + hold Q to swing (HINT: Make sure the crosshair is spinning before swinging).";
                break;

            case 6:
                cannonCenter.SetActive(true);
                instructionText.text = "Good job! Look at the cannon in the middle of the map and left click to shoot (HINT: Make sure the crosshair is red before shooting).";
                if (Input.GetMouseButtonDown(0))
                {
                    currentStep = 7;
                }
                break;

            case 7:
                instructionText.text = "Destroy the turrets!";

                if (!sideCannonsActivated)
                {
                    if (cannonLeft != null)
                    {
                        cannonLeft.SetActive(true);
                    }
                    if (cannonRight != null)
                    {
                        cannonRight.SetActive(true);
                    }
                    sideCannonsActivated = true;
                }

                CheckCannonDestroyed(cannonCenter, ref cannonCenterDestroyed);
                CheckCannonDestroyed(cannonLeft, ref cannonLeftDestroyed);
                CheckCannonDestroyed(cannonRight, ref cannonRightDestroyed);

                if (cannonCenterDestroyed && cannonLeftDestroyed && cannonRightDestroyed)
                {
                    currentStep = 8;
                }
                break;

            case 8:
                instructionText.text = "Great! You can now enter the purple portal to begin the first level or the blue portal to return to the main menu. Fall damage will be enabled for levels (HINT: To pause the game click P).";
                startPortal.SetActive(true);
                startPortal.GetComponentInChildren<SceneLoader>().isActive = true;
                mainPortal.SetActive(true);
                mainPortal.GetComponentInChildren<SceneLoader>().isActive = true;
                break;
        }
    }

    private void CheckCannonDestroyed(GameObject cannon, ref bool isDestroyed)
    {
        if (cannon == null)
        {
            return;
        }

        EnemyHealth enemyHealth = cannon.GetComponent<EnemyHealth>();
        if (enemyHealth != null && enemyHealth.isDestroyed)
        {
            isDestroyed = true;
        }
    }
}