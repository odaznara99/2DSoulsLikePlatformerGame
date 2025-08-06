using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance.VisualScripting;

public class EnemySkeletonStartDead : MonoBehaviour
{
    EnemyHealth enemyHealth;
    Animator animator;
    void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        animator = GetComponentInParent<Animator>();

        animator.SetBool("IsDead", true);
        animator.SetTrigger("Die");
        enemyHealth.isDead = true;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enemyHealth.isDead) {
            animator.SetBool("IsDead", false);
            animator.SetTrigger("Recover");
            enemyHealth.isDead = false;

            Destroy(this);
        }
    }
}
