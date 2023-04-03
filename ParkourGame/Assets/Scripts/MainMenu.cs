using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("TestScene");
    }

    public void OpenSettings()
    {
        // Code to open the settings menu 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
