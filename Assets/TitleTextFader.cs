using UnityEngine;
using TMPro; // For TextMeshProUGUI
using System.Collections;

public class TitleTextFader : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public float fadeDuration = 1.5f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;

    void Start()
    {
        if (titleText == null)
            titleText = GetComponent<TextMeshProUGUI>();

        StartCoroutine(FadeLoop());
    }

    private IEnumerator FadeLoop()
    {
        while (true)
        {
            // Fade In
            yield return StartCoroutine(FadeTo(maxAlpha));
            // Fade Out
            yield return StartCoroutine(FadeTo(minAlpha));
        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        Color currentColor = titleText.color;
        float startAlpha = currentColor.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            titleText.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        titleText.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}
