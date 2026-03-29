using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonHoverAnimate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public TextMeshProUGUI buttonText; // Reference to the button text
    public GameObject button; // Reference to the button component
    public float hoverScale = 1.2f;    // Scale factor when hovered
    private Vector3 originalScale;     // Original scale of the text
    private Vector3 originalScaleButton; // Original scale of the button

    void Start()
    {
        if (!buttonText)
        {
            //Debug.Log("[ButtonHoverAnimate] (Optional) buttonText is not assigned in the inspector.");
            //return;
        }
        else 
        {
            // Store the original scale of the text
            originalScale = buttonText.transform.localScale;
        }

        button = this.gameObject;
        if (!button)
        {
            Debug.LogWarning("[ButtonHoverAnimate] button is not assigned in the inspector.");
            //return;
        }
        else 
        {
            // Store the original scale of the gameObject
            originalScaleButton = button.transform.localScale;
        }

        
        
    }

    // Called when the mouse pointer enters the button
    private bool isHovered = false;

    void Update()
    {
        if (buttonText)
        {
            Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
            buttonText.transform.localScale = Vector3.Lerp(buttonText.transform.localScale, targetScale, Time.deltaTime * 10f);
        }

        if (button)
        {
            Vector3 targetScale = isHovered ? originalScaleButton * hoverScale : originalScaleButton;
            button.transform.localScale = Vector3.Lerp(button.transform.localScale, targetScale, Time.deltaTime * 10f);
        }


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

}
