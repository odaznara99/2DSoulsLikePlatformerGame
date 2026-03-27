using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [System.Serializable]
    public class HealthParameters
    {
        [SerializeField] public float maxHealth = 100f;
        [Tooltip("Health of the enemy.")]
        public float currentHealth = 100f;
    }

    [System.Serializable]
    public class FloatingHealthbarSettings
    {
        [SerializeField] public GameObject healthBarPrefab;
        [Tooltip("Offset for the health bar.")]
        [SerializeField] public Vector3 healthBarOffset = new Vector3(0, 1.0f, 0);
    }

    [System.Serializable]
    public class FloatingDamageTextSettings
    {
        public GameObject floatingTextPrefab;
        public Transform worldCanvas;
    }

    [System.Serializable]
    public class BloodSplashSettings
    {
        [SerializeField] public GameObject m_bloodSplash;
        [Tooltip("Offset for the blood splash effect.")]
        public Vector3 offset = new Vector3(0, 0.5f, 0);
    }

    [System.Serializable]
    public class SoundEffectsSettings
    {
        public List<string> damageSounds = new List<string>();
        public List<string> deathSounds = new List<string>();
    }

    [System.Serializable]
    public class KnockbackSettings
    {
        [Tooltip("Duration of the knockback effect.")]
        public float knockbackDuration = 0.5f;
        [Range(0f, 1f)]
        [Tooltip("Resistance to knockback (0-1, where 1 is no resistance).")]
        public float knockbackResistance = 0.1f;
    }

    [System.Serializable]
    public class SoulsRewardSettings
    {
        [Tooltip("Number of souls awarded to the player when this enemy dies.")]
        [SerializeField] public int soulsReward = 10;
    }

    [System.Serializable]
    public class EnemyStatusFlags
    {
        [Tooltip("Flag to check if the enemy is dead.")]
        public bool isDead = false;
        [Tooltip("Flag to check if the enemy is currently hurt.")]
        public bool isHurt = false;
        [Tooltip("Flag to check if the enemy is knocked back.")]
        public bool isKnocked = false;
        //public float hurtDuration = 0.3f; // Duration of the hurt animation
    }

    [Header("Health Parameters")]
    public HealthParameters healthParams = new HealthParameters();

    [Header("Floating Healthbar")]
    public FloatingHealthbarSettings floatingHealthbar = new FloatingHealthbarSettings();
    private FloatingHealthbar healthBarUI;

    [Header("Floating Damage Text")]
    public FloatingDamageTextSettings floatingDamageText = new FloatingDamageTextSettings();

    [Header("Blood Splash Effect")]
    public BloodSplashSettings bloodSplash = new BloodSplashSettings();

    [Header("Sound Effects")]
    public SoundEffectsSettings soundEffects = new SoundEffectsSettings();

    [Header("Knockback Settings")]
    public KnockbackSettings knockback = new KnockbackSettings();

    [Header("Souls Reward")]
    public SoulsRewardSettings soulsRewardSettings = new SoulsRewardSettings();

    [Header("Flags")]
    public EnemyStatusFlags flags = new EnemyStatusFlags();

    private Animator m_animator;
    private Rigidbody2D rb;

    /// <summary>
    /// Initializes components, sets up the floating health bar UI, and resets animator triggers.
    /// </summary>
    void Start()
    {
        m_animator = GetComponent<Animator>();
        //m_animator.SetBool("IsDead", false);
        //m_animator.SetBool("IsHurting", false);
        m_animator.ResetTrigger("Hurt");
        m_animator.ResetTrigger("Die");

        floatingDamageText.worldCanvas = GameObject.Find("WorldSpaceCanvas").GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();

        // --<< HEALTH BAR UI SETUP
        healthParams.currentHealth = healthParams.maxHealth;

        if (floatingHealthbar.healthBarPrefab != null)
        {
            GameObject hb = Instantiate(floatingHealthbar.healthBarPrefab, transform.position, Quaternion.identity);
            healthBarUI = hb.GetComponent<FloatingHealthbar>();
            healthBarUI.SetTarget(this.transform);
        }

        healthBarUI.SetHealth(healthParams.currentHealth, healthParams.maxHealth);
        healthBarUI.offset = floatingHealthbar.healthBarOffset; // Set the offset for the health bar
        // -->> HEALTH BAR UI SETUP
    }

    /// <summary>
    /// Applies damage to the enemy, triggers hurt or death animations, and handles associated visual and audio effects.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (flags.isDead || flags.isHurt)
        {
            Debug.Log("Enemy is taking damage or dead");
            return; // Exit if the enemy is dead
        }

        healthParams.currentHealth -= damageAmount;
        healthParams.currentHealth = Mathf.Clamp(healthParams.currentHealth, 0f, healthParams.maxHealth);

        // Update Floating Healthbar UI
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(healthParams.currentHealth, healthParams.maxHealth);
        }

        if (healthParams.currentHealth <= 0)
        {
           flags.isDead = true;
           // Trigger Death Animation
           m_animator.SetTrigger("Die");
           m_animator.SetBool("IsDead", true);
            // Hide or Destroy Health Bar
            if (healthBarUI != null)
            {
                healthBarUI.DestroyBar();
            }
            // Award souls to the player
            GameManager.Instance?.AddSouls(soulsRewardSettings.soulsReward);
            if (floatingDamageText.floatingTextPrefab)
            {
                GameObject ft = Instantiate(floatingDamageText.floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, floatingDamageText.worldCanvas);
                ft.GetComponent<FloatingText>().SetText("+" + soulsRewardSettings.soulsReward.ToString() + " Souls");
            }
        }
        else
        {
            flags.isHurt = true;
            // Trigger Hurt Animation
            m_animator.SetTrigger("Hurt");
            m_animator.SetBool("IsHurting", true);
            // BloodSlash Effect
            Instantiate(bloodSplash.m_bloodSplash, transform.position + bloodSplash.offset, Quaternion.identity); // Instantiate blood splash effect
            // Show Floating Damage Text
            if (floatingDamageText.floatingTextPrefab)
            {
                GameObject ft = Instantiate(floatingDamageText.floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, floatingDamageText.worldCanvas);
                ft.GetComponent<FloatingText>().SetText("-" + damageAmount.ToString());
            }
            // Play Sound
            if(soundEffects.damageSounds.Count !=0)
                AudioManager.Instance.PlaySFX(soundEffects.damageSounds[Random.Range(0, soundEffects.damageSounds.Count)]);
        }

    }

    /// <summary>
    /// Resets the hurt state and animator parameters. Call this via State Behavior or Animation Event.
    /// </summary>
    public void ResetHurtState() // Call this via State Behavior or Animation Event
    {
        m_animator.ResetTrigger("Hurt");
        m_animator.SetBool("IsHurting", false);
        flags.isHurt = false;
    }

    /// <summary>
    /// Revives the enemy by restoring full health and resetting the dead animator state. Call this to revive the enemy.
    /// </summary>
    public void ReviveEnemy() // Call this to revive the enemy
    {
        flags.isDead = false;
        healthParams.currentHealth = healthParams.maxHealth;
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(healthParams.currentHealth, healthParams.maxHealth);
            //healthBarUI.ShowBar();
        }
        m_animator.SetBool("IsDead", false);
    }

    /// <summary>
    /// Initiates a knockback force on the enemy if it is not already knocked back or dead.
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float knockbackForce)
    {
        if (flags.isDead)
        {
            return;
        }

        if (!flags.isKnocked)
        {
            StartCoroutine(KnockbackCoroutine(direction, knockbackForce));
        }
    }

    /// <summary>
    /// Coroutine that applies a knockback impulse and resets the knocked state after the knockback duration elapses.
    /// </summary>
    IEnumerator KnockbackCoroutine(Vector2 direction, float knockbackForce)
    {
        if (flags.isDead)
        {
            yield break; // Do not apply knockback if already dead
        }

        flags.isKnocked = true;

        float adjustedForce = knockbackForce * (1f - Mathf.Clamp01(knockback.knockbackResistance));

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * adjustedForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockback.knockbackDuration);

        rb.linearVelocity = Vector2.zero;
        flags.isKnocked = false;
    }
}
