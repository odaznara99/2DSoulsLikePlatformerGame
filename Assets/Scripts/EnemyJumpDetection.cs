using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyJumpDetection : MonoBehaviour
{
    [Header("Jump Detection Settings")]
    public float jumpForce = 3f;

    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private EnemyMovement enemyMovement; // Reference to the EnemyMovement component
    private Animator animator; // Reference to the Animator component
    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        enemyMovement = GetComponentInParent<EnemyMovement>();
        animator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the enemy is grounded
        if (enemyMovement.isGrounded)
        {
            // Apply jump force
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            Debug.Log("Enemy jumped!");
            animator.SetTrigger("Jump"); // Trigger jump animation
        }
    }
}
