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
    private float timer = 60f;
    private int score = 0;
    private int FinalScore = 0;
    private int lives = 3;
    private float stamina = 100f;
    private float staminaCooldownTime = 5f;
    private float staminaRegenRate = 10f;

    [Header("UI Elements")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI finalScoreText;


    private bool isPaused = false;
    private float staminaCooldownTimer = 0f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //SceneManager.LoadScene("Menu");
        scoreText.text = "Score: " + score;

    }

    void Update()
    {
        HandleInput();

        if (!isPaused)
        {
            UpdateTimer();
            UpdateStamina();
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
        SceneManager.LoadScene("TutorialScene");
    }

    public void LoadLevel1Scene()
    {
        SceneManager.LoadScene("Level1Scene");
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
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
            pauseCanvas.SetActive(true);
            SetCursorVisibility(true);
            UpdateScore(1);
        }
        else
        {
            Time.timeScale = 1f;
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

    public void UpdateScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
        FinalScore = score;
        finalScoreText.text = "Score: " + score;
    }
    

    public void UpdateLives(int amount)
    {
        lives += amount;
        livesText.text = "Lives: " + lives;

        if (lives <= 0)
        {
            EndGame(false);
        }
    }
    public void LifeLoss(int amount)
    {
        lives -= amount;
        livesText.text = "Lives: " + lives;
        timer = 60f;

        if (lives <= 0)
        {
            EndGame(false);
        }
    }

    private void UpdateStamina()
    {
        if (stamina < 100f)
        {
            staminaCooldownTimer -= Time.deltaTime;

            if (staminaCooldownTimer <= 0f)
            {
                stamina += staminaRegenRate * Time.deltaTime;
                stamina = Mathf.Clamp(stamina, 0f, 100f);
                staminaText.text = "Stamina: " + Mathf.RoundToInt(stamina);
            }
        }
        else
        {
            staminaCooldownTimer = staminaCooldownTime;
        }
    }

    public void EndGame(bool hasWon)
    {
        if (hasWon)
        {
            winScreen.SetActive(true);
        }
        else
        {
            loseScreen.SetActive(true);
        }

        TogglePause();
    }

    private void SetCursorVisibility(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}