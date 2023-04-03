using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Game State")]
    private float timer = 90f;
    private int score = 0;
    private int lives = 3;
    private int amount = 100;


    [Header("UI Elements")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI lossScoreText;


    private bool isPaused = false;
    public AudioSource audioSource;


  

    void Start()
    {
        scoreText.text = "Score: " + score;
    }

    void Update()
    {
        HandleInput();

        if (!isPaused)
        {
            UpdateTimer();
        }
  
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void LoadTutorialScene()
    {
        SceneManager.LoadScene("Tutorial");
        Time.timeScale = 1f;

    }

    public void LoadLevel1Scene()
    {
        SceneManager.LoadScene("Level1");
        Time.timeScale = 1f;
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            audioSource.volume = 0;
            pauseCanvas.SetActive(true);
            SetCursorVisibility(true);        }
        else
        {
            Time.timeScale = 1f;
            audioSource.volume = 1;
            pauseCanvas.SetActive(false);
            SetCursorVisibility(true);
        }
    }

    private void UpdateTimer()
    {
        timer -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.RoundToInt(timer);
        


        if (timer <= 0f)
        {
            EndGame(false);
        }
    }

    public void UpdateScore()
    {
        score += amount;
        

        scoreText.text = "Score: " + score;
        
    }
    

    public void UpdateLives(int amount)
    {
        lives += amount;

        if (lives <= 0)
        {
            EndGame(false);
        }
    }
    public void LifeLoss(int amount)
    {
        lives -= amount;
        timer = 90f;

        if (lives <= 0)
        {
            EndGame(false);
        }
    }

   

    public void EndGame(bool hasWon)
    {
        if (hasWon)
        {
            winScreen.SetActive(true);
            audioSource.volume = 0;
            winScoreText.text = "Final Score: " + score;


        }
        else
        {
            loseScreen.SetActive(true);
            audioSource.volume = 0;
            lossScoreText.text = "Final Score: " + score;


        }

        TogglePause();
    }

    private void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}