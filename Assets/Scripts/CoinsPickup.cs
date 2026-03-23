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
        if (floatingTextPrefab == null) return;

        Transform canvas = GetWorldCanvas();
        Vector3 pos = target.position + Vector3.up;

        GameObject ft = canvas != null
            ? Instantiate(floatingTextPrefab, pos, Quaternion.identity, canvas)
            : Instantiate(floatingTextPrefab, pos, Quaternion.identity);

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
