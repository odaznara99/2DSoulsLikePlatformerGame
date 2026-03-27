//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealth : MonoBehaviour
{
    private PlayerControllerVersion2 player;
    private Animator        playerAnimator;
    private GameManager     gameManager;
    private Rigidbody2D     rb;

    [Tooltip("The maximum health the player can have.")]
    public float maxHealth = 100f;
    [Tooltip("The player's current health.")]
    public float currentHealth = 0;
    public float shieldDamageReduction = 0.90f;
    [Tooltip("Force applied to the enemy when parrying.")]
    public float shieldKnockForce = 3f;
    [Tooltip("Damage taken from hazards.")]
    public float hazardDamage = 25;
    [Tooltip("Seconds the player remains in the HURT state.")]
    public float hurtSeconds = 0.2f;
    [Tooltip("When true the player cannot take damage.")]
    public bool isInvincible = false;

    [Header("Health UI")]
    public UnityEngine.UI.Image healthBar;
    public Text healthText;

    [Header("Floating Damage Text")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvas;

    /// <summary>
    /// Initialises health from saved player data and refreshes the health UI.
    /// </summary>
    void Start()
    {
        if (currentHealth == 0)
            currentHealth = maxHealth;

        player          = this.GetComponent<PlayerControllerVersion2>();
        playerAnimator  = this.GetComponent<Animator>();
        rb              = this.GetComponent<Rigidbody2D>();
        gameManager     = GameManager.Instance;

        if (gameManager != null)
        {
            // Apply any permanent max-health bonuses from pickups (additive, on top of inspector value)
            if (gameManager.playerData.bonusMaxHealth > 0f)
                maxHealth += gameManager.playerData.bonusMaxHealth;
            // Restore shield damage-reduction bonus from pickups
            if (gameManager.playerData.bonusDamageReduction > 0f)
                shieldDamageReduction += gameManager.playerData.bonusDamageReduction;
            currentHealth = gameManager.playerData.currentHealth;
            // Keep playerData.maxHealth in sync with the effective max (used by RestartGame)
            gameManager.playerData.maxHealth = maxHealth;
        }

        UpdateHealthUI();
    }

    /// <summary>
    /// Searches the ScreenCanvas hierarchy for a child GameObject with the given tag.
    /// </summary>
    /// <param name="childTag">The tag to search for.</param>
    /// <returns>The first matching child GameObject, or null if not found.</returns>
    GameObject FindingChildObjects(string childTag)
    {
        Transform parentTransform = GameObject.Find("ScreenCanvas").transform;
        return FindChildWithTagRecursive(parentTransform, childTag);
    }

    /// <summary>
    /// Recursively searches a transform hierarchy for a child with the specified tag.
    /// </summary>
    /// <param name="parent">The root transform to search from.</param>
    /// <param name="tag">The Unity tag to match.</param>
    /// <returns>The first matching child GameObject, or null if not found.</returns>
    GameObject FindChildWithTagRecursive(Transform parent, string tag)
    {
        if (parent.CompareTag(tag))
        {
            return parent.gameObject;
        }

        foreach (Transform child in parent)
        {
            GameObject result = FindChildWithTagRecursive(child, tag);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Lazily resolves the world canvas and health UI references, and ensures the UI is kept in sync.
    /// </summary>
    private void Update()
    {
        if (worldCanvas == null)
        {
            worldCanvas = GameObject.FindGameObjectWithTag("WorldSpaceCanvas").GetComponent<Transform>();
        }

        if (healthBar == null)
        {
            healthBar = FindingChildObjects("HealthBar").GetComponent<UnityEngine.UI.Image>();
            healthText = FindingChildObjects("HealthText").GetComponent<Text>();
        }

        if (healthText.text == "0/" + maxHealth && !gameManager.IsGameOver())
        {
            UpdateHealthUI();
        }
    }

    /// <summary>
    /// Returns true when the player's current health has reached zero.
    /// </summary>
    /// <returns>True if the player is dead.</returns>
    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    /// <summary>
    /// Applies damage to the player, factoring in parry, shield, and knockback.
    /// Handles the death flow when health reaches zero.
    /// </summary>
    /// <param name="damageAmount">The raw amount of damage to deal.</param>
    /// <param name="attacker">The GameObject that dealt the damage.</param>
    /// <param name="knockBackX">Horizontal knockback impulse magnitude.</param>
    /// <param name="knockBackY">Vertical knockback impulse magnitude.</param>
    public void TakeDamage(float damageAmount, GameObject attacker, float knockBackX = 1f, float knockBackY = 1f)
    {
        Debug.Log("Player was attacked by " + attacker);
        if (isInvincible) return;

        if (IsDead()) return;

        // Get direction to attacker (normalized)
        Vector2 toAttacker = (attacker.transform.position - transform.position).normalized;

        // Get player's facing direction as vector
        Vector2 playerFacing = player.IsFacingRight() ? Vector2.right : Vector2.left;

        // Dot product: +1 = same direction, -1 = opposite direction
        float dot = Vector2.Dot(playerFacing, toAttacker);

        bool attackerInFront = dot > 0.5f; // roughly in front
        Debug.Log("Attacker in front: " + attackerInFront);

        if (player.currentState != PlayerState.Dead && player.currentState != PlayerState.Hurting)
        {
            if (!player.detection.isParry)
            {
                //Shielded the Attack
                if (player.currentState == PlayerState.Shielding && attackerInFront)
                {
                    playerAnimator.SetTrigger("Block");

                    damageAmount = damageAmount - (damageAmount * shieldDamageReduction);
                    currentHealth -= damageAmount;
                    if (floatingTextPrefab)
                    {
                        GameObject ft2 = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                        ft2.GetComponent<FloatingText>().SetText("BLOCKED");
                    }
                    AudioManager.Instance.PlaySFX("Block");
                    Debug.Log("Shielded an attack! Took less damage. Current health: " + currentHealth);
                }

                else if (player.currentState != PlayerState.Dead && player.currentState != PlayerState.Hurting)
                {
                    // Killable Hit to the Player
                    if (currentHealth <= damageAmount)
                    {
                        currentHealth = 0;
                        if (floatingTextPrefab)
                        {
                            GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                            ft.GetComponent<FloatingText>().SetText("-" + damageAmount.ToString());
                        }
                        Die();
                        return;
                    }
                    // Direct Hit to the Player
                    else
                    {
                        player.OnHurt(); ; // Switch to the Hurting state
                        currentHealth -= damageAmount;

                        // Apply knockback if attacker is not null
                        KnockBack(attacker, knockBackX, knockBackY);
                        if (floatingTextPrefab)
                        {
                            GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                            ft.GetComponent<FloatingText>().SetText(damageAmount.ToString());
                        }
                    }
                }
            }
            //Parry Successful No Damage to the Player
            else
            {
                playerAnimator.SetTrigger("Block");

                if (attacker != null)
                {
                    ParrySuccess(attacker);
                }
                GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                ft.GetComponent<FloatingText>().SetText("PARRIED");
                Debug.Log("Player parry the attack! No Damage Taken!");
            }
        }

        if (IsDead())
        {
            currentHealth = 0;
            Die();
        }

        UpdateHealthUI();

        // Save back to GameManager
        gameManager.playerData.currentHealth = currentHealth;
    }

    /// <summary>
    /// Applies a physics-based knockback impulse to the player away from the attacker.
    /// </summary>
    /// <param name="attacker">The GameObject that is knocking the player back.</param>
    /// <param name="knockbackForceX">Horizontal component of the knockback impulse.</param>
    /// <param name="knockbackForceY">Vertical component of the knockback impulse.</param>
    void KnockBack(GameObject attacker, float knockbackForceX = 0, float knockbackForceY = 0)
    {
        if (attacker != null)
        {
            Vector2 knockDirection = (transform.position - attacker.transform.position).normalized;
            Vector2 knockback = new Vector2(knockDirection.x * knockbackForceX, knockbackForceY);

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Handles a successful parry by applying knockback to the enemy and triggering its hurt state.
    /// Only acts on GameObjects tagged "Enemy" that have a <see cref="Bandit"/> component.
    /// </summary>
    /// <param name="enemy">The enemy GameObject that was parried.</param>
    void ParrySuccess(GameObject enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            Bandit ec = enemy.GetComponent<Bandit>();
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockDirection = (enemy.transform.position - transform.position).normalized;
                ec.ApplyKnockback(knockDirection, shieldKnockForce);
            }

            if (ec != null)
            {
                ec.SwitchEnemyState(EnemyState.Hurt, 1);
            }
        }
    }

    /// <summary>
    /// Restores the specified number of health points, clamped to <see cref="maxHealth"/>.
    /// </summary>
    /// <param name="healAmount">The amount of health to restore.</param>
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Player healed " + healAmount + " points. Current health: " + currentHealth);

        UpdateHealthUI();
    }

    /// <summary>
    /// Refreshes the health UI display without modifying the current health value.
    /// Useful after <see cref="maxHealth"/> changes.
    /// </summary>
    public void RefreshHealthUI()
    {
        UpdateHealthUI();
    }

    /// <summary>
    /// Resets health to the maximum value.
    /// </summary>
    public void ResetHealth()
    {
        Heal((int)maxHealth);
    }

    /// <summary>
    /// Handles the player death sequence: triggers the death animation, drops souls,
    /// and notifies the GameManager to show the game-over screen.
    /// </summary>
    private void Die()
    {
        Debug.Log("Player died!");
        player.OnDead();
        UpdateHealthUI();

        // Drop souls at death position and reset currencies.
        gameManager.NotifyPlayerDeath(transform.position);

        gameManager.TriggerGameOverWithDelay();
    }

    /// <summary>
    /// Updates the health bar fill amount and health text to reflect the current health value.
    /// Attempts to locate the UI elements if they are not yet assigned.
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthBar == null)
        {
            Debug.LogWarning("Health bar still not assigned! Trying to find...");
            healthBar = FindingChildObjects("HealthBar").GetComponent<UnityEngine.UI.Image>();
            healthText = FindingChildObjects("HealthText").GetComponent<Text>();
            UpdateHealthUI();
            return;
        }

        healthBar.fillAmount = (float)currentHealth / maxHealth;
        healthText.text = currentHealth + "/" + maxHealth;
    }

    /// <summary>
    /// Handles collision with Hazard and DeadZone tagged colliders, dealing the appropriate damage.
    /// </summary>
    /// <param name="collision">The collision data for the contact.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazard"))
        {
            TakeDamage(hazardDamage, collision.gameObject);
        }

        if (collision.collider.CompareTag("DeadZone"))
        {
            TakeDamage(currentHealth, collision.gameObject);
        }
    }

    /// <summary>
    /// Handles trigger overlap with DeadZone and Hazard tagged colliders, dealing the appropriate damage.
    /// </summary>
    /// <param name="collision">The collider that entered this trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadZone"))
        {
            TakeDamage(currentHealth, collision.gameObject);
        }

        if (collision.CompareTag("Hazard"))
        {
            TakeDamage(hazardDamage, collision.gameObject);
        }
    }
}
