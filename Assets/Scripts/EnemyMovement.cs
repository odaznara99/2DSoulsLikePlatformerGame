using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public string enemyName = "Skeleton"; // Name of the enemy
    [Header("Patrol Params")]
    public Transform pointA; // Patrol point A
    public Transform pointB; // Patrol point B
    public float patrolSpeed = 2f; // Speed of patrolling
    [Header("Chase Params")]
    public float chaseSpeed = 3f; // Speed when chasing the player
    public float detectionRange = 5f; // Range to detect the player
    public Transform player; // Reference to the player
    private Transform currentTarget; // Current patrol target

    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private Animator m_animator; // Reference to the Animator component

    [Header("Ground Check")]
    public Transform groundCheck; // A point to check if the enemy is grounded
    public float groundCheckRadius = 0.2f; // Radius of the ground check
    public LayerMask groundLayer; // LayerMask to specify what is considered ground

    [Header("Flags")]
    public bool isGrounded; // Whether the enemy is grounded
    [SerializeField] private bool isChasing = false; // Whether the skeleton is chasing the player
    [SerializeField] private bool isFacingRight = false;
    public bool isAttacking = false; // Whether the skeleton is currently attacking

    private EnemyHealth enemyHealth; // Reference to the EnemyHealth component

    [Header("Attack Settings")]
    public BoxCollider2D attackBoxTrigger; // Trigger for the attack box
    public float attackDamage = 20f; // Damage dealt by the attack
    public float attackRange = 1f; // Range to detect the player
    public string[] attackAnimationTrigger; // Animation trigger for the attack
    [Tooltip("Time in seconds between attacks (attack speed).")]
    public float attackCooldown = 1.5f; // Cooldown between attacks
    private float lastAttackTime = -999f; // Track when the enemy last attacked

    private EnemyJumpDetection obstacle_Detector; // Reference to Jump Detection Trigger


    private void Start()
    {
        // Start patrolling towards point A
        currentTarget = pointA;

        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is missing on the: " + enemyName + "!");
        }

        // Get the Animator component
        m_animator = GetComponent<Animator>();
        if (m_animator == null)
        {
            Debug.LogError("Animator is missing on the: " + enemyName + "!");
        }

        // Get the EnemyHealth component
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyHealth is missing on the: " + enemyName + "!");
        }

        obstacle_Detector = GetComponentInChildren<EnemyJumpDetection>();
        if (obstacle_Detector == null)
        {
            Debug.LogError("EnemyJumpDetection component not found in children!");
        }
    }

    private void Update()
    {
        if (enemyHealth.isDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            // Start chasing the player
            isChasing = true;
        }
        else
        {
            // Return to patrolling if the player is out of range
            isChasing = false;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackRange
            && !isAttacking
            && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            int randomIndex = Random.Range(0, attackAnimationTrigger.Length);
            m_animator.SetTrigger(attackAnimationTrigger[randomIndex]); // Trigger the attack animation
        }

        // Set Running Parameters in the Animator
        m_animator.SetFloat("Velocity_X", Mathf.Abs(rb.linearVelocity.x));
        m_animator.SetFloat("Velocity_Y", rb.linearVelocity.y);
        m_animator.SetBool("IsGrounded", isGrounded);

        // Flip the sprite based on velocity
        if (!enemyHealth.isHurt && !enemyHealth.isDead)
        {
            FlipSpriteBasedOnVelocity();
        }
    }

    private void FixedUpdate()
    {
        // Check if the enemy is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (enemyHealth.isDead || enemyHealth.isHurt || isAttacking || !pointA || !pointB ||
            obstacle_Detector.obstacleDetected)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Calculate direction to the current patrol target
        Vector2 direction = (currentTarget.position - transform.position).normalized;

        // Set velocity towards the target
        rb.linearVelocity = new Vector3(direction.x * patrolSpeed, rb.linearVelocity.y);

        // Switch target when reaching the current patrol point
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    private void ChasePlayer()
    {
        if (enemyHealth.isDead || enemyHealth.isHurt || isAttacking || obstacle_Detector.obstacleDetected)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        } // Don't do anything if the enemy is dead

        // Calculate direction to the player
        Vector2 direction = (player.position - transform.position).normalized;

        // Set velocity towards the player
        rb.linearVelocity = new Vector3(direction.x * chaseSpeed, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the detection range in the editor for debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw the attack range in the editor for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw the attack range in the editor for debugging
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    void FlipSpriteBasedOnVelocity()
    {
        if (isAttacking || !isGrounded) 
        {
            return;
        }
        // Check the enemy's velocity on the X-axis to determine direction
        if (rb.linearVelocity.x > 0 && !isFacingRight)
        {
            // Moving right but currently facing left, so flip to face right
            FlipSprite();
        }
        else if (rb.linearVelocity.x < 0 && isFacingRight)
        {
            // Moving left but currently facing right, so flip to face left
            FlipSprite();
        }
    }

    void FlipSprite()
    {
        // Flip the sprite by changing the local scale on the X-axis
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1; // Reverse the scale on X-axis
        transform.localScale = localScale;

        // Flip the attack box trigger as well
        //Vector3 localScale_attackBox = attackBoxTrigger.transform.localScale;
        //localScale_attackBox.x *= -1; // Reverse the scale on X-axis
        //attackBoxTrigger.transform.localScale = localScale_attackBox;
    }

    void PerformAttack() // will be called in Animation Event
    {
        attackBoxTrigger.GetComponent<EnemyAttack>().attackDamage = attackDamage; // Set the attack damage
        // Enable attack box temporarily
        attackBoxTrigger.enabled = true;

        // Optionally disable it after short delay
        Invoke(nameof(DisableAttackBox), 0.1f); // adjust timing
    }

    void DisableAttackBox()
    {
        attackBoxTrigger.enabled = false;
    }
}
