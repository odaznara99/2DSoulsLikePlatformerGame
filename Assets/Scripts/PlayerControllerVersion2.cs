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
    // == Nested Serializable Classes ==

    /// <summary>Editor-only toggle settings exposed in the Inspector.</summary>
    [System.Serializable]
    public class EditorSettings
    {
        public bool enabledDebugLog = true;
        [Tooltip("Enable Keyboard Input for Player Controller")]
        public bool enabledKeyboardInput = false;
    }

    /// <summary>Horizontal movement speed parameters.</summary>
    [System.Serializable]
    public class MovementSettings
    {
        public float inputX;
        public float movementSpeed = 4.0f;
        public float slowMovementSpeed = 1.5f;
        public float rollingSpeed = 5.0f;
    }

    /// <summary>Jump force and double-jump configuration.</summary>
    [System.Serializable]
    public class JumpSettings
    {
        [Tooltip("Force of the jump, Y velocity of player when jumping")]
        public float jumpForce = 6.0f;
        [Tooltip("Maximum number of jumps player can perform (Double Jump)")]
        public int maxDoubleJumpCount = 1;
        public int currentDoubleJumpCount = 0;
    }

    /// <summary>Wall-slide speed and wall-jump force configuration.</summary>
    [System.Serializable]
    public class WallJumpSettings
    {
        [Tooltip("Y velocity of player during wall sliding. Should be negative")]
        public float wallSlidingSpeed = -0.3f;
        [Tooltip("X velocity of player when wall jumping")]
        public float wallJumpForceX = 5.0f;
        [Tooltip("Y velocity of player when wall jumping")]
        public float wallJumpForceY = 6.0f;
    }

    /// <summary>Air and ground control factors that shape movement feel.</summary>
    [System.Serializable]
    public class MomentumSettings
    {
        [Tooltip("How much control the player has in the air")]
        public float airControlFactor = 0.5f;
        [Tooltip("How much control the player has on the ground")]
        public float groundControlFactor = 1.0f;
        [Tooltip("How quickly the player can change direction")]
        public float directionChangeSpeed = 5f;
    }

    /// <summary>Attack box references and attack timing parameters.</summary>
    [System.Serializable]
    public class AttackSettings
    {
        [Tooltip("The object with BoxCollider2D")]
        public GameObject attackBoxObject;
        public Collider2D attackBoxCollider;
        public float attackIntervalTime = 0.5f;
        [Tooltip("Knock back to enemies")]
        public float playerKnockbackForce = 15f;
    }

    /// <summary>Runtime sensor flags that reflect what the player is currently touching.</summary>
    [System.Serializable]
    public class DetectionFlags
    {
        [Tooltip("Sensor to detect if player is on a Ground (Tag)")]
        public bool isGrounded = false;
        [Tooltip("Set to true for split seconds, when player is shielding")]
        public bool isParry = false;
        [Tooltip("Sensor to detect if there is a wall in front of the player")]
        public bool isWallDetected = false;
        [Tooltip("Sensor to detect if there is a wall on player's head.")]
        public bool isUpperWallDetected = false;
    }

    /// <summary>Duration values controlling how long certain states last.</summary>
    [System.Serializable]
    public class DurationSettings
    {
        [Tooltip("Duration to parry an attack")]
        public float parryingTime = 0.3f;
        [Tooltip("Duration in Rolling state.")]
        public float rollDuration = 0.5f;
        [Tooltip("Duration in Hurting State")]
        public float hurtSeconds = 0.3f;
    }

    /// <summary>Cooldown durations and elapsed timestamps for roll and shield.</summary>
    [System.Serializable]
    public class CooldownSettings
    {
        [Tooltip("Cooldown for rolling")]
        public float rollingCooldown = 3.0f;
        [Tooltip("Cooldown for shielding")]
        public float shieldingCooldown = 2.0f;
        [Tooltip("Last time player rolled")]
        public float lastRollingTimestamp = 0.0f;
        [Tooltip("Last time player shielded")]
        public float lastShieldingTimestamp = 0.0f;
    }

    /// <summary>Stamina component reference and stamina cost values.</summary>
    [System.Serializable]
    public class StaminaSettings
    {
        [Tooltip("Assign in Inspector (your Player GameObject)")]
        public PlayerStamina stamina;
        [Tooltip("Stamina cost for each attack")]
        public float attackStaminaCost = 15f;
        [Tooltip("Stamina cost for rolling")]
        public float rollStaminaCost = 25f;
        [Tooltip("Optional SFX when not enough stamina")]
        public AudioClip outOfStaminaSfx;
    }

    // == Variable for Unity Editor
    [Header("Unity Editor Settings")]
    public EditorSettings editorSettings = new EditorSettings();

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
    private float originalMovementSpeed = 4.0f;
    private float originalRollingSpeed = 5.0f;

    [Header("Player States")]

    // == Variables for Player State Tracking
    private Coroutine currentStateCoroutine;
    private Coroutine currentDelayingCoroutine; // A slight delay when transitioning to a newState
    public PlayerState currentState;
    // === Variables for X Velocity === // these variables can override x velocity
    public XVelocityState currentXVelocityState;
    private Coroutine currentXVelocityStateCoroutine;

    [Header("Movement Parameters")]
    public MovementSettings movement = new MovementSettings();

    [Header("Jump Settings")]
    public JumpSettings jump = new JumpSettings();

    [Header("Wall Jump Settings")]
    public WallJumpSettings wallJump = new WallJumpSettings();

    [Header("Momentum Settings")]
    public MomentumSettings momentum = new MomentumSettings();

    [Header("Attack Settings")]
    public AttackSettings attack = new AttackSettings();
    [Tooltip("Damage dealt by the attack")] public float attackDamage = 20f;
    [SerializeField] private GameObject attackBoxObject; // The object with BoxCollider2D
    [SerializeField] private Collider2D attackBoxCollider;
    public float attackIntervalTime = 0.5f;
    [Tooltip("Knock back to enemies")]public float playerKnockbackForce = 15f; // Knock back to enemies

    [Header("Detection Flags")]
    public DetectionFlags detection = new DetectionFlags();

    [Header("Duration Variables")]
    public DurationSettings duration = new DurationSettings();

    [Header("Cooldown Variables")]
    public CooldownSettings cooldown = new CooldownSettings();

    //public static PlayerControllerVersion2 Instance;

    [Header("Death Sounds List")]
    public List<string> deathSoundNames = new List<string>();
    [Header("Damage Sounds List")]
    public List<string> damageSoundNames = new List<string>();

    [Header("Stamina Integration")]
    public StaminaSettings staminaSettings = new StaminaSettings();

    [Header("UI Prefabs")]
    [Tooltip("Prefab for damage popup, assign in Inspector")]
    public GameObject uiButtons;

    // === Unity Methods ===

    /// <summary>Initialises component references, sensors, physics layer ignores, and applies any persisted PlayerData bonuses.</summary>
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

        originalMovementSpeed = movement.movementSpeed;
        originalRollingSpeed = movement.rollingSpeed;
        //UIButtonsManager.Instance.AssignPlayer(this);

        // Apply permanent bonuses from pickups stored in PlayerData
        var pd = GameManager.Instance != null ? GameManager.Instance.playerData : null;
        if (pd != null)
        {
            if (pd.bonusJumpCount > 0)
                jump.maxDoubleJumpCount += pd.bonusJumpCount;
            if (pd.bonusMovementSpeed > 0f)
            {
                movement.movementSpeed += pd.bonusMovementSpeed;
                originalMovementSpeed = movement.movementSpeed;
            }
        }

