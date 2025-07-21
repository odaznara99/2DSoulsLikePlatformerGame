using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;  // Singleton instance
    private bool isGamePaused = false;   // Flag to check if the game is paused
    private bool isGameOver = false;     // Flag to check if the game is over
    private int playerScore = 0;         // Player's score

    public Vector3 lastPlayerPosition; // Stores the player's position
    public string lastSceneName;

    void Awake()
    {
        // Ensure there's only one GameManager instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Prevent destruction between scenes
        }
        else
        {
            Destroy(gameObject);  // Destroy any duplicate GameManagers
        }
    }

    void Start()
    {
        // Initialize game state
        isGamePaused = false;
        isGameOver = false;

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene is loaded, find the UI elements again
        // FindUIElements();
        Time.timeScale = 1f;
    }


    void Update()
    {

        // Handle Pause/Unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isGameOver)
            {
                if (isGamePaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }
        // Restart the Game 
        else if (Input.GetKeyUp(KeyCode.R)) {

            RestartGame();

        }
        // Handle Game Over Controls
        if (isGameOver && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            RestartGame();  // Restart the game when Enter is pressed
        }
    }

    // Function to restart the game
    public void RestartGame()
    {
        // Reload the active scene to restart
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        SceneLoader.Instance.LoadScene(SceneManager.GetActiveScene().name);

        // Reset game over flag and score
        isGameOver = false;
        playerScore = 0;

        FindObjectOfType<UIScreensManager>().ShowScreenHideOthers("In-Game Screen");
    }

    // Function to pause the game
    public void PauseGame()
    {
        //pauseScreen.SetActive(true);     // Enable pause menu UI
        FindObjectOfType<UIScreensManager>().ShowScreen("Pause Screen");
        Time.timeScale = 0f;             // Stop the time in-game
        isGamePaused = true;
        AudioManager.Instance.PlaySFX("Click"); 
    }

    // Function to resume the game from pause
    public void ResumeGame()
    {
        //pauseScreen.SetActive(false);    // Disable pause menu UI
        Time.timeScale = 1f;             // Resume the time in-game
        isGamePaused = false;
        FindObjectOfType<UIScreensManager>().HideScreen("Pause Screen");
        AudioManager.Instance.PlaySFX("Click");

    }

    // Function to trigger Game Over with a delay
    public void TriggerGameOverWithDelay()
    {
        AudioManager.Instance.StopMusic(); // Stop current music
        AudioManager.Instance.PlayMusic("GameOver"); // Play game over sound

        //StartCoroutine(DelayFadeInPanel(gameOverScreen, gameOverFadeInSeconds));
        FindObjectOfType<UIScreensManager>().ShowScreenWithFadeIn("Game Over Screen",2f);
        //isGameOver = true;  // Set the game over flag
        Invoke(nameof(SetIsGameOver), 2f); // isGameOver flag after 2 seconds
        Debug.Log("Game Over!");
    }

    public void SetIsGameOver() {
        isGameOver = true;
    }

    // Function to quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // Function to add score
    public void AddScore(int points)
    {
        playerScore += points;
        Debug.Log("Score: " + playerScore);
    }

    // Getter for checking if the game is paused
    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    // Getter for checking if the game is over
    public bool IsGameOver()
    {
        return isGameOver;
    }

    // Getter for getting the player's score
    public int GetPlayerScore()
    {
        return playerScore;
    }

    public void GoToMainMenu(){
        AudioManager.Instance.PlayMusic("Ballad");
        Time.timeScale = 1f;
        //SceneManager.LoadScene(0);
        SceneLoader.Instance.LoadScene("StartMenu");
    }


}
