using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPreviousLevel : MonoBehaviour
{

    private bool wasLoading = false; // To prevent multiple triggers
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !wasLoading)
        {
            wasLoading = true;
            // Load the next level
            AudioManager.Instance.PlaySFX("Click");
            //SceneLoader.Instance.LoadPreviousScene();
            SceneLoader.Instance.LoadScene(GameManager.instance.lastSceneName);
        }
    }
}
