using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private bool disableAfterUse = true;

    private bool activated;

    [SerializeField] private EnableGlowCheckpoint enableGlowScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player"))
        {
            return;
        }

        var gm = GameManager.Instance;
        if (gm == null)
        {
            return;
        }

        var player = other.GetComponent<PlayerControllerVersion2>();
        if (player != null)
        {
            gm.playerData.position = player.transform.position;

            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                gm.playerData.currentHealth = health.currentHealth;
                gm.playerData.maxHealth = health.maxHealth;
            }
        }

        gm.SaveCheckpointSnapshot();
        activated = true;

        if (enableGlowScript != null)
        {
            enableGlowScript.EnableGlowingLights();
        }

        if (disableAfterUse)
        {
            gameObject.SetActive(false);
        }

        MessageManager.Instance.ShowMessage("Stone of Recall Activated", false, 100);
    }
}