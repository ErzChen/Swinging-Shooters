using UnityEngine;

public static class FrameRateManager 
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}