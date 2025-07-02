// Second Organized version of PlayerController.cs which we use enum as Player States
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// === Player States === 
// === Description: Using enum, to manage states. This way we can sure that PLAYER only have ONE STATE at a time. 
// ===            : so we can avoid overriding Physics and Animations. Managing each state properly and isolatedly.
public enum PlayerState
{
    Neutral,
    Jumping,
    Falling,
    WallSliding,
    WallJumping,
    Attacking,
    Shielding,
    Hurting,
    LedgeGrabbing,
    Pushing,
    Pulling,
    ForceInterupt, //This is a special state, to allow any state to be interrupted by this state
    Dead,
    DelaySwitchingState
}

// === Separate Horizontal Movement, this state can coexist
public enum XVelocityState
{
    Normal,
    Stop,
    Slow,
    Rolling,
    Overriden
}

public class PlayerControllerVersion2 : MonoBehaviour
{
    [Header("Player Parameters")]
    public bool m_noBlood = false;

    [Header("Player Effects")]
    [SerializeField] GameObject m_slideDust;

    // === Private Variables === //
    private Animator playerAnimator;
    private Rigidbody2D rb;
    private Sensor_HeroKnight m_groundSensor, m_wallSensorR1, m_wallSensorR2, m_wallSensorL1, m_wallSensorL2;
    private BoxCollider2D upperBodyCollider;
    private int facingDirection = 1;
    private int currentAttackAnimation = 0;
    private float m_delayToIdle = 0.0f;

    [Header("Player States")]

    // == Variables for Player State Tracking
    private Coroutine currentStateCoroutine;
    private Coroutine currentDelayingCoroutine; // A slight delay when transitioning to a newState
    public PlayerState currentState;
    // === Variables for X Velocity === // these variables can override x velocity
    public XVelocityState currentXVelocityState;
    private Coroutine currentXVelocityStateCoroutine;
    [SerializeField] private float inputX;


    [Header("Physics Parameters")]
    // === Variables for Movement Speed === //
    public float jumpForce = 6.0f; // Force of the jump, Y velocity of player when jumping
    public int maxDoubleJumpCount = 1; // Maximum number of jumps player can perform (Double Jump)
    [SerializeField]private int currentDoubleJumpCount = 0;
    public float movementSpeed = 4.0f;
    public float slowMovementSpeed = 1.5f; private float originalMovementSpeed = 4.0f;
    public float rollingSpeed = 5.0f; private float originalRollingSpeed = 5.0f;
    public float wallSlidingSpeed = -0.3f; // Y velocity of player during wall sliding. Should be negative
    public float wallJumpForceX = 5.0f; // X velocity of player when wall jumping
    public float wallJumpForceY = 6.0f; // Y velocity of player when wall jumping

    // === Variables for Attack === //
    [Header("Attack Parameters")]
    public Transform attackPoint;
    public float attackRadius = 2f;
    public int attackDamage = 20;
    public float attackIntervalTime = 0.5f;

    [Header("Detection Triggers")]
    // == Variable for Sensors / Conditions / SubStates
    public bool isGrounded = false; // Sensor to detect if player is on a Ground (Tag)
    public bool isParry = false; // Set to true for split seconds, when player is shielding
    public bool isWallDetected = false; // Sensor to detect if there is a wall in front of the player
    public bool isUpperWallDetected = false; // Sensor to detect if there is a wall on player's head.

    [Header("Duration Variables")]
    // == Variables for Timing
    public float parryingTime = 0.3f; // Duration to parry an attack
    public float rollDuration = 0.5f; // Duration in Rolling state.
    public float hurtSeconds = 0.3f; // Duration in Hurting State

    // == Variable for Unity Editor
    public bool enabledDebugLog = true;
    public bool enabledKeyboardInput = false; // Enable Keyboard Input for Player Controller

    // === Variables for Cooldowns === //
    [Header("Cooldown Variables")]
    public float rollingCooldown = 3.0f; // Cooldown for rolling
    public float shieldingCooldown = 2.0f; // Cooldown for shielding
    // === Variables for Timeestamp === //
    [SerializeField] private float lastRollingTimestamp = 0.0f; // Last time player rolled
    [SerializeField] private float lastShieldingTimestamp = 0.0f; // Last time player shielded

    // === Unity Methods ===
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        upperBodyCollider = GetComponent<BoxCollider2D>();

        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();

