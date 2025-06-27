using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;           // Player to follow
    public Vector3 offset;             // Offset from the target
    public float smoothTime = 0.3f;    // Time for camera to catch up

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // Keep the camera's Z position fixed (2D view)
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
