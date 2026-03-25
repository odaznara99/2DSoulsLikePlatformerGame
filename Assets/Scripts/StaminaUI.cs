using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public PlayerStamina stamina;
    public Image fillImage; // assign UI Image (fill type)
    public UnityEngine.UI.Text valueText; // optional

    void Start()
    {
        if (stamina == null)
            stamina = FindAnyObjectByType<PlayerStamina>();
    }

    private void OnEnable()
    {
        if (stamina != null)
        {
            stamina.onStaminaChanged.AddListener(OnStaminaChanged);
            // initialize
            OnStaminaChanged(stamina != null ? stamina.GetComponent<PlayerStamina>().CurrentStamina : 0f, stamina != null ? stamina.GetMaxStamina() : 1f);
        }
    }

    private void OnDisable()
    {
        if (stamina != null)
            stamina.onStaminaChanged.RemoveListener(OnStaminaChanged);
    }

    private void OnStaminaChanged(float current, float max)
    {
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(current / max);
        if (valueText != null) valueText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
    }
}