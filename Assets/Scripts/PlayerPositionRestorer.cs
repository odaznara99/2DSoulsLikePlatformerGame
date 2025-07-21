using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionRestorer : MonoBehaviour
{
    private void Start()
    {

        TeleportToStartSpawn();
    }

    public void TeleportTo(Vector3 spawnPointVector3)
    {
        transform.position = spawnPointVector3;
    }

    public void TeleportToStartSpawn() {
        Transform spawnPointStart = GameObject.Find("StartSpawnPoint").transform;
        // If not returning, set a default spawn point (e.g., origin)
        TeleportTo(spawnPointStart);
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
