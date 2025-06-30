// Organized version of PlayerController.cs
using UnityEngine;
using System.Collections;
using Cainos.LucidEditor;

public class PlayerController : MonoBehaviour
{
    // === Header: Parameters ===
    [Header("Player Parameters")]
    [SerializeField] float m_movementSpeed = 4.0f;
    [SerializeField] float m_originalMovementSpeed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_wallJumpForce = 4.0f;
    [SerializeField] float m_rollForce = 6.0f;
    public float m_rollCooldownSeconds = 1f; // Cooldown for rolling
    public float blockCooldownsSeconds = 0.5f;
    public bool m_noBlood = false;

    [Header("Player Effects")]
    [SerializeField] GameObject m_slideDust;

    [Header("Attack Parameters")]
    public Transform attackPoint;
    public float attackRange = 2f;
    public int attackDamage = 20;
    public float attackCooldown = 1f;
    public float attackInBetweenTime = 0.5f;
    public bool isBlockCooldown = false;
    public bool isRollInCooldown = false;

    [Header("Track Player's State")]
    public bool playerIsDead = false;
    public bool playerisHurt = false;
    public bool isBlocking = false;
    public bool isParry = false;
    public bool isAttacking = false;
    public bool isWallJumping = false;
    public bool m_isWallSliding = false;
    public bool m_grounded = false;
    public bool m_rolling = false;
    public bool allowMovement = true;

    // === Private Fields ===
    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor, m_wallSensorR1, m_wallSensorR2, m_wallSensorL1, m_wallSensorL2;
    private BoxCollider2D upperBodyCollider;

    private int m_facingDirection = 1;
    private int m_currentAttack = 0;

    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private float m_attackCurrentTime;
    private float m_attackDuration = 0.4f;
    private float lastComboAttackTime = 0.0f;
    private float lastAttackTime;
    private float m_wallJumpCurrentTime;
    private float m_wallJumpDuration = 0.8f;

    private Coroutine attackCoroutine;

    public float inputX;
    public float slowFactor = 0.5f;

    // === Unity Methods ===
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

