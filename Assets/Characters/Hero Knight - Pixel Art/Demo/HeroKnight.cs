using UnityEngine;
using System.Collections;
using Cainos.LucidEditor;

public class HeroKnight : MonoBehaviour {

    [Header("Player Parameters")]
    [SerializeField] float m_movementSpeed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_wallJumpForce = 4.0f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;

    [Header("Player Effects")]
    [SerializeField] GameObject m_slideDust;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;

    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_delayToIdle = 0.0f;

    //Roll Variables
    private BoxCollider2D upperBodyCollider;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private float inputX;


    //Attack Variables
    [Header("Attack Parameters")]
    public Transform    attackPoint;
    public float        attackRange = 2f; // Player's attack range
    public int          attackDamage = 20;
    public float        attackCooldown = 1f; // Cooldown between Full Combo attacks
    public float        attackInBetweenTime = 0.5f; //Interval between combo

    private float lastComboAttackTime = 0.0f; //Time between attacks of combo   
    private float lastAttackTime; // Time after full combo
    private float m_attackCurrentTime;
    private float m_attackDuration = 0.4f;

    //WallJump Variables
    private float m_wallJumpCurrentTime;
    private float m_wallJumpDuration = 0.8f;

    //Player States
    [Header("Track Player's State")]
    public bool playerIsDead    = false; //Track Player Dead State
    public bool isBlocking      = false;
    public bool isParry         = false;
    public bool isAttacking     = false; // Track the player attacking state
    public bool isWallJumping   = false; // Trach the player is wallJumping
    public bool m_isWallSliding = false;
    public bool m_grounded      = false;
    public bool m_rolling       = false;
    public bool allowMovement   = true;

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        upperBodyCollider = GetComponent<BoxCollider2D>();

        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();

        lastAttackTime = Time.time - attackCooldown;

