using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject[] hitEffectPrefabs;
    public float attackDamage = 20f;

    private int randomIndex;

    private void Start()
    {
        GetComponent<BoxCollider2D>().enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pick random index
        if (hitEffectPrefabs.Length == 0)
        {
            Debug.LogWarning("No hit effect prefabs assigned to the EnemyAttack!");
            //return;
        }
        else 
        {
            randomIndex = Random.Range(0, hitEffectPrefabs.Length); 
        }

        // Calculate the closest contact point between your attack and the object
        Vector2 contactPoint = other.ClosestPoint(transform.position);

        if (other.CompareTag("Player"))
        {
            // Get the enemy script
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth.IsDead() == false)
            {

                // Apply damage to the enemy
                playerHealth.TakeDamage(attackDamage,this.gameObject);
                
                // Instantiate the randomly chosen effect
                if (hitEffectPrefabs.Length != 0)
                {
                    GameObject fx = Instantiate(hitEffectPrefabs[randomIndex], contactPoint, Quaternion.identity);
                }

                AudioManager.Instance.PlaySFX("SwordImpact");
            }

        }
    }
}
