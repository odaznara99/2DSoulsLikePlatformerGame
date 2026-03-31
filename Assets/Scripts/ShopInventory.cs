using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that holds the list of items sold by a specific shop or NPC.
/// Create via Assets → Create → Shop → Shop Inventory.
/// Assign to a <see cref="ShopNPC"/> to make that NPC sell these items.
/// </summary>
[CreateAssetMenu(fileName = "NewShopInventory", menuName = "Shop/Shop Inventory")]
public class ShopInventory : ScriptableObject
{
    [Header("Shop Identity")]
    [Tooltip("Display name shown as the shop title in the UI.")]
    public string shopName = "Shop";

    [Tooltip("Opening line spoken by the shopkeeper.")]
    [TextArea(2, 3)]
    public string shopkeeperDialogue = "Welcome, traveler. What can I get for you?";

    [Header("Items for Sale")]
    [Tooltip("Items available to purchase in this shop.")]
    public List<ShopItem> items = new List<ShopItem>();
}
