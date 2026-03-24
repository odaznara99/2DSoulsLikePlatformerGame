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

    public EnemyState currentState = EnemyState.Idle; // Current state of the enemy
    private Coroutine currentStateCoroutine; // Current coroutine according to the state

    //[SerializeField] float      m_speed = 4.0f;
    //[SerializeField] float      m_jumpForce = 7.5f;

    private Animator m_animator;
    private Rigidbody2D rb;
    private Sensor_Bandit m_groundSensor;
    public ParticleSystem m_bloodSplash; // Reference to the blood splash particle system
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    //[SerializeField]private bool                EnemyState.Dead = false;

    //Added: Odaz 09/29/2024    
    public Transform attackPoint; // Attach this to a point in the scene or a child of the enemya
    public Transform headPoint; // Attach this to a point in the scene or a child of the enemy for head detection
    public float moveSpeed = 2f; // Speed of the enemy
    public float followRange = 10f; // Range in which the enemy follows the player
    public float attackRange = 1.5f; // Range in which the enemy attacks the player    
    public float attackCooldown = 1f; // Time between attacks
    public float attackTiming = 0.5f; // Timing the end of Attack Animation
    
    public float damage = 10; // Damage dealt to the player

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth = 100f; // Health of the enemy
    [SerializeField] private GameObject healthBarPrefab;
    private FloatingHealthbar healthBarUI;

    [Header("Floating Damage Text")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvas;

    [Header("References")]
    [SerializeField] private Transform player; // Reference to the player position
    [SerializeField] private PlayerHealth playerHealth; // Reference to the player's currentHealth script
    [SerializeField] private PlayerControllerVersion2 playerScript; // Reference to the player main script
    [SerializeField] private float lastAttackTime = 0f; // Track when the enemy last attacked
    //[SerializeField] private bool        isAttacking = false; // Track if the enemy is currently attacking
    [SerializeField] private bool isFacingRight = false; // Track which direction the enemy is facing
    //[SerializeField] private bool        isHurting = false; // Track when the bandit is being Hurt

    //private Coroutine attackCoroutine;

    [Header("Knockback Variables")]
    //public Rigidbody2D rb;
    //public float knockbackForce = 10f;
    public float knockbackResistance = 0f;
    public float knockbackDuration = 0.2f;
    private bool isKnocked = false;

    [Header("Sensors to Jump")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public float checkDistance = 0.2f;
    public float jumpForce = 10f;
    // Set X velocity for jump (adjust 2.0f to your desired jump horizontal speed)
    public float jumpHorizontalSpeed = 5.0f;
    private float lastJumpTime = 0f;
    public float jumpCooldown = 5f;

    private bool wasSpawned = false;

    [Header("Sound Effects")]
    public List<string> damageSounds = new List<string>();
    public List<string> deathSounds = new List<string>();

    // Called by spawner after instantiation
    public void SetAsSpawned()
    {
        wasSpawned = true;
    }
    public bool enableLogging = false; // Toggle for logging
    private void Log(string logMessage) { 
        if (enableLogging)
            Debug.Log(logMessage);
        else
            return; // Do nothing if logging is disabled
    }



    private void Awake()
    {
        // isAttacking = false; // Initialize attacking state
        // isHurting = false; // Initialize hurting state
        m_animator = GetComponent<Animator>();
        m_animator.ResetTrigger("Jump");
    }

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();

        playerScript = GameObject.Find("HeroKnight").GetComponent<PlayerControllerVersion2>();
        player = GameObject.Find("HeroKnight").GetComponent<Transform>();
        playerHealth = player.GetComponent<PlayerHealth>();
        attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();
        lastAttackTime = Time.time - attackCooldown;
        worldCanvas = GameObject.Find("WorldSpaceCanvas").GetComponent<Transform>();

        // Ignore Collision Between Enemy
        Physics2D.IgnoreLayerCollision(6, 6);

        // HEALTH BAR SETUP
        currentHealth = maxHealth;

        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarUI = hb.GetComponent<FloatingHealthbar>();
            healthBarUI.SetTarget(this.transform);
        }

        healthBarUI.SetHealth(currentHealth, maxHealth);

    }

    // Update is called once per frame
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

        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);
        bool isWallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * transform.localScale.x, checkDistance, groundLayer);

        // Jump if there's a wall or no ground
        if ((isWallAhead || !isGroundAhead) && m_grounded)
        {
            if (Time.time >= lastJumpTime + jumpCooldown)
            {
                lastJumpTime = Time.time;
                SwitchEnemyState(EnemyState.Jump); // Switch to Jump state
            }
        }

        Debug.DrawRay(groundCheck.position, Vector2.down * checkDistance, Color.red);
        Debug.DrawRay(wallCheck.position, Vector2.right * transform.localScale.x * checkDistance, Color.blue);

    }

    // Method to follow the player
    void ChaseState()
    {
        // Calculate the direction to the player
        Vector2 direction = (player.position - transform.position).normalized;

        // Set Y to zero, so the enemy can only moves Horizontally
        direction.y = 0;

        // Maintain the current vertical velocity (y) to preserve gravity
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    // Method to attack the player
    IEnumerator AttackState()
    {
        if (Time.time >= lastAttackTime + attackCooldown + attackTiming)
        {
            lastAttackTime = Time.time; // Update the time of the last attack  
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

    // Method to receive damage when attacked by the player
    public void BanditReceiveDamage(int damageAmount)
    {
        if (currentHealth > 0 && currentState != EnemyState.Dead)
        {
            Log("Enemy: Receives " + damageAmount + " damage!");
            StartCoroutine(HurtState(damageAmount));
        }
        else
        {
            Log("Enemy is already dead, cannot take damage.");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        SwitchEnemyState(EnemyState.Hurt, damageAmount); // Switch to Hurt state with damage amount
    }

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
            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            if (healthBarUI != null)
            {
                healthBarUI.SetHealth(currentHealth, maxHealth);
            }

            if (currentHealth <= 0)
            {
                //Die();
                SwitchEnemyState(EnemyState.Dead); // Switch to Dead state
                //isHurting = false;
            }
            else
            {
                Instantiate(m_bloodSplash, headPoint.position, Quaternion.identity); // Instantiate blood splash effect
                //StopXVelocity();
                if (floatingTextPrefab)
                {
                    GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                    ft.GetComponent<FloatingText>().SetText(damageAmount.ToString());
                }
                AudioManager.Instance.PlaySFX(damageSounds[Random.Range(0, damageSounds.Count)]); // Play random damage sound
                //Log("Enemy took " + damageAmount + " damage! Remaining currentHealth: " + currentHealth);
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

    //Method to make the Bandit Jump
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

        float facingDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(jumpHorizontalSpeed * facingDirection, jumpForce);

        Log("Jump velocity: " + rb.linearVelocity);

        m_groundSensor.Disable(0.2f);
    }
    public void ApplyKnockback(Vector2 direction, float knockbackForce)
    {
        if (!isKnocked)
        {
            StartCoroutine(KnockbackCoroutine(direction, knockbackForce));
        }
    }

    IEnumerator KnockbackCoroutine(Vector2 direction, float knockbackForce)
    {
        if (currentState == EnemyState.Dead)
        {
            yield break; // Do not apply knockback if already dead
        }

        isKnocked = true;

        float adjustedForce = knockbackForce * (1f - Mathf.Clamp01(knockbackResistance));

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * adjustedForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }

    // Optional: For visual representation, you can use Gizmos to show the follow and attack ranges in the editor
    private void OnDrawGizmosSelected()
    {
        // Draw follow range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);

        // Draw attack range around the attackPoint
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * checkDistance);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = Vector3.right * transform.localScale.x;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + direction * checkDistance);
        }
    }

    void FlipSpriteBasedOnVelocity()
    {
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
    }

    void StopXVelocity()
    {
        if (currentState != EnemyState.Jump)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void PatrolState()
    {         // Implement patrol logic here
        // For example, move back and forth between two points or randomly within a defined area
        // This is a placeholder for the patrol logic
        Log("Patrolling...");
    }

    void IdleState()
    {
        StopXVelocity();
        Log("Idle");

    }

    void DeadState()
    {
        Log("Enemy died!");
        AudioManager.Instance.PlaySFX(deathSounds[Random.Range(0, deathSounds.Count)]); // Play random death sound

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

    public void SwitchEnemyState(EnemyState newState, float damageAmount = 0)
    {
        if (currentState == EnemyState.Dead) {
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