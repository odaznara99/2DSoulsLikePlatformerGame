using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public Rigidbody2D targetRigidbody;  // Assign the player's Rigidbody2D
    public PlayerControllerVersion2 targetHeroKnight; // Reference to the PlayerController script for additional checks
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public float fallLookAhead = -2f;  // How much to look down when falling
    public float lookAheadVelocity = -0.5f; // Factor to adjust the look-ahead
    public float minX; // Set this to your desired leftmost camera boundary

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (target == null || targetRigidbody == null || targetHeroKnight.currentState==PlayerState.Dead) return;

        Vector3 dynamicOffset = offset;

        // If falling, shift the camera downward slightly
        if (targetRigidbody.velocity.y < lookAheadVelocity & !targetHeroKnight.isGrounded)
        {
            dynamicOffset.y += fallLookAhead; // Looks down
        }

        Vector3 desiredPosition = target.position + dynamicOffset;
        desiredPosition.x = Mathf.Max(desiredPosition.x, minX); // Clamp left side
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // Only update x and y to keep the Z constant (2D camera)
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
