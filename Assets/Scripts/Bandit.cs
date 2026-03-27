using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;


public enum EnemyState
{
    Idle,
    Patrol,
    Jump,
    Chase,
    Attack,
    StopAttack,
    Hurt,
    StopHurt,
    Dead
}

public class Bandit : MonoBehaviour
{
    // ─── Nested Serializable Classes ───────────────────────────────────────────

    /// <summary>Inspector-visible settings for the enemy's health and health bar.</summary>
    [System.Serializable]
    public class HealthSettings
    {
        public float maxHealth = 100f;

        [Tooltip("Current health of the enemy")]
        public float currentHealth = 100f;

        public GameObject healthBarPrefab;
    }

    /// <summary>Inspector-visible settings for floating damage text display.</summary>
    [System.Serializable]
    public class FloatingDamageTextSettings
    {
        public GameObject floatingTextPrefab;
    }

    /// <summary>Inspector-visible combat state values that track runtime attack and facing data.</summary>
    [System.Serializable]
    public class ReferencesSettings
    {
        [Tooltip("Track when the enemy last attacked")]
        public float lastAttackTime = 0f;

        [Tooltip("Track which direction the enemy is facing")]
        public bool isFacingRight = false;
    }

    /// <summary>Inspector-visible settings that control knockback behaviour.</summary>
    [System.Serializable]
    public class KnockbackSettings
    {
        public float knockbackResistance = 0f;
        public float knockbackDuration = 0.2f;
    }

    /// <summary>Inspector-visible settings for the ground/wall jump sensors.</summary>
    [System.Serializable]
    public class JumpSensorSettings
    {
        public Transform groundCheck;
        public Transform wallCheck;
        public LayerMask groundLayer;
        public float checkDistance = 0.2f;
        public float jumpForce = 10f;
        // Set X velocity for jump (adjust 2.0f to your desired jump horizontal speed)
        public float jumpHorizontalSpeed = 5.0f;
        public float jumpCooldown = 5f;
    }

    /// <summary>Inspector-visible lists of audio cue names for damage and death events.</summary>
    [System.Serializable]
    public class SoundEffectsSettings
    {
        public List<string> damageSounds = new List<string>();
        public List<string> deathSounds = new List<string>();
    }

    // ─── Fields ────────────────────────────────────────────────────────────────

    [Tooltip("Current state of the enemy")]
    public EnemyState currentState = EnemyState.Idle;
    private Coroutine currentStateCoroutine;

    //[SerializeField] float      m_speed = 4.0f;
    //[SerializeField] float      m_jumpForce = 7.5f;

    private Animator m_animator;
    private Rigidbody2D rb;
    private Sensor_Bandit m_groundSensor;

    [Tooltip("Reference to the blood splash particle system")]
    public ParticleSystem m_bloodSplash;

    private bool m_grounded = false;
    private bool m_combatIdle = false;
    //[SerializeField]private bool                EnemyState.Dead = false;

    //Added: Odaz 09/29/2024
    [Tooltip("Attach this to a point in the scene or a child of the enemy")]
    public Transform attackPoint;

    [Tooltip("Attach this to a point in the scene or a child of the enemy for head detection")]
    public Transform headPoint;

    [Tooltip("Speed of the enemy")]
    public float moveSpeed = 2f;

    [Tooltip("Range in which the enemy follows the player")]
    public float followRange = 10f;

    [Tooltip("Range in which the enemy attacks the player")]
    public float attackRange = 1.5f;

    [Tooltip("Time between attacks")]
    public float attackCooldown = 1f;

    [Tooltip("Timing the end of Attack Animation")]
    public float attackTiming = 0.5f;

    [Tooltip("Damage dealt to the player")]
    public float damage = 10;

    [Header("Health")]
    public HealthSettings health = new HealthSettings();
    private FloatingHealthbar healthBarUI;

    [Header("Floating Damage Text")]
    public FloatingDamageTextSettings floatingDamageText = new FloatingDamageTextSettings();
    public Transform worldCanvas;

    [Header("References")]
    public ReferencesSettings references = new ReferencesSettings();

    [Tooltip("Reference to the player position")]
    [SerializeField] private Transform player;

