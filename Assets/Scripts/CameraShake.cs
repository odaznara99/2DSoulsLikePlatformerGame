using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultMagnitude = 0.3f;

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        originalPos = transform.localPosition;
        // Use defaults if no values passed
        if (duration < 0) duration = defaultDuration;
        if (magnitude < 0) magnitude = defaultMagnitude;

        // Restart shake if already running
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            float offsetZ = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetY, offsetY, offsetZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}
