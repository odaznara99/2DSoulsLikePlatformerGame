using UnityEngine;

/// <summary>
/// The type of shop that sells this item. Used to filter inventory in editor.
/// </summary>
public enum ShopItemType
{
    Weapon,
    Armor,
    Consumable,
    Spell,
    Ability,
}

/// <summary>
/// Which in-game currency is used to buy this item.
/// </summary>
public enum ShopCurrencyType
{
    Souls,
    Coins,
}

/// <summary>
/// The gameplay effect applied to the player when this item is purchased.
/// </summary>
public enum ShopItemEffect
{
    None,
    RestoreHealth,          // Heals player by effectValue
    IncreaseMaxHealth,      // Permanently adds effectValue to max health
    IncreaseDamage,         // Permanently adds effectValue to attack damage
    IncreaseDamageReduction,// Permanently adds effectValue to shield damage reduction
    IncreaseMovementSpeed,  // Permanently adds effectValue to movement speed
    ExtraJump,              // Grants Mathf.RoundToInt(effectValue) additional mid-air jumps
    FasterStaminaRegen,     // Increases stamina level by 1 (effectValue ignored)
}

/// <summary>
/// ScriptableObject defining a single item sold in a shop.
/// Create via Assets → Create → Shop → Shop Item.
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItem : ScriptableObject
{
    [Header("Item Info")]
    [Tooltip("Unique identifier used to track one-time purchases. Must be unique across all shop items.")]
    public string itemId;

    [Tooltip("Display name shown in the shop UI.")]
    public string itemName;

    [Tooltip("Description shown in the item detail panel.")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Icon displayed in the item slot and detail panel.")]
    public Sprite icon;

    [Tooltip("Categorises the item (Weapon, Armor, Consumable, Spell, Ability).")]
    public ShopItemType itemType;

    [Header("Pricing")]
    [Tooltip("Which currency the player pays with.")]
    public ShopCurrencyType currencyType = ShopCurrencyType.Souls;

    [Tooltip("Cost in the chosen currency.")]
    public int price = 100;

    [Header("Effect")]
    [Tooltip("Gameplay effect applied to the player on purchase.")]
    public ShopItemEffect effect = ShopItemEffect.None;

    [Tooltip("Strength of the effect (e.g. 20 = +20 max health, 0.05 = +5% damage reduction).")]
    public float effectValue = 0f;

    [Header("Purchase Rules")]
    [Tooltip("If true, this item can only be bought once per save (e.g. permanent upgrades).")]
    public bool isOneTimePurchase = false;
}
