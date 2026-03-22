using System.Collections.Generic;
using UnityEngine;

// Passive abilities that can be unlocked by collecting Memory Shards.
public enum PassiveAbility
{
    None,
    IncreasedMaxHealth,      // Permanently increases maximum health
    FasterStaminaRegen,      // Increases stamina level (higher regen + capacity)
    ExtraJump,               // Grants one additional mid-air jump
    DamageReduction,         // Increases shield / block damage-reduction
    IncreasedMovementSpeed,  // Permanently increases movement speed
}

public class PlayerData
{
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public int mana, coins, xp;

    // You can add more later:
    public Vector2 position;

    // ── Pickups & Upgrades ──────────────────────────────────────────────────
    // Memory Shards collected (persists across death)
    public int memoryShards = 0;

    // Passive abilities unlocked via Memory Shards
    public List<PassiveAbility> unlockedPassives = new List<PassiveAbility>();

    // Permanent stat bonuses accumulated from passives / relics
    public float bonusMaxHealth = 0f;           // added to PlayerHealth.maxHealth
    public float bonusDamageReduction = 0f;     // added to PlayerHealth.shieldDamageReduction
    public int   bonusJumpCount = 0;            // added to PlayerControllerVersion2.maxDoubleJumpCount
    public float bonusMovementSpeed = 0f;       // added to PlayerControllerVersion2.movementSpeed

    // Permanent stamina-relic upgrades (applied via PlayerStamina.LevelUp on init)
    public int staminaRelicLevel = 0;
}

