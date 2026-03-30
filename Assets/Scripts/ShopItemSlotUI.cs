using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single item slot in the shop UI grid.
/// Wire up the sub-object references in the Inspector, then call
/// <see cref="Setup"/> from <see cref="ShopUI"/> when building the item list.
/// </summary>
public class ShopItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image used to display the item's icon sprite.")]
    public Image itemIcon;

    [Tooltip("Label showing the item's display name.")]
    public TextMeshProUGUI itemNameText;

    [Tooltip("Label showing the item's price and currency symbol.")]
    public TextMeshProUGUI itemPriceText;

    [Tooltip("Optional overlay (e.g. a 'SOLD' banner) shown when a one-time item has been purchased.")]
    public GameObject purchasedOverlay;

    [Tooltip("Button that the player clicks to select / preview this item.")]
    public Button selectButton;

    // ── Runtime data ──────────────────────────────────────────────────────────

    private ShopItem item;
    private Action<ShopItem> onClickCallback;

    /// <summary>The shop item this slot represents.</summary>
    public ShopItem Item => item;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the slot with the given item data and click handler.
    /// Called by <see cref="ShopUI"/> after instantiating the slot prefab.
    /// </summary>
    public void Setup(ShopItem shopItem, bool alreadyPurchased, Action<ShopItem> onClick)
    {
        item = shopItem;
        onClickCallback = onClick;

        if (itemIcon != null)
        {
            itemIcon.sprite = shopItem.icon;
            itemIcon.enabled = shopItem.icon != null;
        }

        if (itemNameText != null)
            itemNameText.text = shopItem.itemName;

        if (itemPriceText != null)
        {
            string currencySymbol = shopItem.currencyType == ShopCurrencyType.Souls ? "Souls" : "Coins";
            itemPriceText.text = $"{shopItem.price} {currencySymbol}";
        }

        SetPurchased(alreadyPurchased);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClickCallback?.Invoke(item));
        }
    }

    /// <summary>Marks this slot as purchased (or not), updating the overlay and button state.</summary>
    public void SetPurchased(bool purchased)
    {
        if (purchasedOverlay != null)
            purchasedOverlay.SetActive(purchased);

        if (selectButton != null)
            selectButton.interactable = !purchased;
    }
}
