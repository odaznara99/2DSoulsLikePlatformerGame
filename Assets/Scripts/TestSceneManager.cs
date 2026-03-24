using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
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

        PopulateStagesFromBuildSettings();
        PopulateFeaturesFromBuildSettingsAndInspector();
    }

    void PopulateStagesFromBuildSettings()
    {
        if (stagesContainer == null) return;

        // Clear existing children
        foreach (Transform t in stagesContainer) Destroy(t.gameObject);

        int count = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"Found {count} scenes in Build Settings. Populating stage buttons...");
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(path);

            // Only include scenes that follow the Stage naming convention: "Stage..." (case-insensitive)
            if (!sceneName.StartsWith("Stage", StringComparison.OrdinalIgnoreCase))
                continue;

            GameObject go = Instantiate(buttonPrefab);
            go.transform.SetParent(stagesContainer, false);

            // robustly find and set text for either TextMeshProUGUI or UnityEngine.UI.Text
            var txtTransform = go.transform.Find("Text");
            if (txtTransform != null)
            {
                var tmpUGUI = txtTransform.GetComponent<TextMeshProUGUI>();
                if (tmpUGUI != null)
                {
                    tmpUGUI.text = sceneName;
                }
                else
                {
                    var uiText = txtTransform.GetComponent<Text>();
                    if (uiText != null)
                    {
                        uiText.text = sceneName;
                    }
                    else
                    {
                        Debug.LogWarning("TestSceneManager: 'Text' child found but no TextMeshProUGUI or Text component present on prefab.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("TestSceneManager: button prefab does not contain a child named 'Text'.");
            }

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                string capturedName = sceneName;
                btn.onClick.AddListener(() => LoadStage(capturedName));
            }
            else
            {
                Debug.LogWarning("TestSceneManager: instantiated prefab does not have a Button component.");
            }
        }
    }

    void PopulateFeaturesFromBuildSettingsAndInspector()
    {
        if (featuresContainer == null) return;

        // Clear existing children
        foreach (Transform t in featuresContainer) Destroy(t.gameObject);

        int count = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"Found {count} scenes in Build Settings. Populating feature-scene buttons...");
        // First, auto-add scenes that follow the Feat naming convention: "Feat..."
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(path);

            if (!sceneName.StartsWith("Feat", StringComparison.OrdinalIgnoreCase))
                continue;

            GameObject go = Instantiate(buttonPrefab);
            go.transform.SetParent(featuresContainer, false);

            var txtTransform = go.transform.Find("Text");
            if (txtTransform != null)
            {
                var tmpUGUI = txtTransform.GetComponent<TextMeshProUGUI>();
                if (tmpUGUI != null)
                {
                    tmpUGUI.text = sceneName;
                }
                else
                {
                    var uiText = txtTransform.GetComponent<Text>();
                    if (uiText != null)
                    {
                        uiText.text = sceneName;
                    }
                    else
                    {
                        Debug.LogWarning("TestSceneManager: 'Text' child found but no TextMeshProUGUI or Text component present on prefab.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("TestSceneManager: button prefab does not contain a child named 'Text'.");
            }

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                string capturedName = sceneName;
                btn.onClick.AddListener(() => LoadStage(capturedName));
            }
            else
            {
                Debug.LogWarning("TestSceneManager: instantiated prefab does not have a Button component.");
            }
        }

        // Then add any features configured manually in the Inspector (these invoke UnityEvents)
        foreach (var feat in features)
        {
            GameObject go = Instantiate(buttonPrefab);
            go.transform.SetParent(featuresContainer, false);

            var txtTransform = go.transform.Find("Text");
            if (txtTransform != null)
            {
                var tmpUGUI = txtTransform.GetComponent<TextMeshProUGUI>();
                if (tmpUGUI != null)
                {
                    tmpUGUI.text = feat.name;
                }
                else
                {
                    var uiText = txtTransform.GetComponent<Text>();
                    if (uiText != null)
                    {
                        uiText.text = feat.name;
                    }
                    else
                    {
                        Debug.LogWarning("TestSceneManager: 'Text' child found but no TextMeshProUGUI or Text component present on prefab.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("TestSceneManager: button prefab does not contain a child named 'Text'.");
            }

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                UnityAction action = () => feat.onClick?.Invoke();
                btn.onClick.AddListener(action);
            }
            else
            {
                Debug.LogWarning("TestSceneManager: instantiated prefab does not have a Button component.");
            }
        }
    }

    // Called when a stage button is clicked
    public void LoadStage(string sceneName)
    {
        // Optional: play SFX, show confirmation, etc.
        //SceneManager.LoadScene(sceneName);
        // For simplicity, we assume the scene name format is "StageX_Description" or "FeatY_Description", and we want to load just "StageX" or "FeatY"
        string result = sceneName.Split('_')[0];
        SceneLoader.Instance.LoadScene(result);
    }

}