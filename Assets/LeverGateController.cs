using UnityEngine;
using System.Collections;

public class LeverGateController : MonoBehaviour
{
    [Header("References")]
    public GameObject gate;
    public bool isGateOpen = false;

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

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleGate();
        }
    }

    void ToggleGate()
    {
        if (!isGateOpen)
        {
            isGateOpen = true;

            // Start lever rotation (e.g., rotate to -45 degrees on Z axis)
            StartCoroutine(RotateLever(new Vector3(0, 0, 50f)));

            StartCoroutine(OpenGate());
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

        
    }

    IEnumerator RotateLever(Vector3 targetRotation, float speed = 180f)
    {
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
