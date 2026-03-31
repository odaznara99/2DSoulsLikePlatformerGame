using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerData playerData = new();

    [Header("Game State")]
    [SerializeField] private bool isGamePaused;
    [SerializeField] private bool isGameOver;
    [SerializeField] private bool isGameLoading;

    [Header("Player Score")]
    [SerializeField] private int playerScore;

    [Header("Souls Drop")]
    [Tooltip("Prefab for the dropped-souls pickup spawned at the player's death position. Assign in Inspector.")]
    public GameObject droppedSoulsPrefab;

    


    // Reference to the currently active DroppedSoulsPickup in the scene.
    private GameObject activeDroppedSoulsPickup;

    // Snapshot of playerData taken at the last checkpoint.
    private PlayerData checkpointSnapshot;

    // ── Currency Events ──────────────────────────────────────────────────────
    /// <summary>Raised whenever the souls count changes. Parameter is the new total.</summary>
    public event Action<int> OnSoulsChanged;

    /// <summary>Raised whenever the coins count changes. Parameter is the new total.</summary>
    public event Action<int> OnCoinsChanged;

    // ── Currency Helpers ─────────────────────────────────────────────────────

    /// <summary>Adds (or subtracts) souls and notifies listeners.</summary>
    public void AddSouls(int amount)
    {
        playerData.souls += amount;
        OnSoulsChanged?.Invoke(playerData.souls);
    }

    /// <summary>Adds (or subtracts) coins and notifies listeners.</summary>
    public void AddCoins(int amount)
    {
        playerData.coins += amount;
        OnCoinsChanged?.Invoke(playerData.coins);
    }

    /// <summary>Forces a UI refresh for both currencies (e.g. after a scene load or checkpoint restore).</summary>
    public void BroadcastCurrencyUpdate()
    {
        OnSoulsChanged?.Invoke(playerData.souls);
        OnCoinsChanged?.Invoke(playerData.coins);
    }

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
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame && !isGameOver)
        {
            if (isGamePaused) ResumeGame();
            else PauseGame();
        }

        if (keyboard.rKey.wasReleasedThisFrame)
        {
            SceneLoader.Instance.ReloadCurrentScene();
        }

        if (isGameOver &&
            (keyboard.enterKey.wasPressedThisFrame
            || Mouse.current?.leftButton.wasPressedThisFrame == true
            || Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true))
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        // "Try again" on death — return to last checkpoint, dropping souls/coins.
        ReturnToLastCheckpoint();
        isGamePaused = false;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
        FindAnyObjectByType<UIScreensManager>().ShowScreen("Pause Screen");
        AudioManager.Instance.PlaySFX("Click");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;

        FindAnyObjectByType<UIScreensManager>().HideScreen("Pause Screen");
        AudioManager.Instance.PlaySFX("Click");
    }

    public void TriggerGameOverWithDelay()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("GameOver");

        FindAnyObjectByType<UIScreensManager>().ShowScreenWithFadeIn("Game Over Screen", 2f);
        Invoke(nameof(SetGameOver), 2f);
    }

    private void SetGameOver() => isGameOver = true;

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("Ballad");

        SceneLoader.Instance.LoadScene("StartMenu");
    }

    public void AddScore(int points) => playerScore += points;

    public bool IsGamePaused() => isGamePaused;
    public bool IsGameOver() => isGameOver;

    /// <summary>
    /// Pauses or resumes the game without opening the Pause Screen overlay.
    /// Useful for dialogue, cutscenes, or any system that needs to freeze
    /// gameplay while keeping its own UI visible.
    /// </summary>
    public void PauseSilent(bool pause)
    {
        isGamePaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    public bool IsGameLoading() => isGameLoading;
    public void SetGameLoading(bool loading) => isGameLoading = loading;

    /// <summary>
    /// Resets pause/game-over state and hides all UI overlays.
    /// Called before any scene transition that should start fresh.
    /// </summary>
    public void ResetGameState()
    {
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        var uiManager = FindAnyObjectByType<UIScreensManager>();
        if (uiManager != null)
            uiManager.HideAllScreens();
    }

    // ── Checkpoint Snapshot ──────────────────────────────────────────────────

    /// <summary>
    /// Saves a snapshot of the current playerData, including the active scene name.
    /// Call this whenever the player reaches/activates a checkpoint.
    /// Also persists the snapshot to the local save file via SaveManager.
    /// </summary>
    public void SaveCheckpointSnapshot()
    {
        var player = FindAnyObjectByType<PlayerControllerVersion2>();
        if (player != null)
        {
            playerData.position = player.transform.position;

            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                playerData.currentHealth = health.currentHealth;
                playerData.maxHealth = health.maxHealth;
            }
        }

        playerData.checkpointSceneName = SceneManager.GetActiveScene().name;
        checkpointSnapshot = playerData.Clone();
        SaveManager.Instance?.SaveCheckpoint(checkpointSnapshot);

        Debug.Log("Checkpoint snapshot saved. Scene: " + playerData.checkpointSceneName);
    }

    /// <summary>
    /// Restores the in-memory checkpoint snapshot from a previously persisted
    /// PlayerData instance.  Called by SaveManager.LoadSavedGame().
    /// </summary>
    public void RestoreCheckpointSnapshot(PlayerData snapshot)
    {
        checkpointSnapshot = snapshot.Clone();
    }

    /// <summary>
    /// Returns the player to the last checkpoint, reverting playerData to the
    /// snapshot (health, position, scene). Souls and coins collected since the
    /// checkpoint are dropped at the death location and can be recovered once.
    /// This is also used as the "Try Again" flow upon player death.
    /// </summary>
    public void ReturnToLastCheckpoint()
    {
        if (checkpointSnapshot == null)
        {
            Debug.LogWarning("No checkpoint snapshot found. Cannot return to checkpoint.");
            // Simple error message
            MessageManager.Instance.ShowMessage("No checkpoint snapshot found. Cannot return to checkpoint.", false, 24);
            return;
        }

        // Determine which scene the checkpoint belongs to
        string targetScene = checkpointSnapshot.checkpointSceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("Checkpoint snapshot has no scene name. Falling back to current scene.");
            targetScene = SceneManager.GetActiveScene().name;
        }

        // Revert playerData to the checkpoint snapshot
        // (souls & coins were already zeroed by NotifyPlayerDeath;
        //  the snapshot restores the checkpoint-time values which are also
        //  the "before collection" amounts, effectively dropping everything
        //  collected after the checkpoint.)
        playerData.RestoreFrom(checkpointSnapshot);

        ResetGameState();

        // Destroy any active dropped-souls pickup since progress is reverted
        if (activeDroppedSoulsPickup != null)
        {
            Destroy(activeDroppedSoulsPickup);
            activeDroppedSoulsPickup = null;
        }

        Time.timeScale = 1f;

        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("Ballad");

        // Load the checkpoint scene — RespawnManager will place the player
        // at playerData.position (the saved checkpoint position).
        SceneLoader.Instance.LoadSceneWithRespawn(targetScene);

        // Notify UI of restored currency values after checkpoint revert.
        BroadcastCurrencyUpdate();
    }

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

        // Notify UI that currencies were reset.
        BroadcastCurrencyUpdate();
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