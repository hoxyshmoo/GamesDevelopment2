using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public float timer = 60f;
    public int score = 0;
    public int lives = 3;
    public float stamina = 100f;
    public float staminaCooldownTime = 5f;
    public float staminaRegenRate = 10f;

    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject pauseScreen;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI staminaText;

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
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (!isPaused)
        {
            UpdateTimer();
            UpdateStamina();
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
            pauseScreen.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
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
}
