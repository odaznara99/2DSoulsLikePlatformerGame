using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MonoBehaviour))]
public class PlayerStamina : MonoBehaviour
{
    [System.Serializable]
    public class StaminaSettings
    {
        public float maxStamina = 100f;
        [SerializeField] public float currentStamina = 100f;
    }

    [System.Serializable]
    public class RegenerationSettings
    {
        [Tooltip("Stamina regenerated per second.")]
        public float baseRegenRate = 10f;
        [Tooltip("Seconds after stamina consumption before regeneration resumes.")]
        public float regenDelay = 1.5f;
    }

    [System.Serializable]
    public class LevelScalingSettings
    {
        public int level = 0;
        [Tooltip("+10% regen rate per stamina relic level.")]
        public float regenPerLevelMultiplier = 0.1f;
        [Tooltip("+5 max stamina per level.")]
        public float maxStaminaPerLevel = 5f;
    }

    [Header("Stamina")]
    public StaminaSettings stamina = new StaminaSettings();

    [Header("Regeneration")]
    public RegenerationSettings regen = new RegenerationSettings();

    [Header("Level / Scaling")]
    public LevelScalingSettings scaling = new LevelScalingSettings();

    [Header("Floating Damage Text")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvas;

    // Event: (current, max)
    [Serializable] public class StaminaChangedEvent : UnityEvent<float, float> { }
    public StaminaChangedEvent onStaminaChanged;

    private float regenTimer = 0f;

    /// <summary>
    /// Gets or sets the current stamina value, clamped to [0, GetMaxStamina()] and firing the changed event.
    /// </summary>
    public float CurrentStamina
    {
        get => stamina.currentStamina;
        private set
        {
            stamina.currentStamina = Mathf.Clamp(value, 0f, GetMaxStamina());
            onStaminaChanged?.Invoke(stamina.currentStamina, GetMaxStamina());
        }
    }

    /// <summary>
    /// Applies any persisted stamina-relic upgrades from PlayerData and fires the initial UI event.
    /// </summary>
    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerData.staminaRelicLevel > 0)
        {
            scaling.level = GameManager.Instance.playerData.staminaRelicLevel;
        }

        // ensure initial values trigger UI
        CurrentStamina = stamina.currentStamina;
    }

    /// <summary>
    /// Ticks the regen delay timer and regenerates stamina each frame when the timer has expired.
    /// </summary>
    private void Update()
    {
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return;
        }

        float regenRate = GetEffectiveRegenRate();
        if (CurrentStamina < GetMaxStamina())
        {
            CurrentStamina += regenRate * Time.deltaTime;
        }
    }

    /// <summary>
    /// Returns the effective maximum stamina including per-level bonuses.
    /// </summary>
    /// <returns>The total maximum stamina.</returns>
    public float GetMaxStamina()
    {
        return stamina.maxStamina + scaling.level * scaling.maxStaminaPerLevel;
    }

    /// <summary>
    /// Returns the effective regeneration rate including per-level multipliers.
    /// </summary>
    /// <returns>Stamina regenerated per second.</returns>
    public float GetEffectiveRegenRate()
    {
        return regen.baseRegenRate * (1f + scaling.level * scaling.regenPerLevelMultiplier);
    }

    /// <summary>
    /// Attempts to consume the specified amount of stamina. Returns true and resets the regen
    /// delay if sufficient stamina is available; otherwise shows a "no stamina" floating text and returns false.
    /// </summary>
    /// <param name="amount">The amount of stamina to consume.</param>
    /// <returns>True if the stamina was consumed; false if insufficient.</returns>
    public bool TryConsume(float amount)
    {
        if (amount <= 0f) return true;
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            regenTimer = regen.regenDelay;
            return true;
        }

        ShowNoStaminaFloatingText();
        return false;
    }

    /// <summary>
    /// Force-consumes stamina, clamping to zero, and resets the regeneration delay timer.
    /// </summary>
    /// <param name="amount">The amount of stamina to consume.</param>
    public void Consume(float amount)
    {
        if (amount <= 0f) return;
        CurrentStamina -= amount;
        regenTimer = regen.regenDelay;
    }

    /// <summary>
    /// Adds stamina, clamped to the current maximum.
    /// </summary>
    /// <param name="amount">The amount of stamina to restore.</param>
    public void AddStamina(float amount)
    {
        if (amount <= 0f) return;
        CurrentStamina += amount;
    }

    /// <summary>
    /// Increases the stamina level by the given number of levels, optionally refilling stamina to the new maximum.
    /// </summary>
    /// <param name="levels">Number of levels to add.</param>
    /// <param name="refill">If true, refills stamina to the new maximum after levelling up.</param>
    public void LevelUp(int levels = 1, bool refill = false)
    {
        scaling.level += Mathf.Max(0, levels);
        if (refill) CurrentStamina = GetMaxStamina();
        onStaminaChanged?.Invoke(CurrentStamina, GetMaxStamina());
    }

    /// <summary>
    /// Resets the regeneration delay timer to the configured delay duration.
    /// </summary>
    public void ResetRegenDelay()
    {
        regenTimer = regen.regenDelay;
    }

    /// <summary>
    /// Applies temporary capacity and regen bonuses for a fixed duration.
    /// Multiple overlapping calls are safe; each coroutine independently removes only the amounts it added.
    /// </summary>
    /// <param name="capacityBonus">Extra max stamina to add temporarily.</param>
    /// <param name="regenBonus">Extra regen rate to add temporarily.</param>
    /// <param name="duration">Duration in seconds before the bonuses are removed.</param>
    public void StartTemporaryEffect(float capacityBonus, float regenBonus, float duration)
    {
        if (duration <= 0f) return;
        StartCoroutine(TemporaryEffectCoroutine(capacityBonus, regenBonus, duration));
    }

    /// <summary>
    /// Coroutine that adds the specified bonuses, waits for the duration, then removes them.
    /// </summary>
    /// <param name="capacityBonus">Extra max stamina added for the duration.</param>
    /// <param name="regenBonus">Extra regen rate added for the duration.</param>
    /// <param name="duration">How long in seconds the bonuses last.</param>
    private System.Collections.IEnumerator TemporaryEffectCoroutine(float capacityBonus, float regenBonus, float duration)
    {
        stamina.maxStamina  += capacityBonus;
        regen.baseRegenRate += regenBonus;
        onStaminaChanged?.Invoke(CurrentStamina, GetMaxStamina());

        yield return new WaitForSeconds(duration);

        stamina.maxStamina  -= capacityBonus;
        regen.baseRegenRate -= regenBonus;
        if (stamina.currentStamina > GetMaxStamina())
            CurrentStamina = GetMaxStamina();
        onStaminaChanged?.Invoke(CurrentStamina, GetMaxStamina());
    }

    /// <summary>
    /// Spawns a "Not enough Stamina" floating text above the player.
    /// Lazily resolves the world canvas if not already assigned.
    /// </summary>
    private void ShowNoStaminaFloatingText()
    {
        if (floatingTextPrefab == null) return;

        if (worldCanvas == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("WorldSpaceCanvas");
            if (go != null) worldCanvas = go.transform;
        }

        Vector3 spawnPos = transform.position + Vector3.up;

        GameObject ft;
        if (worldCanvas != null)
            ft = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity, worldCanvas);
        else
            ft = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);

        var floating = ft.GetComponent<FloatingText>();
        if (floating != null)
            floating.SetText("Not enough Stamina");
    }
}
