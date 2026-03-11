using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionRestorer : MonoBehaviour
{
    public Vector3 lastCheckpointPosition; // Store the last checkpoint position
    [SerializeField] Transform spawnPointStart;
    [SerializeField] Transform spawnPointEnd;
    private void Start()
    {
        try
        {
            spawnPointStart = GameObject.FindGameObjectWithTag("StartPoint").transform;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("StartPoint not found: " + ex.Message);
        }
        try
        {
            spawnPointEnd = GameObject.FindGameObjectWithTag("EndPoint").transform;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("EndPoint not found: " + ex.Message);
        }

        //Invoke(nameof(GameManager.instance.SaveCheckPoint), 1f); // Delay to ensure the StartPoint is found
    }

    public void TeleportTo(Vector3 spawnPointVector3)
    {
        transform.position = spawnPointVector3;
        Camera.main.transform.position = spawnPointVector3 + new Vector3(0,0,-10);
    }

    public void TeleportToStartSpawn() {
        spawnPointStart = GameObject.FindGameObjectWithTag("StartPoint").transform;
        TeleportTo(spawnPointStart);
        GameManager.instance.SaveCheckPoint();
    }

    public void TeleportToEndSpawn()
    {
        spawnPointEnd = GameObject.FindGameObjectWithTag("EndPoint").transform;
        TeleportTo(spawnPointEnd);
        GameManager.instance.SaveCheckPoint();
    }

    public void TeleportToCheckpoint()
    {
        TeleportTo(GameManager.instance.lastCheckPointSave);
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
