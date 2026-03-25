using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        // Fallback: if no match found, targetScene remains the original sceneName
        Debug.Log($"Loading scene: {sceneName} with spawn point: {spawnPointType}");
        SceneLoader.Instance.LoadScene(sceneName);
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

            if (Keyboard.current.eKey.wasPressedThisFrame)
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
