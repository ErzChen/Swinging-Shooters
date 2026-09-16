using UnityEngine;
using TMPro;

public class Level4ShotManager : MonoBehaviour
{
    public Shooting shoot;
    public TMP_Text bulletText;
    public Health health;
    
    void Update()
    {
        if (100 - shoot.shotsFired <= 0) {
            health.TakeDamage(100);
        }
        bulletText.text = (100 - shoot.shotsFired).ToString();
    }
}
