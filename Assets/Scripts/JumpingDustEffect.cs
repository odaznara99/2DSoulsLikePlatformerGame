using UnityEngine;

public class JumpingDustEffect : MonoBehaviour
{
    [Header("Dust Prefabs")]
    [SerializeField] private GameObject jumpDustPrefab;
    [SerializeField] private GameObject landDustPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform dustSpawnPoint;
    [Tooltip("Destroy dust particles after this many seconds.")]
    [SerializeField] private float dustLifetime = 0.5f;

    private PlayerControllerVersion2 playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerControllerVersion2>();
    }

    /// <summary>
    /// Call this from an Animation Event on the jump animation frame
    /// to spawn a dust puff at the player's feet on takeoff.
    /// </summary>
    public void AE_JumpDust()
    {
        SpawnDust(jumpDustPrefab);
    }

    /// <summary>
    /// Call this from an Animation Event on the landing animation frame,
    /// or invoke manually when the player touches the ground.
    /// </summary>
    public void AE_LandDust()
    {
        SpawnDust(landDustPrefab);
    }

    private void SpawnDust(GameObject prefab)
    {
        if (prefab == null || playerController == null)
            return;

        Vector3 spawnPos = dustSpawnPoint != null ? dustSpawnPoint.position : transform.position;

        int facing = playerController.IsFacingRight() ? 1 : -1;
        GameObject dust = Instantiate(prefab, spawnPos, Quaternion.identity);
        dust.transform.localScale = new Vector3(facing, 1, 1);

        Destroy(dust, dustLifetime);
    }
}