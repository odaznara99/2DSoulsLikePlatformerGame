using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableButton : MonoBehaviour
{
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Disable()
    {
        if (button != null)
        {
            button.interactable = false; // Disable the button when this script is disabled
        }
    }

}
