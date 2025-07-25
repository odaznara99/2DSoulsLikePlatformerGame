using UnityEngine;

public class BossZoneCameraTrigger : MonoBehaviour
{
    [Tooltip("Assign your SmoothCameraFollow script (usually on Main Camera)")]
    public SmoothCameraFollow cameraFollow;

    [Tooltip("Assign the boss transform (what the camera will center with player)")]
    public Transform bossTarget;

    [Tooltip("Revert to player-only camera when player leaves?")]
    public bool revertOnExit = true;

    public BossHealthUI bossHealthUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && cameraFollow != null && bossTarget != null)
        {
            cameraFollow.bossTarget = bossTarget;
            cameraFollow.SetFollowBothTargets(true);
            bossHealthUI.SetHealthUIActive(true); // Show boss health UI
            //AudioManager.Instance.StopMusic();
            //AudioManager.Instance.PlayMusic("MedievalOpener"); // Play sound when entering boss zone
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (revertOnExit && other.CompareTag("Player") && cameraFollow != null)
        {
            cameraFollow.SetFollowBothTargets(false);
            bossHealthUI.SetHealthUIActive(false); // Show boss health UI
            //AudioManager.Instance.StopMusic(); // Stop boss music when player exits
            //AudioManager.Instance.PlayMusic("Ballad");
        }
    }
}
