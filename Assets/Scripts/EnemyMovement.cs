using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [System.Serializable]
    public class PatrolSettings
    {
        [Tooltip("Patrol point A.")]
        public Transform pointA;
        [Tooltip("Patrol point B.")]
        public Transform pointB;
        [Tooltip("Speed of patrolling.")]
        public float patrolSpeed = 2f;
    }

    [System.Serializable]
    public class ChaseSettings
    {
        [Tooltip("Speed when chasing the player.")]
        public float chaseSpeed = 3f;
        [Tooltip("Range to detect the player.")]
        public float detectionRange = 5f;
        [Tooltip("Reference to the player.")]
        public Transform player;
    }

    [System.Serializable]
    public class GroundCheckSettings
    {
        [Tooltip("A point to check if the enemy is grounded.")]
        public Transform groundCheck;
        [Tooltip("Radius of the ground check.")]
        public float groundCheckRadius = 0.2f;
        [Tooltip("LayerMask to specify what is considered ground.")]
        public LayerMask groundLayer;
    }

    [System.Serializable]
    public class EnemyFlags
    {
        [Tooltip("Whether the enemy is grounded.")]
        public bool isGrounded;
        [Tooltip("Whether the skeleton is chasing the player.")]
        [SerializeField] public bool isChasing = false;
        [SerializeField] public bool isFacingRight = false;
        [Tooltip("Whether the skeleton is currently attacking.")]
        public bool isAttacking = false;
    }

    [System.Serializable]
    public class AttackSettings
    {
        [Tooltip("Trigger for the attack box.")]
        public BoxCollider2D attackBoxTrigger;
        [Tooltip("Damage dealt by the attack.")]
        public float attackDamage = 20f;
        [Tooltip("Range to detect the player.")]
        public float attackRange = 1f;
        [Tooltip("Animation trigger for the attack.")]
        public string[] attackAnimationTrigger;
        [Tooltip("Time in seconds between attacks (attack speed).")]
        public float attackCooldown = 1.5f;
    }

    [Tooltip("Name of the enemy.")]
    public string enemyName = "Skeleton";

    [Header("Patrol Params")]
    public PatrolSettings patrol = new PatrolSettings();

    [Header("Chase Params")]
    public ChaseSettings chase = new ChaseSettings();

    private Transform currentTarget;
    private Rigidbody2D rb;
    private Animator m_animator;

    [Header("Ground Check")]
    public GroundCheckSettings groundDetect = new GroundCheckSettings();

    [Header("Flags")]
    public EnemyFlags flags = new EnemyFlags();

    private EnemyHealth enemyHealth;

    [Header("Attack Settings")]
    public AttackSettings attack = new AttackSettings();

    private float lastAttackTime = -999f;
    private EnemyJumpDetection obstacle_Detector;

    /// <summary>
    /// Initializes references to required components and sets the initial patrol target.
    /// </summary>
    private void Start()
    {
        // Start patrolling towards point A
        currentTarget = patrol.pointA;

        // Find the player by tag
        chase.player = GameObject.FindGameObjectWithTag("Player").transform;

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

    /// <summary>
    /// Handles per-frame logic including player detection, attack triggering, animator parameter updates, and sprite flipping.
    /// </summary>
    private void Update()
    {
        if (enemyHealth.flags.isDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (Vector3.Distance(transform.position, chase.player.position) <= chase.detectionRange)
        {
            // Start chasing the player
            flags.isChasing = true;
        }
        else
        {
            // Return to patrolling if the player is out of range
            flags.isChasing = false;
        }

        if (Vector3.Distance(transform.position, chase.player.position) <= attack.attackRange
            && !flags.isAttacking
            && Time.time >= lastAttackTime + attack.attackCooldown)
        {
            lastAttackTime = Time.time;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            int randomIndex = Random.Range(0, attack.attackAnimationTrigger.Length);
            m_animator.SetTrigger(attack.attackAnimationTrigger[randomIndex]); // Trigger the attack animation
        }

        // Set Running Parameters in the Animator
        m_animator.SetFloat("Velocity_X", Mathf.Abs(rb.linearVelocity.x));
        m_animator.SetFloat("Velocity_Y", rb.linearVelocity.y);
        m_animator.SetBool("IsGrounded", flags.isGrounded);

        // Flip the sprite based on velocity
        if (!enemyHealth.flags.isHurt && !enemyHealth.flags.isDead)
        {
            FlipSpriteBasedOnVelocity();
        }
    }

    /// <summary>
    /// Handles physics-based updates including ground detection and movement selection between patrol and chase.
    /// </summary>
    private void FixedUpdate()
    {
        // Check if the enemy is grounded
        flags.isGrounded = Physics2D.OverlapCircle(groundDetect.groundCheck.position, groundDetect.groundCheckRadius, groundDetect.groundLayer);

        if (flags.isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    /// <summary>
    /// Moves the enemy back and forth between patrol points A and B when not chasing the player.
    /// </summary>
    private void Patrol()
    {
        if (enemyHealth.flags.isDead || enemyHealth.flags.isHurt || flags.isAttacking || !patrol.pointA || !patrol.pointB ||
            obstacle_Detector.obstacleDetected)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Calculate direction to the current patrol target
        Vector2 direction = (currentTarget.position - transform.position).normalized;

        // Set velocity towards the target
        rb.linearVelocity = new Vector3(direction.x * patrol.patrolSpeed, rb.linearVelocity.y);

        // Switch target when reaching the current patrol point
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
        {
            currentTarget = currentTarget == patrol.pointA ? patrol.pointB : patrol.pointA;
        }
    }

    /// <summary>
    /// Moves the enemy toward the player when within detection range.
    /// </summary>
    private void ChasePlayer()
    {
        if (enemyHealth.flags.isDead || enemyHealth.flags.isHurt || flags.isAttacking || obstacle_Detector.obstacleDetected)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        } // Don't do anything if the enemy is dead

        // Calculate direction to the player
        Vector2 direction = (chase.player.position - transform.position).normalized;

        // Set velocity towards the player
        rb.linearVelocity = new Vector3(direction.x * chase.chaseSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Draws debug gizmos for detection range, attack range, and ground check radius in the editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw the detection range in the editor for debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chase.detectionRange);

        // Draw the attack range in the editor for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attack.attackRange);

        // Draw the attack range in the editor for debugging
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundDetect.groundCheck.position, groundDetect.groundCheckRadius);
    }

    /// <summary>
    /// Flips the enemy sprite based on the current horizontal velocity direction.
    /// </summary>
    private void FlipSpriteBasedOnVelocity()
    {
        if (flags.isAttacking || !flags.isGrounded)
        {
            return;
        }
        // Check the enemy's velocity on the X-axis to determine direction
        if (rb.linearVelocity.x > 0 && !flags.isFacingRight)
        {
            // Moving right but currently facing left, so flip to face right
            FlipSprite();
        }
        else if (rb.linearVelocity.x < 0 && flags.isFacingRight)
        {
            // Moving left but currently facing right, so flip to face left
            FlipSprite();
        }
    }

    /// <summary>
    /// Flips the enemy sprite by inverting the local X scale.
    /// </summary>
    private void FlipSprite()
    {
        // Flip the sprite by changing the local scale on the X-axis
        flags.isFacingRight = !flags.isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1; // Reverse the scale on X-axis
        transform.localScale = localScale;

        // Flip the attack box trigger as well
        //Vector3 localScale_attackBox = attackBoxTrigger.transform.localScale;
        //localScale_attackBox.x *= -1; // Reverse the scale on X-axis
        //attackBoxTrigger.transform.localScale = localScale_attackBox;
    }

    /// <summary>
    /// Enables the attack box collider and sets its damage value. Called via Animation Event.
    /// </summary>
    void PerformAttack() // will be called in Animation Event
    {
        attack.attackBoxTrigger.GetComponent<EnemyAttack>().attackDamage = attack.attackDamage; // Set the attack damage
        // Enable attack box temporarily
        attack.attackBoxTrigger.enabled = true;

        // Optionally disable it after short delay
        Invoke(nameof(DisableAttackBox), 0.1f); // adjust timing
    }

    /// <summary>
    /// Disables the attack box collider after the attack window has elapsed.
    /// </summary>
    void DisableAttackBox()
    {
        attack.attackBoxTrigger.enabled = false;
    }
}
