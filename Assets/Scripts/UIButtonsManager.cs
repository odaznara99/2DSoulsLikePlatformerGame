using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class UIButtonsManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private PlayerControllerVersion2 playerController;
    public GameObject buttonLeft;
    public GameObject buttonRight;
    public GameObject buttonAttack;
    public GameObject buttonBlock;
    public GameObject buttonRoll;
    public GameObject buttonJump;

    // Update is called once per frame  
    void Update()
    {
        if (playerController == null || playerController.Equals(null))
        {
            playerController = FindFirstObjectByType<PlayerControllerVersion2>();
            
            if (playerController != null)
            {
                SetEventTrigger();
            }
        }
    }

    private void Awake()
    {
        //playerController = FindFirstObjectByType<PlayerControllerVersion2>();
        SetEventTrigger();
    }

    private void Start()
    {
        //playerController = FindFirstObjectByType<PlayerControllerVersion2>();
        SetEventTrigger();
    }

    private void SetEventTrigger()
    {
        if (playerController == null || playerController.Equals(null))
        {
            Debug.LogWarning("PlayerController not found or inactive. Attempting to find it again.");
            playerController = FindFirstObjectByType<PlayerControllerVersion2>();
        }

        // Auto-assign event triggers for the buttons
        AddEventTrigger(buttonLeft, EventTriggerType.PointerEnter, playerController.OnMoveLeft);
        AddEventTrigger(buttonLeft, EventTriggerType.PointerExit, playerController.OnStop);
        AddEventTrigger(buttonLeft, EventTriggerType.PointerUp, playerController.OnMoveLeft);
        AddEventTrigger(buttonLeft, EventTriggerType.PointerDown, playerController.OnStop);

        AddEventTrigger(buttonRight, EventTriggerType.PointerEnter, playerController.OnMoveRight);
        AddEventTrigger(buttonRight, EventTriggerType.PointerExit, playerController.OnStop);
        AddEventTrigger(buttonRight, EventTriggerType.PointerUp, playerController.OnMoveRight);
        AddEventTrigger(buttonRight, EventTriggerType.PointerDown, playerController.OnStop);


        AddEventTrigger(buttonAttack, EventTriggerType.PointerDown, playerController.OnHoldAttack);
        AddEventTrigger(buttonBlock, EventTriggerType.PointerDown, playerController.OnHoldShield);
        AddEventTrigger(buttonBlock, EventTriggerType.PointerUp, playerController.OnNeutral);
        AddEventTrigger(buttonBlock, EventTriggerType.PointerExit, playerController.OnNeutral);
        AddEventTrigger(buttonRoll, EventTriggerType.PointerDown, playerController.OnRoll);
        AddEventTrigger(buttonJump, EventTriggerType.PointerDown, playerController.OnJump);
    }

    private void AddEventTrigger(GameObject button, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
    {
        // Ensure the button has an EventTrigger component  
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.AddComponent<EventTrigger>();
        }

        // Create a new entry for the specified event type  
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType // Corrected property name from 'eventType' to 'eventID'  
        };

        // Add the callback action to the entry  
        entry.callback.AddListener((eventData) => action());

        // Add the entry to the EventTrigger  
        trigger.triggers.Add(entry);
    }
}
