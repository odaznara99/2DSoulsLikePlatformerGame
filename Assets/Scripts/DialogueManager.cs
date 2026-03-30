using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Singleton manager responsible for displaying and advancing dialogue sequences.
/// Wire up the UI references via the Inspector or place a GameObject named
/// "DialoguePanel" under "ScreenCanvas" in your scene for auto-discovery.
///
/// Usage:
///   DialogueManager.Instance.StartDialogue(dialogueData);
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Dialogue UI")]
    [Tooltip("The root panel for the dialogue UI. Assign in Inspector or rely on auto-find.")]
    public GameObject dialoguePanel;

    [Tooltip("GameObject that wraps the speaker name label; hidden when speaker name is empty.")]
    public GameObject speakerNameBox;

    [Tooltip("TextMeshPro component for the speaker name.")]
    public TextMeshProUGUI speakerNameText;

    [Tooltip("TextMeshPro component for the dialogue body text.")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Button to advance to the next line (also advances when E is pressed).")]
    public Button continueButton;

    [Header("Typewriter Settings")]
    [Tooltip("Seconds between each character appearing (uses real time, unaffected by pause).")]
    public float typewriterDelay = 0.04f;

    private DialogueLine[] currentLines;
    private int currentLineIndex;
    private bool isTyping;
    private Coroutine typewriterCoroutine;

    // ── Unity lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        AutoFindUIReferences();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            OnContinueClicked();
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>Begins displaying a dialogue sequence from a <see cref="DialogueData"/> asset.</summary>
    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager: No dialogue lines to display.");
            return;
        }

        AutoFindUIReferences();

        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialogueManager: dialoguePanel reference is missing. Cannot show dialogue.");
            return;
        }

        currentLines = data.lines;
        currentLineIndex = 0;

        dialoguePanel.SetActive(true);
        GameManager.Instance?.PauseSilent(true);

        ShowLine(currentLineIndex);
    }

    /// <summary>Ends the active dialogue and resumes gameplay.</summary>
    public void EndDialogue()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;

        if (dialogueText != null)
            dialogueText.maxVisibleCharacters = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        GameManager.Instance?.PauseSilent(false);
    }

    /// <summary>Returns <c>true</c> while a dialogue sequence is being shown.</summary>
    public bool IsDialogueActive() => dialoguePanel != null && dialoguePanel.activeSelf;

    // ── Internal helpers ────────────────────────────────────────────────────

    private void ShowLine(int index)
    {
        DialogueLine line = currentLines[index];

        // Speaker name
        bool hasSpeaker = !string.IsNullOrEmpty(line.speakerName);
        if (speakerNameBox != null)
            speakerNameBox.SetActive(hasSpeaker);
        if (speakerNameText != null)
            speakerNameText.text = hasSpeaker ? line.speakerName : string.Empty;

        // Start typewriter
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(line.text));
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;
        dialogueText.text = fullText;
        dialogueText.maxVisibleCharacters = 0;

        int totalChars = fullText.Length;
        for (int i = 0; i <= totalChars; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typewriterDelay);
        }

        isTyping = false;
    }

    private void OnContinueClicked()
    {
        if (isTyping)
        {
            // Skip typewriter — reveal full line immediately
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            isTyping = false;
            return;
        }

        currentLineIndex++;
        if (currentLineIndex < currentLines.Length)
        {
            ShowLine(currentLineIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Attempts to locate UI references automatically by searching for a
    /// "DialoguePanel" child under "ScreenCanvas" in the active scene.
    /// </summary>
    private void AutoFindUIReferences()
    {
        if (dialoguePanel != null) return;

        Transform found = GameObject.Find("ScreenCanvas")?.transform.Find("DialoguePanel");
        if (found == null) return;

        dialoguePanel = found.gameObject;

        if (speakerNameBox == null)
            speakerNameBox = found.Find("SpeakerNameBox")?.gameObject;

        if (speakerNameText == null)
            speakerNameText = found.Find("SpeakerNameBox/SpeakerNameText")?.GetComponent<TextMeshProUGUI>();

        if (dialogueText == null)
            dialogueText = found.Find("DialogueText")?.GetComponent<TextMeshProUGUI>();

        if (continueButton == null)
        {
            continueButton = found.Find("ContinueButton")?.GetComponent<Button>();
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        Debug.Log("DialogueManager: UI references auto-assigned.");
    }
}
