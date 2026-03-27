using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [System.Serializable]
    public class BossFollowSettings
    {
        public Transform bossTarget;
        [Tooltip("Toggle to follow both player and boss during a boss fight.")]
        public bool followBothTargets = false;
    }

    [System.Serializable]
    public class CameraSettings
    {
        public float smoothTime = 0.3f;
        public Vector3 offset = new Vector3(0f, 1.5f, -10f);
        [Tooltip("How much to look down when the player is falling.")]
        public float fallLookAhead = -2f;
        [Tooltip("Velocity threshold factor that triggers the fall look-ahead.")]
        public float lookAheadVelocity = -0.5f;
        [Tooltip("Leftmost X boundary for the camera.")]
        public float minX;
        [Tooltip("Rightmost X boundary for the camera.")]
        public float maxX;
    }

    [Header("References")]
    public Transform playerTarget;

    [Header("Boss Follow")]
    public BossFollowSettings bossFollow = new BossFollowSettings();

    [Header("Camera Settings")]
    public CameraSettings cameraSettings = new CameraSettings();

    private Rigidbody2D playerRigidbody;
    private PlayerControllerVersion2 playerController;
    private Vector3 velocity = Vector3.zero;

    /// <summary>
    /// Resolves the player target if not set in the inspector, then caches component references
    /// and snaps the camera to the player's initial position.
    /// </summary>
    private void Start()
    {
        if (!playerTarget)
        {
            Debug.LogWarning("Player target for Camera Follow not assigned in inspector. Might cause some issues. Finding now...");
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }

        playerRigidbody  = playerTarget.GetComponent<Rigidbody2D>();
        playerController = playerTarget.GetComponent<PlayerControllerVersion2>();

        if (!playerTarget)
        {
            Debug.LogError("Player target not found! Ensure the player GameObject has the 'Player' tag.");
        }

        transform.position = playerTarget.position + cameraSettings.offset;
    }

    /// <summary>
    /// Re-resolves player component references if the player target is lost (e.g. after a scene reload).
    /// </summary>
    private void Update()
    {
        if (!playerTarget)
        {
            playerTarget     = GameObject.FindGameObjectWithTag("Player").transform;
            playerRigidbody  = playerTarget.GetComponent<Rigidbody2D>();
            playerController = playerTarget.GetComponent<PlayerControllerVersion2>();
        }
    }

    /// <summary>
    /// Smoothly moves the camera each physics step towards the desired target position,
    /// optionally centring between the player and boss during a boss encounter.
    /// </summary>
    void FixedUpdate()
    {
        if (playerTarget == null || playerRigidbody == null || playerController.currentState == PlayerState.Dead) return;

        Vector3 dynamicOffset = cameraSettings.offset;

        if (!bossFollow.followBothTargets
            && playerRigidbody.linearVelocity.y < cameraSettings.lookAheadVelocity
            && !playerController.detection.isGrounded)
        {
            dynamicOffset.y += cameraSettings.fallLookAhead;
        }

        Vector3 targetPosition;

        if (bossFollow.followBothTargets && bossFollow.bossTarget != null)
        {
            Vector3 midpoint = (playerTarget.position + bossFollow.bossTarget.position) / 2f;
            targetPosition = midpoint + dynamicOffset;
        }
        else
        {
            targetPosition = playerTarget.position + dynamicOffset;
        }

        targetPosition.x = Mathf.Clamp(targetPosition.x, cameraSettings.minX, cameraSettings.maxX);

        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, cameraSettings.smoothTime);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }

    /// <summary>
    /// Sets both the minimum and maximum X boundaries for the camera.
    /// </summary>
    /// <param name="minX">The leftmost allowed camera X position.</param>
    /// <param name="maxX">The rightmost allowed camera X position.</param>
    public void SetMaxCameraBounds(float minX, float maxX)
    {
        cameraSettings.minX = minX;
        cameraSettings.maxX = maxX;
    }

    /// <summary>
    /// Sets only the maximum X boundary for the camera.
    /// </summary>
    /// <param name="maxX">The rightmost allowed camera X position.</param>
    public void SetMaxCameraBounds(float maxX)
    {
        cameraSettings.maxX = maxX;
    }

    /// <summary>
    /// Enables or disables the dual-target (player + boss) follow mode.
    /// </summary>
    /// <param name="enabled">True to track both player and boss; false to track only the player.</param>
    public void SetFollowBothTargets(bool enabled)
    {
        bossFollow.followBothTargets = enabled;
    }
}
