using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverAnimate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public TextMeshProUGUI buttonText; // Reference to the button text
    public float hoverScale = 1.2f;    // Scale factor when hovered
    private Vector3 originalScale;     // Original scale of the text

    void Start()
    {
        if (!buttonText)
        {
            Debug.LogError("ButtonHoverAnimate: buttonText is not assigned in the inspector.");
            return;
        }

        // Store the original scale of the text
        originalScale = buttonText.transform.localScale;
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
