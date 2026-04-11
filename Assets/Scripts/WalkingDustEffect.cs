using UnityEngine;

public class WalkingDustEffect : MonoBehaviour
{
    [Header("Dust Settings")]
    [SerializeField] private GameObject walkDustPrefab;
    [SerializeField] private Transform dustSpawnPoint;
    [Tooltip("Destroy dust particles after this many seconds.")]
    [SerializeField] private float dustLifetime = 0.5f;

    private PlayerControllerVersion2 playerController;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        playerController = GetComponent<PlayerControllerVersion2>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Call this from an Animation Event on walk/run frames
    /// to spawn a dust puff at the player's feet.
    /// </summary>
    public void AE_WalkDust()
    {
        // Only spawn dust when grounded and actually moving
        if (playerController == null || !playerController.isGrounded)
            return;

        if (Mathf.Abs(playerController.GetFloatInputX()) < Mathf.Epsilon)
            return;

        if (walkDustPrefab == null)
            return;

        // Use the spawn point if assigned, otherwise fall back to the player position
        Vector3 spawnPos = dustSpawnPoint != null ? dustSpawnPoint.position : transform.position;

        // Flip dust direction based on player facing
        int facing = playerController.IsFacingRight() ? 1 : -1;
        GameObject dust = Instantiate(walkDustPrefab, spawnPos, Quaternion.identity);
        dust.transform.localScale = new Vector3(facing, 1, 1);

        Destroy(dust, dustLifetime);
    }
}