using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO; // Needed for Path methods
using System.Collections;

public enum SpawnPointType
{
    Start,
    Last,
    Checkpoint
}

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
    public Text loadingDotsText;  // or TMP_Text if using TextMeshPro
    private string baseText = "Loading";
    private float dotTimer = 0f;
    private int dotCount = 0;



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

    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1f);
            StartCoroutine(Fade(1f, 0f));
        }
    }

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



    public void LoadScene(string sceneName, SpawnPointType spawnPoint)
    {
        try
        {
            // If sceneName follows the convention "Stage1_Scenename" and you only want "Stage1"
            // we will look for the first scene in Build Settings that starts with the prefix
            string targetScene = sceneName;

            if (!string.IsNullOrEmpty(sceneName))
            {
                //Debug.Log($"Scene name '{sceneName}' contains an underscore. Attempting to find a matching scene in Build Settings.");
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

            StartCoroutine(LoadSceneAsync(targetScene, spawnPoint));
            //AudioManager.Instance.PlayMusic("MedievalOpener");
            AudioManager.Instance.StopMusic(); // Stop any currently playing music
            AudioManager.Instance.PlayMusic("Ballad");
        }catch
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
        }

    }

    private IEnumerator LoadSceneAsync(string sceneName, SpawnPointType spawnPoint)
    {
        if (sceneName == null)
        {
            Debug.LogError("Scene name is null. Cannot load scene.");
            yield break;
        }

        // Find current player instance in the Scene

        PlayerControllerVersion2 playerScript = FindObjectOfType<PlayerControllerVersion2>();

        if (playerScript != null) {
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

        // Find the Current Player in the New Scene

        playerScript = FindObjectOfType<PlayerControllerVersion2>();

        if (playerScript != null)
        {
            // Position the Player on the Start Point
            if (spawnPoint == SpawnPointType.Start)
            {
                playerScript.GetComponent<PlayerPositionRestorer>().TeleportToStartSpawn();
            }
            // Position the Player on the EndPoint
            else if (spawnPoint == SpawnPointType.Last)
            {
                playerScript.GetComponent<PlayerPositionRestorer>().TeleportToEndSpawn();
            }
            // Position the Player on the Checkpoint
            else if (spawnPoint == SpawnPointType.Checkpoint)
            {
                playerScript.GetComponent<PlayerPositionRestorer>().TeleportToCheckpoint();
            }


        }

        // Delay for Camera Follow the Player's New Position
        yield return new WaitForSeconds(2f);

        // Loading Screen Disabled
        loadingScreen.SetActive(false);

        // Start Fading Out - showing the Scene
        yield return StartCoroutine(Fade(1f, 0f));

        // Scene loaded, show the scene name without naming-prefixes
        string displayName = GetDisplayLevelName(sceneName);
        MessageManager.Instance.ShowMessage(displayName, false, 100);

    }

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

    // Returns a cleaned display name for the level:
    // - If scene name contains an underscore, returns the substring after the first underscore.
    // - Otherwise, if it starts with "Stage" followed by digits, strips "Stage" + digits and any separator ('_', '-', ' ').
    // - Otherwise returns the original scene name.
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
