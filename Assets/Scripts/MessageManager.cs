using UnityEngine;
using TMPro;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance;

    [Header("Message UI")]
    public GameObject levelNameObject;
    public TMP_Text levelNameText;
    public float levelNameDuration = 2f;
    public float fadeDuration = 0.5f;

    [Header("Fade Text References")]
    public FadeText victoryAchievedText;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        //AutoFindUIReferences();
    }

    private void Start()
    {
        AutoFindUIReferences();

        if (levelNameObject != null)
        {
            levelNameObject.SetActive(false); // Ensure the message box is initially hidden
        }
        else
            Debug.LogWarning("MessageBox not found! Please assign it in the inspector or ensure it exists in the scene.");

    }

    private void AutoFindUIReferences()
    {
        if (levelNameObject == null)
        {
            Transform found = GameObject.Find("ScreenCanvas")?.transform.Find("MessageBox");
            if (found != null)
                levelNameObject = found.gameObject;
            Debug.Log("Message UI references was auto-assigned.");
        }

        if (levelNameObject != null)
        {
            if (canvasGroup == null)
                canvasGroup = levelNameObject.GetComponent<CanvasGroup>();

            if (levelNameText == null)
                levelNameText = levelNameObject.GetComponentInChildren<TMP_Text>(true); // true = include inactive
        } 
    }

    public void ShowMessage(string message, bool blink = false, int fontSize = 72)
    {
        if (levelNameObject == null)
            AutoFindUIReferences();

        if (levelNameObject == null || levelNameText == null || canvasGroup == null)
        {
            Debug.LogWarning("MessageManager is still missing UI references.");
            return;
        }

        levelNameText.fontSize = fontSize;
        StopAllCoroutines();

        if (blink)
        {
            StartCoroutine(BlinkMessageRoutine(message));
        }
        else
        {
            StartCoroutine(ShowMessageRoutine(message));
        }
    }



    private IEnumerator ShowMessageRoutine(string message)
    {
        levelNameText.text = message;
        levelNameObject.SetActive(true);

        // Fade In
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // Wait while fully visible
        yield return new WaitForSeconds(levelNameDuration);

        // Fade Out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        levelNameObject.SetActive(false);
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float timer = 0f;
        canvasGroup.alpha = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator BlinkMessageRoutine(string message)
    {
        levelNameText.text = message;
        levelNameObject.SetActive(true);
        canvasGroup.alpha = 1f;

        float blinkTime = 0.5f;
        float timer = 0f;

        while (timer < levelNameDuration)
        {
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(blinkTime);
            canvasGroup.alpha = 0f;
            yield return new WaitForSeconds(blinkTime);

            timer += blinkTime * 2f;
        }

        canvasGroup.alpha = 0f;
        levelNameObject.SetActive(false);
    }

}
