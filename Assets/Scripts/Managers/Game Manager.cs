using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int highestLevel;
    public int[] levelAttempts;
    public int totalAttempts;
    public bool rifleUnlocked;
    public float sensitivity = 0.5f;
    public bool audioOn = true;
    public bool gameWon = false;

    private LevelUnlocked[] levelButtons;
    private KeyCode[] unlockSequence = { KeyCode.H, KeyCode.E, KeyCode.L, KeyCode.P };
    private int currentIndex = 0;
    private float lastKeyTime;

    private void Awake()
    {
        Time.timeScale = 1f;
        
        if (highestLevel > 4)
        {
            rifleUnlocked = true;
        }

        if (levelAttempts == null || levelAttempts.Length == 0)
        {
            levelAttempts = new int[15];
        }
        
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "Level 16" || SceneManager.GetActiveScene().name == "Home" || SceneManager.GetActiveScene().name == "Credits")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (SceneManager.GetActiveScene().name == "Home") 
        {
            if (currentIndex > 0 && Time.time - lastKeyTime > 1)
            {
                currentIndex = 0;
            }

            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(unlockSequence[currentIndex]))
                {
                    lastKeyTime = Time.time;
                    currentIndex += 1;
                    if (currentIndex >= unlockSequence.Length)
                    {
                        highestLevel = 15;

                        levelButtons = FindObjectsByType<LevelUnlocked>(FindObjectsSortMode.None);
                        foreach (LevelUnlocked btn in levelButtons) {
                            btn.RefreshButton();
                        }
                    } 
                }
                else
                {
                    currentIndex = 0;
                }
            }
        }
    }

    public void CompleteLevel(int level)
    {
        if (level > highestLevel)
        {
            highestLevel = level;
        }
        if (highestLevel > 4)
        {
            rifleUnlocked = true;
        }
    }

    public void SetAudio(bool on)
    {
        audioOn = on;

        if (PauseGame.Instance == null) 
        {
            return;
        }

        if (on)
        {
            PauseGame.Instance.UnmuteAudio(); 
        }
        else
        {
            PauseGame.Instance.MuteAudio();
        }
    }

    public void AddLevelAttempt(int level)
    {
        int index = level - 1;
        if (index >= 0 && index < levelAttempts.Length && index >= highestLevel)
        {
            levelAttempts[index]++;
        }
        totalAttempts++;
    }
}
