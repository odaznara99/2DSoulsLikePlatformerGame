using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the in-game shop panel.
/// Attach to the root shop UI GameObject and wire up all references in the Inspector.
///
/// Usage
/// -----
/// • Add a ShopUI component somewhere in the persistent canvas hierarchy.
/// • Assign <see cref="itemSlotPrefab"/> (a prefab with <see cref="ShopItemSlotUI"/>).
/// • The shop is opened by <see cref="ShopNPC"/> via <see cref="OpenShop"/>.
///
/// The game is silently paused (<see cref="GameManager.PauseSilent"/>) while the
/// shop is open so the player cannot move or attack.  Closing the shop (Escape /
/// E key / Close button) resumes the game.
/// </summary>
public class ShopUI : MonoBehaviour
{
    // ── Inspector References ──────────────────────────────────────────────────

    [Header("Panel")]
    [Tooltip("Root panel GameObject of the shop UI. Shown/hidden by this component.")]
    public GameObject shopPanel;

    [Header("Shop Header")]
    [Tooltip("Label displaying the shop's name.")]
    public TextMeshProUGUI shopNameText;

    [Tooltip("Label displaying the shopkeeper's greeting dialogue.")]
    public TextMeshProUGUI shopkeeperDialogueText;

    [Tooltip("Label showing the player's current Souls.")]
    public TextMeshProUGUI playerSoulsText;

    [Tooltip("Label showing the player's current Coins.")]
    public TextMeshProUGUI playerCoinsText;

    [Header("Item Grid")]
    [Tooltip("Parent transform where item-slot GameObjects are instantiated.")]
    public Transform itemGrid;

    [Tooltip("Prefab that contains a ShopItemSlotUI component.")]
    public GameObject itemSlotPrefab;

    [Header("Item Detail Panel")]
    [Tooltip("Panel that shows details about the selected item.")]
    public GameObject itemDetailPanel;

    [Tooltip("Icon image inside the detail panel.")]
    public Image itemDetailIcon;

    [Tooltip("Item name label inside the detail panel.")]
    public TextMeshProUGUI itemDetailName;

    [Tooltip("Item description label inside the detail panel.")]
    public TextMeshProUGUI itemDetailDescription;

    [Tooltip("Price label inside the detail panel.")]
    public TextMeshProUGUI itemDetailPrice;

    [Tooltip("Button that triggers the purchase of the selected item.")]
    public Button buyButton;

    [Tooltip("Text on the buy button (changes to 'Purchased' for one-time items already bought).")]
    public TextMeshProUGUI buyButtonText;

    [Header("Close Button")]
    [Tooltip("Button that closes the shop.")]
    public Button closeButton;

    [Header("Audio")]
    [Tooltip("SFX clip name played on a successful purchase.")]
    public string purchaseSfxName = "";

    [Tooltip("SFX clip name played when the player cannot afford an item.")]
    public string purchaseFailSfxName = "";

    // ── Private State ─────────────────────────────────────────────────────────

    private ShopInventory currentInventory;
    private ShopItem selectedItem;
    private readonly List<ShopItemSlotUI> itemSlots = new List<ShopItemSlotUI>();

    // Cached player component references (resolved once on open, cleared on close)
    private PlayerControllerVersion2 cachedPlayer;
    private PlayerHealth  cachedHealth;
    private PlayerStamina cachedStamina;

