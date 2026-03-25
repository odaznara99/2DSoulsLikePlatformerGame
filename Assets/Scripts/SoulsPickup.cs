using UnityEngine;

/// <summary>
/// Souls pickup — the main currency of the game.
/// Attach to a GameObject with a trigger Collider2D.
/// Souls accumulate in PlayerData.souls and are reset to zero on death;
/// the dropped amount is left at the death location as a DroppedSoulsPickup.
/// </summary>
public class SoulsPickup : MonoBehaviour
{
    [Header("Souls Settings")]
    [Tooltip("Number of souls granted on pickup.")]
    public int soulsValue = 10;

    [Header("Feedback")]
    public GameObject floatingTextPrefab;
    [Tooltip("SFX clip name registered in AudioManager. Leave empty to skip.")]
    public string pickupSfxName = "";
    [Tooltip("Text shown in the floating message on pickup.")]
    public string pickupMessage = "Souls";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.AddSouls(soulsValue);

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

        ft.GetComponent<FloatingText>()?.SetText($"+{soulsValue} {pickupMessage}");
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
