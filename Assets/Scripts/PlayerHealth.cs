//using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance.VisualScripting;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    private PlayerControllerVersion2      player; //Reference to player script
    private Animator        playerAnimator; //Reference to player animator
    private GameManager     gameManager; //Reference to GameManager script
    public  float             maxHealth = 100f; // The maximum health the player can have
    public  float             currentHealth;  // The player's current health
    public float shieldDamageReduction = 0.90f;
    public float shieldKnockForce = 3f; // Force applied to the enemy when parrying
    public float hazardDamage = 25; // Damage taken from hazards
    public float hurtSeconds = 0.2f; // Seconds the player is in HURT state

    [Header("Health UI")]
    public UnityEngine.UI.Image healthBar;
    public Text healthText;

    [Header("Floating Damage Text")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvas;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the player's health to the maximum at the start
        currentHealth = maxHealth;
        UpdateHealthUI(); // If you have a UI to display health, update it here

        player          = this.GetComponent<PlayerControllerVersion2>();
        playerAnimator  = this.GetComponent<Animator>();
        gameManager     = GameManager.instance; // Get the GameManager instance
        worldCanvas     = GameObject.Find("WorldSpaceCanvas").GetComponent<Transform>();
    }

    // Method to handle taking damage
    public void TakeDamage(float damageAmount, GameObject attacker)
    {
        if (player.currentState != PlayerState.Dead && player.currentState != PlayerState.Hurting)
        {
            if (!player.isParry)
            {

                //Shielded the Attack
                if (player.currentState == PlayerState.Shielding)
                {
                    playerAnimator.SetTrigger("Block");
                    currentHealth -= 2;
                    damageAmount = damageAmount - (damageAmount * shieldDamageReduction);
                    if (floatingTextPrefab)
                    {
                        GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                        ft.GetComponent<FloatingText>().SetText("-" + 2.ToString());
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
                        Die(); // If damage exceeds current health, call Die method
                    }
                    // Direct Hit to the Player
                    else 
                    {
                        player.OnHurt(); ; // Switch to the Hurting state
                        currentHealth -= damageAmount;
                        if (floatingTextPrefab)
                        {
                            GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                            ft.GetComponent<FloatingText>().SetText("-" + damageAmount.ToString());
                        }
                        //Debug.Log("Player: Took direct hit " + damageAmount + " damage. Current health: " + currentHealth);
                    }
                }
 
                UpdateHealthUI(); // Update the UI to reflect the health change

            }
            //Parry Successful No Damage to the Player
            else
            {
                playerAnimator.SetTrigger("Block");

                if(attacker != null){
                    ParrySuccess(attacker); // Knock the enemy back
                }
                
                Debug.Log("Player parry the attack! No Damage Taken!");
            }
        }
    }

    // Method to handle parry success
    void ParrySuccess(GameObject enemy)
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
            ec.SwitchEnemyState(EnemyState.Hurt,1);
        }
    }


    // Method to handle healing the player (optional)
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;

        // Make sure health doesn't exceed the maximum
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Player healed " + healAmount + " points. Current health: " + currentHealth);

        UpdateHealthUI(); // Update the UI to reflect the health change
    }

    // Method called when the player's health reaches zero
    private void Die()
    {
        Debug.Log("Player died!");
        player.OnDead();
        // Optionally, you can trigger a death screen, restart the level, or respawn the player.
        gameManager.TriggerGameOverWithDelay(); // Call the GameOver method from GameManager
    }

    // Optional: Method to update the health UI
    private void UpdateHealthUI()
    {
        // Implement this method to update the player's health bar or any other UI element that displays health
        // For example, if using Unity UI:
        healthBar.fillAmount = (float)currentHealth / maxHealth;
        Debug.Log("Fill Amount: "+(float)currentHealth / maxHealth);

        healthText.text = currentHealth + "/" + maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazard")) {

            TakeDamage(hazardDamage,null);
        
        }

        if (collision.collider.CompareTag("DeadZone"))
        {

            TakeDamage(currentHealth, null);
            //Die();

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("DeadZone"))
        {

            TakeDamage(currentHealth, null);
            //Die();

        }
    }
}
