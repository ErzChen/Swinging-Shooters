using UnityEngine;
using UnityEngine.UI;

public class LevelUnlocked : MonoBehaviour
{
    public int levelNumber;
    public GameObject levelNum;
    public GameObject cover;
    public Animator animator;
    private Button button;
    private SceneLoader sceneLoader;

    void Start()
    {
        button = GetComponent<Button>();
        sceneLoader = GameObject.Find("Play").GetComponent<SceneLoader>();
        button.onClick.AddListener(() => sceneLoader.Scene("Level " + levelNumber)); 
        RefreshButton();
    }

    public void RefreshButton() {
        if (GameManager.Instance == null || button == null) return;

        bool unlocked = GameManager.Instance.highestLevel >= (levelNumber - 1); 
        button.interactable = unlocked; 

        if (unlocked) { 
            levelNum.SetActive(true); 
            cover.SetActive(false); 
            animator.enabled = true; 
        } else { 
            levelNum.SetActive(false); 
            cover.SetActive(true); 
            animator.enabled = false; 
        } 
    }
}