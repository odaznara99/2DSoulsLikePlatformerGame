// Second Organized version of PlayerController.cs which we use enum as Player States
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

// === Player States === 
// === Description: Using enum, to manage states. This way we can sure that PLAYER only have ONE STATE at a time. 
// ===            : so we can avoid overriding Physics and Animations. Managing each state properly and isolatedly.
public enum PlayerState
{
    Neutral,
    Jumping,
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
    // == Variable for Unity Editor
    [Header("Unity Editor Settings")]
    public bool enabledDebugLog = true;

    [Header("Player Parameters")]
    public bool m_noBlood = false;

    [Header("Player Effects")]
    [SerializeField] GameObject m_slideDust;
    //[SerializeField] private GameObject[] hitEffectPrefabs;

    // === Private Variables === //
    private Animator playerAnimator;
    private Rigidbody2D rb;
    private PlayerHealth playerHealth;
    private Sensor_HeroKnight m_groundSensor, m_wallSensorR1, m_wallSensorR2, m_wallSensorL1, m_wallSensorL2;
    public CapsuleCollider2D upperBodyCollider;
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
    

    [Header("Movement Parameters")]
    [SerializeField] private float inputX;
    public float movementSpeed = 4.0f;
    public float slowMovementSpeed = 1.5f; private float originalMovementSpeed = 4.0f;
    public float rollingSpeed = 5.0f; private float originalRollingSpeed = 5.0f;

    [Header("Jump Settings")]
    public float jumpForce = 6.0f; // Force of the jump, Y velocity of player when jumping
    public int maxDoubleJumpCount = 1; // Maximum number of jumps player can perform (Double Jump)
    [SerializeField]private int currentDoubleJumpCount = 0;
    
    [Header("Wall Jump Settings")]
    public float wallSlidingSpeed = -0.3f; // Y velocity of player during wall sliding. Should be negative
    public float wallJumpForceX = 5.0f; // X velocity of player when wall jumping
    public float wallJumpForceY = 6.0f; // Y velocity of player when wall jumping

    [Header("Momentum Settings")]
    public float airControlFactor = 0.5f; // How much control the player has in the air
    public float groundControlFactor = 1.0f; // How much control the player has on the ground
    public float directionChangeSpeed = 5f; // How quickly the player can change direction


    // === Variables for Attack === //
    [Header("Attack Settings")]
    [Tooltip("Damage dealt by the attack")] public float attackDamage = 20f;
    [SerializeField] private GameObject attackBoxObject; // The object with BoxCollider2D
    [SerializeField] private Collider2D attackBoxCollider;
    public float attackIntervalTime = 0.5f;
    [Tooltip("Knock back to enemies")]public float playerKnockbackForce = 15f; // Knock back to enemies

    [Header("Detection Flags")]
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

    // === Variables for Cooldowns === //
    [Header("Cooldown Variables")]
    public float rollingCooldown = 3.0f; // Cooldown for rolling
    public float shieldingCooldown = 2.0f; // Cooldown for shielding
    // === Variables for Timeestamp === //
    [SerializeField] private float lastRollingTimestamp = 0.0f; // Last time player rolled
    [SerializeField] private float lastShieldingTimestamp = 0.0f; // Last time player shielded

    //public static PlayerControllerVersion2 Instance;

    [Header("Death Sounds List")]
    public List<string> deathSoundNames = new List<string>();
    [Header("Damage Sounds List")]
    public List<string> damageSoundNames = new List<string>();

    // Stamina integration
    [Header("Stamina Integration")]
    public PlayerStamina stamina;              // assign in Inspector (your Player GameObject)
    public float attackStaminaCost = 15f;      // stamina cost for each attack
    public float rollStaminaCost = 25f;        // stamina cost for rolling
    public AudioClip outOfStaminaSfx;          // optional SFX when not enough stamina


    // === Unity Methods ===
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        //upperBodyCollider = GetComponent<BoxCollider2D>();
        playerHealth = GetComponent<PlayerHealth>();

        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        //attackPoint = transform.Find("AttackPoint").GetComponent<Transform>();

        // Ignore collisions between Player and Enemies
        Physics2D.IgnoreLayerCollision(3, 6);
        Physics2D.IgnoreLayerCollision(3, 3);

