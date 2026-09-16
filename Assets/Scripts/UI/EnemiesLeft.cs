using TMPro;
using UnityEngine;

public class EnemiesLeft : MonoBehaviour
{

    public int enemyCount;
    private TextMeshProUGUI text;

    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void LoseEnemy()
    {
        if (enemyCount > 0)
        {
            enemyCount -= 1;
            UpdateDisplay();
        }
    }

    public void UpdateDisplay()
    {
        
        text.text = "Enemies Left: " + enemyCount;
    }
}
