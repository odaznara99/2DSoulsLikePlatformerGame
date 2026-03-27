using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIButtonsManager : MonoBehaviour
{
    public GameObject buttonLeft;
    public GameObject buttonRight;
    public GameObject buttonAttack;
    public GameObject buttonBlock;
    public GameObject buttonRoll;
    public GameObject buttonJump;

    // Update is called once per frame  
    void Update()
    {
    }

    private void Start()
    {
        ResetEventTrigger();
    }

    public void ResetEventTrigger()
    {
        // Route all UI button events through InputSystemManager so that both
        // keyboard/mouse input and mobile touch input flow through a single manager.
        InputSystemManager inputManager = InputSystemManager.Instance != null
            ? InputSystemManager.Instance
            : FindAnyObjectByType<InputSystemManager>();

        if (inputManager != null)
        {
            AddEventTrigger(buttonLeft, EventTriggerType.PointerEnter, inputManager.OnMoveLeft);
            AddEventTrigger(buttonLeft, EventTriggerType.PointerExit,  inputManager.OnStop);
            AddEventTrigger(buttonLeft, EventTriggerType.PointerUp,    inputManager.OnStop);
            AddEventTrigger(buttonLeft, EventTriggerType.PointerDown,  inputManager.OnMoveLeft);

            AddEventTrigger(buttonRight, EventTriggerType.PointerEnter, inputManager.OnMoveRight);
            AddEventTrigger(buttonRight, EventTriggerType.PointerExit,  inputManager.OnStop);
            AddEventTrigger(buttonRight, EventTriggerType.PointerUp,    inputManager.OnStop);
            AddEventTrigger(buttonRight, EventTriggerType.PointerDown,  inputManager.OnMoveRight);

            AddEventTrigger(buttonAttack, EventTriggerType.PointerDown, inputManager.OnHoldAttack);

            AddEventTrigger(buttonBlock, EventTriggerType.PointerDown, inputManager.OnHoldShield);
            AddEventTrigger(buttonBlock, EventTriggerType.PointerUp,   inputManager.OnNeutral);
            AddEventTrigger(buttonBlock, EventTriggerType.PointerExit, inputManager.OnNeutral);

            AddEventTrigger(buttonRoll, EventTriggerType.PointerDown, inputManager.OnRoll);

            AddEventTrigger(buttonJump, EventTriggerType.PointerDown, inputManager.OnJump);
        }
        else
        {
            // Fallback: wire buttons directly to the player controller when no InputSystemManager is present.
            PlayerControllerVersion2 playerController = FindAnyObjectByType<PlayerControllerVersion2>();
            if (playerController == null) return;

            AddEventTrigger(buttonLeft, EventTriggerType.PointerEnter, playerController.OnMoveLeft);
            AddEventTrigger(buttonLeft, EventTriggerType.PointerExit,  playerController.OnStop);
            AddEventTrigger(buttonLeft, EventTriggerType.PointerUp,    playerController.OnStop);
            AddEventTrigger(buttonLeft, EventTriggerType.PointerDown,  playerController.OnMoveLeft);

            AddEventTrigger(buttonRight, EventTriggerType.PointerEnter, playerController.OnMoveRight);
            AddEventTrigger(buttonRight, EventTriggerType.PointerExit,  playerController.OnStop);
            AddEventTrigger(buttonRight, EventTriggerType.PointerUp,    playerController.OnStop);
            AddEventTrigger(buttonRight, EventTriggerType.PointerDown,  playerController.OnMoveRight);

            AddEventTrigger(buttonAttack, EventTriggerType.PointerDown, playerController.OnHoldAttack);

            AddEventTrigger(buttonBlock, EventTriggerType.PointerDown, playerController.OnHoldShield);
            AddEventTrigger(buttonBlock, EventTriggerType.PointerUp,   playerController.OnNeutral);
            AddEventTrigger(buttonBlock, EventTriggerType.PointerExit, playerController.OnNeutral);

            AddEventTrigger(buttonRoll, EventTriggerType.PointerDown, playerController.OnRoll);

            AddEventTrigger(buttonJump, EventTriggerType.PointerDown, playerController.OnJump);
        }
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
            eventID = eventType
        };

        // Add the callback action to the entry  
        entry.callback.AddListener((eventData) => action());

        // Add the entry to the EventTrigger  
        trigger.triggers.Add(entry);
    }
}