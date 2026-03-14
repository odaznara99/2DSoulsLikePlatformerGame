using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject settingsScreen;
    public GameObject testingHubScreen;

    private void Start()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("Ballad");
    }
    public void NewGame()
    {
        //SceneManager.LoadScene("GameScene1"); // Use the actual scene name
        AudioManager.Instance.PlaySFX("Click");
        SceneLoader.Instance.LoadScene("Stage1",SpawnPointType.Start); // Use the SceneLoader to load the scene
    }

    public void LoadGame()
    {
        // Your load logic here
        AudioManager.Instance.PlaySFX("Click");
        Debug.Log("Load Game clicked");
    }

    public void TestGame(bool setActive)
    {
        // Your testing logic here
        AudioManager.Instance.PlaySFX("Click");
        Debug.Log("TestGame is set to" + setActive);
        testingHubScreen.SetActive(setActive);
    }

    public void Settings(bool setActive)
    {
        Debug.Log("Settings is set to" + setActive);
        AudioManager.Instance.PlaySFX("Click");
        settingsScreen.SetActive(setActive);
        //SceneManager.LoadScene("Options");
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySFX("Click");
        Debug.Log("Quit Game");
        Application.Quit();

        // Quit doesn't work in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
