using UnityEngine;

/// <summary>
/// Health pickup (Vitality Shard or health item).
/// Attach to a GameObject with a trigger Collider2D.
/// Heals the player by a configurable amount OR permanently increases max health.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    public enum HealthEffectType
    {
        Heal,               // Restores a fixed amount of current health
        MaxHealthIncrease,  // Permanently increases max health (and heals the same amount)
    }

    [Header("Health Settings")]
    public HealthEffectType effectType = HealthEffectType.Heal;

    [Tooltip("Amount of health to restore (Heal) or permanently add to max health (MaxHealthIncrease).")]
    public float healthAmount = 25f;

    [Header("Feedback")]
    public GameObject floatingTextPrefab;
    [Tooltip("SFX clip name registered in AudioManager. Leave empty to skip.")]
    public string pickupSfxName = "";
    [Tooltip("Text shown in the floating message on pickup.")]
    public string pickupMessage = "Health Restored";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        switch (effectType)
        {
            case HealthEffectType.Heal:
                // Only pick up if the player actually needs health
                if (ph.currentHealth >= ph.maxHealth) return;
                ph.Heal(Mathf.RoundToInt(healthAmount));
                break;

            case HealthEffectType.MaxHealthIncrease:
                ph.maxHealth += healthAmount;
                ph.Heal(Mathf.RoundToInt(healthAmount));
                // Persist the bonus so it survives respawn
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.playerData.maxHealth = ph.maxHealth;
                    GameManager.Instance.playerData.bonusMaxHealth += healthAmount;
                }
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
