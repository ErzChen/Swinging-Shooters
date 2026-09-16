using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Attempts : MonoBehaviour
{
    private int attempts;
    private TextMeshProUGUI text;
    private GameManager gameManager;

    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        gameManager = GameManager.Instance;
        string[] parts = SceneManager.GetActiveScene().name.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[1], out int levelIndex))
        {
            attempts = gameManager.levelAttempts[levelIndex - 1];
        }
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        text.text = "Attempt " + attempts;
    }
}
