using System.Collections;
using UnityEngine;

public class RifleUnlock : MonoBehaviour
{
    public GameManager gameManager;
    public EnemyCheck check;
    public GameObject canvas;
    private bool shown = true;

    void Start()
    {
        gameManager = GameManager.Instance;
    }
    void Update()
    {
        if (check.levelComplete && shown && !gameManager.rifleUnlocked)
        {
            shown = false;
            gameManager.rifleUnlocked = true;
            StartCoroutine(weaponDisplay());
        }
    }
    public IEnumerator weaponDisplay() {
        canvas.SetActive(true);
        yield return new WaitForSeconds(4f);
        canvas.SetActive(false);
    }
}
