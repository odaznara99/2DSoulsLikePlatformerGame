using UnityEngine;

/// <summary>
/// Stamina Relic pickup.
/// Attach to a GameObject with a trigger Collider2D.
/// Permanently increases the player's stamina level (capacity + regen)
/// OR applies a time-limited capacity / regen bonus.
/// </summary>
public class StaminaRelicPickup : MonoBehaviour
{
    public enum RelicEffectType
    {
        PermanentLevelUp,   // Calls PlayerStamina.LevelUp() — permanently raises capacity and regen
        TemporaryCapacity,  // Adds bonus stamina capacity for a limited duration
        TemporaryRegen,     // Adds bonus regen rate for a limited duration
        TemporaryBoth,      // Adds both capacity and regen bonuses for a limited duration
    }

    [Header("Relic Settings")]
    public RelicEffectType effectType = RelicEffectType.PermanentLevelUp;

    [Tooltip("Levels granted when effectType is PermanentLevelUp.")]
    public int permanentLevels = 1;
    [Tooltip("Refill stamina to max after a permanent level-up?")]
    public bool refillOnLevelUp = true;

    [Tooltip("Temporary stamina capacity bonus (for Temporary* effect types).")]
    public float temporaryCapacityBonus = 30f;
    [Tooltip("Temporary regen rate bonus (for Temporary* effect types).")]
    public float temporaryRegenBonus = 5f;
    [Tooltip("Duration in seconds for temporary effects.")]
    public float temporaryDuration = 15f;

    [Header("Feedback")]
    public GameObject floatingTextPrefab;
    [Tooltip("SFX clip name registered in AudioManager. Leave empty to skip.")]
    public string pickupSfxName = "";
    [Tooltip("Text shown in the floating message on pickup.")]
    public string pickupMessage = "Stamina Relic";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var ps = other.GetComponent<PlayerStamina>();
        if (ps == null) return;

        switch (effectType)
        {
            case RelicEffectType.PermanentLevelUp:
                ps.LevelUp(permanentLevels, refillOnLevelUp);
                // Persist the new relic level in PlayerData
                if (GameManager.Instance != null)
                    GameManager.Instance.playerData.staminaRelicLevel = ps.level;
                break;

            case RelicEffectType.TemporaryCapacity:
                ps.StartTemporaryEffect(temporaryCapacityBonus, 0f, temporaryDuration);
                break;

            case RelicEffectType.TemporaryRegen:
                ps.StartTemporaryEffect(0f, temporaryRegenBonus, temporaryDuration);
                break;

            case RelicEffectType.TemporaryBoth:
                ps.StartTemporaryEffect(temporaryCapacityBonus, temporaryRegenBonus, temporaryDuration);
                break;
        }

        ShowFloatingText(other.transform);
        PlaySFX();
        Destroy(gameObject);
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