    [Tooltip("Reference to the player's currentHealth script")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Reference to the player main script")]
    [SerializeField] private PlayerControllerVersion2 playerScript;

    //[SerializeField] private bool        isAttacking = false; // Track if the enemy is currently attacking
    //[SerializeField] private bool        isHurting = false; // Track when the bandit is being Hurt

    //private Coroutine attackCoroutine;

    [Header("Knockback Variables")]
    //public Rigidbody2D rb;
    //public float knockbackForce = 10f;
    public KnockbackSettings knockback = new KnockbackSettings();
    private bool isKnocked = false;

    [Header("Sensors to Jump")]
    public JumpSensorSettings jumpSensors = new JumpSensorSettings();
    private float lastJumpTime = 0f;

    private bool wasSpawned = false;

    [Header("Sound Effects")]
    public SoundEffectsSettings soundEffects = new SoundEffectsSettings();

    [Tooltip("Toggle for logging")]
    public bool enableLogging = false;

    // ─── Methods ───────────────────────────────────────────────────────────────

    /// <summary>Marks this enemy as having been spawned by the <see cref="EnemyManager"/> spawner.</summary>
    public void SetAsSpawned()
    {
        wasSpawned = true;
    }

    /// <summary>Writes <paramref name="logMessage"/> to the console when <see cref="enableLogging"/> is true.</summary>
    private void Log(string logMessage)
    {
        if (enableLogging)
            Debug.Log(logMessage);
        else
            return; // Do nothing if logging is disabled
    }

    /// <summary>Called by Unity before the first frame. Grabs the <see cref="Animator"/> component and resets the Jump trigger.</summary>
    private void Awake()
    {
        // isAttacking = false; // Initialize attacking state
        // isHurting = false; // Initialize hurting state
        m_animator = GetComponent<Animator>();
        m_animator.ResetTrigger("Jump");
    }

    /// <summary>Initializes component references, sets up the health bar, and configures the starting state.</summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();

        playerScript = GameObject.Find("HeroKnight").GetComponent<PlayerControllerVersion2>();
        player = GameObject.Find("HeroKnight").GetComponent<Transform>();
        playerHealth = player.GetComponent<PlayerHealth>();
        attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();
        references.lastAttackTime = Time.time - attackCooldown;
        worldCanvas = GameObject.Find("WorldSpaceCanvas").GetComponent<Transform>();

        // Ignore Collision Between Enemy
        Physics2D.IgnoreLayerCollision(6, 6);

        // HEALTH BAR SETUP
        health.currentHealth = health.maxHealth;

        if (health.healthBarPrefab != null)
        {
            GameObject hb = Instantiate(health.healthBarPrefab, transform.position, Quaternion.identity);
            healthBarUI = hb.GetComponent<FloatingHealthbar>();
            healthBarUI.SetTarget(this.transform);
        }

        healthBarUI.SetHealth(health.currentHealth, health.maxHealth);
    }

    /// <summary>Called once per frame. Evaluates distances to the player and drives the enemy state machine.</summary>
    void Update()
    {
        //Reference to the AttackPoint if not assigned
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();
        }

        //Reference to the Player if not assigned
        if (player == null)
        {
            player = GameObject.Find("HeroKnight").GetComponent<Transform>();
            playerHealth = player.GetComponent<PlayerHealth>();
            playerScript = player.GetComponent<PlayerControllerVersion2>();
            Debug.LogWarning("Player not found, still finding HeroKnight in the scene.");
            return;
        }

        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Calculate the distance between the enemy and the attack point
        float distanceToAttackPoint = Vector2.Distance(attackPoint.position, player.position);

        // Check if this enemy is dead or the player is dead
        if (currentState != EnemyState.Dead
            && currentState != EnemyState.Hurt
            && playerScript.currentState != PlayerState.Dead)
        {
            //Chase the Player in Follow Range
            if (distanceToPlayer <= followRange
                && distanceToAttackPoint > attackRange
                && currentState != EnemyState.Attack
                && distanceToPlayer > attackRange
                //&& currentState != EnemyState.Jump
                && m_grounded)
            {
                //ChaseState();
                SwitchEnemyState(EnemyState.Chase); // Switch to Chase state
                //m_combatIdle = true;
            }
            //Attack the Player in Attack Range
            else if (distanceToPlayer <= attackRange)
            {
                m_combatIdle = true; // Combat Pose of the Enemy
                StopXVelocity();

                if (currentState != EnemyState.Attack)
                {
                    //attackCoroutine = StartCoroutine(AttackState());
                    //StartCoroutine(AttackState());
                    SwitchEnemyState(EnemyState.Attack); // Switch to Attack state
                }
            }
            //Player Out of Range
            else
            {
                // Stop moving if outside the follow range or too close (attack range)
                StopXVelocity();
                //SwitchEn0emyState(EnemyState.Idle); // Switch to Idle state
                m_combatIdle = false;
            }
        }
        //Player is Dead
        else if (playerScript.currentState == PlayerState.Dead)
        {
            m_combatIdle = false;
        }
        //Enemy is Dead
        else
        {
            //StopXVelocity();
            //SwitchEnemyState(EnemyState.Dead); // Switch to Dead state
        }

