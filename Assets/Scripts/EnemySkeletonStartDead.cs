using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance.VisualScripting;

public class EnemySkeletonStartDead : MonoBehaviour
{
    EnemyHealth enemyHealth;
    Animator animator;

    bool triggered = false;
    void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        animator = GetComponentInParent<Animator>();

        animator.SetBool("IsDead", true);
        enemyHealth.isDead = true;

        Invoke(nameof(TriggerDie), 0.5f); // Delay to Ensure to capture the trigger
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enemyHealth.isDead && !triggered) 
        {
            triggered = true;
            animator.SetTrigger("Recover");
            this.enabled = false; // Disable this script after triggering the recovery
        }
    }

    void TriggerDie()
    {
        animator.SetTrigger("Die");
    }
}
