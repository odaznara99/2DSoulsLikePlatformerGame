using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to a shop NPC or shop-sign GameObject that has a trigger Collider2D.
/// When the player enters the trigger and presses the Interact button (or the
/// fallback key) the shop UI opens.
/// Works with any <see cref="ShopInventory"/> ScriptableObject assigned in the
/// Inspector, making this component reusable for weapon shops, consumable shops,
/// spell/ability shops, etc.
/// </summary>
public class ShopNPC : MonoBehaviour
{
    [Header("Shop Data")]
    [Tooltip("The inventory of items this shop NPC sells.")]
    public ShopInventory inventory;

    [Header("Interaction Settings")]
    [Tooltip("Fallback keyboard key used to open the shop when the player is in range.")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Audio")]
    [Tooltip("SFX clip name (registered in AudioManager) played when the shop opens. Leave empty to skip.")]
    public string openShopSfxName = "";

    [Header("Prompt UI")]
    [Tooltip("Optional GameObject shown while the player is inside the trigger zone (e.g. 'Press E to shop').")]
    public GameObject interactPrompt;

    // ── Private State ─────────────────────────────────────────────────────────

    private bool playerInRange;
    private ShopUI shopUI;
    private InputAction interactInputAction;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        shopUI = FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Cache the Interact input action so it doesn't get looked up every frame
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
            OpenShop();
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

    private void OpenShop()
    {
        if (inventory == null)
        {
            Debug.LogWarning("[ShopNPC] No ShopInventory assigned on " + gameObject.name, this);
            return;
        }

        if (shopUI == null)
        {
            Debug.LogWarning("[ShopNPC] No ShopUI found in scene. Add a ShopUI component to the UI canvas.", this);
            return;
        }

        shopUI.OpenShop(inventory);

        if (!string.IsNullOrEmpty(openShopSfxName))
            AudioManager.Instance?.PlaySFX(openShopSfxName);
    }
}
