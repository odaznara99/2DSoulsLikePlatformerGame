using UnityEngine;
using TMPro;
using System.Collections; // Or UnityEngine.UI if using legacy UI

public class BlinkText : MonoBehaviour
{
    public TextMeshProUGUI text; // Assign via Inspector
    public float blinkSpeed = 1.0f;

    void Start()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            // Fade out
            for (float alpha = 1f; alpha >= 0f; alpha -= Time.deltaTime * blinkSpeed)
            {
                SetAlpha(alpha);
                yield return null;
            }

            // Fade in
            for (float alpha = 0f; alpha <= 1f; alpha += Time.deltaTime * blinkSpeed)
            {
                SetAlpha(alpha);
                yield return null;
            }
        }
    }

    void SetAlpha(float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}
