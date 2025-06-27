using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("GameScene1"); // Use the actual scene name
    }

    public void LoadGame()
    {
        // Your load logic here
        Debug.Log("Load Game clicked");
    }

    public void Options()
    {
        Debug.Log("Options clicked");
        //SceneManager.LoadScene("Options");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

        // Quit doesn't work in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
