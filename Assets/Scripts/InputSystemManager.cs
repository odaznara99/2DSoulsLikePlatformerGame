using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralised Input System Manager.
/// Reads physical keyboard/mouse via Unity's New Input System and routes the
/// calls to <see cref="PlayerControllerVersion2"/>.
/// Mobile UI Buttons (UIButtonsManager) also route through this manager so
/// that all player input flows through a single point.
/// </summary>
public class InputSystemManager : MonoBehaviour
{
    public static InputSystemManager Instance { get; private set; }

    [Header("Keyboard / Mouse Input")]
    [Tooltip("Enable keyboard and mouse input. Automatically disabled on Android and iOS.")]
    public bool enableKeyboardInput = true;

    /// <summary>The player controller currently managed by this Input System Manager.</summary>
    public PlayerControllerVersion2 PlayerController { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (PlayerController == null)
            PlayerController = FindAnyObjectByType<PlayerControllerVersion2>();

#if UNITY_ANDROID || UNITY_IOS
        enableKeyboardInput = false;
#endif
    }

    /// <summary>
    /// Assigns the player controller. Called automatically from
    /// <see cref="PlayerControllerVersion2.Start"/> so that the manager
    /// always references the active player.
    /// </summary>
    public void AssignPlayer(PlayerControllerVersion2 controller)
    {
        PlayerController = controller;
    }

    private void Update()
    {
        if (!enableKeyboardInput || PlayerController == null) return;
        ProcessKeyboardInput();
    }

    private void ProcessKeyboardInput()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null) return;

        // Continuous horizontal input (replacement for Input.GetAxis("Horizontal"))
        float horizontal = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        PlayerController.SetFloatInputX(Mathf.Clamp(horizontal, -1f, 1f));

        // Edge-triggered movement callbacks
        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            OnMoveLeft();
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            OnMoveRight();

        // Jump
        if (keyboard.spaceKey.wasPressedThisFrame)
            OnJump();

        // Inputs that cannot happen at the same time (same priority/order as before)
        bool attackPressed  = (mouse != null && mouse.leftButton.wasPressedThisFrame)  || keyboard.jKey.wasPressedThisFrame;
        bool shieldHeld     = (mouse != null && mouse.rightButton.isPressed)            || keyboard.kKey.isPressed;
        bool shieldReleased = (mouse != null && mouse.rightButton.wasReleasedThisFrame) || keyboard.kKey.wasReleasedThisFrame;
        bool rollPressed    = keyboard.leftShiftKey.wasPressedThisFrame                 || keyboard.lKey.wasPressedThisFrame;

        if (attackPressed)       OnHoldAttack();
        else if (shieldHeld)     OnHoldShield();
        else if (shieldReleased) OnNeutral();
        else if (rollPressed)    OnRoll();
    }

    // ==== UI Button Input Methods ====
    // Called by UIButtonsManager for mobile UI buttons so that all input
    // (keyboard and touch) is routed through a single manager.

    public void OnMoveRight()  => PlayerController?.OnMoveRight();   // PointerDown, PointerEnter
    public void OnMoveLeft()   => PlayerController?.OnMoveLeft();    // PointerDown, PointerEnter
    public void OnStop()       => PlayerController?.OnStop();        // PointerUp, PointerExit of move buttons
    public void OnNeutral()    => PlayerController?.OnNeutral();     // PointerExit, PointerUp of any control button
    public void OnJump()       => PlayerController?.OnJump();        // PointerDown
    public void OnHoldAttack() => PlayerController?.OnHoldAttack();  // PointerDown
    public void OnRoll()       => PlayerController?.OnRoll();        // PointerDown
    public void OnHoldShield() => PlayerController?.OnHoldShield();  // PointerDown, PointerEnter
}
