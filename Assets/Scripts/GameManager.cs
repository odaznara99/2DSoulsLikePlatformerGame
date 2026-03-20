using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerData playerData = new();

    private bool isGamePaused;
    private bool isGameOver;
    private int playerScore;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isGamePaused) ResumeGame();
            else PauseGame();
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            RestartGame();
        }

        if (isGameOver && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        isGameOver = false;
        playerScore = 0;
        playerData.currentHealth = playerData.maxHealth;

        SceneLoader.Instance.LoadScene(SceneManager.GetActiveScene().name, SpawnPointType.Checkpoint);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
        FindObjectOfType<UIScreensManager>().ShowScreen("Pause Screen");
        AudioManager.Instance.PlaySFX("Click");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;

        FindObjectOfType<UIScreensManager>().HideScreen("Pause Screen");
        AudioManager.Instance.PlaySFX("Click");
    }

    public void TriggerGameOverWithDelay()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("GameOver");

        FindObjectOfType<UIScreensManager>().ShowScreenWithFadeIn("Game Over Screen", 2f);
        Invoke(nameof(SetGameOver), 2f);
    }

    private void SetGameOver() => isGameOver = true;

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("Ballad");

        SceneLoader.Instance.LoadScene("StartMenu", SpawnPointType.Start);
    }

    public void AddScore(int points) => playerScore += points;

    public bool IsGamePaused() => isGamePaused;
    public bool IsGameOver() => isGameOver;
}