    // Maximum shield damage reduction (prevents full immunity, same cap as MemoryShardPickup)
    private const float MaxDamageReduction = 0.99f;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        if (buyButton != null)
            buyButton.onClick.AddListener(BuySelectedItem);
    }

    private void Update()
    {
        if (shopPanel == null || !shopPanel.activeSelf) return;

        // Allow closing with Escape or the same Interact key used to open the shop
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            CloseShop();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Opens the shop panel and populates it with items from <paramref name="inventory"/>.</summary>
    public void OpenShop(ShopInventory inventory)
    {
        currentInventory = inventory;
        selectedItem = null;

        // Cache player references for the duration of this shop session
        cachedPlayer  = FindAnyObjectByType<PlayerControllerVersion2>();
        cachedHealth  = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerHealth>()  : null;
        cachedStamina = cachedPlayer != null ? cachedPlayer.GetComponent<PlayerStamina>() : null;

        // Silently pause the game so the player cannot move or fight while shopping
        GameManager.Instance?.PauseSilent(true);

        // Populate header
        if (shopNameText != null)
            shopNameText.text = inventory.shopName;

        if (shopkeeperDialogueText != null)
            shopkeeperDialogueText.text = inventory.shopkeeperDialogue;

        RefreshCurrencyDisplay();
        PopulateItems();

        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(true);
    }

    /// <summary>Closes the shop panel and resumes the game.</summary>
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        GameManager.Instance?.PauseSilent(false);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private void RefreshCurrencyDisplay()
    {
        if (GameManager.Instance == null) return;

        var pd = GameManager.Instance.playerData;

        if (playerSoulsText != null)
            playerSoulsText.text = "Souls: " + pd.souls;

        if (playerCoinsText != null)
            playerCoinsText.text = "Coins: " + pd.coins;
    }

    private void PopulateItems()
    {
        // Destroy existing slots
        foreach (var slot in itemSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        itemSlots.Clear();

        if (itemSlotPrefab == null || itemGrid == null || currentInventory == null) return;

        foreach (var item in currentInventory.items)
        {
            if (item == null) continue;

            bool alreadyPurchased = item.isOneTimePurchase
                && GameManager.Instance != null
                && GameManager.Instance.playerData.purchasedItemIds.Contains(item.itemId);

            GameObject go = Instantiate(itemSlotPrefab, itemGrid);
            var slot = go.GetComponent<ShopItemSlotUI>();
            if (slot != null)
            {
                slot.Setup(item, alreadyPurchased, OnItemSlotClicked);
                itemSlots.Add(slot);
            }
        }
    }

    private void OnItemSlotClicked(ShopItem item)
    {
        selectedItem = item;
        ShowItemDetail(item);
    }

    private void ShowItemDetail(ShopItem item)
    {
        if (itemDetailPanel == null) return;

        itemDetailPanel.SetActive(true);

        if (itemDetailName != null)
            itemDetailName.text = item.itemName;

        if (itemDetailDescription != null)
            itemDetailDescription.text = item.description;

        if (itemDetailIcon != null)
        {
            itemDetailIcon.sprite = item.icon;
            itemDetailIcon.enabled = item.icon != null;
        }

        string currencyLabel = item.currencyType == ShopCurrencyType.Souls ? "Souls" : "Coins";
        if (itemDetailPrice != null)
            itemDetailPrice.text = $"{item.price} {currencyLabel}";

        bool alreadyPurchased = item.isOneTimePurchase
            && GameManager.Instance != null
            && GameManager.Instance.playerData.purchasedItemIds.Contains(item.itemId);

        if (buyButton != null)
            buyButton.interactable = !alreadyPurchased;

        if (buyButtonText != null)
            buyButtonText.text = alreadyPurchased ? "Purchased" : "Buy";
    }

    private void BuySelectedItem()
    {
        if (selectedItem == null) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        var pd = gm.playerData;

        // Guard: already purchased (one-time items)
        if (selectedItem.isOneTimePurchase && pd.purchasedItemIds.Contains(selectedItem.itemId))
        {
            MessageManager.Instance?.ShowMessage("Already purchased!");
            return;
        }

        // Guard: insufficient funds
        int playerCurrency = selectedItem.currencyType == ShopCurrencyType.Souls
            ? pd.souls
            : pd.coins;

        if (playerCurrency < selectedItem.price)
        {
            string currency = selectedItem.currencyType == ShopCurrencyType.Souls ? "Souls" : "Coins";
            MessageManager.Instance?.ShowMessage($"Not enough {currency}!");

            if (!string.IsNullOrEmpty(purchaseFailSfxName))
                AudioManager.Instance?.PlaySFX(purchaseFailSfxName);
            return;
        }

        // Deduct currency
        if (selectedItem.currencyType == ShopCurrencyType.Souls)
            gm.AddSouls(-selectedItem.price);
        else
            gm.AddCoins(-selectedItem.price);

        // Apply the item's gameplay effect
        ApplyItemEffect(selectedItem);

        // Track one-time purchases
        if (selectedItem.isOneTimePurchase)
        {
            pd.purchasedItemIds.Add(selectedItem.itemId);
            RefreshSlotForItem(selectedItem);
        }

        // Refresh UI
        RefreshCurrencyDisplay();

        if (itemDetailPanel != null && itemDetailPanel.activeSelf)
            ShowItemDetail(selectedItem);

        if (!string.IsNullOrEmpty(purchaseSfxName))
            AudioManager.Instance?.PlaySFX(purchaseSfxName);

        MessageManager.Instance?.ShowMessage("Purchased: " + selectedItem.itemName);
    }

    /// <summary>
    /// Applies the gameplay effect of <paramref name="item"/> to the player.
    /// Mirrors the logic in <see cref="MemoryShardPickup"/> so bonuses are
    /// stored in <see cref="PlayerData"/> and survive scene reloads.
    /// </summary>
    private void ApplyItemEffect(ShopItem item)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        var pd      = gm.playerData;
        var player  = cachedPlayer;
        var health  = cachedHealth;
        var stamina = cachedStamina;

        switch (item.effect)
        {
            case ShopItemEffect.RestoreHealth:
            {
                if (health != null)
                    health.Heal(Mathf.RoundToInt(item.effectValue > 0f ? item.effectValue : 30f));
                break;
            }

            case ShopItemEffect.IncreaseMaxHealth:
            {
                float amount = item.effectValue > 0f ? item.effectValue : 20f;
                if (health != null)
                {
                    health.maxHealth += amount;
                    health.RefreshHealthUI();
                    pd.maxHealth = health.maxHealth;
                }
                pd.bonusMaxHealth += amount;
                break;
            }

            case ShopItemEffect.IncreaseDamage:
            {
                float amount = item.effectValue > 0f ? item.effectValue : 5f;
                if (player != null)
                    player.attackDamage += amount;
                pd.bonusAttackDamage += amount;
                break;
            }

            case ShopItemEffect.IncreaseDamageReduction:
            {
                float amount = item.effectValue > 0f ? item.effectValue : 0.05f;
                if (health != null)
                    health.shieldDamageReduction = Mathf.Min(
                        health.shieldDamageReduction + amount, MaxDamageReduction);
                pd.bonusDamageReduction += amount;
                break;
            }

            case ShopItemEffect.IncreaseMovementSpeed:
            {
                float amount = item.effectValue > 0f ? item.effectValue : 0.5f;
                if (player != null)
                {
                    player.movementSpeed += amount;
                    player.SetFloatMovementSpeed(player.movementSpeed);
                }
                pd.bonusMovementSpeed += amount;
                break;
            }

            case ShopItemEffect.ExtraJump:
            {
                int jumps = item.effectValue > 0f ? Mathf.RoundToInt(item.effectValue) : 1;
                if (player != null)
                    player.maxDoubleJumpCount += jumps;
                pd.bonusJumpCount += jumps;
                break;
            }

            case ShopItemEffect.FasterStaminaRegen:
            {
                if (stamina != null)
                {
                    stamina.LevelUp(1, false);
                    pd.staminaRelicLevel = stamina.level;
                }
                break;
            }
        }
    }

    private void RefreshSlotForItem(ShopItem item)
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.Item == item)
            {
                slot.SetPurchased(true);
                break;
            }
        }
    }
}
