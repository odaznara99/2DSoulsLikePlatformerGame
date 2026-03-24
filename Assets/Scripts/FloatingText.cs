using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float lifetime = 1f;
    public Vector3 moveDirection = new Vector3(0, 1, 0);

    private TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Optional: fade out
        Color color = text.color;
        color.a -= Time.deltaTime / lifetime;
        text.color = color;
    }

    public void SetText(string value)
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        text.text = value;
    }

    public void SetTextColor(Color color)
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        text.color = color;
    }
}
