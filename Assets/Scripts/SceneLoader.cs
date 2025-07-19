using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO; // Needed for Path methods
using System.Collections;

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



    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
        //AudioManager.Instance.PlayMusic("MedievalOpener");
        AudioManager.Instance.PlayMusic("Ballad");
    }

    public void LoadNextScene() {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Get full path (like "Assets/Scenes/Level2.unity")
            string nextScenePath = SceneUtility.GetScenePathByBuildIndex(nextIndex);

            // Extract just the scene name ("Level2")
            string nextSceneName = Path.GetFileNameWithoutExtension(nextScenePath);

            Debug.Log("Next scene name is: " + nextSceneName);
            LoadScene(nextSceneName);
        }

    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
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


        MessageManager.Instance.ShowMessage(sceneName,false,100);
        yield return StartCoroutine(Fade(1f, 0f));

        
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
