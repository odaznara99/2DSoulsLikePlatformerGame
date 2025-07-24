using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour
{
    public Button locationButton; // Reference to the UI Button

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