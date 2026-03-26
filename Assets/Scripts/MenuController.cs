using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject settingsScreen;
    public GameObject testingHubScreen;

    public GameObject newGameButtonObject;
    public Button newGameButton;

    public GameObject loadGameButtonObject;
    public Button loadGameButton;

    private void Start()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic("Ballad");

        UpdateMainMenuButtons();
    }

    private void UpdateMainMenuButtons()
    {
        bool hasLocalSave = SaveManager.Instance != null && SaveManager.Instance.HasSave();

        SetButtonState(newGameButtonObject, newGameButton, !hasLocalSave);
        SetButtonState(loadGameButtonObject, loadGameButton, hasLocalSave);
    }

    private void SetButtonState(GameObject buttonObject, Button button, bool enabledAndVisible)
    {
        if (buttonObject != null)
        {
            buttonObject.SetActive(enabledAndVisible);
        }

        if (button != null)
        {
            button.interactable = enabledAndVisible;
        }
    }

    public void NewGame()
    {
        AudioManager.Instance.PlaySFX("Click");
        SceneLoader.Instance.LoadScene("Stage1");
    }

    public void LoadGame()
    {
        AudioManager.Instance.PlaySFX("Click");

        if (SaveManager.Instance != null && SaveManager.Instance.LoadSavedGame())
        {
            Debug.Log("Loaded save from disk.");
            return;
        }

        Debug.LogWarning("No save found. Starting a new game.");
        SceneLoader.Instance.LoadScene("Stage1");
    }

    public void TestGame(bool setActive)
    {
        AudioManager.Instance.PlaySFX("Click");
        Debug.Log("TestGame is set to" + setActive);
        testingHubScreen.SetActive(setActive);
    }

    public void Settings(bool setActive)
    {
        Debug.Log("Settings is set to" + setActive);
        AudioManager.Instance.PlaySFX("Click");
        settingsScreen.SetActive(setActive);
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySFX("Click");
        Debug.Log("Quit Game");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
