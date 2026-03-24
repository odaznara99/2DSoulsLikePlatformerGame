using UnityEngine;

/// <summary>
/// Coins pickup — the temporary currency of the game.
/// Attach to a GameObject with a trigger Collider2D.
/// Coins accumulate in PlayerData.coins and are lost on death
/// (along with any items purchased with coins).
/// </summary>
public class CoinsPickup : MonoBehaviour
{
    [Header("Coins Settings")]
    [Tooltip("Number of coins granted on pickup.")]
    public int coinsValue = 5;

    [Header("Feedback")]
    public GameObject floatingTextPrefab;
    [Tooltip("SFX clip name registered in AudioManager. Leave empty to skip.")]
    public string pickupSfxName = "";
    [Tooltip("Text shown in the floating message on pickup.")]
    public string pickupMessage = "Coins";

    private Transform worldCanvas;

    private void Start()
    {
        if (worldCanvas == null)
        {
            worldCanvas = GameObject.FindGameObjectWithTag("WorldSpaceCanvas").GetComponent<Transform>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.playerData.coins += coinsValue;

        ShowFloatingText(other.transform);
        PlaySFX();
        Destroy(gameObject);
    }

    private void ShowFloatingText(Transform target)
    {
       if (floatingTextPrefab == null || worldCanvas == null) return;

       GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);

       ft.GetComponent<FloatingText>()?.SetText($"+{coinsValue} {pickupMessage}");
    }

    private void PlaySFX()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(pickupSfxName))
            AudioManager.Instance.PlaySFX(pickupSfxName);
    }

    private Transform GetWorldCanvas()
    {
        var go = GameObject.FindGameObjectWithTag("WorldSpaceCanvas");
        return go != null ? go.transform : null;
    }
}
