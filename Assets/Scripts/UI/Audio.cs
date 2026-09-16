using UnityEngine;
using UnityEngine.UI;

public class Audio : MonoBehaviour
{
    private Toggle toggle;
    private GameManager gameManager;

    void Start() 
    {
        toggle = GetComponent<Toggle>();
        gameManager = GameManager.Instance;
        toggle.isOn = gameManager.audioOn;
        toggle.onValueChanged.AddListener(OntoggleChanged); 
    }

    void OntoggleChanged(bool audioOn)
    {
        gameManager.SetAudio(audioOn);
    }
}
