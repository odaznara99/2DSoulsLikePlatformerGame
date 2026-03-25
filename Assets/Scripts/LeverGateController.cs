using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeverGateController : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("Unique ID for this lever within the scene. Set this to a non-empty string to make the gate state (open/closed) persist across scene reloads and game restarts.")]
    public string persistentId;

    [Header("References")]
    public GameObject gate;
    public bool isGateOpen = false;
    private bool stillRotatingLever = false;

    [Header("Gate Settings")]
    public Vector3 openPositionOffset = new Vector3(0, 3f, 0);
    public float openSpeed = 2f;

    private const float LeverOpenAngle   = 50f;
    private const float LeverClosedAngle = 130f;

    private Vector3 initialGatePosition;

    private bool isPlayerNearby = false;

    public Button leverButton; // Reference to the UI Button

    void Start()
    {
        if (gate != null)
            initialGatePosition = gate.transform.position;

        leverButton.onClick.AddListener(OnButtonClick);

        // Restore persisted gate state (no animation — instant snap to saved position).
        if (!string.IsNullOrEmpty(persistentId) &&
            SaveManager.Instance != null &&
            SaveManager.Instance.GetObjectState(SceneManager.GetActiveScene().name, persistentId))
        {
            isGateOpen = true;
            if (gate != null)
                gate.transform.position = initialGatePosition + openPositionOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, LeverOpenAngle);
        }
    }

    bool canToggleGate()
    {
        // Check if the lever is currently rotating
        if (stillRotatingLever)
            return false;

        // Check if the player is nearby
        if (!isPlayerNearby)
            return false;

        return true;
    }

    void Update()
    {
        if (canToggleGate())
        {
            leverButton.gameObject.SetActive(true);

            if (Keyboard.current.eKey.wasPressedThisFrame) {
                ToggleGate();
            }
        }
        else
        {
            leverButton.gameObject.SetActive(false);
        }
    }

    void OnButtonClick() {
        ToggleGate();
        leverButton.gameObject.SetActive(false);

    }

    void ToggleGate()
    {
        AudioManager.Instance.PlaySFX("Lever1");

        if (!isGateOpen)
        {
            isGateOpen = true;

            // Persist the new state.
            if (!string.IsNullOrEmpty(persistentId) && SaveManager.Instance != null)
                SaveManager.Instance.SetObjectState(SceneManager.GetActiveScene().name, persistentId, true);

            // Start lever rotation (e.g., rotate to 50 degrees on Z axis)
            StartCoroutine(RotateLever(new Vector3(0, 0, LeverOpenAngle)));

            
        } 
        else
        {
            isGateOpen = false;

            // Persist the new state.
            if (!string.IsNullOrEmpty(persistentId) && SaveManager.Instance != null)
                SaveManager.Instance.SetObjectState(SceneManager.GetActiveScene().name, persistentId, false);

            // Start lever rotation back to 0 degrees
            StartCoroutine(RotateLever(new Vector3(0, 0, LeverClosedAngle)));
        }
    }

    private IEnumerator OpenGate()
    {

        Vector3 targetPos = initialGatePosition + openPositionOffset;
        while (Vector3.Distance(gate.transform.position, targetPos) > 0.01f)
        {
            gate.transform.position = Vector3.MoveTowards(gate.transform.position, targetPos, openSpeed * Time.deltaTime);
            yield return null;
        }
        gate.transform.position = targetPos;
        stillRotatingLever = false; // Reset the lever rotation state



    }

    private IEnumerator CloseGate()
    {
        Vector3 targetPos = initialGatePosition;
        while (Vector3.Distance(gate.transform.position, targetPos) > 0.01f)
        {
            gate.transform.position = Vector3.MoveTowards(gate.transform.position, targetPos, openSpeed * Time.deltaTime);
            yield return null;
        }
        gate.transform.position = targetPos;
        stillRotatingLever = false; // Reset the lever rotation state
    }

    IEnumerator RotateLever(Vector3 targetRotation, float speed = 180f)
    {
        stillRotatingLever = true;
        Quaternion start = transform.rotation;
        Quaternion end = Quaternion.Euler(targetRotation);
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed / 90f; // Adjust divisor for speed
            transform.rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }

        transform.rotation = end;

        AudioManager.Instance.PlaySFX("GateOpening");
        if (isGateOpen)
            StartCoroutine(OpenGate());
        else
            StartCoroutine(CloseGate());
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // Optional: Show "Press E to interact"
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            // Optional: Hide interaction prompt
        }
    }
}
