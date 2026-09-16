using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public static PauseGame Instance;
    public GameObject pausedGame;
    public GameObject settings;
    public AudioSource uiSource;
    
    private bool gamePaused;
    private bool isLoading;
    public bool audioMuted;
    
    private Rigidbody[] allRigidbodies;
    private Vector3[] savedVelocities;
    private Vector3[] savedAngularVelocities;
    private Animator[] allAnimators;
    private ParticleSystem[] allParticles;
    private bool[] particleWasPlaying;
    private AudioSource[] allAudioSources;
    private bool[] audioWasPlaying;

    void Start()
    {
        Time.timeScale = 1f;
        uiSource = GetComponent<AudioSource>();
        CacheObjects();
        
        if (Instance == null)
        {
            Instance = this;
        }
    
        if (GameManager.Instance != null && !GameManager.Instance.audioOn)
        {
            MuteAudio();
        }
    }

    void CacheObjects()
    {
        allRigidbodies = FindObjectsOfType<Rigidbody>();
        savedVelocities = new Vector3[allRigidbodies.Length];
        savedAngularVelocities = new Vector3[allRigidbodies.Length];
        
        allAnimators = FindObjectsOfType<Animator>();
        
        allParticles = FindObjectsOfType<ParticleSystem>();
        particleWasPlaying = new bool[allParticles.Length];
        
        allAudioSources = FindObjectsOfType<AudioSource>();
        audioWasPlaying = new bool[allAudioSources.Length];
    }

    void Update()
    {
        if (Health.PlayerInstance == null || Health.PlayerInstance.isGameOver)
        {
            return;
        }

        if (gamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (isLoading)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            if (gamePaused)
            {
                UnpauseBackground();
            }
            else
            {
                PauseBackground();
            }
        }
    }

    public void MuteAudio()
    {
        audioMuted = true;
        foreach (AudioSource src in FindObjectsOfType<AudioSource>(true))
        {
            if (src != null) src.mute = true;
        }
    }

    public void UnmuteAudio()
    {
        audioMuted = false;
        foreach (AudioSource src in FindObjectsOfType<AudioSource>(true))
        {
            if (src != null) src.mute = false;
        }
    }

    public void PauseBackgroundWithoutScreen(bool loading = false)
    {
        isLoading = loading;
        Time.timeScale = 0f;

        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            if (allRigidbodies[i] == null) continue;
            
            savedVelocities[i] = allRigidbodies[i].velocity;
            savedAngularVelocities[i] = allRigidbodies[i].angularVelocity;
            allRigidbodies[i].Sleep();
        }

        foreach (Animator anim in allAnimators)
        {
            if (anim != null) anim.speed = 0f;
        }

        for (int i = 0; i < allParticles.Length; i++)
        {
            if (allParticles[i] == null) continue;
            
            particleWasPlaying[i] = allParticles[i].isPlaying;
            if (particleWasPlaying[i])
            {
                allParticles[i].Pause();
            }
        }

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (allAudioSources[i] == null) continue;

            if (allAudioSources[i].CompareTag("UI")) continue;

            if (allAudioSources[i] == uiSource) continue;

            audioWasPlaying[i] = allAudioSources[i].isPlaying;
            if (audioWasPlaying[i])
            {
                allAudioSources[i].Pause();
            }
        }
    }

    public void PauseBackground()
    {
        gamePaused = true;
        if (pausedGame != null)
        {
            pausedGame.SetActive(true);
        }

        uiSource.PlayOneShot(uiSource.clip);
        PauseBackgroundWithoutScreen();
    }

    public void UnpauseBackground()
    {
        gamePaused = false;
        if (settings != null) settings.SetActive(false);
        if (pausedGame != null) pausedGame.SetActive(false);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            if (allRigidbodies[i] == null) 
            {
                continue;
            }
            
            allRigidbodies[i].WakeUp();
            allRigidbodies[i].velocity = savedVelocities[i];
            allRigidbodies[i].angularVelocity = savedAngularVelocities[i];
        }

        foreach (Animator anim in allAnimators)
        {
            if (anim != null) anim.speed = 1f;
        }

        for (int i = 0; i < allParticles.Length; i++)
        {
            if (allParticles[i] == null) 
            {
                continue;
            }
            
            if (particleWasPlaying[i]) allParticles[i].Play();
        }

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (allAudioSources[i] == null) 
            {
                continue;
            }

            if (allAudioSources[i] == uiSource) 
            {
                continue;
            }

            if (audioWasPlaying[i]) 
            {
                allAudioSources[i].UnPause();
            }
        }

        uiSource.PlayOneShot(uiSource.clip);
    }
}