#if UNITY_EDITOR
        editorSettings.enabledKeyboardInput = true;
#endif
    }

    /// <summary>Physics update — applies horizontal movement when the velocity state permits.</summary>
    private void FixedUpdate()
    {
        if (currentXVelocityState == XVelocityState.Normal ||
            currentXVelocityState == XVelocityState.Slow ||
            currentXVelocityState == XVelocityState.Stop)
        {
            UpdateMovement();
        }

    }

    /// <summary>Per-frame update for detection, sprite flipping, animation, cooldowns, and keyboard input.</summary>
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

        // === Player Inputs on KeyBoard ==== //
        UpdateKeyboardInputs();
        
    }

    /// <summary>Logs a message to the console only when debug logging is enabled.</summary>
    private void DisplayLog(string messageLog)
    {
        if (editorSettings.enabledDebugLog)
        {
            Debug.Log(messageLog);
        }
    }

    /// <summary>Sets the horizontal input axis value, ignored while dead or hurt.</summary>
    public void SetFloatInputX(float newInputX)
    {
        if (currentState != PlayerState.Dead && currentState != PlayerState.Hurting)
        {
            movement.inputX = newInputX;
        }
    }

    /// <summary>Directly overrides the current movement speed.</summary>
    public void SetFloatMovementSpeed(float newMoveSpeed)
    {
        movement.movementSpeed = newMoveSpeed;
    }

    /// <summary>Stops any running state coroutine and forces the player back to the Neutral state, triggering the Revive animation.</summary>
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

    /// <summary>Plays the out-of-stamina debug log and optional audio feedback.</summary>
    private void PlayOutOfStaminaFeedback()
    {
        DisplayLog("Not enough stamina.");
        if (staminaSettings.outOfStaminaSfx != null)
        {
            AudioManager.Instance.PlaySFX(staminaSettings.outOfStaminaSfx.name);
        }
    }

    /// <summary>
    /// Attempts to transition the player into <paramref name="newState"/>, enforcing all interrupt rules,
    /// cooldown checks, and stamina costs before starting the corresponding state coroutine.
    /// </summary>
    private void SwitchPlayerState(PlayerState newState, GameObject switcher)
    {
        //if (currentState == PlayerState.Jumping && newState == PlayerState.Jumping) {
        //    return;
        //}
        

        // Set Cooldown for Shielding when switching from Shielding state to new state
        if (currentState == PlayerState.Shielding 
            && newState != PlayerState.Shielding)
        {
            cooldown.lastShieldingTimestamp = cooldown.shieldingCooldown; // Set the cooldown for shielding
        }

        if (newState == PlayerState.Shielding && cooldown.lastShieldingTimestamp > 0) {
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
            && staminaSettings.stamina != null && !staminaSettings.stamina.TryConsume(staminaSettings.attackStaminaCost))
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

    /// <summary>
    /// Transitions the horizontal velocity state machine, enforcing the rolling cooldown and stamina check,
    /// stopping any in-progress X-velocity coroutine, then starting the appropriate coroutine or applying
    /// the speed immediately.
    /// </summary>
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
            if (staminaSettings.stamina != null && !staminaSettings.stamina.TryConsume(staminaSettings.rollStaminaCost))
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
                SetFloatMovementSpeed(movement.slowMovementSpeed);
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

    /// <summary>
    /// Coroutine that applies jump velocity when grounded or within double-jump budget,
    /// then waits until the player begins to fall before returning to Neutral.
    /// </summary>
    IEnumerator DoJumping()
    {
        if (detection.isGrounded || 
                (  !detection.isGrounded 
                    && jump.currentDoubleJumpCount <= jump.maxDoubleJumpCount 
                    && !detection.isWallDetected
                )
           )
        {
            playerAnimator.SetTrigger("Jump");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump.jumpForce);
            yield return new WaitForSeconds(0.2f);
        }
        else if (!detection.isGrounded && detection.isWallDetected)
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

    /// <summary>
    /// Coroutine that locks horizontal movement and applies the wall-slide velocity,
    /// continuing until the player lands or loses wall contact.
    /// </summary>
    IEnumerator DoWallSliding()
    {
        // No Horizontal movement
        SwitchXVelocityState(XVelocityState.Overriden);
        rb.linearVelocity = new Vector2(0, wallJump.wallSlidingSpeed);
        // Wait until not grounded or there is no wall detected
        while (!detection.isGrounded && detection.isWallDetected)
        {
            yield return new WaitForSeconds(0.3f);
        }
        //playerAnimator.SetBool("WallSlide", false);
        SwitchPlayerState(PlayerState.Neutral, gameObject);

    }

    /// <summary>
    /// Coroutine that launches the player off a wall in the opposite direction,
    /// then waits until vertical velocity turns negative before returning to Neutral.
    /// </summary>
    IEnumerator DoWallJumping()
    {
        SwitchXVelocityState(XVelocityState.Overriden);
        // Jumps to opposite direction
        playerAnimator.SetTrigger("Jump");
        rb.linearVelocity = new Vector2(wallJump.wallJumpForceX * -facingDirection, wallJump.wallJumpForceY);
        yield return new WaitForSeconds(0.2f);
        // Wait until we start falling
        while (rb.linearVelocity.y > 0)
        {
            yield return null;
        }
        SwitchPlayerState(PlayerState.Neutral, gameObject);

    }

    /// <summary>
    /// Coroutine that executes a roll: grants invincibility, disables the upper collider,
    /// applies roll velocity for <see cref="DurationSettings.rollDuration"/> seconds,
    /// loops while the player's head is still inside geometry, then restores normal state and applies the cooldown.
    /// </summary>
    IEnumerator DoRolling()
    {
        // XVelocityState.Rolling
        if (currentState == PlayerState.WallSliding)
        {
            SwitchXVelocityState(XVelocityState.Overriden);
            yield break;
        }
        if (cooldown.lastRollingTimestamp > 0)
        {
            DisplayLog("Rolling is on cooldown!");
            SwitchXVelocityState(XVelocityState.Normal);
            yield break; // If rolling is on cooldown, then stop the coroutine
        }

        SwitchPlayerState(PlayerState.ForceInterupt, gameObject);
        upperBodyCollider.enabled = false;
        playerAnimator.SetTrigger("Roll");
        playerHealth.isInvincible = true; // Set player invincible during rolling
        rb.linearVelocity = new Vector2(facingDirection * movement.rollingSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(duration.rollDuration);

        // Continue Rolling While there are still obstacles in Players Head
        while (detection.isUpperWallDetected)
        {
            playerAnimator.SetTrigger("Roll");
            yield return new WaitForSeconds(duration.rollDuration);
        }

        // Go back to Normal State and Apply a Cooldown
        playerHealth.isInvincible = false;
        upperBodyCollider.enabled = true;
        cooldown.lastRollingTimestamp = cooldown.rollingCooldown; // Set the cooldown for rolling
        SwitchXVelocityState(XVelocityState.Normal);
        SwitchPlayerState(PlayerState.Neutral, gameObject);
    }

    /// <summary>
    /// Coroutine that slows the player, triggers one attack cycle, then waits for the attack interval before returning to Neutral.
    /// </summary>
    IEnumerator DoContinuousAttack()
    {
        // PlayerState.Attacking
        // is a state that can be interrupted by other states,
        // so we can use this to attack continuously
        SwitchXVelocityState(XVelocityState.Slow);
        DoAttacking();
        yield return new WaitForSeconds(attack.attackIntervalTime);
        SwitchPlayerState(PlayerState.Neutral, gameObject);

    }

    /// <summary>Enables the attack collider box; called from an Animation Event.</summary>
    void PerformAttack() // will be called in Animation Event
    {
        // Enable attack box temporarily
        attack.attackBoxCollider.enabled = true;

        // Optionally disable it after short delay
        Invoke(nameof(DisableAttackBox), 0.1f); // adjust timing
    }

    /// <summary>Disables the attack collider box after the hit window expires.</summary>
    void DisableAttackBox()
    {
        attack.attackBoxCollider.enabled = false;
    }

    /// <summary>Advances the combo counter, triggers the matching attack animation, and plays the attack sound.</summary>
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

    /// <summary>Triggers the death animation, plays a random death sound, and disables this component.</summary>
    void DoDying() {
        Debug.Log("Player died!");
        playerAnimator.SetBool("noBlood", m_noBlood);
        playerAnimator.SetTrigger("Death");
        playerAnimator.SetBool("IsDead", true);

        AudioManager.Instance.PlaySFX(deathSoundNames[Random.Range(0, deathSoundNames.Count)]);

        this.enabled = false;
    }

    /// <summary>
    /// Coroutine that stops horizontal movement, triggers the hurt animation with camera shake and sound,
    /// waits for <see cref="DurationSettings.hurtSeconds"/>, then force-interrupts back to Neutral.
    /// </summary>
    IEnumerator DoHurting()
    {
        SwitchXVelocityState(XVelocityState.Stop);
        playerAnimator.SetTrigger("Hurt");
        CameraShake.Instance.Shake();
        AudioManager.Instance.PlaySFX(damageSoundNames[Random.Range(0, damageSoundNames.Count)]);

        yield return new WaitForSeconds(duration.hurtSeconds);

        SwitchPlayerState(PlayerState.ForceInterupt, gameObject);
    }

    /// <summary>
    /// Coroutine that holds the shielding state indefinitely (slowing movement and starting the parry window)
    /// until an external state change ends the coroutine.
    /// </summary>
    IEnumerator DoShielding()
    {
        if (cooldown.lastShieldingTimestamp > 0)
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

    /// <summary>
    /// Coroutine that sets <see cref="DetectionFlags.isParry"/> true for <see cref="DurationSettings.parryingTime"/> seconds,
    /// enabling parry logic in that window.
    /// </summary>
    IEnumerator DoParry()
    {
        detection.isParry = true;
        yield return new WaitForSeconds(duration.parryingTime);
        detection.isParry = false;
    }

    // ==== UI Button Controls/Triggers ==== 
    // ==== Add New Component - Event Trigger for your UI Buttons (Compatible for mobile devices)
    // ==== Add New Event Type ==== Check Comments what Event Type for each method.

    /// <summary>Sets horizontal input to +1 (move right). Bind to PointerDown / PointerEnter.</summary>
    public void OnMoveRight() => SetFloatInputX(1);              // PointerDown, PointerEnter

    /// <summary>Sets horizontal input to -1 (move left). Bind to PointerDown / PointerEnter.</summary>
    public void OnMoveLeft() => SetFloatInputX(-1);              // PointerDown, PointerEnter

    /// <summary>Zeroes horizontal input.</summary>
    public void OnStop() => SetFloatInputX(0);

    /// <summary>Returns the player to Neutral state. Bind to PointerExit / PointerUp of any control button.</summary>
    public void OnNeutral() => SwitchPlayerState(PlayerState.Neutral, gameObject);   // PointerExit, PointerUp of Any Control Buttons

    /// <summary>Requests a jump if the double-jump budget allows, or a wall jump when wall-sliding.</summary>
    public void OnJump()
    {

        if (jump.currentDoubleJumpCount < jump.maxDoubleJumpCount && currentXVelocityState != XVelocityState.Rolling)
        {

            SwitchPlayerState(PlayerState.Jumping, gameObject);
            if (jump.currentDoubleJumpCount < jump.maxDoubleJumpCount)
            {
                jump.currentDoubleJumpCount++; // Increment the double jump count
            }
            else
            {
                DisplayLog("Max Double Jump Count reached!");
            }
        }
        else if (currentState == PlayerState.WallSliding && detection.isWallDetected)
        {
            // If player is wall sliding, then wall jump instead
            SwitchPlayerState(PlayerState.WallJumping, gameObject);
        }
        else
        {
            DisplayLog("Cannot Jump, either not grounded or max double jump count reached!");
        }
    }

    /// <summary>Requests an attack state transition; ignored if already attacking.</summary>
    public void OnHoldAttack() { 
        if (currentState == PlayerState.Attacking)
        {
            DisplayLog("Already Attacking, cannot attack again!");
            return; // If already attacking, then do nothing
        }
        SwitchPlayerState(PlayerState.Attacking, gameObject); 
    }

    /// <summary>Triggers a roll via the X-velocity state machine. Bind to PointerDown / PointerEnter.</summary>
    public void OnRoll() => SwitchXVelocityState(XVelocityState.Rolling);  // PointerDown, PointerEnter

    /// <summary>Requests a shield/block state. Bind to PointerDown / PointerEnter.</summary>
    public void OnHoldShield() => SwitchPlayerState(PlayerState.Shielding, gameObject);     // PointerDown, PointerEnter

    /// <summary>Forces the player into the Hurting state (useful for testing).</summary>
    public void OnHurt() => SwitchPlayerState(PlayerState.Hurting, gameObject);

    /// <summary>Forces the player into the Dead state (useful for testing).</summary>
    public void OnDead() => SwitchPlayerState(PlayerState.Dead, gameObject);

    /// <summary>Counts down rolling and shielding cooldown timers each frame.</summary>
    void UpdateCooldownTimers()
    {
        // Handle Rolling Cooldown
        if (cooldown.lastRollingTimestamp > 0)
        {
            cooldown.lastRollingTimestamp -= Time.deltaTime;
            if (cooldown.lastRollingTimestamp <= 0)
            {
                cooldown.lastRollingTimestamp = 0;
                DisplayLog("Rolling Cooldown is over!");
            }
        }

        // Handle Shielding Cooldown
        if (cooldown.lastShieldingTimestamp > 0)
        {
            cooldown.lastShieldingTimestamp -= Time.deltaTime;
            if (cooldown.lastShieldingTimestamp <= 0)
            {
                cooldown.lastShieldingTimestamp = 0;
                DisplayLog("Shielding Cooldown is over!");
            }
        }

    }

    /// <summary>Pushes grounded, air-speed, wall-slide, block, roll, and run/idle animation parameters to the Animator each frame.</summary>
    void UpdateAnimationStates()
    {
        playerAnimator.SetBool("Grounded", detection.isGrounded);
        playerAnimator.SetFloat("AirSpeedY", rb.linearVelocity.y);
        playerAnimator.SetBool("WallSlide", currentState == PlayerState.WallSliding);
        playerAnimator.SetBool("IdleBlock", currentState == PlayerState.Shielding);
        playerAnimator.SetBool("stillRolling", currentXVelocityState == XVelocityState.Rolling);
        
        //bool isDead = GetComponent<PlayerHealth>().IsDead();
        //playerAnimator.SetBool("IsDead", isDead);

        if (Mathf.Abs(movement.inputX) > Mathf.Epsilon)
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

    /// <summary>
    /// Reads all sensor states, updates <see cref="detection"/> flags, resets the double-jump counter on landing,
    /// and auto-triggers wall-slide or neutral transitions based on sensor results.
    /// </summary>
    void UpdateDetectionTriggers() {
        // === If Player is Dead, then disable all sensors and return === //
        if (currentState != PlayerState.Dead)
        {
            // === Wall Detection ==== //
            detection.isWallDetected = (
                (m_wallSensorR1.State() && m_wallSensorR2.State() && facingDirection == 1)
            || (m_wallSensorL1.State() && m_wallSensorL2.State() && facingDirection == -1)
            );

            // === Ground Detection === //
            detection.isGrounded = m_groundSensor.State();

            // Reset Double Jump Count if Player is grounded
            if (detection.isGrounded && currentState != PlayerState.Jumping) { 
                jump.currentDoubleJumpCount = 0;
            }


            // === Upper Wall Detection === //
            detection.isUpperWallDetected = m_wallSensorL2.State() && m_wallSensorR2.State();

            // === States that is automatically being triggered by a sensor detection ==== //
            if (!detection.isGrounded && detection.isWallDetected
                && currentState != PlayerState.Hurting
                && currentState != PlayerState.Dead
                && currentState != PlayerState.WallJumping)
            {
                if (movement.inputX == 1 && facingDirection == 1)
                {
                    SwitchPlayerState(PlayerState.WallSliding, gameObject);
                }
                else if (movement.inputX == -1 && facingDirection == -1)
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
            detection.isGrounded = false;
            detection.isWallDetected = false;
            detection.isUpperWallDetected = false;
        }
    }

    /// <summary>Flips the sprite and updates <see cref="facingDirection"/> based on velocity (airborne) or input (grounded); skipped during attacking, shielding, and hurting.</summary>
    void FlipPlayerSprite()
    {
        if (currentState == PlayerState.Attacking || currentState == PlayerState.Shielding || currentState == PlayerState.Hurting)
        {
            // Do not flip player sprite from the following conditions above
            return;
        }
        if (!detection.isGrounded)
        {
            if (rb.linearVelocity.x > 0) { GetComponent<SpriteRenderer>().flipX = false; facingDirection = 1; }
            else if (rb.linearVelocity.x < 0) { GetComponent<SpriteRenderer>().flipX = true; facingDirection = -1; }
        }
        else
        {
            if (movement.inputX > 0) { GetComponent<SpriteRenderer>().flipX = false; facingDirection = 1; }
            else if (movement.inputX < 0) { GetComponent<SpriteRenderer>().flipX = true; facingDirection = -1; }
        }
    }

    /// <summary>Spawns a slide-dust particle at the appropriate wall sensor position; called from an Animation Event.</summary>
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

    /// <summary>Fires the Jump trigger on the Animator; can be called externally to synchronise visuals.</summary>
    public void TriggerJumpAnimation()
    {
        playerAnimator.SetTrigger("Jump");

    }

    /// <summary>Polls keyboard and mouse input (Editor-only) and maps them to the public action methods.</summary>
    void UpdateKeyboardInputs()
    {
#if UNITY_EDITOR
        if (!editorSettings.enabledKeyboardInput)
        {
            return;
        }

        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null)
        {
            return;
        }

        // Continuous horizontal input (replacement for Input.GetAxis("Horizontal"))
        float horizontal = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        movement.inputX = Mathf.Clamp(horizontal, -1f, 1f);

        // Optional edge-trigger movement callbacks (kept from your original logic)
        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            OnMoveLeft();
        }
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            OnMoveRight();
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            OnJump();
        }

        // Inputs that cannot happen at the same time (kept same priority/order)
        bool attackPressed = (mouse != null && mouse.leftButton.wasPressedThisFrame) || keyboard.jKey.wasPressedThisFrame;
        bool shieldHeld = (mouse != null && mouse.rightButton.isPressed) || keyboard.kKey.isPressed;
        bool shieldReleased = (mouse != null && mouse.rightButton.wasReleasedThisFrame) || keyboard.kKey.wasReleasedThisFrame;
        bool rollPressed = keyboard.leftShiftKey.wasPressedThisFrame || keyboard.lKey.wasPressedThisFrame;

        if (attackPressed)
        {
            OnHoldAttack();
        }
        else if (shieldHeld)
        {
            OnHoldShield();
        }
        else if (shieldReleased)
        {
            OnNeutral();
        }
        else if (rollPressed)
        {
            OnRoll();
        }
#endif
    }

    /// <summary>Returns <c>true</c> when the player sprite is facing right.</summary>
    public bool IsFacingRight()
    {
        return facingDirection == 1;
    }

    /// <summary>
    /// Applies smoothed horizontal movement via Lerp using the current input, movement speed,
    /// and a control factor that differs between grounded and airborne states.
    /// </summary>
    private void UpdateMovement()
    {
        // Determine if the player is grounded
        bool isGrounded = m_groundSensor.State();

        // Calculate the control factor based on whether the player is grounded or in the air
        float controlFactor = isGrounded ? momentum.groundControlFactor : momentum.airControlFactor;

        // Calculate the target velocity based on input
        float targetVelocityX = movement.inputX * movement.movementSpeed;

        // Gradually adjust the player's velocity to the target velocity
        float newVelocityX = Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, momentum.directionChangeSpeed * controlFactor * Time.deltaTime);

        // Apply the new velocity
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }


}
