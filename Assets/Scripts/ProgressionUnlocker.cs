using UnityEngine;

public class ProgressionUnlocker : MonoBehaviour
{
    public float newMaxX = 50f; // extended camera limit

    private void Start()
    {
        EnemyManager.Instance.OnAllEnemiesCleared += UnlockNextArea;
    }

    void UnlockNextArea()
    {
        Debug.Log("Extending camera max X!");
        SmoothCameraFollow cam = FindObjectOfType<SmoothCameraFollow>();
        if (cam != null)
        {
            cam.maxX = newMaxX;
        }

        // OR: activate door, play sound, open path, etc.
    }

    private void OnDestroy()
    {
        // Unsubscribe for safety
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.OnAllEnemiesCleared -= UnlockNextArea;
    }
}
