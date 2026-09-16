using TMPro;
using UnityEngine;

public class CreditsRoll : MonoBehaviour
{
    public float speed = 100f;
    public RectTransform containerCanvas;

    private TextMeshProUGUI textComponent;
    private RectTransform rectTransform;
    private float startY;
    private float endY;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TextMeshProUGUI>();

        float canvasHeight = containerCanvas.rect.height;
        float textHeight = textComponent.preferredHeight;
        startY = -canvasHeight;
        endY = canvasHeight + textHeight;

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (rectTransform.anchoredPosition.y < endY)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y += speed * Time.deltaTime; 
            rectTransform.anchoredPosition = pos;
        }
    }
}