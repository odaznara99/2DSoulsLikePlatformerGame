using System;
using UnityEngine;

/// <summary>
/// Identifies each levellable stat, mirroring Dark Souls' eight attributes.
/// </summary>
public enum StatType
{
    Vitality,       // +15 max HP per level
    Attunement,     // +10 mana per level (future: spell slots)
    Endurance,      // +1 stamina level per level
    Strength,       // +5 attack damage per level
    Dexterity,      // +3 attack damage + 0.1 movement speed per level
    Resistance,     // +0.03 shield damage reduction per level (max 0.99)
    Intelligence,   // +10 mana per level (future: magic scaling)
    Faith,          // reserved for faith / miracle scaling
}

/// <summary>
/// Handles the logic of the Dark Souls-style leveling system.
///
/// Design
/// ------
/// • Each stat can be leveled independently using souls.
/// • Every individual stat level-up increases the overall Player Level by 1.
/// • The soul cost to reach the next Player Level follows a polynomial curve
///   (see <see cref="GetSoulCostToLevelUp"/>).
/// • Stat effects are applied immediately to the live player components and
///   are also stored in <see cref="PlayerData"/> so they survive scene reloads.
///
/// Usage
/// -----
/// • Add this component to the persistent GameManager GameObject (or any
///   DontDestroyOnLoad object) so it is always available.
/// • Call <see cref="TryLevelUpStat"/> from <see cref="LevelUpUI"/> when the
///   player confirms a stat investment.
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    // ── Tuning ────────────────────────────────────────────────────────────────

    [Header("Stat Effect Values")]
    [Tooltip("Max HP gained per Vitality level.")]
    public float vitalityHpPerLevel = 15f;

    [Tooltip("Mana gained per Attunement level.")]
    public int attunementManaPerLevel = 10;

    [Tooltip("Stamina levels gained per Endurance level.")]
    public int enduranceStaminaLevelsPerLevel = 1;

    [Tooltip("Attack damage gained per Strength level.")]
    public float strengthDamagePerLevel = 5f;

    [Tooltip("Attack damage gained per Dexterity level.")]
    public float dexterityDamagePerLevel = 3f;

    [Tooltip("Movement speed gained per Dexterity level.")]
    public float dexteritySpeedPerLevel = 0.1f;

    [Tooltip("Shield damage reduction gained per Resistance level (max 0.99).")]
    public float resistanceDamageReductionPerLevel = 0.03f;

    [Tooltip("Mana gained per Intelligence level.")]
    public int intelligenceManaPerLevel = 10;

    [Header("Soft Cap")]
    [Tooltip("Stat level above which effect-per-level is halved for physical stats (Vitality, Strength, Dexterity).")]
    public int softCap = 40;

    [Tooltip("Maximum level any individual stat can reach.")]
    public int hardCap = 99;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Raised after a successful stat level-up.
    /// Parameters: (StatType stat, int newStatLevel, int newPlayerLevel, int remainingSouls)
    /// </summary>
    public event Action<StatType, int, int, int> OnStatLeveledUp;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the soul cost to raise the player from <paramref name="currentPlayerLevel"/>
    /// to the next level.  The cost is determined by the <em>next</em> level, so
    /// levelling 1 → 2 uses the cost for L = 2.
    ///
    /// Formula (inspired by Dark Souls 1):
    ///   Cost(L) = max(100, round(0.02 × L³ + 3.06 × L² + 105.6 × L − 895))
    /// </summary>
    public int GetSoulCostToLevelUp(int currentPlayerLevel)
    {
        float L = currentPlayerLevel + 1f;
        float raw = 0.02f * L * L * L + 3.06f * L * L + 105.6f * L - 895f;
        return Mathf.Max(100, Mathf.RoundToInt(raw));
    }

    /// <summary>
    /// Returns the current level of the given <paramref name="stat"/> from
    /// <see cref="GameManager.playerData"/>.
    /// </summary>
    public int GetStatLevel(StatType stat)
    {
        var pd = GameManager.Instance?.playerData;
        if (pd == null) return 0;
        return GetStatLevelFromData(stat, pd);
    }

    /// <summary>
    /// Attempts to spend souls to raise <paramref name="stat"/> by one level.
    /// Returns <c>true</c> on success, <c>false</c> if funds are insufficient
    /// or the stat is already at the hard cap.
    /// </summary>
    public bool TryLevelUpStat(StatType stat)
    {
        var gm = GameManager.Instance;
        if (gm == null) return false;

        var pd = gm.playerData;

        // Hard cap guard
        int currentStatLevel = GetStatLevelFromData(stat, pd);
        if (currentStatLevel >= hardCap)
        {
            MessageManager.Instance?.ShowMessage($"{stat} is already at maximum level ({hardCap}).");
            return false;
        }

        // Cost check
        int cost = GetSoulCostToLevelUp(pd.PlayerLevel);
        if (pd.souls < cost)
        {
            MessageManager.Instance?.ShowMessage($"Not enough souls! Need {cost}, have {pd.souls}.");
            return false;
        }

        // Deduct souls
        gm.AddSouls(-cost);

        // Increment the stat
        IncrementStat(stat, pd);

        // Apply live gameplay effect
        ApplyStat(stat, currentStatLevel + 1);

        int newStatLevel   = GetStatLevelFromData(stat, pd);
        int newPlayerLevel = pd.PlayerLevel;

        OnStatLeveledUp?.Invoke(stat, newStatLevel, newPlayerLevel, pd.souls);

        Debug.Log($"[LevelUpManager] {stat} levelled to {newStatLevel}. Player Level: {newPlayerLevel}. Souls remaining: {pd.souls}");
        return true;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private int GetStatLevelFromData(StatType stat, PlayerData pd)
    {
        return stat switch
        {
            StatType.Vitality     => pd.vitality,
            StatType.Attunement   => pd.attunement,
            StatType.Endurance    => pd.endurance,
            StatType.Strength     => pd.strength,
            StatType.Dexterity    => pd.dexterity,
            StatType.Resistance   => pd.resistance,
            StatType.Intelligence => pd.intelligence,
            StatType.Faith        => pd.faith,
            _                     => 0,
        };
    }

    private void IncrementStat(StatType stat, PlayerData pd)
    {
        switch (stat)
        {
            case StatType.Vitality:     pd.vitality++;     break;
            case StatType.Attunement:   pd.attunement++;   break;
            case StatType.Endurance:    pd.endurance++;    break;
            case StatType.Strength:     pd.strength++;     break;
            case StatType.Dexterity:    pd.dexterity++;    break;
            case StatType.Resistance:   pd.resistance++;   break;
            case StatType.Intelligence: pd.intelligence++; break;
            case StatType.Faith:        pd.faith++;        break;
        }
    }

    /// <summary>
    /// Applies the gameplay effect for one level of the given <paramref name="stat"/>
    /// to the live player components.  <paramref name="newStatLevel"/> is the
    /// post-increment value used to determine whether the soft cap should halve
    /// the effect.
    /// </summary>
    private void ApplyStat(StatType stat, int newStatLevel)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        var pd      = gm.playerData;
        var player  = FindAnyObjectByType<PlayerControllerVersion2>();
        var health  = player != null ? player.GetComponent<PlayerHealth>()  : null;
        var stamina = player != null ? player.GetComponent<PlayerStamina>() : null;

        // Physical stats (Vitality, Strength, Dexterity) are halved above the soft cap.
        bool aboveSoftCap = newStatLevel > softCap;

        switch (stat)
        {
            case StatType.Vitality:
            {
                float gain = aboveSoftCap ? vitalityHpPerLevel * 0.5f : vitalityHpPerLevel;
                if (health != null)
                {
                    health.maxHealth += gain;
                    health.RefreshHealthUI();
                    pd.maxHealth = health.maxHealth;
                }
                pd.bonusMaxHealth += gain;
                break;
            }

            case StatType.Attunement:
            {
                pd.mana += attunementManaPerLevel;
                break;
            }

            case StatType.Endurance:
            {
                if (stamina != null)
                {
                    stamina.LevelUp(enduranceStaminaLevelsPerLevel, false);
                    pd.staminaRelicLevel = stamina.level;
                }
                break;
            }

            case StatType.Strength:
            {
                float gain = aboveSoftCap ? strengthDamagePerLevel * 0.5f : strengthDamagePerLevel;
                if (player != null)
                    player.attackDamage += gain;
                pd.bonusAttackDamage += gain;
                break;
            }

            case StatType.Dexterity:
            {
                float dmgGain   = aboveSoftCap ? dexterityDamagePerLevel * 0.5f : dexterityDamagePerLevel;
                float speedGain = aboveSoftCap ? dexteritySpeedPerLevel  * 0.5f : dexteritySpeedPerLevel;
                if (player != null)
                {
                    player.attackDamage  += dmgGain;
                    player.movementSpeed += speedGain;
                    player.SetFloatMovementSpeed(player.movementSpeed);
                }
                pd.bonusAttackDamage  += dmgGain;
                pd.bonusMovementSpeed += speedGain;
                break;
            }

            case StatType.Resistance:
            {
                if (health != null)
                {
                    health.shieldDamageReduction = Mathf.Min(
                        health.shieldDamageReduction + resistanceDamageReductionPerLevel, 0.99f);
                }
                pd.bonusDamageReduction += resistanceDamageReductionPerLevel;
                break;
            }

            case StatType.Intelligence:
            {
                pd.mana += intelligenceManaPerLevel;
                break;
            }

            case StatType.Faith:
                // Reserved for future miracle / faith-scaling implementation.
                break;
        }
    }
}
