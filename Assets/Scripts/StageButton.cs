using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Optional small helper if you prefer to set up buttons manually in the editor
public class StageButton : MonoBehaviour
{
    public Text label;
    public string sceneName;

    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
        if (label != null) label.text = sceneName;
    }

    void OnClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}