        // Ignore Collision Between Player (Layer 3) and Enemy (Layer 6)
        Physics2D.IgnoreLayerCollision(3, 6);
        Physics2D.IgnoreLayerCollision(3, 3);
    }

    // Update is called once per frame
    void Update()
    {

        // Increase timer that controls attack combo
        lastComboAttackTime += Time.deltaTime;

        // Increase timer that checks roll duration
        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // Disable rolling if timer extends duration
        if (m_rollCurrentTime > m_rollDuration)
        {
            m_rolling = false;
            upperBodyCollider.enabled = true;
            //Reset roll timer
            m_rollCurrentTime = 0f;
        }

        // Track time of Attack State
        if (isAttacking)
            m_attackCurrentTime += Time.deltaTime;

        // Disable attack state if timer extends duration
        if (m_attackCurrentTime > m_attackDuration)
        {
            isAttacking = false;
            //Reset attack timer
            m_attackCurrentTime = 0f;
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

        HorizontalMovement();

        FlipPlayerSprite();

        WallSliding();
       
        if (isAttacking||isBlocking)
        {
            StopMovement();
        }

        // -- Handle Inputs --
        //Attack
        if (Input.GetMouseButtonDown(0) && m_grounded)
        {
            Attack();
        }

        // Block
        else if (Input.GetMouseButtonDown(1))
        {
            Block();
        }
        //Release Block
        else if (Input.GetMouseButtonUp(1))
        {
            ReleaseBlock();
        }
        // Roll
        else if (Input.GetKeyDown("left shift"))
        {
            Roll();
        }
        //Jump
        else if (Input.GetKeyDown("space"))
        {
            Jump();
        }
        //Run Animation
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        //Idle Animation
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);
    }

    void Attack()
    {
        // Reference to the AttackPoint if not assigned
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();
        }

        // Check Cooldown
        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            // Check In Between Time
            if (lastComboAttackTime > attackInBetweenTime && !m_rolling)
            {
                // Enter Attacking State
                isAttacking = true;
                // Release Block State
                isBlocking = false;
                //Stop Horizontal Movement
                //StopMovement();

                // Find all nearby enemies within the attack range
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

                foreach (Collider2D enemy in hitEnemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        // Apply damage to the enemy
                        StartCoroutine(enemy.GetComponent<Bandit>().TakeDamage(attackDamage));
                    }
                }

                //Variable for Current Attack Animation
                m_currentAttack++;

                // Call one of three attack animations "Attack1", "Attack2", "Attack3"
                m_animator.SetTrigger("Attack" + m_currentAttack);
                Debug.Log("Attack" + m_currentAttack);

                // Reset timer
                lastComboAttackTime = 0.0f;

                // If the combo is complete (after the third attack), apply the cooldown
                if (m_currentAttack >= 3)
                {
                    // Loop back to one for next combo
                    m_currentAttack = 0;

                    // Set cooldown after the full combo
                    lastAttackTime = Time.time;

                    //Allow Movement Horizontally
                    //AllowMovement();
                    Debug.Log("Combo completed, entering cooldown.");
                }
            }

        }

        // Reset combo animation if too much time has passed between attacks
        if (lastComboAttackTime > attackInBetweenTime + 2f)
        {
            m_currentAttack = 0;
            //AllowMovement();
            Debug.Log("Combo is reset due to delay.");
        }


    }

    private void OnDrawGizmosSelected()
    {
        // Draw the attack range when the player is selected in the editor
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    //Method to Block Attacks
    void Block() {
        if (!m_rolling && !isAttacking && m_grounded)
        {
            //StopMovement();
            StartCoroutine(Parry());
            m_animator.SetTrigger("Block");
            isBlocking = true;
            m_animator.SetBool("IdleBlock", true);
        }

    }

    void ReleaseBlock() {
        m_animator.SetBool("IdleBlock", false);
        isBlocking = false;
        AllowMovement();
    }

    void Roll() {
        if (!m_rolling && !m_isWallSliding)
        {
            upperBodyCollider.enabled = false;
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }
    }

    // Method to Trigger Jump Animation for other Objects like Bounding Platform
    public void TriggerJumpAnimation() {
        m_animator.SetTrigger("Jump");
        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);

    }

    void Jump() {
        //Check if on ground or wallsliding
        if ((m_grounded||m_isWallSliding) && !m_rolling)
        {
            TriggerJumpAnimation();

            //Wall Jump
            //Add Sideways Velocity to Opposite Direction
            if (m_isWallSliding) {
                //AllowMovement();
                isWallJumping = true;
                m_body2d.velocity = new Vector2((m_wallJumpForce * -m_facingDirection), m_body2d.velocity.y);
                Debug.Log("Added force is:" + m_wallJumpForce * -m_facingDirection);
            }

            //Add Upward Velocity to Jump
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
            
        }
    }

    IEnumerator Parry() {
        isParry = true;
        yield return new WaitForSeconds(0.3f);
        isParry = false;
    }

    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    void FlipPlayerSprite()
    { // Swap direction of sprite depending on Player Velocity
        if (m_body2d.velocity.x > 0)//(inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }

        else if (m_body2d.velocity.x < 0)//(inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }
    }

    void WallSliding() {
        //Wall Slide

        //Check Wall Sensor and Ground
        if (!m_grounded)
        {
            m_isWallSliding =
                (   (m_wallSensorR1.State() && m_wallSensorR2.State() && m_facingDirection ==  1)
                ||  (m_wallSensorL1.State() && m_wallSensorL2.State() && m_facingDirection == -1)
                &&  !m_grounded && !isWallJumping);
        }
        else 
        {
            m_isWallSliding = false;           
        };

        m_animator.SetBool("WallSlide", m_isWallSliding);

        //Disable Horizontal movement when wall sliding
        if (m_isWallSliding)
        {
            StopMovement();
        }
        else if (!m_isWallSliding)
        {
            AllowMovement();
        }

        //WallJumping Tracker

        // Increase timer that checks wall Jump duration
        if (isWallJumping)
            m_wallJumpCurrentTime += Time.deltaTime;

        // Disable wallJumping state if timer extends duration or player is Wall Sliding
        if ((m_wallJumpCurrentTime > m_wallJumpDuration) || (m_isWallSliding))
        {
            isWallJumping = false;
            //Reset wallJump timer
            m_wallJumpCurrentTime = 0f;
        }


    }

    public void HorizontalMovement() {
        // -- Handle input for movement --
        if (!allowMovement)
        {
            inputX = 0;

        }
        else if (allowMovement)
        {
            inputX = Input.GetAxis("Horizontal");
        }

        // Horizontal Movement on RigidBody using Input X
        if (!m_rolling && !isWallJumping)
        {
            m_body2d.velocity = new Vector2(inputX * m_movementSpeed, m_body2d.velocity.y);
        }


    }

    public bool NoBlood() { 
    return m_noBlood;
    }

    public bool SetPlayerDead() {
        StopMovement();
        playerIsDead = true;
        return playerIsDead;
    }

    void StopMovement()
    {
        m_body2d.velocity = new Vector2(0, m_body2d.velocity.y);
        //m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_body2d.velocity.y);
        allowMovement = false;
    }

    public void AllowMovement() {
        if (!isWallJumping)
        {
            allowMovement = true;
        }
    }
}
