using UnityEngine;

public class ProgressionUnlocker : MonoBehaviour
{
    [Tooltip("The new maximum X boundary to extend the camera when the area is unlocked.")]
    public float newMaxX = 50f;

    private void Start()
    {
        EnemyManager.Instance.OnAllEnemiesCleared += UnlockNextArea;
    }

    void UnlockNextArea()
    {
        Debug.Log("Extending camera max X!");
        MessageManager.Instance.ShowMessage("Bandits was Defeated!");
        SmoothCameraFollow cam = FindAnyObjectByType<SmoothCameraFollow>();
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
