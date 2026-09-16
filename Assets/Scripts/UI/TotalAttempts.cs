using UnityEngine;
using TMPro;

public class TotalAttempts : MonoBehaviour
{
    private TextMeshProUGUI text;
    private GameManager gameManager;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        gameManager = GameManager.Instance;
        text.text = "Total Attempts: " + (gameManager.totalAttempts - gameManager.levelAttempts.Length);
    }
}
