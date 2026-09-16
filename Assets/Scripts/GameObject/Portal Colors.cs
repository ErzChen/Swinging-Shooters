using UnityEngine;

public class PortalColors : MonoBehaviour
{
    public bool fake;
    public bool mainMenu;
    private Color[] targetColors = new Color[3];
    private float transitionSpeed = 1.0f;
    private Renderer meshRenderer;
    private int currentColorIndex = 0;
    private int targetColorIndex = 1;
    private float transitionProgress = 0f;

    void Awake()
    {
        if (fake) 
        {
            Color color1;
            ColorUtility.TryParseHtmlString("#FF000080", out color1);
            targetColors[0] = color1;

            Color color2;
            ColorUtility.TryParseHtmlString("#FF000080", out color2); 
            targetColors[1] = color2;

            Color color3;
            ColorUtility.TryParseHtmlString("#FF000080", out color3); 
            targetColors[2] = color3;
        }
        else if (mainMenu)
        {
            Color color1;
            ColorUtility.TryParseHtmlString("#00e0c680", out color1);
            targetColors[0] = color1;

            Color color2;
            ColorUtility.TryParseHtmlString("#047ccc80", out color2); 
            targetColors[1] = color2;

            Color color3;
            ColorUtility.TryParseHtmlString("#0029e080", out color3); 
            targetColors[2] = color3;
        }
        else
        {
            Color color1;
            ColorUtility.TryParseHtmlString("#ca00e080", out color1);
            targetColors[0] = color1;

            Color color2;
            ColorUtility.TryParseHtmlString("#a404cc80", out color2); 
            targetColors[1] = color2;

            Color color3;
            ColorUtility.TryParseHtmlString("#4700e080", out color3); 
            targetColors[2] = color3;
        }
    }

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        meshRenderer.material.color = targetColors[currentColorIndex];
    }


    void Update()
    {
        transitionProgress += Time.deltaTime / transitionSpeed;
        meshRenderer.material.color = Color.Lerp(targetColors[currentColorIndex], targetColors[targetColorIndex], transitionProgress);
        if (transitionProgress >= 1f)
        {
            transitionProgress = 0f; 

            currentColorIndex = targetColorIndex;
            targetColorIndex = (targetColorIndex + 1) % targetColors.Length; 
        }
    }
}
