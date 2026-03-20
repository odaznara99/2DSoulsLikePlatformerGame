using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIButtonsManager : MonoBehaviour
{
    [Header("Player (assign in Inspector or via AssignPlayer at runtime)")]
    [SerializeField] private PlayerControllerVersion2 m_playerController = null;

    [Header("UI Buttons")]
    public GameObject buttonLeft;
    public GameObject buttonRight;
    public GameObject buttonAttack;
    public GameObject buttonBlock;
    public GameObject buttonRoll;
    public GameObject buttonJump;

    public static UIButtonsManager Instance;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
        // Try to auto-assign once if not set in inspector
        if (m_playerController == null)
            m_playerController = FindObjectOfType<PlayerControllerVersion2>();

        if (m_playerController == null)
        {
            Debug.LogWarning("UIButtonsManager: PlayerControllerVersion2 not assigned and not found in scene. Call AssignPlayer(...) when player is available.");
            return;
        }

        ResetEventTrigger();
    }

    // Public API for other systems to inject the player reference (preferred over Find at runtime)
    public void AssignPlayer(PlayerControllerVersion2 player)
    {
        if (player == null) { Debug.Log("You assigned a null player"); return; }
        m_playerController = player;
        Debug.Log("UIButtonsManager: Player assigned via AssignPlayer. Wiring event triggers.");
        ResetEventTrigger();
    }

    // Recreate triggers and wire to the current m_playerController
    public void ResetEventTrigger()
    {
        if (m_playerController == null)
        {
            Debug.LogWarning("UIButtonsManager.ResetEventTrigger: m_playerController is null. Skipping wiring.");
            return;
        }

        // Auto-assign event triggers for the buttons (safe: checks for null buttons)
        AddEventTrigger(buttonLeft, EventTriggerType.PointerEnter, m_playerController.OnMoveLeft);
        AddEventTrigger(buttonLeft, EventTriggerType.PointerExit, m_playerController.OnStop);
        AddEventTrigger(buttonLeft, EventTriggerType.PointerUp, m_playerController.OnStop);
        AddEventTrigger(buttonLeft, EventTriggerType.PointerDown, m_playerController.OnMoveLeft);

        AddEventTrigger(buttonRight, EventTriggerType.PointerEnter, m_playerController.OnMoveRight);
        AddEventTrigger(buttonRight, EventTriggerType.PointerExit, m_playerController.OnStop);
        AddEventTrigger(buttonRight, EventTriggerType.PointerUp, m_playerController.OnStop);
        AddEventTrigger(buttonRight, EventTriggerType.PointerDown, m_playerController.OnMoveRight);

        AddEventTrigger(buttonAttack, EventTriggerType.PointerDown, m_playerController.OnHoldAttack);

        AddEventTrigger(buttonBlock, EventTriggerType.PointerDown, m_playerController.OnHoldShield);
        AddEventTrigger(buttonBlock, EventTriggerType.PointerUp, m_playerController.OnNeutral);
        AddEventTrigger(buttonBlock, EventTriggerType.PointerExit, m_playerController.OnNeutral);

        AddEventTrigger(buttonRoll, EventTriggerType.PointerDown, m_playerController.OnRoll);

        AddEventTrigger(buttonJump, EventTriggerType.PointerDown, m_playerController.OnJump);
    }

    private void AddEventTrigger(GameObject button, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null) return;

        // Ensure the button has an EventTrigger component
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.AddComponent<EventTrigger>();
        }
        else
        {
            // Clear existing entries to avoid duplicating listeners when ResetEventTrigger is called
            trigger.triggers.Clear();
        }

        // Create a new entry for the specified event type
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };

        // Add the callback action to the entry
        entry.callback.AddListener((eventData) => action());

        // Add the entry to the EventTrigger
        trigger.triggers.Add(entry);
    }
}
