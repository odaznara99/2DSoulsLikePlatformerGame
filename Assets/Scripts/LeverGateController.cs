using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeverGateController : MonoBehaviour
{
    [System.Serializable]
    public class GateReferences
    {
        public GameObject gate;
        public bool isGateOpen = false;
        public Button leverButton;
    }

    [System.Serializable]
    public class GateMovementSettings
    {
        public Vector3 openPositionOffset = new Vector3(0, 3f, 0);
        public float openSpeed = 2f;
    }

    [Header("Persistence")]
    [Tooltip("Unique ID for this lever within the scene. Set this to a non-empty string to make the gate state (open/closed) persist across scene reloads and game restarts.")]
    public string persistentId;

    [Header("References")]
    public GateReferences refs = new GateReferences();

    [Header("Gate Settings")]
    public GateMovementSettings gateSettings = new GateMovementSettings();

    private const float LeverOpenAngle   = 50f;
    private const float LeverClosedAngle = 130f;

    private Vector3 initialGatePosition;
    private bool isPlayerNearby    = false;
    private bool stillRotatingLever = false;

    /// <summary>
    /// Caches the gate's initial position, registers the button listener, and restores any
    /// persisted open/closed state without playing the open animation.
    /// </summary>
    void Start()
    {
        if (refs.gate != null)
            initialGatePosition = refs.gate.transform.position;

        refs.leverButton.onClick.AddListener(OnButtonClick);

        // Restore persisted gate state (no animation — instant snap to saved position).
        if (!string.IsNullOrEmpty(persistentId) &&
            SaveManager.Instance != null &&
            SaveManager.Instance.GetObjectState(SceneManager.GetActiveScene().name, persistentId))
        {
            refs.isGateOpen = true;
            if (refs.gate != null)
                refs.gate.transform.position = initialGatePosition + gateSettings.openPositionOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, LeverOpenAngle);
        }
    }

    /// <summary>
    /// Returns true when the gate can currently be toggled (player is nearby and lever is not animating).
    /// </summary>
    bool canToggleGate()
    {
        if (stillRotatingLever) return false;
        if (!isPlayerNearby)    return false;
        return true;
    }

    /// <summary>
    /// Shows or hides the interaction button and listens for the E key to toggle the gate.
    /// </summary>
    void Update()
    {
        if (canToggleGate())
        {
            refs.leverButton.gameObject.SetActive(true);

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                ToggleGate();
            }
        }
        else
        {
            refs.leverButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the UI button click by toggling the gate and hiding the button.
    /// </summary>
    void OnButtonClick()
    {
        ToggleGate();
        refs.leverButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggles the gate between open and closed, persists the new state, and starts the lever rotation coroutine.
    /// </summary>
    void ToggleGate()
    {
        AudioManager.Instance.PlaySFX("Lever1");

        if (!refs.isGateOpen)
        {
            refs.isGateOpen = true;

            if (!string.IsNullOrEmpty(persistentId) && SaveManager.Instance != null)
                SaveManager.Instance.SetObjectState(SceneManager.GetActiveScene().name, persistentId, true);

            StartCoroutine(RotateLever(new Vector3(0, 0, LeverOpenAngle)));
        }
        else
        {
            refs.isGateOpen = false;

            if (!string.IsNullOrEmpty(persistentId) && SaveManager.Instance != null)
                SaveManager.Instance.SetObjectState(SceneManager.GetActiveScene().name, persistentId, false);

            StartCoroutine(RotateLever(new Vector3(0, 0, LeverClosedAngle)));
        }
    }

    /// <summary>
    /// Coroutine that slides the gate to its open position and then clears the rotation lock.
    /// </summary>
    private IEnumerator OpenGate()
    {
        Vector3 targetPos = initialGatePosition + gateSettings.openPositionOffset;
        while (Vector3.Distance(refs.gate.transform.position, targetPos) > 0.01f)
        {
            refs.gate.transform.position = Vector3.MoveTowards(refs.gate.transform.position, targetPos, gateSettings.openSpeed * Time.deltaTime);
            yield return null;
        }
        refs.gate.transform.position = targetPos;
        stillRotatingLever = false;
    }

    /// <summary>
    /// Coroutine that slides the gate back to its closed position and then clears the rotation lock.
    /// </summary>
    private IEnumerator CloseGate()
    {
        Vector3 targetPos = initialGatePosition;
        while (Vector3.Distance(refs.gate.transform.position, targetPos) > 0.01f)
        {
            refs.gate.transform.position = Vector3.MoveTowards(refs.gate.transform.position, targetPos, gateSettings.openSpeed * Time.deltaTime);
            yield return null;
        }
        refs.gate.transform.position = targetPos;
        stillRotatingLever = false;
    }

    /// <summary>
    /// Coroutine that smoothly rotates the lever to the target Euler angle, then triggers the
    /// appropriate gate open or close coroutine.
    /// </summary>
    /// <param name="targetRotation">The target Euler angles for the lever.</param>
    /// <param name="speed">Rotation speed in degrees per second.</param>
    IEnumerator RotateLever(Vector3 targetRotation, float speed = 180f)
    {
        stillRotatingLever = true;
        Quaternion start = transform.rotation;
        Quaternion end   = Quaternion.Euler(targetRotation);
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed / 90f;
            transform.rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }

        transform.rotation = end;

        AudioManager.Instance.PlaySFX("GateOpening");
        if (refs.isGateOpen)
            StartCoroutine(OpenGate());
        else
            StartCoroutine(CloseGate());
    }

    /// <summary>
    /// Detects when the player enters the interaction trigger zone.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    /// <summary>
    /// Detects when the player leaves the interaction trigger zone.
    /// </summary>
    /// <param name="other">The collider that exited the trigger.</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
