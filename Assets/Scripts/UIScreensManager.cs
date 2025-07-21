using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UIScreensManager : MonoBehaviour
{
    [System.Serializable]
    public class UIScreen
    {
        public string screenName;
        public GameObject screenObject;
    }

    public List<UIScreen> screens;

    private Dictionary<string, GameObject> screenDict;

    public static UIScreensManager Instance;

    private void Awake()
    {
        screenDict = new Dictionary<string, GameObject>();

        foreach (var screen in screens)
        {
            if (!screenDict.ContainsKey(screen.screenName))
            {
                screenDict.Add(screen.screenName, screen.screenObject);
            }
            else
            {
                Debug.LogWarning($"Duplicate screen name: {screen.screenName}");
            }
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void ShowScreenHideOthers(string name)
    {
        foreach (var screen in screenDict.Values)
        {
            screen.SetActive(false);
        }

        if (screenDict.TryGetValue(name, out GameObject target))
        {
            target.SetActive(true);
        }
        else
        {
            Debug.LogError($"Screen {name} not found!");
        }
    }

    public void ShowScreen(string name)
    {
        if (screenDict.TryGetValue(name, out GameObject target))
        {
            target.SetActive(true);
        }
        else
        {
            Debug.LogError($"Screen {name} not found!");
        }
    }

    public void HideScreen(string name)
    {
        if (screenDict.TryGetValue(name, out GameObject target))
        {
            target.SetActive(false);
        }
    }

    public void HideAllScreens()
    {
        foreach (var screen in screenDict.Values)
        {
            screen.SetActive(false);
        }
    }

    public bool IsScreenActive(string name)
    {
        return screenDict.ContainsKey(name) && screenDict[name].activeSelf;
    }

    public void ShowScreenWithFadeIn(string name, float duration)
    {
        StartCoroutine(FadeInScreen(name, duration));
    }

    private IEnumerator FadeInScreen(string name, float duration)
    {
        HideAllScreens(); // optional

        if (!screenDict.TryGetValue(name, out GameObject screen))
        {
            Debug.LogError($"Screen {name} not found!");
            yield break;
        }

        screen.SetActive(true);

        CanvasGroup cg = screen.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogWarning("No CanvasGroup found. Fading skipped.");
            yield break;
        }

        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(timer / duration);
            yield return null;
        }

        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        
    }
}
