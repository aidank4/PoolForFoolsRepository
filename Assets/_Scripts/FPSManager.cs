using UnityEditor;
using UnityEngine;

public class FPSManager : MonoBehaviour
{
    private int targetFPS = 144;
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
