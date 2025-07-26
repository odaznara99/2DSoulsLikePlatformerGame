using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private int hitPoints = 3;
    //[SerializeField] private GameObject hitVFX;
    //[SerializeField] private AudioClip hitSound;

    [Header("Break Settings")]
    [SerializeField] private GameObject brokenPrefab;
    //[SerializeField] private GameObject breakVFX;
    //[SerializeField] private AudioClip breakSound;
    [SerializeField] private float debrisForce = 3f;
    [SerializeField] private float debrisLifetime = 3f;

    private bool isBroken = false;

    public void TakeHit()
    {
        if (isBroken) return;

        hitPoints--;

        // 🔥 Play hit feedback
       /* if (hitVFX != null)
            Instantiate(hitVFX, transform.position, Quaternion.identity);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position); */

        if (hitPoints <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        if (isBroken) return;
        isBroken = true;

        // 💥 Spawn broken object
        GameObject brokenInstance = Instantiate(brokenPrefab, transform.position, transform.rotation);

        Rigidbody2D[] parts = brokenInstance.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in parts)
        {
            Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)).normalized * debrisForce;
            rb.AddForce(force, ForceMode2D.Impulse);
            Destroy(rb.gameObject, debrisLifetime);
        }

       /* if (breakVFX != null)
            Instantiate(breakVFX, transform.position, Quaternion.identity);

        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);*/

        Destroy(gameObject);
    }

}
