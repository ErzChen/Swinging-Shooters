using UnityEngine;
using UnityEngine.UI;

public class OpenLevelsButton : MonoBehaviour
{
    private Button button;
    public GameObject options;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OpenLevel());
    }

    public void OpenLevel() {
        options.SetActive(true);
    }
}
