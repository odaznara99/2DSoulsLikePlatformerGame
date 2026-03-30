using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

/// <summary>
/// Drop-in replacement for Unity's OnScreenStick that correctly handles multi-touch.
/// Only the finger that initially touched the stick can move or release it.
/// Supports multiple behaviour modes matching Unity's built-in OnScreenStick.
/// </summary>
public class CustomOnScreenStick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public enum StickBehaviour
    {
        RelativePositionWithStaticOrigin,
        ExactPositionWithStaticOrigin,
        ExactPositionWithDynamicOrigin
    }

    [Header("Stick Settings")]
    public float movementRange = 50f;
    public StickBehaviour behaviour = StickBehaviour.RelativePositionWithStaticOrigin;

    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    private RectTransform stickTransform;
    private RectTransform parentTransform;
    private Vector2 startPosition;
    private Vector2 pointerDownLocalPoint;
    private int activePointerId = -1;

    protected override void OnEnable()
    {
        base.OnEnable();
        stickTransform = GetComponent<RectTransform>();
        parentTransform = stickTransform.parent as RectTransform;
        startPosition = stickTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Only accept the first finger
        if (activePointerId != -1)
            return;

        activePointerId = eventData.pointerId;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerDownLocalPoint);

        if (behaviour == StickBehaviour.ExactPositionWithDynamicOrigin)
        {
            // Move the stick origin to where the player pressed
            startPosition = pointerDownLocalPoint;
            stickTransform.anchoredPosition = startPosition;
        }

        HandleDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
            return;

        HandleDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
            return;

        activePointerId = -1;

        if (behaviour == StickBehaviour.ExactPositionWithDynamicOrigin)
        {
            // Restore original position when using dynamic origin
            startPosition = stickTransform.parent != null
                ? GetComponent<RectTransform>().anchoredPosition
                : startPosition;
        }

        stickTransform.anchoredPosition = startPosition;
        SendValueToControl(Vector2.zero);
    }

    private void HandleDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        Vector2 delta;

        switch (behaviour)
        {
            case StickBehaviour.RelativePositionWithStaticOrigin:
                // Delta is measured from the initial press point, not the stick center
                delta = localPoint - pointerDownLocalPoint;
                break;

            case StickBehaviour.ExactPositionWithStaticOrigin:
            case StickBehaviour.ExactPositionWithDynamicOrigin:
                // Delta is measured from the stick origin
                delta = localPoint - startPosition;
                break;

            default:
                delta = Vector2.zero;
                break;
        }

        delta = Vector2.ClampMagnitude(delta, movementRange);
        stickTransform.anchoredPosition = startPosition + delta;
        SendValueToControl(delta / movementRange);
    }
}