using UnityEngine;

/// <summary>
/// Memory Shard pickup.
/// Attach to a GameObject with a trigger Collider2D.
/// When the player walks into it the shard is collected, an optional passive
/// ability is unlocked, and the object is destroyed.
/// </summary>
public class MemoryShardPickup : MonoBehaviour
{
    // Maximum allowed shield damage-reduction (99 %) – prevents full immunity.
    private const float MaxDamageReduction = 0.99f;
    [Header("Shard Settings")]
    [Tooltip("Number of memory shards granted on pickup.")]
    public int shardValue = 1;

    [Header("Passive Unlock")]
    [Tooltip("Passive ability unlocked when this shard is collected. Set to None for lore-only shards.")]
    public PassiveAbility unlocksPassive = PassiveAbility.None;
    [Tooltip("Strength of the passive bonus (e.g. +20 max health, +0.1 damage reduction, +1 jump).")]
    public float passiveBonusValue = 0f;

    [Header("Feedback")]
    public GameObject floatingTextPrefab;
    [Tooltip("SFX clip name registered in AudioManager. Leave empty to skip.")]
    public string pickupSfxName = "";
    [Tooltip("Text shown in the floating message on pickup.")]
    public string pickupMessage = "Memory Shard";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        // --- collect the shard ---
        gm.playerData.memoryShards += shardValue;

        // --- unlock passive (once per type) ---
        if (unlocksPassive != PassiveAbility.None
            && !gm.playerData.unlockedPassives.Contains(unlocksPassive))
        {
            gm.playerData.unlockedPassives.Add(unlocksPassive);
            ApplyPassive(other.gameObject, unlocksPassive, passiveBonusValue);
        }

        ShowFloatingText(other.transform);
        PlaySFX();
        Destroy(gameObject);
    }

    private void ApplyPassive(GameObject player, PassiveAbility passive, float bonus)
    {
        switch (passive)
        {
            case PassiveAbility.IncreasedMaxHealth:
            {
                float amount = bonus > 0f ? bonus : 20f;
                var ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.maxHealth += amount;
                    ph.RefreshHealthUI(); // Update UI to reflect new max health
                    GameManager.Instance.playerData.maxHealth = ph.maxHealth;
                }
                GameManager.Instance.playerData.bonusMaxHealth += amount;
                break;
            }

            case PassiveAbility.FasterStaminaRegen:
            {
                var ps = player.GetComponent<PlayerStamina>();
                if (ps != null)
                {
                    ps.LevelUp(1, false);
                    GameManager.Instance.playerData.staminaRelicLevel = ps.level;
                }
                break;
            }

            case PassiveAbility.ExtraJump:
            {
                int jumps = bonus > 0f ? Mathf.RoundToInt(bonus) : 1;
                var pc = player.GetComponent<PlayerControllerVersion2>();
                if (pc != null)
                    pc.maxDoubleJumpCount += jumps;
                GameManager.Instance.playerData.bonusJumpCount += jumps;
                break;
            }

            case PassiveAbility.DamageReduction:
            {
                float amount = bonus > 0f ? bonus : 0.05f;
                var ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.shieldDamageReduction = Mathf.Min(ph.shieldDamageReduction + amount, MaxDamageReduction);
                GameManager.Instance.playerData.bonusDamageReduction += amount;
                break;
            }

            case PassiveAbility.IncreasedMovementSpeed:
            {
                float amount = bonus > 0f ? bonus : 0.5f;
                var pc = player.GetComponent<PlayerControllerVersion2>();
                if (pc != null)
                {
                    pc.movementSpeed += amount;
                    pc.SetFloatMovementSpeed(pc.movementSpeed);
                }
                GameManager.Instance.playerData.bonusMovementSpeed += amount;
                break;
            }

            case PassiveAbility.PlungeAttack:
            {
                var pc = player.GetComponent<PlayerControllerVersion2>();
                if (pc != null)
                    pc.hasPlungeAttack = true;
                break;
            }
        }
    }

    private void ShowFloatingText(Transform target)
    {
        if (floatingTextPrefab == null) return;

        Transform canvas = GetWorldCanvas();
        Vector3 pos = target.position + Vector3.up;

        GameObject ft = canvas != null
            ? Instantiate(floatingTextPrefab, pos, Quaternion.identity, canvas)
            : Instantiate(floatingTextPrefab, pos, Quaternion.identity);

        ft.GetComponent<FloatingText>()?.SetText(pickupMessage);
    }

    private void PlaySFX()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(pickupSfxName))
            AudioManager.Instance.PlaySFX(pickupSfxName);
    }

    private Transform GetWorldCanvas()
    {
        var go = GameObject.FindGameObjectWithTag("WorldSpaceCanvas");
        return go != null ? go.transform : null;
    }
}
