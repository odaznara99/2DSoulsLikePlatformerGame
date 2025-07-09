using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public float parallaxFactor = 0.5f; // 0 = static, 1 = moves with camera
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