        // Ignore collisions between Player and Enemies
        Physics2D.IgnoreLayerCollision(3, 6);
        Physics2D.IgnoreLayerCollision(3, 3);

        originalMovementSpeed = movementSpeed;
        originalRollingSpeed = rollingSpeed;
    }

    void Update()
    {


        // === Horizontal Movements === // 
        // === Same logic, but variables was being modified by:
        // === SetFloatInputX/SetFloatMovementSpeed
        if (currentXVelocityState == XVelocityState.Normal ||
            currentXVelocityState == XVelocityState.Slow ||
            currentXVelocityState == XVelocityState.Stop)
        {
            rb.velocity = new Vector2(inputX * movementSpeed, rb.velocity.y);
        }

        // === Handle Player Detection Triggers === //
        UpdateDetectionTriggers();

        // Flip Player Sprite based on the direction of movement
        FlipPlayerSprite();
        // === Handle Animation States === //
        UpdateAnimationStates();
        // === Handle Cooldown Timers === //
        UpdateCooldownTimers();

        // === Player Inputs on KeyBoard ==== //
        InputsFromKeyboard();
        
    }

    private void DisplayLog(string messageLog)
    {
        if (enabledDebugLog)
        {
            Debug.Log(messageLog);
        }
    }

    public void SetFloatInputX(float newInputX)
    {
        if (currentState != PlayerState.Dead && currentState != PlayerState.Hurting)
        {
            inputX = newInputX;
        }
    }

    public void SetFloatMovementSpeed(float newMoveSpeed)
    {
        movementSpeed = newMoveSpeed;
    }

    // == Method/Function to change a player state
    private void SwitchPlayerState(PlayerState newState, PlayerState delayNewState = PlayerState.DelaySwitchingState)
    {

        // Set Cooldown for Shielding when switching from Shielding state to new state
        if (currentState == PlayerState.Shielding 
            && newState != PlayerState.Shielding)
        {
            lastShieldingTimestamp = shieldingCooldown; // Set the cooldown for shielding
        }

        if (newState == PlayerState.Shielding && lastShieldingTimestamp > 0) {
            DisplayLog("Shielding is on cooldown! Cannot switch to Shielding state!");
            return;
        }

            // Uninterruptable States
            if (currentState == PlayerState.Dead || (currentState == PlayerState.Hurting && newState != PlayerState.ForceInterupt))
        {
            DisplayLog("Player is currently " + currentState + " cannot change this state!");
            return;
        }
        else if (currentXVelocityState == XVelocityState.Rolling
            && newState != PlayerState.ForceInterupt
            && newState != PlayerState.WallSliding)
        {
            DisplayLog(newState + " Cannot interupt " + currentXVelocityState + "!");
            return;
        }
        else if (currentState == PlayerState.Attacking
            && newState == PlayerState.Attacking)
        {
            DisplayLog(newState + " Cannot interupt " + currentState + "!");
            return;
        }
        else if (currentState == PlayerState.WallSliding
            && (newState == PlayerState.Attacking || newState == PlayerState.Shielding))
        {
            DisplayLog(newState + " Cannot interupt " + currentState + "!");
            return;
        }

        else
        {
            // Interrupt or STOP the currentState Coroutine
            if (currentStateCoroutine != null)
                StopCoroutine(currentStateCoroutine);

            DisplayLog("Switched to" + newState + " state. (from " + currentState + ")");
            // Replace the currentState to our newState
            currentState = newState;

            // Now, call the new state Coroutine depending on the newState
            switch (newState)
            {
                case PlayerState.Jumping:
                    currentStateCoroutine = StartCoroutine(DoJumping());
                    break;
                case PlayerState.Falling:
                    currentStateCoroutine = StartCoroutine(DoFalling());
                    //playerAnimator.SetBool("WallSlide", false);
                    break;
                case PlayerState.WallSliding:
                    currentStateCoroutine = StartCoroutine(DoWallSliding());
                    break;
                case PlayerState.WallJumping:
                    currentStateCoroutine = StartCoroutine(DoWallJumping());
                    break;
                case PlayerState.Attacking:
                    currentStateCoroutine = StartCoroutine(DoContinuousAttack());
                    break;
                case PlayerState.Shielding:
                    currentStateCoroutine = StartCoroutine(DoShielding());
                    break;
                case PlayerState.Hurting:
                    StopAllCoroutines(); // Stop all coroutines to avoid conflicts
                    currentStateCoroutine = StartCoroutine(DoHurting());
                    break;
                case PlayerState.Neutral:
                    //DisplayLog(newState + " return Normal X speed");
                    //playerAnimator.SetBool("IdleBlock", false);
                    //playerAnimator.SetBool("WallSlide", false);
                    //SetFloatInputX(0); // Reset Input X
                    SwitchXVelocityState(XVelocityState.Normal);
                    break;
                case PlayerState.ForceInterupt:
                    // Force Interupt, return to Neutral State
                    DisplayLog("Force Interupted, returning to Neutral State");
                    SwitchPlayerState(PlayerState.Neutral);
                    break;
                case PlayerState.Dead:
                    // Force Interupt, return to Neutral State
                    DoDying();
                    break;
                default:
                    DisplayLog(newState + " is not recognized.");
                    break;
            }
        }
    }

    private void SwitchXVelocityState(XVelocityState newXVelocityState)
    {
        if (currentXVelocityState == XVelocityState.Rolling 
            && newXVelocityState == XVelocityState.Rolling)
        {
            DisplayLog("Already in Rolling State, cannot switch to Rolling again!");
            return; // If already in Rolling state, then do nothing
        }

        // Interrupt any Coroutine Related to X Velocity State
        if (currentXVelocityStateCoroutine != null)
        {
            StopCoroutine(currentXVelocityStateCoroutine);
        }

        currentXVelocityState = newXVelocityState;

        // Now, call the new state Coroutine/Method depending on the newState
        switch (newXVelocityState)
        {
            case XVelocityState.Normal:
                SetFloatMovementSpeed(originalMovementSpeed);
                //playerAnimator.SetBool("WallSlide", false);
                //DisplayLog("Normal X Velocity State");
                break;
            case XVelocityState.Stop:
                SetFloatInputX(0);
                break;
            case XVelocityState.Slow:
                SetFloatMovementSpeed(slowMovementSpeed);
                break;
            case XVelocityState.Rolling:
                currentXVelocityStateCoroutine = StartCoroutine(DoRolling());
                break;
            case XVelocityState.Overriden:
                //DisplayLog("X Velocity is being overriden forcefully!");
                break;
            default:
                DisplayLog(newXVelocityState + " is not recognized.");
                break;
        }
    }

    // == IEnumerators , what player will DO during a specific state.
    // == Description: Using IEnumerators to be able to set WaitForSeconds, because some state is in a timer.
    IEnumerator DoJumping()
    {
        if (isGrounded || (!isGrounded && currentDoubleJumpCount != maxDoubleJumpCount))
        {
            playerAnimator.SetTrigger("Jump");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            currentDoubleJumpCount++;
            yield return new WaitForSeconds(0.2f);
        }
        else if (!isGrounded && isWallDetected)
        {
            // If not grounded and there is a Wall, then Wall Jump instead.
            SwitchPlayerState(PlayerState.WallJumping);
        }

        // Wait until we start falling
        while (rb.velocity.y > 0)
        {
            yield return null;
        }
        SwitchPlayerState(PlayerState.Falling);
    }
    IEnumerator DoFalling()
    {
        // Wait until grounded
        while (!isGrounded)
        {
            yield return null;
        }
        SwitchPlayerState(PlayerState.Neutral);
    }
    IEnumerator DoWallSliding()
    {
        // No Horizontal movement
        SwitchXVelocityState(XVelocityState.Overriden);
        rb.velocity = new Vector2(0, wallSlidingSpeed);
        // Wait until not grounded or there is no wall detected
        while (!isGrounded && isWallDetected)
        {
            yield return new WaitForSeconds(0.3f);
        }
        //playerAnimator.SetBool("WallSlide", false);
        SwitchPlayerState(PlayerState.Neutral);

    }
    IEnumerator DoWallJumping()
    {
        SwitchXVelocityState(XVelocityState.Overriden);
        // Jumps to opposite direction
        playerAnimator.SetTrigger("Jump");
        rb.velocity = new Vector2(wallJumpForceX * -facingDirection, wallJumpForceY);
        yield return new WaitForSeconds(0.2f);
        // Wait until we start falling
        while (rb.velocity.y > 0)
        {
            yield return null;
        }
        SwitchPlayerState(PlayerState.Falling);

    }
    IEnumerator DoRolling()
    {
        // XVelocityState.Rolling
        if (currentState == PlayerState.WallSliding)
        {
            SwitchXVelocityState(XVelocityState.Overriden);
            yield break;
        }
        if (lastRollingTimestamp > 0)
        {
            DisplayLog("Rolling is on cooldown!");
            SwitchXVelocityState(XVelocityState.Normal);
            yield break; // If rolling is on cooldown, then stop the coroutine
        }

        SwitchPlayerState(PlayerState.ForceInterupt);
        upperBodyCollider.enabled = false;
        playerAnimator.SetTrigger("Roll");
        rb.velocity = new Vector2(facingDirection * rollingSpeed, rb.velocity.y);
        yield return new WaitForSeconds(rollDuration);

        // Continue Rolling While there are still obstacles in Players Head
        while (isUpperWallDetected)
        {
            playerAnimator.SetTrigger("Roll");
            yield return new WaitForSeconds(rollDuration);
        }

        // Go back to Normal State and Apply a Cooldown
        upperBodyCollider.enabled = true;
        lastRollingTimestamp = rollingCooldown; // Set the cooldown for rolling
        SwitchXVelocityState(XVelocityState.Normal);
        SwitchPlayerState(PlayerState.Neutral);
    }
    IEnumerator DoContinuousAttack()
    {
        // PlayerState.Attacking
        // is a state that can be interrupted by other states,
        // so we can use this to attack continuously
        SwitchXVelocityState(XVelocityState.Slow);
        DoAttacking();
        yield return new WaitForSeconds(attackIntervalTime);
        SwitchPlayerState(PlayerState.Neutral);

    }
    private void DoAttacking()
    {
        // Reference to the AttackPoint if not assigned
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();
        }


        // Slow your movement speed during attack
        SwitchXVelocityState(XVelocityState.Slow);

        // Find all nearby enemies within the attack range/radius
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to the enemy
                StartCoroutine(enemy.GetComponent<Bandit>().TakeDamage(attackDamage));
            }
        }

        //Variable for Current Attack Animation
        currentAttackAnimation++;
        currentAttackAnimation = Random.Range(1, 4); // Randomly choose between 1, 2, or 3 for attack animation
        // Call one of three attack animations "Attack1", "Attack2", "Attack3"
        playerAnimator.SetTrigger("Attack" + currentAttackAnimation);
        Debug.Log("Attack: " + currentAttackAnimation);

        // If the combo is complete (after the third attack), apply the cooldown
        if (currentAttackAnimation >= 3)
        {
            // Reset animation
            currentAttackAnimation = 0;
            DisplayLog("Combo completed");
        }
    }
    void DoDying() {
        Debug.Log("Player died!");
        playerAnimator.SetBool("noBlood", m_noBlood);
        playerAnimator.SetTrigger("Death");
        this.enabled = false;
    }

    IEnumerator DoHurting()
    {
        SwitchXVelocityState(XVelocityState.Stop);
        playerAnimator.SetTrigger("Hurt");

        yield return new WaitForSeconds(hurtSeconds);

        SwitchPlayerState(PlayerState.ForceInterupt);
    }

    IEnumerator DoShielding()
    {
        if (lastShieldingTimestamp > 0)
        {
            DisplayLog("Shielding is on cooldown!");
            SwitchXVelocityState(XVelocityState.Normal);
            yield break; // If shielding is on cooldown, then stop the coroutine
        }

        SwitchXVelocityState(XVelocityState.Slow);
        StartCoroutine(DoParry());
        //playerAnimator.SetTrigger("Block");
        //playerAnimator.SetBool("IdleBlock", true);
        while (true)
        {
            yield return null;
        }
        // Wait until someone change your state
    }
    IEnumerator DoParry()
    {
        isParry = true;
        yield return new WaitForSeconds(parryingTime);
        isParry = false;
    }

    // ==== UI Button Controls/Triggers ==== 
    // ==== Add New Component - Event Trigger for your UI Buttons (Compatible for mobile devices)
    // ==== Add New Event Type ==== Check Comments what Event Type for each method.
    public void OnMoveRight() => SetFloatInputX(1);              // PointerDown, PointerEnter
    public void OnMoveLeft() => SetFloatInputX(-1);              // PointerDown, PointerEnter
    public void OnStop() => SetFloatInputX(0);
    public void OnNeutral() => SwitchPlayerState(PlayerState.Neutral);   // PointerExit, PointerUp of Any Control Buttons
    public void OnJump() => SwitchPlayerState(PlayerState.Jumping);           // PointerDown, PointerEnter
    public void OnHoldAttack() => SwitchPlayerState(PlayerState.Attacking);    // PointerDown, PointerEnter
    public void OnRoll() => SwitchXVelocityState(XVelocityState.Rolling);  // PointerDown, PointerEnter
    public void OnHoldShield() => SwitchPlayerState(PlayerState.Shielding);     // PointerDown, PointerEnter
    public void OnHurt() => SwitchPlayerState(PlayerState.Hurting);
    public void OnDead() => SwitchPlayerState(PlayerState.Dead);

    void UpdateCooldownTimers()
    {
        // Handle Rolling Cooldown
        if (lastRollingTimestamp > 0)
        {
            lastRollingTimestamp -= Time.deltaTime;
            if (lastRollingTimestamp <= 0)
            {
                lastRollingTimestamp = 0;
                DisplayLog("Rolling Cooldown is over!");
            }
        }

        // Handle Shielding Cooldown
        if (lastShieldingTimestamp > 0)
        {
            lastShieldingTimestamp -= Time.deltaTime;
            if (lastShieldingTimestamp <= 0)
            {
                lastShieldingTimestamp = 0;
                DisplayLog("Shielding Cooldown is over!");
            }
        }

    }
    void UpdateAnimationStates()
    {
        playerAnimator.SetBool("Grounded", isGrounded);
        playerAnimator.SetFloat("AirSpeedY", rb.velocity.y);
        playerAnimator.SetBool("WallSlide", currentState == PlayerState.WallSliding);
        playerAnimator.SetBool("IdleBlock", currentState == PlayerState.Shielding);

        if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            playerAnimator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                playerAnimator.SetInteger("AnimState", 0);
        }
    }

    void UpdateDetectionTriggers() {
        // === If Player is Dead, then disable all sensors and return === //
        if (currentState != PlayerState.Dead)
        {
            // === Wall Detection ==== //
            isWallDetected = (
                (m_wallSensorR1.State() && m_wallSensorR2.State() && facingDirection == 1)
            || (m_wallSensorL1.State() && m_wallSensorL2.State() && facingDirection == -1)
            );

            // === Ground Detection === //
            isGrounded = m_groundSensor.State();

            if (isGrounded) { 
                currentDoubleJumpCount = 0;
            }


            // === Upper Wall Detection === //
            isUpperWallDetected = m_wallSensorL2.State() && m_wallSensorR2.State();

            // === States that is automatically being triggered by a sensor detection ==== //
            if (!isGrounded && isWallDetected
                && currentState != PlayerState.Hurting
                && currentState != PlayerState.Dead
                && currentState != PlayerState.WallJumping)
            {
                SwitchPlayerState(PlayerState.WallSliding);
            }
        }
        else
        {
            isGrounded = false;
            isWallDetected = false;
            isUpperWallDetected = false;
        }
    }

    void FlipPlayerSprite()
    {
        if (rb.velocity.x > 0) { GetComponent<SpriteRenderer>().flipX = false; facingDirection = 1; }
        else if (rb.velocity.x < 0) { GetComponent<SpriteRenderer>().flipX = true; facingDirection = -1; }
    }

    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(facingDirection, 1, 1);
        }
    }

    public void TriggerJumpAnimation()
    {
        playerAnimator.SetTrigger("Jump");

    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }


    void InputsFromKeyboard() {
        if (enabledKeyboardInput) 
        { 
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnMoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnMoveRight();
        }
        else
        {
            // If no input, then stop moving
            //SetFloatInputX(0); 
        }
#if UNITY_EDITOR
        inputX = Input.GetAxis("Horizontal"); // This is for Unity Input System, to get the input from the keyboard
#endif
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJump(); // Jump
        }

        // Inputs that cannot be on the same time.
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.J))
        {
            OnHoldAttack(); // Attack
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKey(KeyCode.K))
        {
            OnHoldShield(); // Upon Press
        }
        else if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.K))
        {
            OnNeutral(); // Stop Shielding // Switch to Neutral State
            //OnDelayedNeutral(); // Stop Shielding with a slight delay
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.L))
        {
            OnRoll(); // Rolling
        }
    }
    }


}