        // Update sprite direction based on movement
        if (!isKnocked && currentState != EnemyState.Dead)
        {
            FlipSpriteBasedOnVelocity();
        }

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeed", rb.linearVelocity.y);

        //Run
        if (Mathf.Abs(rb.linearVelocity.x) > Mathf.Epsilon)
            m_animator.SetInteger("AnimState", 2);

        //Combat Neutral
        else if (m_combatIdle)
            m_animator.SetInteger("AnimState", 1);

        //Neutral
        else
            m_animator.SetInteger("AnimState", 0);

        bool isGroundAhead = Physics2D.Raycast(jumpSensors.groundCheck.position, Vector2.down, jumpSensors.checkDistance, jumpSensors.groundLayer);
        bool isWallAhead = Physics2D.Raycast(jumpSensors.wallCheck.position, Vector2.right * transform.localScale.x, jumpSensors.checkDistance, jumpSensors.groundLayer);

        // Jump if there's a wall or no ground
        if ((isWallAhead || !isGroundAhead) && m_grounded)
        {
            if (Time.time >= lastJumpTime + jumpSensors.jumpCooldown)
            {
                lastJumpTime = Time.time;
                SwitchEnemyState(EnemyState.Jump); // Switch to Jump state
            }
        }

        Debug.DrawRay(jumpSensors.groundCheck.position, Vector2.down * jumpSensors.checkDistance, Color.red);
        Debug.DrawRay(jumpSensors.wallCheck.position, Vector2.right * transform.localScale.x * jumpSensors.checkDistance, Color.blue);
    }

    /// <summary>Moves the enemy horizontally toward the player at <see cref="moveSpeed"/>.</summary>
    void ChaseState()
    {
        // Calculate the direction to the player
        Vector2 direction = (player.position - transform.position).normalized;

        // Set Y to zero, so the enemy can only moves Horizontally
        direction.y = 0;

        // Maintain the current vertical velocity (y) to preserve gravity
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>Coroutine that triggers the attack animation, waits for the hit frame, then deals damage if the player is still in range.</summary>
    IEnumerator AttackState()
    {
        if (Time.time >= references.lastAttackTime + attackCooldown + attackTiming)
        {
            references.lastAttackTime = Time.time; // Update the time of the last attack
            //isAttacking = true;
            m_animator.SetTrigger("Attack");

            // Assuming you want to wait for the animation to reach a certain point before applying damage
            yield return new WaitForSeconds(attackTiming); // Adjust this timing as per your animation

            if (playerHealth != null
                && currentState != EnemyState.Hurt
                && currentState != EnemyState.Dead)
            {
                // Calculate the distance between the enemy and the player
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);

                if (distanceToPlayer <= attackRange)
                {
                    Log("Enemy: Attacks the player!");
                    playerHealth.TakeDamage(damage, this.gameObject);
                }
            }
            else
            {
                Log("Enemy: Attack was Interrupted!");
                //SwitchEnemyState(EnemyState.StopAttack);
            }
            //isAttacking = false;
            //SwitchEnemyState(EnemyState.StopAttack); // Switch to StopHurt state after attacking
        }
        SwitchEnemyState(EnemyState.StopAttack);
    }

    /// <summary>Public entry point for dealing integer damage to this enemy; ignored when already dead.</summary>
    public void BanditReceiveDamage(int damageAmount)
    {
        if (health.currentHealth > 0 && currentState != EnemyState.Dead)
        {
            Log("Enemy: Receives " + damageAmount + " damage!");
            StartCoroutine(HurtState(damageAmount));
        }
        else
        {
            Log("Enemy is already dead, cannot take damage.");
        }
    }

    /// <summary>Public entry point that switches the enemy into the Hurt state with the given <paramref name="damageAmount"/>.</summary>
    public void TakeDamage(float damageAmount)
    {
        SwitchEnemyState(EnemyState.Hurt, damageAmount); // Switch to Hurt state with damage amount
    }

    /// <summary>Coroutine that applies damage, updates the health bar, spawns VFX and SFX, then transitions back to Idle or Dead.</summary>
    private IEnumerator HurtState(float damageAmount)
    {
        if (currentState == EnemyState.Dead)
        {
            Log("Enemy is already dead, cannot take damage.");
            yield break; // Exit if the enemy is dead
        }

        if (currentState != EnemyState.Dead)
        {
            m_animator.SetTrigger("Hurt");
            health.currentHealth -= damageAmount;
            health.currentHealth = Mathf.Clamp(health.currentHealth, 0f, health.maxHealth);

            if (healthBarUI != null)
            {
                healthBarUI.SetHealth(health.currentHealth, health.maxHealth);
            }

            if (health.currentHealth <= 0)
            {
                //Die();
                SwitchEnemyState(EnemyState.Dead); // Switch to Dead state
                //isHurting = false;
            }
            else
            {
                Instantiate(m_bloodSplash, headPoint.position, Quaternion.identity); // Instantiate blood splash effect
                //StopXVelocity();
                if (floatingDamageText.floatingTextPrefab)
                {
                    GameObject ft = Instantiate(floatingDamageText.floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                    ft.GetComponent<FloatingText>().SetText(damageAmount.ToString());
                }
                AudioManager.Instance.PlaySFX(soundEffects.damageSounds[Random.Range(0, soundEffects.damageSounds.Count)]); // Play random damage sound
                //Log("Enemy took " + damageAmount + " damage! Remaining currentHealth: " + health.currentHealth);
                //Duration when the Enemy will be on Hurt State
                yield return new WaitForSeconds(0.3f);
                Log("Hurting Stops");
                m_animator.ResetTrigger("Hurt");
                SwitchEnemyState(EnemyState.StopHurt); // Switch back to Idle state after hurting
            }
        }
        else
        {
            Log("Enemy is already dead, cannot take damage.");
        }
        yield break;
    }

    // Method to destroy the enemy when its currentHealth reaches zero

    /// <summary>Applies an upward and horizontal impulse to make the enemy jump over walls or gaps.</summary>
    void DoJump()
    {
        if (!m_grounded || isKnocked || currentState == EnemyState.Dead)
        {
            Log("Cannot jump, not grounded or already knocked or dead.");
            SwitchEnemyState(EnemyState.Idle); // Switch to Idle state if not grounded
            return; // Exit if not grounded or already knocked or dead
        }
        m_animator.SetTrigger("Jump");
        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);

        float facingDirection = references.isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(jumpSensors.jumpHorizontalSpeed * facingDirection, jumpSensors.jumpForce);

        Log("Jump velocity: " + rb.linearVelocity);

        m_groundSensor.Disable(0.2f);
    }

    /// <summary>Starts the knockback coroutine if the enemy is not already being knocked back.</summary>
    public void ApplyKnockback(Vector2 direction, float knockbackForce)
    {
        if (!isKnocked)
        {
            StartCoroutine(KnockbackCoroutine(direction, knockbackForce));
        }
    }

    /// <summary>Coroutine that applies a physics impulse in <paramref name="direction"/> for <see cref="KnockbackSettings.knockbackDuration"/> seconds.</summary>
    IEnumerator KnockbackCoroutine(Vector2 direction, float knockbackForce)
    {
        if (currentState == EnemyState.Dead)
        {
            yield break; // Do not apply knockback if already dead
        }

        isKnocked = true;

        float adjustedForce = knockbackForce * (1f - Mathf.Clamp01(knockback.knockbackResistance));

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * adjustedForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockback.knockbackDuration);

        rb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }

    /// <summary>Draws follow-range and attack-range gizmos in the Scene view when the object is selected.</summary>
    private void OnDrawGizmosSelected()
    {
        // Draw follow range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);

        // Draw attack range around the attackPoint
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        if (jumpSensors.groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(jumpSensors.groundCheck.position, jumpSensors.groundCheck.position + Vector3.down * jumpSensors.checkDistance);
        }

        if (jumpSensors.wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = Vector3.right * transform.localScale.x;
            Gizmos.DrawLine(jumpSensors.wallCheck.position, jumpSensors.wallCheck.position + direction * jumpSensors.checkDistance);
        }
    }

    /// <summary>Flips the sprite to match the current horizontal movement direction.</summary>
    void FlipSpriteBasedOnVelocity()
    {
        // Check the enemy's velocity on the X-axis to determine direction
        if (rb.linearVelocity.x > 0 && !references.isFacingRight)
        {
            // Moving right but currently facing left, so flip to face right
            FlipSprite();
        }
        else if (rb.linearVelocity.x < 0 && references.isFacingRight)
        {
            // Moving left but currently facing right, so flip to face left
            FlipSprite();
        }
    }

    /// <summary>Toggles <see cref="ReferencesSettings.isFacingRight"/> and mirrors the transform's X scale.</summary>
    void FlipSprite()
    {
        // Flip the sprite by changing the local scale on the X-axis
        references.isFacingRight = !references.isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1; // Reverse the scale on X-axis
        transform.localScale = localScale;
    }

    /// <summary>Zeroes the rigidbody's horizontal velocity unless the enemy is currently jumping.</summary>
    void StopXVelocity()
    {
        if (currentState != EnemyState.Jump)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>Placeholder patrol behaviour; currently just logs the patrol state.</summary>
    void PatrolState()
    {         // Implement patrol logic here
        // For example, move back and forth between two points or randomly within a defined area
        // This is a placeholder for the patrol logic
        Log("Patrolling...");
    }

    /// <summary>Stops horizontal movement and logs the idle state.</summary>
    void IdleState()
    {
        StopXVelocity();
        Log("Idle");
    }

    /// <summary>Plays the death animation and SFX, destroys the health bar, notifies the <see cref="EnemyManager"/>, then destroys this GameObject.</summary>
    void DeadState()
    {
        Log("Enemy died!");
        AudioManager.Instance.PlaySFX(soundEffects.deathSounds[Random.Range(0, soundEffects.deathSounds.Count)]); // Play random death sound

        // Destroy health bar
        if (healthBarUI != null)
        {
            healthBarUI.DestroyBar();
        }

        m_animator.SetTrigger("Death");
        //EnemyState.Dead = true;
        if (wasSpawned)
        {
            EnemyManager.Instance.UnregisterEnemy();
        }
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// Transitions the enemy to <paramref name="newState"/>, stopping any running state coroutine first.
    /// Ignores the transition when guarded states (Dead, Hurt, duplicate Attack) are active.
    /// </summary>
    public void SwitchEnemyState(EnemyState newState, float damageAmount = 0)
    {
        if (currentState == EnemyState.Dead)
        {
            return;
        }

        if (currentState == EnemyState.Hurt
            && newState != EnemyState.StopHurt
            && newState != EnemyState.Dead)
        {
            Log("Enemy is currently hurting, cannot switch to " + newState);
            return;
        }

        if (currentState == EnemyState.Attack && newState == EnemyState.Attack)
        {
            Log("Enemy is currently attacking, cannot switch to " + newState);
            return;
        }

        if (currentStateCoroutine != null)
        {
            StopCoroutine(currentStateCoroutine);
        }

        currentState = newState; // Update the current state
        switch (newState)
        {
            case EnemyState.Idle:
                //currentStateCoroutine = StartCoroutine(IdleState());
                IdleState();
                break;
            case EnemyState.Patrol:
                //currentStateCoroutine = StartCoroutine(PatrolState());
                PatrolState();
                break;
            case EnemyState.Chase:
                //currentStateCoroutine = StartCoroutine(ChaseState());
                ChaseState();
                break;
            case EnemyState.Jump:
                //currentStateCoroutine = StartCoroutine(DeadState());
                DoJump();
                break;
            case EnemyState.Attack:
                currentStateCoroutine = StartCoroutine(AttackState());
                break;
            case EnemyState.StopAttack:
                //currentStateCoroutine = StartCoroutine(HurtState(damageAmount));
                IdleState();
                break;
            case EnemyState.Hurt:
                currentStateCoroutine = StartCoroutine(HurtState(damageAmount));
                break;
            case EnemyState.StopHurt:
                //currentStateCoroutine = StartCoroutine(HurtState(damageAmount));
                IdleState();
                break;
            case EnemyState.Dead:
                //currentStateCoroutine = StartCoroutine(DeadState());
                DeadState();
                break;
        }
    }
}