using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public Camera zoneCamera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateZoneCamera();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DeactivateZoneCamera();
        }
    }

    void ActivateZoneCamera()
    {
        // Disable all other cameras
        foreach (Camera cam in Camera.allCameras)
        {
            cam.enabled = false;
        }

        zoneCamera.enabled = true;
    }

    void DeactivateZoneCamera()
    {
        zoneCamera.enabled = false;

        // Reactivate main camera (assumes it's tagged properly)
        Camera mainCam = Camera.main;
        if (mainCam != null)
            mainCam.enabled = true;
    }
}
