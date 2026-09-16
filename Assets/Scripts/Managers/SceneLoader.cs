using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public bool isActive;
    public int currentLevel = 0;
    public GameObject loading;
    private PauseGame pauseGame;
    private GameManager gameManager;

    void Start()
    {
        pauseGame = PauseGame.Instance;
        gameManager = GameManager.Instance;
    }

    public void Scene(string scene)
    {
        StartCoroutine(LoadScene(scene));
    }

    private IEnumerator LoadScene(string scene)
    {
        if (pauseGame == null) 
        {
            pauseGame = PauseGame.Instance;
        }
        if (gameManager == null) 
        {
            gameManager = GameManager.Instance;
        }

        string[] parts = scene.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[1], out int levelIndex))
        {
            gameManager.AddLevelAttempt(levelIndex);
        }

        pauseGame.PauseBackgroundWithoutScreen(true);
        loading.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        asyncLoad.allowSceneActivation = false;
        yield return new WaitForSecondsRealtime(2f);
        asyncLoad.allowSceneActivation = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && isActive)
        {
            if (gameObject.tag == "TutPortal")
            {
                Scene("Level " + (currentLevel + 1).ToString());
            }
            else if (gameObject.tag == "MainPortal")
            {
                Scene("Home");
            }
        }
    }

    public void TryAgain()
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().name));
    }
}