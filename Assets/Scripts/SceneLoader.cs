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
    public float fadeDuration = 1f;

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
        if (sceneName == "StartMenu")
        {
            if (PlayerControllerVersion2.Instance != null)
            {
                Destroy(PlayerControllerVersion2.Instance.gameObject);
            }

            if (UIScreensManager.Instance != null)
            {
                Destroy(UIScreensManager.Instance.gameObject);
            }
        }

        StartCoroutine(LoadSceneAsync(sceneName, spawnPoint));
        //AudioManager.Instance.PlayMusic("MedievalOpener");
        AudioManager.Instance.PlayMusic("Ballad");

        
    }

    private IEnumerator LoadSceneAsync(string sceneName, SpawnPointType spawnPoint)
    {
        if (sceneName == null)
        {
            Debug.LogError("Scene name is null. Cannot load scene.");
            yield break;
        }

        PlayerHealth.Instance.isInvincible = true;
        PlayerControllerVersion2.Instance.enabled = false; // Disable player controls during loading

        // Fade Out to black
        yield return StartCoroutine(Fade(0f, 1f));

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

        yield return new WaitForSeconds(3f); // Optional delay

        progressBar.value = 1f;
        progressText.text = "100%";

        op.allowSceneActivation = true;

        // Wait for the next frame before fading in
        yield return null;

        // Fade In to clear
        loadingScreen.SetActive(false);

        // Scene loaded, it will now show the scene name
        MessageManager.Instance.ShowMessage(sceneName,false,100);


        // Position the Player on the Start Point
        if (spawnPoint == SpawnPointType.Start)
        {
            PlayerControllerVersion2.Instance.GetComponent<PlayerPositionRestorer>().TeleportToStartSpawn();
        } 
        // Position the Player on the EndPoint
        else if (spawnPoint == SpawnPointType.Last)
        {
            PlayerControllerVersion2.Instance.GetComponent<PlayerPositionRestorer>().TeleportToEndSpawn();
        }
        // Position the Player on the Checkpoint
        else if (spawnPoint == SpawnPointType.Checkpoint)
        {
            PlayerControllerVersion2.Instance.GetComponent<PlayerPositionRestorer>().TeleportToCheckpoint();
        }

        PlayerHealth.Instance.isInvincible = true;
        // Start Fading Out - showing the Scene
        yield return StartCoroutine(Fade(1f, 0f));
        PlayerControllerVersion2.Instance.enabled = true; // Disable player controls during loading


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


}
