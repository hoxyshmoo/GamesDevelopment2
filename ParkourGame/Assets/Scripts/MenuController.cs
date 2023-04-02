using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;

    private bool isPaused = false;

    private void Update()
    {
        HandleInput();
    }

    // Method to handle input separately
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // Method to start a new game
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    // Method to return to the main menu
    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Method to quit the game
    public void QuitGame()
    {
        Application.Quit();
    }

    // Method to pause the game
    public void Pause()
    {
        Time.timeScale = 0;
        isPaused = true;
        pauseCanvas.SetActive(true);
        SetCursorVisibility(true);
    }

    // Method to resume the game
    public void Resume()
    {
        Time.timeScale = 1;
        isPaused = false;
        pauseCanvas.SetActive(false);
        SetCursorVisibility(false);
    }

    // Method to set cursor visibility and lock state
    private void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
