using UnityEngine;
using System.Collections;

public class LeverGateController : MonoBehaviour
{
    [Header("References")]
    public GameObject gate;
    public bool isGateOpen = false;
    private bool stillRotatingLever = false;

    [Header("Gate Settings")]
    public Vector3 openPositionOffset = new Vector3(0, 3f, 0);
    public float openSpeed = 2f;

    private Vector3 initialGatePosition;

    private bool isPlayerNearby = false;

    void Start()
    {
        if (gate != null)
            initialGatePosition = gate.transform.position;
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
        if (canToggleGate() && Input.GetKeyDown(KeyCode.E))
        {
            ToggleGate();
        }
    }

    void ToggleGate()
    {
        if (!isGateOpen)
        {
            isGateOpen = true;

            // Start lever rotation (e.g., rotate to 50 degrees on Z axis)
            StartCoroutine(RotateLever(new Vector3(0, 0, 50f)));

            
        } 
        else
        {
            isGateOpen = false;

            // Start lever rotation back to 0 degrees
            StartCoroutine(RotateLever(new Vector3(0, 0, 130f)));
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
