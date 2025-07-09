using UnityEngine;

public class RepeatingBackground : MonoBehaviour
{
    public BoxCollider2D backgroundCollider;
    private float backgroundWidth;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        backgroundWidth = backgroundCollider.size.x * transform.localScale.x; // Scale-aware
    }

    void Update()
    {
        float camHorizontalExtent = Camera.main.orthographicSize * Screen.width / Screen.height;
        float edgeVisiblePositionRight = transform.position.x + (backgroundWidth / 2f);
        float edgeVisiblePositionLeft = transform.position.x - (backgroundWidth / 2f);

        float camPosX = cam.position.x;

        // If the camera has moved past the right edge of the background
        if (camPosX > edgeVisiblePositionRight)
        {
            RepositionBackground(+1);
        }
        // If the camera has moved past the left edge of the background
        else if (camPosX < edgeVisiblePositionLeft)
        {
            RepositionBackground(-1);
        }
    }

    private void RepositionBackground(int direction)
    {
        Vector3 offset = new Vector3(backgroundWidth * direction, 0, 0);
        transform.position += offset;
    }
}
