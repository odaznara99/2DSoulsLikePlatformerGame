using TMPro;
using UnityEngine;

/// <summary>
/// Listens to GameManager currency events and keeps the HUD text elements
/// in sync.  Attach to a UI GameObject in the scene canvas and assign the
/// Text references in the Inspector.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Currency UI")]
    [Tooltip("Text element that displays the current souls count.")]
    public TextMeshProUGUI soulsText;

    [Tooltip("Text element that displays the current coins count.")]
    public TextMeshProUGUI coinsText;

    [SerializeField]
    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // Fallback: if OnEnable ran before GameManager.Instance was ready,
        // Start is guaranteed to run after all Awake calls have completed.
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.OnSoulsChanged += UpdateSoulsText;
        GameManager.Instance.OnCoinsChanged += UpdateCoinsText;
        isSubscribed = true;

        // Immediately sync with current values.
        GameManager.Instance.BroadcastCurrencyUpdate();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSoulsChanged -= UpdateSoulsText;
            GameManager.Instance.OnCoinsChanged -= UpdateCoinsText;
        }

        isSubscribed = false;
    }

    private void UpdateSoulsText(int souls)
    {
        if (soulsText != null)
            soulsText.text = souls.ToString();
    }

    private void UpdateCoinsText(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }
}