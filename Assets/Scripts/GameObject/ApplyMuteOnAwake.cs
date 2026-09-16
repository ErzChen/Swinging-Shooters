using UnityEngine;

public class ApplyMuteOnAwake : MonoBehaviour
{
    void Awake()
    {
        if (PauseGame.Instance != null && PauseGame.Instance.audioMuted)
        {
            foreach (AudioSource src in GetComponentsInChildren<AudioSource>(true))
            {
                src.mute = true;
            }
        }
    }
}
