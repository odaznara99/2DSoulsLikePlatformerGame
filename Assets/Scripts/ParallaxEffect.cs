using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Tooltip("Parallax scrolling factor (0 = static background, 1 = moves 1:1 with camera).")]
    public float parallaxFactor = 0.5f;
    private Transform cam;
    private Vector3 previousCamPos;

    private void Start()
    {
        cam = Camera.main.transform;
        previousCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - previousCamPos;
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0);
        previousCamPos = cam.position;
    }
}
