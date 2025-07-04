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

    //public CanvasGroup gameOverScreen;        // Reference to the Game Over UI   
    //private float gameOverFadeInSeconds = 2f;

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

        // Ensure UI elements are disabled at the start
       // if (gameOverScreen != null)
         //   gameOverScreen.gameObject.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene is loaded, find the UI elements again
       // FindUIElements();
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Reset game over flag and score
        isGameOver = false;
        playerScore = 0;

        // Disable Game Over UI
       // if (gameOverScreen != null)
         //   gameOverScreen.gameObject.SetActive(false);
    }

    // Function to pause the game
    public void PauseGame()
    {
        //pauseScreen.SetActive(true);     // Enable pause menu UI
        FindObjectOfType<UIScreensManager>().ShowScreen("Pause Screen");
        Time.timeScale = 0f;             // Stop the time in-game
        isGamePaused = true;
    }

    // Function to resume the game from pause
    public void ResumeGame()
    {
        //pauseScreen.SetActive(false);    // Disable pause menu UI
        Time.timeScale = 1f;             // Resume the time in-game
        isGamePaused = false;
        FindObjectOfType<UIScreensManager>().HideScreen("Pause Screen");
        
    }

    // Function to handle Game Over


    // Function to trigger Game Over with a delay
    public void TriggerGameOverWithDelay()
    {

        //StartCoroutine(DelayFadeInPanel(gameOverScreen, gameOverFadeInSeconds));
        FindObjectOfType<UIScreensManager>().ShowScreenWithFadeIn("Game Over Screen",2f);
        //isGameOver = true;  // Set the game over flag
        Invoke("SetIsGameOver", 2f); // isGameOver flag after 2 seconds
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
        SceneManager.LoadScene(0);
    }

    void FindUIElements() {
      //  gameOverScreen  = GameObject.Find("ScreenCanvas").transform.Find("Game Over Screen").gameObject.GetComponent<CanvasGroup>();

    }

    IEnumerator DelayFadeInPanel(CanvasGroup canvasGroup,float fadeDuration)
    {
        Debug.Log("Start Fading In: " + canvasGroup.name);
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true); // make sure it's visible

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            
            yield return null;
        }

       // if (canvasGroup.name == gameOverScreen.name) isGameOver = true;
        Debug.Log("Finish Fading In: " + canvasGroup.name);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

}
