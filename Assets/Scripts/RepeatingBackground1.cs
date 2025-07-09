using UnityEngine;

public class RepeatingBackground1 : MonoBehaviour
{
    private float spriteWidth;
    private Transform cam;
    public bool reverseScale = false;

    private Vector3 lastCamPosition;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPosition = cam.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        spriteWidth = sr.bounds.size.x;
    }

    void LateUpdate()
    {
        float camMovement = cam.position.x - lastCamPosition.x;

        if (Mathf.Abs(cam.position.x - transform.position.x) >= spriteWidth)
        {
            float offset = (cam.position.x - transform.position.x) > 0 ? spriteWidth : -spriteWidth;
            transform.position += new Vector3(offset * 2f, 0, 0);

            if (reverseScale)
            {
                // Optional: Flip the sprite to avoid visible seams
                Vector3 newScale = transform.localScale;
                newScale.x *= -1;
                transform.localScale = newScale;
            }
        }

        lastCamPosition = cam.position;
    }
}
