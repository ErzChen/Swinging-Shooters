using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Sensitivity : MonoBehaviour 
{
    public TextMeshProUGUI count; 
    private Slider slider;
    private GameManager gameManager;

    void Start() 
    {
        slider = GetComponent<Slider>();
        gameManager = GameManager.Instance;
        slider.value = gameManager.sensitivity;
        count.text = slider.value.ToString();
        slider.onValueChanged.AddListener(OnSliderChanged); 
    }

    void OnSliderChanged(float value)
    {
        gameManager.sensitivity = value;
        count.text = value.ToString("F1");
    }
}
