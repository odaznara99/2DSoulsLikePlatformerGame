using UnityEngine;

public enum SpawnPointType
{
    Start,
    End,
    Checkpoint
}

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 checkpoint;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterStart(Transform t) => startPoint = t.position;
    public void RegisterEnd(Transform t) => endPoint = t.position;

    public void SetCheckpoint(Vector3 pos)
    {
        checkpoint = pos;
    }

    public Vector3 GetSpawnPoint(SpawnPointType type)
    {
        return type switch
        {
            SpawnPointType.Start => startPoint,
            SpawnPointType.End => endPoint,
            SpawnPointType.Checkpoint => checkpoint,
            _ => Vector3.zero
        };
    }

    public void RespawnPlayer(SpawnPointType type)
    {
        var player = FindObjectOfType<PlayerControllerVersion2>();

        if (player == null) return;

        player.transform.position = GetSpawnPoint(type);

        // Optional reset
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.isInvincible = false;

        UIButtonsManager.Instance.AssignPlayer(player);
    }
}