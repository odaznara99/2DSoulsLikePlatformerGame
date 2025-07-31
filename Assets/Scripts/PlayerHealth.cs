//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHealth : MonoBehaviour
{
    private PlayerControllerVersion2      player; //Reference to player script
    private Animator        playerAnimator; //Reference to player animator
    private GameManager     gameManager; //Reference to GameManager script
    private Rigidbody2D     rb; // Reference to the player's Rigidbody2D
    public  float             maxHealth = 100f; // The maximum currentHealth the player can have
    public  float             currentHealth = 0;  // The player's current currentHealth
    public float shieldDamageReduction = 0.90f;
    public float shieldKnockForce = 3f; // Force applied to the enemy when parrying
    public float hazardDamage = 25; // Damage taken from hazards
    public float hurtSeconds = 0.2f; // Seconds the player is in HURT state
    public bool isInvincible = false; // Flag to check if the player is invincible

    [Header("Health UI")]
    public UnityEngine.UI.Image healthBar;
    public Text healthText;

    [Header("Floating Damage Text")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvas;

    //public static PlayerHealth Instance; // Singleton instance

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("PlayerHealth script started.");
        // Initialize the player's currentHealth to the maximum at the start
        if (currentHealth == 0)
            currentHealth = maxHealth;

        player          = this.GetComponent<PlayerControllerVersion2>();
        playerAnimator  = this.GetComponent<Animator>();
        rb              = this.GetComponent<Rigidbody2D>();
        gameManager     = GameManager.instance; // Get the GameManager instance

        if (gameManager != null)
        {
            currentHealth = gameManager.playerData.currentHealth;
        }


        UpdateHealthUI(); // If you have a UI to display currentHealth, update it here

    }

    GameObject FindingChildObjects(string childTag) {

        Transform parentTransform = GameObject.Find("ScreenCanvas").transform;

        // Call the recursive helper function
        return FindChildWithTagRecursive(parentTransform, childTag);

    }

    GameObject FindChildWithTagRecursive(Transform parent, string tag)
    {
        // Check if the current parent has the tag
        if (parent.CompareTag(tag))
        {
            return parent.gameObject;
        }

        // Recursively check all children
        foreach (Transform child in parent)
        {
            GameObject result = FindChildWithTagRecursive(child, tag);
            if (result != null)
            {
                return result;
            }
        }

        // Return null if no match is found
        return null;
    }

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
            UpdateHealthUI(); // Update the currentHealth UI if it hasn't been set yet
        }

    }

    public bool IsDead() { 
        return currentHealth <= 0; // Check if the player is dead
    }

    // Method to handle taking damage
    public void TakeDamage(float damageAmount, GameObject attacker, float knockBackX =1f, float knockBackY = 1f)
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

        // Optional Debug
        // Debug.Log("Dot Product: " + dot);
        bool attackerInFront = dot > 0.5f; // roughly in front
        Debug.Log("Attacker in front: " + attackerInFront);


        if (player.currentState != PlayerState.Dead && player.currentState != PlayerState.Hurting)
        {
            if (!player.isParry)
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
                            ft.GetComponent<FloatingText>().SetText("-" +damageAmount.ToString());
                        }
                        Die(); // If damage exceeds current currentHealth, call Die method
                        return;
                    }
                    // Direct Hit to the Player
                    else 
                    {
                        player.OnHurt(); ; // Switch to the Hurting state
                        currentHealth -= damageAmount;

                        // Apply knockback if attacker is not null
                        KnockBack(attacker, knockBackX, knockBackY); // Knockback force can be adjusted
                        if (floatingTextPrefab)
                        {
                            GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                            ft.GetComponent<FloatingText>().SetText(damageAmount.ToString());
                        }
                        //Debug.Log("Player: Took direct hit " + damageAmount + " damage. Current currentHealth: " + currentHealth);
                    }
                }
 
                

            }
            //Parry Successful No Damage to the Player
            else
            {
                playerAnimator.SetTrigger("Block");

                if(attacker != null){
                    ParrySuccess(attacker); // Knock the enemy back
                }
                GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                ft.GetComponent<FloatingText>().SetText("PARRIED");
                Debug.Log("Player parry the attack! No Damage Taken!");
            }
        }

        

        if (IsDead())
        {
            currentHealth = 0; // Ensure currentHealth doesn't go below zero
            Die(); // Call the Die method if currentHealth reaches zero
        }

        UpdateHealthUI(); // Update the UI to reflect the currentHealth change

        // Save back to GameManager
        gameManager.playerData.currentHealth = currentHealth;
    }

    void KnockBack(GameObject attacker, float knockbackForceX =0, float knockbackForceY=0) {

        // Apply Knockback
        if (attacker != null)
        {
            Vector2 knockDirection = (transform.position - attacker.transform.position).normalized;
            Vector2 knockback = new Vector2(knockDirection.x * knockbackForceX, knockbackForceY);

            rb.velocity = Vector2.zero; // Reset current velocity
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
    }

    // Method to handle parry success
    void ParrySuccess(GameObject enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            Bandit ec = enemy.GetComponent<Bandit>();
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockDirection = (enemy.transform.position - transform.position).normalized;
                ec.ApplyKnockback(knockDirection, shieldKnockForce); // Apply knockback to the enemy
            }

            // Optional: stun, animation, etc.

            if (ec != null)
            {
                ec.SwitchEnemyState(EnemyState.Hurt, 1);
            }
        }
    }


    // Method to handle healing the player (optional)
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;

        // Make sure currentHealth doesn't exceed the maximum
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Player healed " + healAmount + " points. Current health: " + currentHealth);

        UpdateHealthUI(); // Update the UI to reflect the currentHealth change
    }

    public void ResetHealth() { 
        Heal((int)maxHealth); // Reset currentHealth to max currentHealth
    }

    // Method called when the player's currentHealth reaches zero
    private void Die()
    {
        Debug.Log("Player died!");
        player.OnDead();
        UpdateHealthUI();
        // Optionally, you can trigger a death screen, restart the level, or respawn the player.
        gameManager.TriggerGameOverWithDelay(); // Call the GameOver method from GameManager
    }

    // Optional: Method to update the currentHealth UI
    private void UpdateHealthUI()
    {
        if (healthBar == null) { 
            Debug.LogWarning("Health bar still not assigned! Trying to find...");
            healthBar = FindingChildObjects("HealthBar").GetComponent<UnityEngine.UI.Image>();
            healthText = FindingChildObjects("HealthText").GetComponent<Text>();
            UpdateHealthUI(); // Retry updating the UI after finding the currentHealth bar
            return;
        }

        // Implement this method to update the player's currentHealth bar or any other UI element that displays currentHealth
        // For example, if using Unity UI:
        healthBar.fillAmount = (float)currentHealth / maxHealth;
        healthText.text = currentHealth + "/" + maxHealth;
        //Debug.Log("Update Health UI: "+ healthText.text);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazard")) {

            TakeDamage(hazardDamage,collision.gameObject);
        
        }

        if (collision.collider.CompareTag("DeadZone"))
        {

            TakeDamage(currentHealth, collision.gameObject);
            //Die();

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("DeadZone"))
        {

            TakeDamage(currentHealth, collision.gameObject);
            //Die();

        }

        if (collision.CompareTag("Hazard"))
        {

            TakeDamage(hazardDamage, collision.gameObject);

        }
    }
}
