using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Attach this component to any NPC GameObject.
/// When the player enters the trigger zone and presses E (or taps the interact
/// button), the assigned <see cref="DialogueData"/> sequence is played via
/// <see cref="DialogueManager"/>.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [Tooltip("The dialogue data to play when the player interacts with this NPC.")]
    public DialogueData dialogueData;

    [Header("Interaction Prompt")]
    [Tooltip("Optional UI button shown when the player is within range (e.g. a mobile on-screen button or world-space prompt).")]
    public Button interactButton;

    private bool isPlayerNearby;
    private bool isButtonVisible;

    // ── Unity lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
            interactButton.onClick.AddListener(TriggerDialogue);
        }
    }

    private void Update()
    {
        bool dialogueActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();
        bool shouldShowButton = isPlayerNearby && !dialogueActive;

        if (interactButton != null && shouldShowButton != isButtonVisible)
        {
            isButtonVisible = shouldShowButton;
            interactButton.gameObject.SetActive(isButtonVisible);
        }

        if (!isPlayerNearby || dialogueActive) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TriggerDialogue();
    }

    // ── Trigger detection ────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactButton != null && isButtonVisible)
            {
                isButtonVisible = false;
                interactButton.gameObject.SetActive(false);
            }
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Starts the dialogue assigned to this NPC.</summary>
    public void TriggerDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning($"DialogueTrigger on '{gameObject.name}': No DialogueData assigned.");
            return;
        }

        if (interactButton != null)
            interactButton.gameObject.SetActive(false);

        DialogueManager.Instance?.StartDialogue(dialogueData);
    }
}
