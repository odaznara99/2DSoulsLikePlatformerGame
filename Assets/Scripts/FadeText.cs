using UnityEngine;
using TMPro; // For TextMeshProUGUI
using System.Collections;

public class FadeText : MonoBehaviour
{
    public TextMeshProUGUI textObject;
    public float fadeDuration = 1.5f;
    private float minAlpha = 0f;
    private float maxAlpha = 1f;

    void Start()
    {
        if (textObject == null)
            textObject = GetComponent<TextMeshProUGUI>();
    }

    public void FadeIn()
    {
        StartCoroutine(FadeTo(maxAlpha));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeTo(minAlpha));
    }

    public void FadeInThenOut(float stayDuration = 2f)
    {
        StartCoroutine(FadeInThenOutCor(stayDuration));
    }

    IEnumerator FadeInThenOutCor(float stayDuration)
    {
        // Fade In
        yield return StartCoroutine(FadeTo(maxAlpha));
        // Optional wait time at full opacity
        yield return new WaitForSeconds(stayDuration); // Optional wait time at full opacity
        // Fade Out
        yield return StartCoroutine(FadeTo(minAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        Color currentColor = textObject.color;
        float startAlpha = currentColor.a;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            textObject.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        textObject.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}
