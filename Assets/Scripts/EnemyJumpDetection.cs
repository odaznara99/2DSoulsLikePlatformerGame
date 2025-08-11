using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyJumpDetection : MonoBehaviour
{
    [Header("Jump Detection Settings")]
    public float jumpForce = 3f;
    public bool obstacleDetected = false; // Flag to indicate if an obstacle is detected

    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private EnemyMovement enemyMovement; // Reference to the EnemyMovement component
    private Animator animator; // Reference to the Animator component
    private CircleCollider2D main_Collider;
    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        enemyMovement = GetComponentInParent<EnemyMovement>();
        animator = GetComponentInParent<Animator>();
        main_Collider = GetComponent<CircleCollider2D>();
        if (!main_Collider)
        {
            Debug.LogError("Collider for Jump Detection is not assigned");
        }


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        obstacleDetected = true;  // Set the flag when an obstacle is detected
        Invoke(nameof(SetFalseDetection), 2f);

        // Check if the enemy is grounded
        if (enemyMovement.isGrounded)
        {
            // Apply jump force
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            Debug.Log("Enemy jumped!");
            animator.SetTrigger("Jump"); // Trigger jump animation
            obstacleDetected = false;
            DisableCollider();
            Invoke(nameof(EnableCollider), 1f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        obstacleDetected = false; // Reset the flag when the obstacle is no longer detected
    }


    void DisableCollider ()
    {
        main_Collider.enabled = false;
    }

    void EnableCollider()
    {
        main_Collider.enabled = true;
    }

    private void SetFalseDetection()
    {
        obstacleDetected = false;
    }
}

