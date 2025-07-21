using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionRestorer : MonoBehaviour
{
    public Vector3 lastCheckpointPosition; // Store the last checkpoint position
    [SerializeField] Transform spawnPointStart;
    [SerializeField] Transform spawnPointEnd;
    private void Start()
    {

        TeleportToStartSpawn();
        lastCheckpointPosition = GameObject.Find("StartSpawnPoint").transform.position;
    }

    public void TeleportTo(Vector3 spawnPointVector3)
    {
        transform.position = spawnPointVector3;
    }

    public void TeleportToStartSpawn() {
        //spawnPointStart = GameObject.Find("StartPoint").transform;
        // If not returning, set a default spawn point (e.g., origin)
        TeleportTo(new Vector3(-7,-2,0));
    }

    public void TeleportToEndSpawn()
    {
        spawnPointEnd = GameObject.FindGameObjectWithTag("EndPoint").transform;
        // If not returning, set a default spawn point (e.g., origin)
        TeleportTo(spawnPointEnd);
    }

    public void TeleportToCheckpoint()
    {
        TeleportTo(lastCheckpointPosition);
    }

    public void TeleportTo(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point is null. Using default position (0,0,0).");
            transform.position = Vector3.zero;
            return;
        }

        transform.position = spawnPoint.position;
    }

}
