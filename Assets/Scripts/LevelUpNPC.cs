using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to a Bonfire Keeper (or any level-up NPC) that has a trigger Collider2D.
/// When the player enters the trigger and presses the Interact button (or the
/// fallback key) the Level-Up UI opens.
///
/// Setup
/// -----
/// 1. Add this component to a NPC or Bonfire GameObject.
/// 2. Add a trigger <c>Collider2D</c> to define the interaction zone.
/// 3. Assign the <see cref="LevelUpUI"/> reference in the Inspector.
/// 4. Optionally assign an <see cref="interactPrompt"/> that shows while the
///    player is in range (e.g. "Press E to level up").
/// </summary>
public class LevelUpNPC : MonoBehaviour
{
    [Header("Level Up UI")]
    [Tooltip("The LevelUpUI component on the persistent canvas. Assign in the Inspector.")]
    public LevelUpUI levelUpUI;

    [Header("Interaction Settings")]
    [Tooltip("Fallback keyboard key used to open the level-up panel when the player is in range.")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Audio")]
    [Tooltip("SFX clip name played when the level-up panel opens. Leave empty to skip.")]
    public string openSfxName = "";

    [Header("Prompt UI")]
    [Tooltip("Optional GameObject shown while the player is inside the trigger zone.")]
    public GameObject interactPrompt;

    // ── Private State ─────────────────────────────────────────────────────────

    private bool playerInRange;
    private InputAction interactInputAction;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (levelUpUI == null)
            levelUpUI = FindAnyObjectByType<LevelUpUI>(FindObjectsInactive.Include);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        var actions = InputSystem.actions;
        if (actions != null)
            interactInputAction = actions.FindAction("Interact");
    }

    private void Update()
    {
        if (!playerInRange) return;

        bool interactPressed =
            (interactInputAction != null && interactInputAction.WasPressedThisFrame())
            || Input.GetKeyDown(interactKey);

        if (interactPressed)
            OpenLevelUpPanel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void OpenLevelUpPanel()
    {
        if (levelUpUI == null)
        {
            Debug.LogWarning("[LevelUpNPC] No LevelUpUI assigned on " + gameObject.name, this);
            return;
        }

        levelUpUI.OpenPanel();

        if (!string.IsNullOrEmpty(openSfxName))
            AudioManager.Instance?.PlaySFX(openSfxName);
    }
}
