using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestSceneManager : MonoBehaviour
{
    [Header("UI")]
    public Transform stagesContainer;      // parent for stage buttons
    public Transform featuresContainer;    // parent for feature buttons
    public GameObject buttonPrefab;        // simple Button prefab (Button + Text child named "Text")

    [Header("Feature list (configure in Inspector)")]
    [SerializeField] private List<Feature> features = new List<Feature>();

    [System.Serializable]
    public class Feature
    {
        public string name;
        public UnityEvent onClick; // assign methods from Inspector
    }

    void Start()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("TestSceneManager: buttonPrefab is not assigned.");
            return;
        }

        PopulateStages();
        PopulateFeatures();
    }

    void PopulateStages()
    {
        if (stagesContainer == null) return;

        // Clear existing children
        foreach (Transform t in stagesContainer) Destroy(t.gameObject);

        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(path);
            GameObject go = Instantiate(buttonPrefab, stagesContainer);
            var txt = go.transform.Find("Text");
            if (txt != null) txt.GetComponent<Text>().text = sceneName;
            var btn = go.GetComponent<Button>();
            string capturedName = sceneName;
            btn.onClick.AddListener(() => LoadStage(capturedName));
        }
    }

    void PopulateFeatures()
    {
        if (featuresContainer == null) return;

        // Clear existing children
        foreach (Transform t in featuresContainer) Destroy(t.gameObject);

        foreach (var feat in features)
        {
            GameObject go = Instantiate(buttonPrefab, featuresContainer);
            var txt = go.transform.Find("Text");
            if (txt != null) txt.GetComponent<Text>().text = feat.name;
            var btn = go.GetComponent<Button>();
            UnityAction action = () => feat.onClick?.Invoke();
            btn.onClick.AddListener(action);
        }
    }

    // Called when a stage button is clicked
    public void LoadStage(string sceneName)
    {
        // Optional: play SFX, show confirmation, etc.
        SceneManager.LoadScene(sceneName);
    }

    // Example helper features you can expose in Inspector or call from other scripts:
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}