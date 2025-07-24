using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Slider healthSlider;
    public GameObject uiContainer; // The root GameObject to toggle visibility
    public BossFollowRangeTrigger followRangeTrigger;

    public void SetMaxHealth(float max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = max;
        uiContainer.SetActive(true);
    }

    public void SetHealth(float current)
    {
        healthSlider.value = current;

        if (current <= 0)
        {
            uiContainer.SetActive(false);
        }
    }

    public void SetHealthUIActive(bool isActive)
    {
        uiContainer.SetActive(isActive);
    }

    void Update()
    {
        // Check if the player is in the boss's follow range
        if (followRangeTrigger != null && followRangeTrigger.IsPlayerInArea 
            && uiContainer.activeSelf == false)
        {
            SetHealthUIActive(true);
        }
        else if (followRangeTrigger != null && !followRangeTrigger.IsPlayerInArea
                 && uiContainer.activeSelf == true)
        {
            SetHealthUIActive(false);
        }
    }
}
