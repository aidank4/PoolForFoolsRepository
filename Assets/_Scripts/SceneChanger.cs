using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class SceneChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public void GoBack()
    {
        Debug.Log("Scene loaded");
        //SceneManager.LoadScene("MainMenu");
        
    }

    public void Sound()
    {
        Debug.Log("Sound");
    }

    public void Music()
    {
        Debug.Log("Music");
    }

    public void SFX()
    {
        Debug.Log("SFH");
    }


  
}
