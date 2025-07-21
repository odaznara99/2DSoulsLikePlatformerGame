using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPreviousLevel : MonoBehaviour
{

    private void Start()
    {
        this.gameObject.SetActive(false); // Disable the object at start
        Invoke(nameof(Enable), 3f); // Enable it after a short delay
    }

    private void Enable()
    {
        this.gameObject.SetActive(true); // Enable the object after the delay
    }

    private bool wasLoading = false; // To prevent multiple triggers
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !wasLoading)
        {
            wasLoading = true;
            // Load the next level
            AudioManager.Instance.PlaySFX("Click");
            //SceneLoader.Instance.LoadPreviousScene();
            //SceneLoader.Instance.LoadScene(GameManager.instance.lastSceneName);
        }
    }

    
}
