using UnityEngine;

public class Close : MonoBehaviour
{
    
    public void CloseWindow(GameObject window) 
    {
        window.SetActive(false);
    }
}
