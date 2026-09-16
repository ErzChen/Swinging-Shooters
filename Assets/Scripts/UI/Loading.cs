using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Loading : MonoBehaviour
{
    private TextMeshProUGUI text;
    private Image background;

    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        background = GetComponentInChildren<Image>();

        StartCoroutine(FadeImage(background, 0f, 1f, 2f));
        StartCoroutine(FadeText(text, 0f, 1f, 2f));
        StartCoroutine(UpdateLoadingText());
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = image.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; 
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;
    }

    private IEnumerator FadeText(TextMeshProUGUI tmp, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;   
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            tmp.color = color;
            yield return null;
        }

        color.a = endAlpha;
        tmp.color = color;
    }

    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            int dotCount = text.text.Count(c => c == '.');

            if (dotCount >= 3)
                text.text = text.text.Replace(".", "");
            else
                text.text += ".";
        }
    }
}