        Physics2D.IgnoreLayerCollision(3, 6);
        Physics2D.IgnoreLayerCollision(3, 3);
    }

    void Update()
    {
        UpdateTimers();
        CheckGroundedState();
        HorizontalMovement();
        FlipPlayerSprite();
        WallSliding();

        HandleAnimationStates();
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        if (isBlocking)
        {
            SetSlowMovementSpeed(slowFactor);
        }

        if (m_isWallSliding && !m_grounded && !isWallJumping)
        {
            DontAllowMovement();
        }
        else if (isWallJumping) 
        {
            AllowMovement();
        }
        else if (playerIsDead)

        {
            DontAllowMovement();
        }
        else if (!m_isWallSliding && m_grounded && !playerIsDead)
        {
            AllowMovement();
        }

    }

    void UpdateTimers()
    {
        lastComboAttackTime += Time.deltaTime;

        if (m_rolling)
        {
            m_rollCurrentTime += Time.deltaTime;
            if (m_rollCurrentTime > m_rollDuration)
            {
                m_rolling = false;
                upperBodyCollider.enabled = true;
                m_rollCurrentTime = 0f;
            }
        }

        if (isAttacking)
        {
            m_attackCurrentTime += Time.deltaTime;
            if (m_attackCurrentTime > m_attackDuration)
            {
                isAttacking = false;
                m_attackCurrentTime = 0f;
            }
        }

        if (isWallJumping)
        {
            m_wallJumpCurrentTime += Time.deltaTime;
            if (m_wallJumpCurrentTime > m_wallJumpDuration || m_isWallSliding)
            {
                isWallJumping = false;
                m_wallJumpCurrentTime = 0f;
            }
        }
    }

    void CheckGroundedState()
    {
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", true);
        }
        else if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", false);
        }
    }

    void HandleAnimationStates()
    {
        if (Input.GetKeyDown("left shift")) { Roll(); }
        else if (Input.GetKeyDown("space")) { Jump(); }
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    void FlipPlayerSprite()
    {
        if (m_body2d.velocity.x > 0) { GetComponent<SpriteRenderer>().flipX = false; m_facingDirection = 1; }
        else if (m_body2d.velocity.x < 0) { GetComponent<SpriteRenderer>().flipX = true; m_facingDirection = -1; }
    }

    void WallSliding()
    {
        if (!m_grounded)
        {
            m_isWallSliding =
                ((m_wallSensorR1.State() && m_wallSensorR2.State() && m_facingDirection == 1)
              || (m_wallSensorL1.State() && m_wallSensorL2.State() && m_facingDirection == -1))
              && !m_grounded && !isWallJumping;
        }
        else
            m_isWallSliding = false;

        m_animator.SetBool("WallSlide", m_isWallSliding);

        
        
    }

    void HorizontalMovement()
    {
        if (!allowMovement) inputX = 0;
#if UNITY_EDITOR
        // inputX = Input.GetAxis("Horizontal"); // Uncomment if needed in Editor
        if (Input.GetKeyDown(KeyCode.A)) 
        {
            SetHorizontalValue(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D)) 
        { 
            SetHorizontalValue(1); 
        }
               
#endif
            if (!m_rolling && !isWallJumping)
            m_body2d.velocity = new Vector2(inputX * m_movementSpeed, m_body2d.velocity.y);
    }

    public void Attack()
    {
        if (!playerisHurt && !playerIsDead)
        {
            // Reference to the AttackPoint if not assigned
            if (attackPoint == null)
            {
                attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();
            }

            ReleaseBlock();

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

                    StartCoroutine(SetSlowMovementSpeed(slowFactor, attackInBetweenTime));

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
                        //StopAttackHold();
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


    }
    //Method to Block Attacks
    public void Block()
    {
        if (!playerIsDead && !playerIsDead && !isBlocking)
        {
            if (!m_rolling && !isAttacking && m_grounded && !isBlockCooldown)
            {
                //StartCoroutine(Parry());
                m_animator.SetTrigger("Block");
                isBlocking = true;
                m_animator.SetBool("IdleBlock", true);
            }
        }

    }
    public void ReleaseBlock()
    {
        if (!playerIsDead && !playerisHurt)
        {
            if (isBlocking)
            {
                m_animator.SetBool("IdleBlock", false);
                isBlocking = false;
                //AllowMovement();
                SetSlowMovementSpeed(1);
                //StartCoroutine(SetSlowMovementSpeed(slowFactor,0.5f));
                StartCoroutine(BlockCooldown());
            }
        }
    }

    IEnumerator BlockCooldown()
    {
        isBlockCooldown = true;
        yield return new WaitForSeconds(blockCooldownsSeconds);
        isBlockCooldown = false;
    }
    public void Roll()
    {
        if (!playerIsDead && !playerisHurt)
        {
            ReleaseBlock();

            if (!m_rolling && !m_isWallSliding && !isRollInCooldown && !isWallJumping)
            {
                upperBodyCollider.enabled = false;
                m_rolling = true;
                m_animator.SetTrigger("Roll");
                m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
                StartCoroutine(SetRollInCooldown());
            }
        }
    }

    IEnumerator SetRollInCooldown()
    {
        isRollInCooldown = true;
        yield return new WaitForSeconds(m_rollCooldownSeconds);
        isRollInCooldown = false;

    }
    public void Jump()
    {
        if (!playerIsDead && !playerisHurt)
        {
            ReleaseBlock();

            //Check if on ground or wallsliding
            if ((m_grounded || m_isWallSliding) && !m_rolling)
            {
                TriggerJumpAnimation();

                //Wall Jump
                //Add Sideways Velocity to Opposite Direction
                if (m_isWallSliding)
                {
                    m_isWallSliding = false;
                    isWallJumping = true;
                    m_body2d.velocity = new Vector2((m_wallJumpForce * -m_facingDirection), m_body2d.velocity.y);
                    Debug.Log("Wall Jump Added force is:" + m_wallJumpForce * -m_facingDirection);
                }

                //Add Upward Velocity to Jump
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);

            }else if (!m_grounded)
            {
                Roll();
            }
        }
    }
    public void TriggerJumpAnimation()
    {
        m_animator.SetTrigger("Jump");
        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);

    }
    IEnumerator Parry()
    {
        isParry = true;
        yield return new WaitForSeconds(0.3f);
        isParry = false;
        yield break;
    }

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

    public void SetHorizontalValue(float p_inputX)
    {
        if (!playerIsDead && !playerisHurt)
        {
            inputX = allowMovement ? p_inputX : 0;
        }
    }

    public bool NoBlood() => m_noBlood;

    public bool SetPlayerDead()
    {
        DontAllowMovement();
        playerIsDead = true;
        return playerIsDead;
    }

    void DontAllowMovement() {
        allowMovement = false;
        SetHorizontalValue(0);  
    }

    void SetSlowMovementSpeed(float p_slowFactor) {
        m_movementSpeed = m_originalMovementSpeed * p_slowFactor; // Apply slow factor if needed
        Debug.Log("Slowing down player movement speed by factor: " + p_slowFactor);
    }


    IEnumerator SetSlowMovementSpeed(float p_slowFactor,float slowDuration)
    {
        Debug.Log("Slowing down player movement speed by factor: " + p_slowFactor);
        m_movementSpeed = m_originalMovementSpeed * p_slowFactor;
        yield return new WaitForSeconds(slowDuration);
        Debug.Log("Resetting player movement speed to original value.");
        m_movementSpeed = m_originalMovementSpeed;
        yield break;
    }

    public void AllowMovement()
    {
        if (!isWallJumping)
            allowMovement = true;
    }

    public void StartAttackHold()
    {
        if (attackCoroutine == null)
            attackCoroutine = StartCoroutine(ContinuousAttack());
    }

    public void StopAttackHold()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    IEnumerator ContinuousAttack()
    {
        while (true)
        {
            DoAttack();
            yield return new WaitForSeconds(0.3f);
        }
    }

    void DoAttack()
    {
        Debug.Log("Attack!");
        Attack();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    public void SetPlayerIsHurtSeconds(float seconds)
    {
        if (!playerisHurt)
        {
            StartCoroutine(SetPlayerIsHurt(seconds));
        }
        else
        {
            StopCoroutine(SetPlayerIsHurt(seconds));
            StartCoroutine(SetPlayerIsHurt(seconds));
        }
    }

    IEnumerator SetPlayerIsHurt(float hurtSeconds)
    {
        DontAllowMovement();
        playerisHurt = true;
        m_animator.SetTrigger("Hurt");
        yield return new WaitForSeconds(hurtSeconds);
        playerisHurt = false;
        AllowMovement();
        yield break;
    }
}