        originalMovementSpeed = movementSpeed;
        originalRollingSpeed = rollingSpeed;
        InputSystemManager.Instance?.AssignPlayer(this);

        // Apply permanent bonuses from pickups stored in PlayerData
        var pd = GameManager.Instance != null ? GameManager.Instance.playerData : null;
        if (pd != null)
        {
            if (pd.bonusJumpCount > 0)
                maxDoubleJumpCount += pd.bonusJumpCount;
            if (pd.bonusMovementSpeed > 0f)
            {
                movementSpeed += pd.bonusMovementSpeed;
                originalMovementSpeed = movementSpeed;
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentXVelocityState == XVelocityState.Normal ||
            currentXVelocityState == XVelocityState.Slow ||
            currentXVelocityState == XVelocityState.Stop)
        {
            UpdateMovement();
        }

    }

    void Update()
    {

        // === Handle Player Detection Triggers === //
        UpdateDetectionTriggers();

        // Flip Player Sprite based on the direction of movement
        FlipPlayerSprite();
        // === Handle Animation States === //
        UpdateAnimationStates();
        // === Handle Cooldown Timers === //
        UpdateCooldownTimers();
        
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

    public void ResetState()
    {
        // Interrupt or STOP the currentState Coroutine
        if (currentStateCoroutine != null)
            StopCoroutine(currentStateCoroutine);

        DisplayLog("Reset to Neutral State");
        // Replace the currentState to our newState
        currentState = PlayerState.Neutral;

        playerAnimator.SetTrigger("Revive");


    }

    // Play feedback when no stamina
    private void PlayOutOfStaminaFeedback()
    {
        DisplayLog("Not enough stamina.");
        if (outOfStaminaSfx != null)
        {
            AudioManager.Instance.PlaySFX(outOfStaminaSfx.name);
        }
    }

    // == Method/Function to change a player state
    private void SwitchPlayerState(PlayerState newState, GameObject switcher)
    {
        //if (currentState == PlayerState.Jumping && newState == PlayerState.Jumping) {
        //    return;
        //}
        

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
        else if (currentState == PlayerState.Dead || (currentState == PlayerState.Hurting && newState != PlayerState.ForceInterupt))
        {
            DisplayLog("Player is currently " + currentState + " cannot change this state!");
            return;
        }
        // Rolling State can't be interrupted
        else if (currentXVelocityState == XVelocityState.Rolling
            && newState != PlayerState.ForceInterupt
            && newState != PlayerState.WallSliding
            && newState != PlayerState.Dead)
        {
            DisplayLog(newState + " Cannot interupt " + currentXVelocityState + "!");
            return;
        }
        // Attackings State can't be interrupted
        else if (currentState == PlayerState.Attacking
            && (newState == PlayerState.Attacking||newState == PlayerState.Jumping ))
        {
            DisplayLog(newState + " Cannot interupt " + currentState + "!");
            return;
        }
        // Wall Sliding State can't be interrupted by Attacking or Shielding
        else if (currentState == PlayerState.WallSliding
            && (newState == PlayerState.Attacking || newState == PlayerState.Shielding))
        {
            DisplayLog(newState + " Cannot interupt " + currentState + "!");
            return;
        }

        // Prevent entering Attacking if no stamina available
        else if (newState == PlayerState.Attacking
            && stamina != null && !stamina.TryConsume(attackStaminaCost))
        {
                PlayOutOfStaminaFeedback();
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
                    //currentDoubleJumpCount++; // Increment the double jump count
                    currentStateCoroutine = StartCoroutine(DoJumping());
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
                    SwitchPlayerState(PlayerState.Neutral,gameObject);
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

        // Require stamina before entering Rolling state
        if (newXVelocityState == XVelocityState.Rolling)
        {
            if (stamina != null && !stamina.TryConsume(rollStaminaCost))
            {
                PlayOutOfStaminaFeedback();
                return;
            }
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
        if (isGrounded || 
                (  !isGrounded 
                    && currentDoubleJumpCount <= maxDoubleJumpCount 
                    && !isWallDetected
                )
           )
        {
            playerAnimator.SetTrigger("Jump");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            yield return new WaitForSeconds(0.2f);
        }
        else if (!isGrounded && isWallDetected)
        {
            // If not grounded and there is a Wall, then Wall Jump instead.
            SwitchPlayerState(PlayerState.WallJumping, gameObject);
        }

        // Wait until we start falling
        while (rb.linearVelocity.y > 0)
        {
            yield return null;
        }
        SwitchPlayerState(PlayerState.Neutral, gameObject);
    }
    IEnumerator DoWallSliding()
    {
        // No Horizontal movement
        SwitchXVelocityState(XVelocityState.Overriden);
        rb.linearVelocity = new Vector2(0, wallSlidingSpeed);
        // Wait until not grounded or there is no wall detected
        while (!isGrounded && isWallDetected)
        {
            yield return new WaitForSeconds(0.3f);
        }
        //playerAnimator.SetBool("WallSlide", false);
        SwitchPlayerState(PlayerState.Neutral, gameObject);

    }
    IEnumerator DoWallJumping()
    {
        SwitchXVelocityState(XVelocityState.Overriden);
        // Jumps to opposite direction
        playerAnimator.SetTrigger("Jump");
        rb.linearVelocity = new Vector2(wallJumpForceX * -facingDirection, wallJumpForceY);
        yield return new WaitForSeconds(0.2f);
        // Wait until we start falling
        while (rb.linearVelocity.y > 0)
        {
            yield return null;
        }
        SwitchPlayerState(PlayerState.Neutral, gameObject);

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

        SwitchPlayerState(PlayerState.ForceInterupt, gameObject);
        upperBodyCollider.enabled = false;
        playerAnimator.SetTrigger("Roll");
        playerHealth.isInvincible = true; // Set player invincible during rolling
        rb.linearVelocity = new Vector2(facingDirection * rollingSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(rollDuration);

        // Continue Rolling While there are still obstacles in Players Head
        while (isUpperWallDetected)
        {
            playerAnimator.SetTrigger("Roll");
            yield return new WaitForSeconds(rollDuration);
        }

        // Go back to Normal State and Apply a Cooldown
        playerHealth.isInvincible = false;
        upperBodyCollider.enabled = true;
        lastRollingTimestamp = rollingCooldown; // Set the cooldown for rolling
        SwitchXVelocityState(XVelocityState.Normal);
        SwitchPlayerState(PlayerState.Neutral, gameObject);
    }
    IEnumerator DoContinuousAttack()
    {
        // PlayerState.Attacking
        // is a state that can be interrupted by other states,
        // so we can use this to attack continuously
        SwitchXVelocityState(XVelocityState.Slow);
        DoAttacking();
        yield return new WaitForSeconds(attackIntervalTime);
        SwitchPlayerState(PlayerState.Neutral, gameObject);

    }

    void PerformAttack() // will be called in Animation Event
    {
        // Enable attack box temporarily
        attackBoxCollider.enabled = true;

        // Optionally disable it after short delay
        Invoke(nameof(DisableAttackBox), 0.1f); // adjust timing
    }

    void DisableAttackBox()
    {
        attackBoxCollider.enabled = false;
    }

    private void DoAttacking()
    {
        // Stamina check moved to SwitchPlayerState to prevent entering Attacking when not enough stamina.
        // Slow your movement speed during attack
        SwitchXVelocityState(XVelocityState.Slow);

        //Variable for Current Attack Animation
        currentAttackAnimation++;
        // Call one of three attack animations "Attack1", "Attack2", "Attack3"
        playerAnimator.SetTrigger("Attack" + currentAttackAnimation);
        // Play Sounds
        AudioManager.Instance.PlaySFX("Attack2");

        // Reset Animation Count
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
        playerAnimator.SetBool("IsDead", true);

        AudioManager.Instance.PlaySFX(deathSoundNames[Random.Range(0, deathSoundNames.Count)]);

        this.enabled = false;
    }

    IEnumerator DoHurting()
    {
        SwitchXVelocityState(XVelocityState.Stop);
        playerAnimator.SetTrigger("Hurt");
        CameraShake.Instance.Shake();
        AudioManager.Instance.PlaySFX(damageSoundNames[Random.Range(0, damageSoundNames.Count)]);

        yield return new WaitForSeconds(hurtSeconds);

        SwitchPlayerState(PlayerState.ForceInterupt, gameObject);
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
    public void OnNeutral() => SwitchPlayerState(PlayerState.Neutral, gameObject);   // PointerExit, PointerUp of Any Control Buttons
    public void OnJump()
    {

        if (currentDoubleJumpCount < maxDoubleJumpCount && currentXVelocityState != XVelocityState.Rolling)
        {

            SwitchPlayerState(PlayerState.Jumping, gameObject);
            if (currentDoubleJumpCount < maxDoubleJumpCount)
            {
                currentDoubleJumpCount++; // Increment the double jump count
            }
            else
            {
                DisplayLog("Max Double Jump Count reached!");
            }
        }
        else if (currentState == PlayerState.WallSliding && isWallDetected)
        {
            // If player is wall sliding, then wall jump instead
            SwitchPlayerState(PlayerState.WallJumping, gameObject);
        }
        else
        {
            DisplayLog("Cannot Jump, either not grounded or max double jump count reached!");
        }
    }        
    public void OnHoldAttack() { 
        if (currentState == PlayerState.Attacking)
        {
            DisplayLog("Already Attacking, cannot attack again!");
            return; // If already attacking, then do nothing
        }
        SwitchPlayerState(PlayerState.Attacking, gameObject); 
    }    
    public void OnRoll() => SwitchXVelocityState(XVelocityState.Rolling);  // PointerDown, PointerEnter
    public void OnHoldShield() => SwitchPlayerState(PlayerState.Shielding, gameObject);     // PointerDown, PointerEnter
    public void OnHurt() => SwitchPlayerState(PlayerState.Hurting, gameObject);
    public void OnDead() => SwitchPlayerState(PlayerState.Dead, gameObject);

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
        playerAnimator.SetFloat("AirSpeedY", rb.linearVelocity.y);
        playerAnimator.SetBool("WallSlide", currentState == PlayerState.WallSliding);
        playerAnimator.SetBool("IdleBlock", currentState == PlayerState.Shielding);
        playerAnimator.SetBool("stillRolling", currentXVelocityState == XVelocityState.Rolling);
        
        //bool isDead = GetComponent<PlayerHealth>().IsDead();
        //playerAnimator.SetBool("IsDead", isDead);

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

            // Reset Double Jump Count if Player is grounded
            if (isGrounded && currentState != PlayerState.Jumping) { 
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
                if (inputX == 1 && facingDirection == 1)
                {
                    SwitchPlayerState(PlayerState.WallSliding, gameObject);
                }
                else if (inputX == -1 && facingDirection == -1)
                {
                    SwitchPlayerState(PlayerState.WallSliding, gameObject);
                } 
                else
                {
                    // If player is not moving, then stop the wall sliding
                    //playerAnimator.SetTrigger("Jump");
                    SwitchPlayerState(PlayerState.Neutral, gameObject);

                }
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
        if (currentState == PlayerState.Attacking || currentState == PlayerState.Shielding || currentState == PlayerState.Hurting)
        {
            // Do not flip player sprite from the following conditions above
            return;
        }
        if (!isGrounded)
        {
            if (rb.linearVelocity.x > 0) { GetComponent<SpriteRenderer>().flipX = false; facingDirection = 1; }
            else if (rb.linearVelocity.x < 0) { GetComponent<SpriteRenderer>().flipX = true; facingDirection = -1; }
        }
        else
        {
            if (inputX > 0) { GetComponent<SpriteRenderer>().flipX = false; facingDirection = 1; }
            else if (inputX < 0) { GetComponent<SpriteRenderer>().flipX = true; facingDirection = -1; }
        }
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

    public bool IsFacingRight()
    {
        return facingDirection == 1;
    }

    private void UpdateMovement()
    {
        // Determine if the player is grounded
        bool isGrounded = m_groundSensor.State();

        // Calculate the control factor based on whether the player is grounded or in the air
        float controlFactor = isGrounded ? groundControlFactor : airControlFactor;

        // Calculate the target velocity based on input
        float targetVelocityX = inputX * movementSpeed;

        // Gradually adjust the player's velocity to the target velocity
        float newVelocityX = Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, directionChangeSpeed * controlFactor * Time.deltaTime);

        // Apply the new velocity
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }


}
