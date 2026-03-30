using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this component to the on-screen stick GameObject alongside CustomOnScreenStick.
/// Tracks touch duration and fires a hold event only while the stick is actively touched.
/// </summary>
public class OnScreenStickTouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hold Settings")]
    [Tooltip("Seconds the stick must be held before the hold event fires.")]
    public float holdThreshold = 0.4f;

    [Header("Events")]
    public UnityEvent onStickTouched;
    public UnityEvent onStickReleased;
    public UnityEvent onHoldCompleted;

    [Header("Debug")]
    public bool enableDebugLog = true;

    /// <summary>
    /// True while the player is actively touching the stick.
    /// </summary>
    public bool IsTouching { get; private set; }

    private float pointerDownTime;
    private bool holdFired;
    private int activePointerId = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Only track the first finger that touches the stick
        if (IsTouching)
            return;

        activePointerId = eventData.pointerId;
        IsTouching = true;
        holdFired = false;
        pointerDownTime = Time.unscaledTime;
        onStickTouched?.Invoke();

        if (enableDebugLog)
            Debug.Log($"Stick touched (pointerId: {activePointerId}).");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Ignore pointer-up from a different finger
        if (eventData.pointerId != activePointerId)
            return;

        activePointerId = -1;
        IsTouching = false;
        holdFired = false;
        onStickReleased?.Invoke();

        if (enableDebugLog)
            Debug.Log("Stick released.");
    }

    void Update()
    {
        if (IsTouching && !holdFired)
        {
            if (Time.unscaledTime - pointerDownTime >= holdThreshold)
            {
                holdFired = true;
                onHoldCompleted?.Invoke();

                if (enableDebugLog)
                    Debug.Log("Stick hold completed.");
            }
        }
    }
}