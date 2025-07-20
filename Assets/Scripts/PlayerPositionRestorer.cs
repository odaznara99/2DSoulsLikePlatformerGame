using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionRestorer : MonoBehaviour
{
    private void Start()
    {
        // Check if the player is returning to this scene
        if (GameManager.instance != null && GameManager.instance.lastSceneName == SceneManager.GetActiveScene().name)
        {
            // Restore the player's position
            transform.position = GameManager.instance.lastPlayerPosition;
        }
    }
}
