using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{

    public static SceneLoader Instance;

    public GameObject loadingScreen;
    public Slider progressBar;
    public Text progressText;

    // Variables for Fade In/Out Transition
    public Image fadeImage;
    private float fadeDuration = 1f;

    // For Loading Dots Animation
    public Text loadingDotsText;
    private string baseText = "Loading";
    private float dotTimer = 0f;
    private int dotCount = 0;

    /// <summary>
    /// Ensures only one SceneLoader instance exists and marks it to persist across scene loads.
    /// </summary>
    // When true, RespawnPlayer is called after the scene loads.
    // Set by LoadSceneWithRespawn; cleared after use.
    private bool shouldRespawnAfterLoad;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Begins the initial fade-in animation from black when the scene first loads.
    /// </summary>
    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1f);
            StartCoroutine(Fade(1f, 0f));
        }
    }

    /// <summary>
    /// Animates the loading-dots text while the loading screen is active.
    /// </summary>
    private void Update()
    {
        if (loadingScreen.activeSelf && loadingDotsText != null)
        {
            dotTimer += Time.deltaTime;
            if (dotTimer >= 0.5f)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % 4;  // cycles between 0 to 3
                loadingDotsText.text = baseText + new string('.', dotCount);
            }
        }
    }

    /// <summary>
    /// Loads the specified scene asynchronously with a fade, loading screen, progress bar,
    /// and post-load respawn. If the name has no underscore a Build-Settings prefix match is attempted.
    /// </summary>
    /// <param name="sceneName">The name or prefix of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        try
        {
            // If sceneName follows the convention "Stage1_Scenename" and you only want "Stage1"
            // we will look for the first scene in Build Settings that starts with the prefix
            string targetScene = sceneName;

            if (!string.IsNullOrEmpty(sceneName) && !sceneName.Contains("_"))
            {
                string prefix = sceneName.Split(new char[] { '_' }, 2)[0];

                int count = SceneManager.sceneCountInBuildSettings;
                for (int i = 0; i < count; i++)
                {
                    string path = SceneUtility.GetScenePathByBuildIndex(i);
                    string candidate = Path.GetFileNameWithoutExtension(path);

                    if (candidate.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetScene = candidate;
                        break; // first match wins
                    }
                }
            }

            StartCoroutine(LoadSceneAsync(targetScene));
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayMusic("Ballad");
        }
        catch
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
        }
    }

    /// <summary>
    /// Coroutine that handles the full asynchronous scene-load sequence: fades to black,
    /// shows the loading screen with progress, activates the scene, respawns the player,
    /// then fades back in and displays the level name.
    /// </summary>
    /// <param name="sceneName">The exact scene name to load asynchronously.</param>
    /// Loads a scene and respawns the player at the saved checkpoint position
    /// afterward. Use this for death/try-again and return-to-checkpoint flows.
    /// </summary>
    public void LoadSceneWithRespawn(string sceneName)
    {
        shouldRespawnAfterLoad = true;
        LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (sceneName == null)
        {
            Debug.LogError("Scene name is null. Cannot load scene.");
            yield break;
        }

        // Find current player instance in the Scene
        PlayerControllerVersion2 playerScript = FindAnyObjectByType<PlayerControllerVersion2>();

        if (playerScript != null)
        {
            playerScript.GetComponent<PlayerHealth>().isInvincible = true;
            playerScript.enabled = false; // Disable player controls during loading
        }

        // Fade Out to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Loading Screen was Displayed
        loadingScreen.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            progressBar.value = progress;
            progressText.text = $"{(int)(progress * 100)}%";
            yield return null;
        }

        yield return new WaitForSeconds(1f); // Optional delay

        progressBar.value = 1f;
        progressText.text = "100%";

        op.allowSceneActivation = true;

        yield return new WaitForSeconds(1f); // Optional delay

        // After scene load
        playerScript = FindAnyObjectByType<PlayerControllerVersion2>();
        if (playerScript != null)
        {
            RespawnManager.Instance.RespawnPlayer();

            // Spawn dropped-souls pickup at death position if any souls were lost.
            GameManager.Instance?.SpawnDroppedSoulsIfAny();
        }
        else
        if (playerScript != null) { 
            if (shouldRespawnAfterLoad)
            {
                RespawnManager.Instance.RespawnPlayer();

                // Spawn dropped-souls pickup at death position if any souls were lost.
                GameManager.Instance?.SpawnDroppedSoulsIfAny();
            }
        } else
        {
            Debug.LogWarning("[SceneLoader]PlayerControllerVersion2 not found in the scene after loading. Player may not be respawned correctly.");
        }

        // Reset the flag after use
        shouldRespawnAfterLoad = false;

        // Delay for Camera Follow the Player's New Position
        yield return new WaitForSeconds(2f);

        // Loading Screen Disabled
        loadingScreen.SetActive(false);

        // Start Fading Out - showing the Scene
        yield return StartCoroutine(Fade(1f, 0f));

        // Scene loaded, show the scene name without naming-prefixes
        string displayName = GetDisplayLevelName(sceneName);
        MessageManager.Instance.ShowMessage(displayName, false, 100);

        // Show In Game UI
        if (sceneName != "StartMenu")
            FindAnyObjectByType<UIScreensManager>().ShowScreenHideOthers("In-Game Screen");
    }

    /// <summary>
    /// Coroutine that fades the overlay image between two alpha values over <see cref="fadeDuration"/> seconds.
    /// </summary>
    /// <param name="startAlpha">The alpha value to start the fade from.</param>
    /// <param name="endAlpha">The alpha value to end the fade at.</param>
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }

    /// <summary>
    /// Returns a human-readable display name for the given scene by stripping stage prefixes.
    /// If the scene name contains an underscore, returns the text after the first underscore.
    /// If it starts with "Stage" followed by digits, strips that prefix and any trailing separator.
    /// Otherwise returns the original scene name.
    /// </summary>
    /// <param name="scene">The internal scene name to clean up.</param>
    /// <returns>A display-friendly level name.</returns>
    private string GetDisplayLevelName(string scene)
    {
        if (string.IsNullOrEmpty(scene)) return scene;

        int underscore = scene.IndexOf('_');
        if (underscore >= 0 && underscore < scene.Length - 1)
            return scene.Substring(underscore + 1);

        if (scene.StartsWith("Stage", System.StringComparison.OrdinalIgnoreCase))
        {
            int i = 5; // after "Stage"
            while (i < scene.Length && char.IsDigit(scene[i])) i++;
            if (i < scene.Length && (scene[i] == '_' || scene[i] == '-' || scene[i] == ' ')) i++;
            if (i < scene.Length) return scene.Substring(i);
        }

        // fallback: return the original
        return scene;
    }
}
