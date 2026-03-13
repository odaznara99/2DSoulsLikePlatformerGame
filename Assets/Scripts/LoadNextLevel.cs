using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadNextLevel : MonoBehaviour
{
    public string sceneName; // Name of the scene to load (can be "Stage1_Scenename")
    public SpawnPointType spawnPointType; // Name of the spawn point to use

    private void Start()
    {
        locationButton.onClick.AddListener(OnButtonClick);
        wasLoading = false;
        this.gameObject.SetActive(false); // Disable the object at start
        Invoke(nameof(Enable), 1f); // Enable it after a short delay
    }

    private void Enable()
    {
            this.gameObject.SetActive(true); // Enable the object after the delay
    }

    private bool isPlayerNearby = false;
    private bool wasLoading = false; // To prevent multiple triggers

    private bool CanLoadNextScene()
    {
        // Check if the next scene can be loaded
        return isPlayerNearby && !wasLoading;
    }

    private void LoadSpecificScene()
    {
        // Play click SFX
        AudioManager.Instance.PlaySFX("Click");

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

        // Fallback: if no match found, targetScene remains the original sceneName
        Debug.Log($"Loading scene: {targetScene} with spawn point: {spawnPointType}");
        SceneLoader.Instance.LoadScene(targetScene, spawnPointType);
    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false ;
        }
    }

    private void Update()
    {
        if (CanLoadNextScene())
        {
            locationButton.gameObject.SetActive(true); // Show the button when player is nearby

            if (Input.GetKeyDown(KeyCode.E))
            {
                OnButtonClick();
            }
        } 
        else
        {
            locationButton.gameObject.SetActive(false); // Hide the button when player is not nearby
        }
    }

    public Button locationButton; // Reference to the UI Button

    private void OnButtonClick()
    {
        AudioManager.Instance.PlaySFX("Click");
        //Debug.Log("Enter Button was clicked!");
        // Add your logic here
        wasLoading = true; // Set loading state to true
        LoadSpecificScene(); // Load the specified scene
    }

}
