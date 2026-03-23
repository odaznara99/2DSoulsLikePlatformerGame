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

    [Header("Floating Damage Text")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvas;

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
        // Apply any permanently stored stamina-relic upgrades from PlayerData
        if (GameManager.Instance != null && GameManager.Instance.playerData.staminaRelicLevel > 0)
        {
            level = GameManager.Instance.playerData.staminaRelicLevel;
        }

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

        // Not enough stamina: show floating text "No Stamina"
        ShowNoStaminaFloatingText();
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

    // Apply a temporary bonus to capacity and/or regen for a given duration (seconds).
    // Pass 0 for either parameter to skip that bonus.
    // Multiple overlapping calls are safe: each coroutine independently tracks and
    // removes only the exact amounts it added, so effects cancel correctly when they expire.
    public void StartTemporaryEffect(float capacityBonus, float regenBonus, float duration)
    {
        if (duration <= 0f) return;
        StartCoroutine(TemporaryEffectCoroutine(capacityBonus, regenBonus, duration));
    }

    private System.Collections.IEnumerator TemporaryEffectCoroutine(float capacityBonus, float regenBonus, float duration)
    {
        maxStamina += capacityBonus;
        baseRegenRate += regenBonus;
        onStaminaChanged?.Invoke(CurrentStamina, GetMaxStamina());

        yield return new WaitForSeconds(duration);

        maxStamina -= capacityBonus;
        baseRegenRate -= regenBonus;
        // Clamp current stamina if it now exceeds the new max
        if (currentStamina > GetMaxStamina())
            CurrentStamina = GetMaxStamina();
        onStaminaChanged?.Invoke(CurrentStamina, GetMaxStamina());
    }

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