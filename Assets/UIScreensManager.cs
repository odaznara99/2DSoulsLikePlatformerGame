using UnityEngine;
using System.Collections.Generic;

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
}
