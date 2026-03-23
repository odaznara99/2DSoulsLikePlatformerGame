using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerData playerData = new();

    [Header("Souls Drop")]
    [Tooltip("Prefab for the dropped-souls pickup spawned at the player's death position. Assign in Inspector.")]
    public GameObject droppedSoulsPrefab;

    private bool isGamePaused;
    private bool isGameOver;
    private int playerScore;

    // Reference to the currently active DroppedSoulsPickup in the scene.
    private GameObject activeDroppedSoulsPickup;

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

    // ── Souls & Coins ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by PlayerHealth when the player dies.
    /// Resets souls/coins, records the dropped-souls state, and destroys any
    /// previously dropped souls (which are then permanently lost).
    /// </summary>
    public void NotifyPlayerDeath(Vector2 deathPosition)
    {
        // If an uncollected souls pickup already exists, destroy it — those
        // souls are permanently lost on a second death.
        if (activeDroppedSoulsPickup != null)
        {
            Destroy(activeDroppedSoulsPickup);
            activeDroppedSoulsPickup = null;
        }

        // Record the souls to be dropped (only if the player had any).
        playerData.droppedSouls = playerData.souls;
        playerData.droppedSoulsPosition = deathPosition;

        // Reset currencies on death.
        playerData.souls = 0;
        playerData.coins = 0;
    }

    /// <summary>
    /// Spawns the dropped-souls pickup at the stored death position after the
    /// scene has been reloaded.  Called by SceneLoader once the scene is ready.
    /// </summary>
    public void SpawnDroppedSoulsIfAny()
    {
        if (playerData.droppedSouls <= 0 || droppedSoulsPrefab == null) return;

        activeDroppedSoulsPickup = Instantiate(
            droppedSoulsPrefab,
            playerData.droppedSoulsPosition,
            Quaternion.identity);

        var pickup = activeDroppedSoulsPickup.GetComponent<DroppedSoulsPickup>();
        if (pickup != null)
            pickup.soulsAmount = playerData.droppedSouls;
    }

    /// <summary>
    /// Called by DroppedSoulsPickup when the player picks up the dropped souls,
    /// clearing the stored state so no new pickup is spawned on the next respawn.
    /// </summary>
    public void ClearDroppedSoulsPickup()
    {
        activeDroppedSoulsPickup = null;
        playerData.droppedSouls = 0;
        playerData.droppedSoulsPosition = Vector2.zero;
    }
}