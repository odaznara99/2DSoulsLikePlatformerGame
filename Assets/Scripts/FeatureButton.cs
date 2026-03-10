using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Optional helper for feature buttons you want to configure per-instance
public class FeatureButton : MonoBehaviour
{
    public Text label;
    public UnityEvent onClick;

    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetLabel(string text)
    {
        if (label) label.text = text;
    }
}