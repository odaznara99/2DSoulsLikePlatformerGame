//using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance.VisualScripting;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    private PlayerController      player; //Reference to player script
    private Animator        playerAnimator; //Reference to player animator
    public GameManager     gameManager; //Reference to GameManager script
    public  int             maxHealth = 100; // The maximum health the player can have
    public  int             currentHealth;  // The player's current health
    public int hazardDamage = 25; // Damage taken from hazards
    public float hurtSeconds = 0.2f; // Seconds the player is in HURT state

    [Header("Health UI")]
    public UnityEngine.UI.Image healthBar;
    public Text healthText; 

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the player's health to the maximum at the start
        currentHealth = maxHealth;
        UpdateHealthUI(); // If you have a UI to display health, update it here

        player          = this.GetComponent<PlayerController>();
        playerAnimator  = this.GetComponent<Animator>();
        gameManager     = GameManager.instance; // Get the GameManager instance
    }

    // Method to handle taking damage
    public void TakeDamage(int damageAmount)
    {
        if (!player.playerIsDead)
        {
            if (!player.isParry)
            {
                //Direct Hit
                if (!player.isBlocking && !player.playerisHurt)
                {
                    //playerAnimator.SetTrigger("Hurt");
                    player.SetPlayerIsHurtSeconds(hurtSeconds);
                    currentHealth -= damageAmount;
                    Debug.Log("Player: Took direct hit " + damageAmount + " damage. Current health: " + currentHealth);
                }
                //Damage Reduced/ Attack Blocked
                else if (player.isBlocking) 
                {
                    playerAnimator.SetTrigger("Block");
                    currentHealth -= 2;
                    Debug.Log("Player: Blocks the attack! Took " + damageAmount + " reduced damage. Current health: " + currentHealth);
                }

                // Make sure health doesn't go below zero
                if (currentHealth < 0)
                {
                    currentHealth = 0;
                }

                UpdateHealthUI(); // Update the UI to reflect the health change

                // Check if the player's health reaches zero
                if (currentHealth == 0)
                {
                    Die();
                }
            }
            //Parry Attack
            else
            {
                playerAnimator.SetTrigger("Block");
                Debug.Log("Player parry the attack! No Damage Taken!");
            }
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
        player.SetPlayerDead();
        playerAnimator.SetBool("noBlood", player.NoBlood());
        playerAnimator.SetTrigger("Death");
        player.enabled = false;

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

            TakeDamage(hazardDamage);
        
        }

        if (collision.collider.CompareTag("DeadZone"))
        {

            TakeDamage(currentHealth);
            //Die();

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("DeadZone"))
        {

            TakeDamage(currentHealth);
            //Die();

        }
    }
}
