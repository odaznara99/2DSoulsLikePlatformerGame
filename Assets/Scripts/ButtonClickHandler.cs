using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour
{
    [Tooltip("Reference to the UI Button used to navigate to the target location.")]
    public Button locationButton;

    private void Start()
    {
        // Add a listener to the button's onClick event
        locationButton.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        Debug.Log("Button was clicked!");
        // Add your logic here
    }
}