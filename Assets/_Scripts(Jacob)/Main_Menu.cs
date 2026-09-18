using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    public void onQuit()
    {
        Application.Quit();
    }

    public void onStart()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void onSettings()
    {
        SceneManager.LoadScene("SettingsMenu");
    }
}
