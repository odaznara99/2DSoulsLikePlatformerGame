using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth = 100f; // Health of the enemy

    [Header("Floating Healthbar")]
    [SerializeField] private GameObject healthBarPrefab;
    private FloatingHealthbar healthBarUI;

    [Header("Floating Damage Text")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvas;

    [Header("Blood Splash Effect")]
    [SerializeField]private GameObject m_bloodSplash;
    public Vector3 offset = new Vector3(0, 0.5f, 0); // Offset for the blood splash effect

    [Header("Sound Effects")]
    public List<string> damageSounds = new List<string>();
    public List<string> deathSounds = new List<string>();

    [Header("Flags")]
    public bool isDead = false; // Flag to check if the enemy is dead
    public bool isHurt = false; // Flag to check if the enemy is currently hurt
    //public float hurtDuration = 0.3f; // Duration of the hurt animation
    private Animator m_animator;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        worldCanvas = GameObject.Find("WorldSpaceCanvas").GetComponent<Transform>();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead || isHurt)
        {
            Debug.Log("Enemy is taking damage or dead");
            return; // Exit if the enemy is dead
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Update Floating Healthbar UI
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
           isDead = true;
           // Trigger Death Animation
           m_animator.SetTrigger("Die");
           m_animator.SetBool("IsDead", true);
        }
        else
        {
            isHurt = true;
            // Trigger Hurt Animation
            m_animator.SetTrigger("Hurt");
            m_animator.SetBool("IsHurting", true);
            // BloodSlash Effect
            Instantiate(m_bloodSplash, transform.position + offset, Quaternion.identity); // Instantiate blood splash effect
            // Show Floating Damage Text
            if (floatingTextPrefab)
            {
                GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up, Quaternion.identity, worldCanvas);
                ft.GetComponent<FloatingText>().SetText(damageAmount.ToString());
            }
            // Play Sound
            if(damageSounds.Count !=0)
                AudioManager.Instance.PlaySFX(damageSounds[Random.Range(0, damageSounds.Count)]);
        }

    }

    public void ResetHurtState() // Call this via State Behavior or Animation Event
    {
        m_animator.ResetTrigger("Hurt");
        m_animator.SetBool("IsHurting", false);
        isHurt = false;
    }
}
