using UnityEngine;
using TMPro;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance;

    [Header("Message UI")]
    public GameObject messageBox;
    public TMP_Text messageText;
    public float messageDuration = 2f;
    public float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        AutoFindUIReferences();
    }

    private void Start()
    {
        AutoFindUIReferences();
    }

    private void AutoFindUIReferences()
    {
        if (messageBox == null)
        {
            Transform found = GameObject.Find("ScreenCanvas")?.transform.Find("MessageBox");
            if (found != null)
                messageBox = found.gameObject;
        }

        if (messageBox != null)
        {
            if (canvasGroup == null)
                canvasGroup = messageBox.GetComponent<CanvasGroup>();

            if (messageText == null)
                messageText = messageBox.GetComponentInChildren<TMP_Text>(true); // true = include inactive
        }

        if (messageBox != null)
        {
            Debug.Log("Message UI references was auto-assigned.");
            messageBox.SetActive(false); // Ensure the message box is initially hidden
        }
        else
            Debug.LogWarning("MessageBox not found! Please assign it in the inspector or ensure it exists in the scene.");

    }

    public void ShowMessage(string message, bool blink = false, int fontSize = 72)
    {
        AutoFindUIReferences();
        if (messageBox == null || messageText == null || canvasGroup == null)
        {
            Debug.LogWarning("MessageManager is still missing UI references.");
            return;
        }

        messageText.fontSize = fontSize;
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
        messageText.text = message;
        messageBox.SetActive(true);

        // Fade In
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // Wait while fully visible
        yield return new WaitForSeconds(messageDuration);

        // Fade Out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        messageBox.SetActive(false);
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
        messageText.text = message;
        messageBox.SetActive(true);
        canvasGroup.alpha = 1f;

        float blinkTime = 0.5f;
        float timer = 0f;

        while (timer < messageDuration)
        {
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(blinkTime);
            canvasGroup.alpha = 0f;
            yield return new WaitForSeconds(blinkTime);

            timer += blinkTime * 2f;
        }

        canvasGroup.alpha = 0f;
        messageBox.SetActive(false);
    }

}
