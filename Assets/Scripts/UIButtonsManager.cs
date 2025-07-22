using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class UIButtonsManager : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerControllerVersion2 playerController() {
        return GameObject.Find("HeroKnight").GetComponent<PlayerControllerVersion2>();
    }

    [SerializeField]private PlayerControllerVersion2 m_playerController = null;
    public GameObject buttonLeft;
    public GameObject buttonRight;
    public GameObject buttonAttack;
    public GameObject buttonBlock;
    public GameObject buttonRoll;
    public GameObject buttonJump;

    // Update is called once per frame  
    void Update()
    {
        if (!m_playerController || m_playerController.Equals(null) || m_playerController == null)
        {
            //m_playerController = playerController();
            ResetEventTrigger();
        }

    }

    private void Start()
    {
        //playerController() = FindFirstObjectByType<PlayerControllerVersion2>();
        //m_playerController = playerController();
        ResetEventTrigger();
    }
    public void ResetEventTrigger()
    {
        m_playerController = playerController();
        // Auto-assign event triggers for the buttons
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
