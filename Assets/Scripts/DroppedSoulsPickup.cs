using UnityEngine;

/// <summary>
/// Dropped Souls pickup — automatically spawned by GameManager at the player's
/// death position, carrying the souls the player had when they died.
/// Rules:
///   • Picking it up restores those souls to the player.
///   • If the player dies a second time before picking it up, this pickup is
///     destroyed and the souls are permanently lost.
/// Attach to a GameObject with a trigger Collider2D.
/// </summary>
public class DroppedSoulsPickup : MonoBehaviour
{
    [Tooltip("Amount of souls contained in this pickup. Set automatically by GameManager.")]
    public int soulsAmount = 0;

    [Header("Feedback")]
    public GameObject floatingTextPrefab;
    [Tooltip("SFX clip name registered in AudioManager. Leave empty to skip.")]
    public string pickupSfxName = "";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.playerData.souls += soulsAmount;
        gm.ClearDroppedSoulsPickup();

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

        ft.GetComponent<FloatingText>()?.SetText($"+{soulsAmount} Souls Recovered");
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
