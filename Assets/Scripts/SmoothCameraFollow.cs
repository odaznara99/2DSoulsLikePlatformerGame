using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("References")]
    public Transform playerTarget;
    public Transform bossTarget;
    private Rigidbody2D playerRigidbody;  // Assign the player's Rigidbody2D
    private PlayerControllerVersion2 playerController; // Reference to the PlayerController script for additional checks

    public bool followBothTargets = false; // Toggle this during boss fight

    [Header("Camera Settings")]
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public float fallLookAhead = -2f;  // How much to look down when falling
    public float lookAheadVelocity = -0.5f; // Factor to adjust the look-ahead
    public float minX; // Set this to your desired leftmost camera boundary
    public float maxX; // Right bound

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        if (!playerTarget)
        {
            Debug.LogWarning("Player target for Camera Follow not assigned in inspector. Might cause some issues. Finding now...");
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }

        playerRigidbody = playerTarget.GetComponent<Rigidbody2D>();
        playerController = playerTarget.GetComponent<PlayerControllerVersion2>();

        if (!playerTarget)
        {
            Debug.LogError("Player target not found! Ensure the player GameObject has the 'Player' tag.");
        }

        transform.position = playerTarget.position + offset; // Initial camera position

    }

    private void Update()
    {
        if (!playerTarget)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
            playerRigidbody = playerTarget.GetComponent<Rigidbody2D>();
            playerController = playerTarget.GetComponent<PlayerControllerVersion2>();
        }
    }

    /*void FixedUpdate()
    {
        if (playerTarget == null || playerRigidbody == null || playerController.currentState==PlayerState.Dead) return;

        Vector3 dynamicOffset = offset;

        // If falling, shift the camera downward slightly
        if (playerRigidbody.velocity.y < lookAheadVelocity & !playerController.isGrounded)
        {
            dynamicOffset.y += fallLookAhead; // Looks down
        }

        Vector3 desiredPosition = playerTarget.position + dynamicOffset;
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX); // Clamp X axis
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // Only update x and y to keep the Z constant (2D camera)
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }*/
    void FixedUpdate()
    {
        if (playerTarget == null || playerRigidbody == null || playerController.currentState == PlayerState.Dead) return;

        Vector3 dynamicOffset = offset;

        if (!followBothTargets && playerRigidbody.velocity.y < lookAheadVelocity && !playerController.isGrounded)
        {
            dynamicOffset.y += fallLookAhead;
        }

        Vector3 targetPosition;

        if (followBothTargets && bossTarget != null)
        {
            // 📌 Center between Player and Boss
            Vector3 midpoint = (playerTarget.position + bossTarget.position) / 2f;
            targetPosition = midpoint + dynamicOffset;
        }
        else
        {
            // 👤 Follow just the player
            targetPosition = playerTarget.position + dynamicOffset;
        }

        // Clamp X axis
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

        // Smoothly move
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }

    public void SetMaxCameraBounds(float minX, float maxX)
    {
        this.minX = minX;
        this.maxX = maxX;
    }

    public void SetMaxCameraBounds(float maxX)
    {
        this.maxX = maxX;
    }

    // Call this when entering or exiting boss area:
    public void SetFollowBothTargets(bool enabled)
    {
        followBothTargets = enabled;
    }
}
