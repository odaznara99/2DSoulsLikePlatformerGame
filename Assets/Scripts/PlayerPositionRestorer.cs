using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionRestorer : MonoBehaviour
{
    public Vector3 lastCheckpointPosition; // Store the last checkpoint position
    [SerializeField] Transform spawnPointStart;
    [SerializeField] Transform spawnPointEnd;
    private void Start()
    {
        spawnPointStart = GameObject.FindGameObjectWithTag("StartPoint").transform;
        spawnPointEnd   = GameObject.FindGameObjectWithTag("EndPoint").transform;

        //TeleportToStartSpawn();
        //lastCheckpointPosition = GameObject.Find("StartSpawnPoint").transform.position;
        lastCheckpointPosition = transform.position;
    }

    public void TeleportTo(Vector3 spawnPointVector3)
    {
        transform.position = spawnPointVector3;
        Camera.main.transform.position = spawnPointVector3 + new Vector3(0,0,-10);
    }

    public void TeleportToStartSpawn() {
        spawnPointStart = GameObject.FindGameObjectWithTag("StartPoint").transform;
        TeleportTo(spawnPointStart);
    }

    public void TeleportToEndSpawn()
    {
        spawnPointEnd = GameObject.FindGameObjectWithTag("EndPoint").transform;
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
