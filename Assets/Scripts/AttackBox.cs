using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{

    [Header("Attack Settings")]
    [SerializeField] private GameObject[] hitEffectPrefabs;
    [SerializeField] private float attackDamage = 20f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pick random index
        int index = Random.Range(0, hitEffectPrefabs.Length);

        // Calculate the closest contact point between your attack and the object
        Vector2 contactPoint = other.ClosestPoint(transform.position);

        if (other.CompareTag("Enemy"))
        {
            // Get the enemy script
            Bandit enemyScript = other.GetComponent<Bandit>();

            if (enemyScript.currentState != EnemyState.Dead)
            {

                // Apply damage to the enemy
                enemyScript.TakeDamage(attackDamage);
                // Instantiate the randomly chosen effect
                GameObject fx = Instantiate(hitEffectPrefabs[index], contactPoint, Quaternion.identity);

                AudioManager.Instance.PlaySFX("SwordImpact");
            }


            // Apply knockback to the enemy
            Vector2 knockDirection = other.transform.position - transform.position;
            enemyScript.ApplyKnockback(knockDirection, 2f);
        }
        else if (other.CompareTag("Skeleton"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth.isDead == false) { 
                enemyHealth.TakeDamage(attackDamage);
                // Instantiate the randomly chosen effect
                GameObject fx = Instantiate(hitEffectPrefabs[index], contactPoint, Quaternion.identity);
                // Play Impact Sound
                AudioManager.Instance.PlaySFX("SwordImpact");
            }

        }
        else if (other.CompareTag("Boss"))
        {
            BossAI bossScript = other.GetComponent<BossAI>();



            if (!bossScript.IsDead())
            {
                // Apply damage to the boss
                bossScript.TakeDamage(attackDamage);
                //AudioManager.Instance.PlaySFX("Attack1");
                AudioManager.Instance.PlaySFX("SwordImpact");
                // Instantiate the randomly chosen effect
                GameObject fx = Instantiate(hitEffectPrefabs[index], contactPoint, Quaternion.identity);

            }
        }
        else if (other.CompareTag("Breakable"))
        {
            // If the enemy is a breakable object, then break it
            BreakableObject breakableObject = other.GetComponent<BreakableObject>();
            if (breakableObject != null)
            {
                breakableObject.TakeHit();
                // Instantiate the randomly chosen effect
                GameObject fx = Instantiate(hitEffectPrefabs[index], contactPoint, Quaternion.identity);
                //AudioManager.Instance.PlaySFX("Attack1");
            }
        }
    }

}
