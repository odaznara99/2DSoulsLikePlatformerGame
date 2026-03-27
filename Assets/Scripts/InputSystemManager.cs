using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralised Input Manager using Unity's Input System package (InputAction workflow).
/// Actions are serialized so their bindings can be edited in the Unity Inspector.
/// Keyboard/gamepad input is handled via InputAction callbacks (performed / canceled).
/// Mobile UI Buttons (UIButtonsManager) route through the same public methods so
/// all player input flows through a single point.
/// </summary>
public class InputSystemManager : MonoBehaviour
{
    public static InputSystemManager Instance { get; private set; }

    // ── Input Actions ─────────────────────────────────────────────────────────
    // Each action is serialized so bindings can be adjusted in the Inspector.
    // Default keyboard/mouse bindings are added in Awake() when no bindings
    // have been configured yet (e.g. on first play or fresh prefab).

    [Header("Input Actions")]
    [Tooltip("1D-axis action: negative = left (A / ←), positive = right (D / →)")]
    [SerializeField] private InputAction moveAction   = new InputAction("Move",   InputActionType.Value);
    [SerializeField] private InputAction jumpAction   = new InputAction("Jump",   InputActionType.Button);
    [SerializeField] private InputAction attackAction = new InputAction("Attack", InputActionType.Button);
    [SerializeField] private InputAction shieldAction = new InputAction("Shield", InputActionType.Button);
    [SerializeField] private InputAction rollAction   = new InputAction("Roll",   InputActionType.Button);

    /// <summary>The player controller currently controlled by this manager.</summary>
    public PlayerControllerVersion2 PlayerController { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        AddDefaultBindingsIfEmpty();
    }

    private void Start()
    {
        if (PlayerController == null)
            PlayerController = FindAnyObjectByType<PlayerControllerVersion2>();
    }

    private void OnEnable()
    {
        // Enable all actions
        moveAction.Enable();
        jumpAction.Enable();
        attackAction.Enable();
        shieldAction.Enable();
        rollAction.Enable();

        // Subscribe button actions to callbacks
        jumpAction.performed   += HandleJump;
        attackAction.performed += HandleAttack;
        shieldAction.performed += HandleShieldPerformed;
        shieldAction.canceled  += HandleShieldCanceled;
        rollAction.performed   += HandleRoll;
    }

    private void OnDisable()
    {
        // Unsubscribe before disabling
        jumpAction.performed   -= HandleJump;
        attackAction.performed -= HandleAttack;
        shieldAction.performed -= HandleShieldPerformed;
        shieldAction.canceled  -= HandleShieldCanceled;
        rollAction.performed   -= HandleRoll;

        moveAction.Disable();
        jumpAction.Disable();
        attackAction.Disable();
        shieldAction.Disable();
        rollAction.Disable();
    }

    private void Update()
    {
        // Read continuous horizontal movement and push it to the player every frame.
        PlayerController?.SetFloatInputX(Mathf.Clamp(moveAction.ReadValue<float>(), -1f, 1f));
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers the active player. Called from
    /// <see cref="PlayerControllerVersion2.Start"/> so the manager always
    /// has a reference to the current player.
    /// </summary>
    public void AssignPlayer(PlayerControllerVersion2 controller)
    {
        PlayerController = controller;
    }

    // ── UI Button Methods (called by UIButtonsManager for mobile touch) ────────
    // These mirror the InputAction callbacks so keyboard and mobile UI buttons
    // share identical behaviour.

    public void OnMoveRight()  => PlayerController?.OnMoveRight();   // PointerDown / PointerEnter
    public void OnMoveLeft()   => PlayerController?.OnMoveLeft();    // PointerDown / PointerEnter
    public void OnStop()       => PlayerController?.OnStop();        // PointerUp   / PointerExit  (move buttons)
    public void OnNeutral()    => PlayerController?.OnNeutral();     // PointerUp   / PointerExit  (any button)
    public void OnJump()       => PlayerController?.OnJump();        // PointerDown
    public void OnHoldAttack() => PlayerController?.OnHoldAttack();  // PointerDown
    public void OnRoll()       => PlayerController?.OnRoll();        // PointerDown
    public void OnHoldShield() => PlayerController?.OnHoldShield();  // PointerDown / PointerEnter

    // ── InputAction Callbacks ─────────────────────────────────────────────────

    private void HandleJump(InputAction.CallbackContext ctx)            => OnJump();
    private void HandleAttack(InputAction.CallbackContext ctx)          => OnHoldAttack();
    private void HandleShieldPerformed(InputAction.CallbackContext ctx) => OnHoldShield();
    private void HandleShieldCanceled(InputAction.CallbackContext ctx)  => OnNeutral();
    private void HandleRoll(InputAction.CallbackContext ctx)            => OnRoll();

    // ── Default Bindings ──────────────────────────────────────────────────────

    /// <summary>
    /// Adds default keyboard/mouse bindings when an action has no bindings yet.
    /// This preserves any bindings already configured in the Inspector.
    /// </summary>
    private void AddDefaultBindingsIfEmpty()
    {
        // Move – 1D Axis composites: one for WASD, one for arrow keys
        if (moveAction.bindings.Count == 0)
        {
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");
        }

        // Jump – Space
        if (jumpAction.bindings.Count == 0)
            jumpAction.AddBinding("<Keyboard>/space");

        // Attack – J key or Left Mouse Button
        if (attackAction.bindings.Count == 0)
        {
            attackAction.AddBinding("<Keyboard>/j");
            attackAction.AddBinding("<Mouse>/leftButton");
        }

        // Shield – K key or Right Mouse Button (hold = shield, release = neutral)
        if (shieldAction.bindings.Count == 0)
        {
            shieldAction.AddBinding("<Keyboard>/k");
            shieldAction.AddBinding("<Mouse>/rightButton");
        }

        // Roll – Left Shift or L key
        if (rollAction.bindings.Count == 0)
        {
            rollAction.AddBinding("<Keyboard>/leftShift");
            rollAction.AddBinding("<Keyboard>/l");
        }
    }
}
