using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    private Button button;
    public GameObject options;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => Exit());
    }

    public void Exit() {
        options.SetActive(false);
    }
}
