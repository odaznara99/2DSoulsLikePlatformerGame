using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MonoBehaviour))]
public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    public float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;

    [Header("Regeneration")]
    public float baseRegenRate = 10f; // per second
    public float regenDelay = 1.5f;   // seconds after consumption before regen starts

    [Header("Level / Scaling")]
    public int level = 0;
    public float regenPerLevelMultiplier = 0.1f; // +10% per level to regen rate
    public float maxStaminaPerLevel = 5f; // +5 stamina per level

    // Event: (current, max)
    [Serializable] public class StaminaChangedEvent : UnityEvent<float, float> { }
    public StaminaChangedEvent onStaminaChanged;

    private float regenTimer = 0f;

    public float CurrentStamina
    {
        get => currentStamina;
        private set
        {
            currentStamina = Mathf.Clamp(value, 0f, GetMaxStamina());
            onStaminaChanged?.Invoke(currentStamina, GetMaxStamina());
        }
    }

    private void Start()
    {
        // ensure initial values trigger UI
        CurrentStamina = currentStamina;
    }

    private void Update()
    {
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return;
        }

        // regen when timer expired
        float regenRate = GetEffectiveRegenRate();
        if (CurrentStamina < GetMaxStamina())
        {
            CurrentStamina += regenRate * Time.deltaTime;
        }
    }

    public float GetMaxStamina()
    {
        return maxStamina + level * maxStaminaPerLevel;
    }

    public float GetEffectiveRegenRate()
    {
        return baseRegenRate * (1f + level * regenPerLevelMultiplier);
    }

    // Try to consume stamina, returns true if successful. Resets regen delay timer.
    public bool TryConsume(float amount)
    {
        if (amount <= 0f) return true;
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            regenTimer = regenDelay;
            return true;
        }
        return false;
    }

    // Force consume (clamps at zero)
    public void Consume(float amount)
    {
        if (amount <= 0f) return;
        CurrentStamina -= amount;
        regenTimer = regenDelay;
    }

    // Add stamina (healing)
    public void AddStamina(float amount)
    {
        if (amount <= 0f) return;
        CurrentStamina += amount;
    }

    // Level up stamina: increase level and optionally refill
    public void LevelUp(int levels = 1, bool refill = false)
    {
        level += Mathf.Max(0, levels);
        if (refill) CurrentStamina = GetMaxStamina();
        onStaminaChanged?.Invoke(CurrentStamina, GetMaxStamina());
    }

    // Public helper to set regen delay (optional)
    public void ResetRegenDelay()
    {
        regenTimer = regenDelay;
    }
}