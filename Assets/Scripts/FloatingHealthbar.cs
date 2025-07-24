using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthbar : MonoBehaviour
{
    public Slider slider;
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    private Transform target;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void SetHealth(float current, float max)
    {
        float percent = current / max;
        slider.value = percent;

        if (percent >= 1f)
        {
            gameObject.SetActive(false); // Hide if full health
        }
        else
        {
            gameObject.SetActive(true); // Show if damaged
        }
    }

    void Update()
    {
        if (target)
        {
            transform.position = target.position + offset;
            transform.LookAt(Camera.main.transform);
        }
    }

    public void DestroyBar()
    {
        Destroy(gameObject);
    }
}
