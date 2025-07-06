using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    public GameObject loadingScreen;
    public Slider progressBar;
    public Text progressText;

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

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
        //AudioManager.Instance.PlayMusic("MedievalOpener");
        AudioManager.Instance.PlayMusic("Ballad");
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
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

        yield return new WaitForSeconds(1f); // Optional delay for smoothness

        progressBar.value = 1f;
        progressText.text = "100%";

        op.allowSceneActivation = true;
        loadingScreen.SetActive(false);
    }
}
